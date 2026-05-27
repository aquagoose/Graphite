using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphite.Vulkan;

internal sealed unsafe class VulkanInstance : Instance
{
    public static readonly uint ApiVersion = Vk.Version13;
    
    public override bool IsDisposed { get; protected set; }
    
    private readonly Vk _vk;
    private readonly VkInstance _instance;
    
    public VulkanInstance(ref readonly InstanceInfo info)
    {
        _vk = Vk.GetApi();

        nint pAppName = SilkMarshal.StringToPtr(info.AppName);
        nint pEngineName = SilkMarshal.StringToPtr("Graphite");
        
        GLog.LogInfo($"App name: {info.AppName}");

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) pAppName,
            ApplicationVersion = Vk.MakeVersion(1, 0, 0),
            PEngineName = (byte*) pEngineName,
            EngineVersion = Vk.MakeVersion(1, 0, 0),
            ApiVersion = ApiVersion
        };

        uint numInstanceExtensionProperties;
        _vk.EnumerateInstanceExtensionProperties((byte*) null, &numInstanceExtensionProperties, null);
        ExtensionProperties* instanceExtensionProperties = stackalloc ExtensionProperties[(int) numInstanceExtensionProperties];
        _vk.EnumerateInstanceExtensionProperties((byte*) null, &numInstanceExtensionProperties, instanceExtensionProperties);
        
        List<string> instanceExtensions = [];
        for (uint i = 0; i < numInstanceExtensionProperties; i++)
        {
            // mmm heap allocations i love it
            string extensionName = new string((sbyte*) instanceExtensionProperties[i].ExtensionName);
            GLog.LogInfo(extensionName);

            switch (extensionName)
            {
                // TODO: macos/cocoa surface?
                case KhrWin32Surface.ExtensionName:
                case KhrWaylandSurface.ExtensionName:
                case KhrXlibSurface.ExtensionName:
                case KhrXcbSurface.ExtensionName:
                    instanceExtensions.Add(extensionName);
                    break;
            }
        }

        nint pInstanceExtensions = SilkMarshal.StringArrayToPtr(instanceExtensions);
        
        InstanceCreateInfo instanceInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            
            EnabledExtensionCount = (uint) instanceExtensions.Count,
            PpEnabledExtensionNames = (byte**) pInstanceExtensions
        };
        
        GLog.Log("Creating instance.");
        _vk.CreateInstance(&instanceInfo, null, out _instance).Check("Create instance");

        SilkMarshal.Free(pInstanceExtensions);
        SilkMarshal.FreeString(pEngineName);
        SilkMarshal.FreeString(pAppName);
    }

    public override Adapter[] EnumerateAdapters()
    {
        uint numPhysicalDevices;
        _vk.EnumeratePhysicalDevices(_instance, &numPhysicalDevices, null);
        PhysicalDevice* physicalDevices = stackalloc PhysicalDevice[(int) numPhysicalDevices];
        _vk.EnumeratePhysicalDevices(_instance, &numPhysicalDevices, physicalDevices);

        List<Adapter> adapters = [];
        
        for (uint i = 0; i < numPhysicalDevices; i++)
        {
            PhysicalDevice device = physicalDevices[i];

            PhysicalDeviceProperties properties;
            _vk.GetPhysicalDeviceProperties(device, &properties);
            
            if (properties.ApiVersion < ApiVersion) // device does not support vulkan 1.3
                continue;

            string name = new string((sbyte*) properties.DeviceName);
            AdapterType type = properties.DeviceType switch
            {
                PhysicalDeviceType.Other => AdapterType.Unknown,
                PhysicalDeviceType.IntegratedGpu => AdapterType.Integrated,
                PhysicalDeviceType.DiscreteGpu => AdapterType.Dedicated,
                PhysicalDeviceType.VirtualGpu => AdapterType.Unknown,
                PhysicalDeviceType.Cpu => AdapterType.Software,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            PhysicalDeviceMemoryProperties memoryProperties;
            _vk.GetPhysicalDeviceMemoryProperties(device, &memoryProperties);

            ulong dedicatedMemory = 0;
            if (memoryProperties.MemoryHeapCount < 1) // device has no memory??
                GLog.LogWarning("Device's memory heap count was 0.");
            else
                dedicatedMemory = memoryProperties.MemoryHeaps[0].Size;
            
            adapters.Add(new Adapter(device.Handle, i, name, type, dedicatedMemory));
        }

        // TODO: return IReadOnlyCollection to remove conversion to array?
        return adapters.ToArray();
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}