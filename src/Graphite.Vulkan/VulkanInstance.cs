using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphite.Vulkan;

internal sealed unsafe class VulkanInstance : Instance
{
    private readonly Vk _vk;
    private readonly VkInstance _instance;
    
    public VulkanInstance(ref readonly InstanceInfo info)
    {
        _vk = Vk.GetApi();

        nint pAppName = SilkMarshal.StringToPtr(info.AppName);
        nint pEngineName = SilkMarshal.StringToPtr("Graphite");
        
        Instance.Log($"App name: {info.AppName}", DebugMessageSeverity.Info);

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) pAppName,
            ApplicationVersion = Vk.MakeVersion(1, 0, 0),
            PEngineName = (byte*) pEngineName,
            EngineVersion = Vk.MakeVersion(1, 0, 0),
            ApiVersion = Vk.Version13
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
            Instance.Log(extensionName, DebugMessageSeverity.Info);

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
        
        Instance.Log("Creating instance.");
        _vk.CreateInstance(&instanceInfo, null, out _instance).Check("Create instance");

        SilkMarshal.Free(pInstanceExtensions);
        SilkMarshal.FreeString(pEngineName);
        SilkMarshal.FreeString(pAppName);
    }
    
    public override void Dispose()
    {
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}