using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphite.Vulkan;

internal sealed unsafe class VulkanDevice : Device
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkInstance _instance;
    private readonly PhysicalDevice _physicalDevice;

    private readonly Queues _queues;

    private readonly VkDevice _device;
    
    public VulkanDevice(Vk vk, VkInstance instance, VulkanSurface surface, PhysicalDevice physicalDevice)
    {
        _vk = vk;
        _instance = instance;
        _physicalDevice = physicalDevice;

        uint numQueueFamilies;
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &numQueueFamilies, null);
        QueueFamilyProperties* queueFamilyProperties = stackalloc QueueFamilyProperties[(int) numQueueFamilies];
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &numQueueFamilies, queueFamilyProperties);

        uint? graphicsQueueIndex = null;
        uint? presentQueueIndex = null;
        
        for (uint i = 0; i < numQueueFamilies; i++)
        {
            if ((queueFamilyProperties[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                graphicsQueueIndex = i;

            surface.KhrSurface.GetPhysicalDeviceSurfaceSupport(_physicalDevice, i, surface.Surface, out Bool32 supported);
            if (supported)
                presentQueueIndex = i;

            if (graphicsQueueIndex != null && presentQueueIndex != null)
                break;
        }

        if (graphicsQueueIndex == null || presentQueueIndex == null)
        {
            throw new Exception(
                $"Graphics/Present queues were not found! Found Graphics: {graphicsQueueIndex.HasValue}, Present: {presentQueueIndex.HasValue}");
        }

        _queues.GraphicsIndex = graphicsQueueIndex.Value;
        _queues.PresentIndex = presentQueueIndex.Value;

        HashSet<uint> uniqueQueues = _queues.UniqueQueues;
        DeviceQueueCreateInfo* queueCreateInfos = stackalloc DeviceQueueCreateInfo[uniqueQueues.Count];
        uint currentQueueIndex = 0;
        float queuePriority = 1.0f;
        foreach (uint queueIndex in uniqueQueues)
        {
            queueCreateInfos[currentQueueIndex++] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueCount = 1,
                QueueFamilyIndex = queueIndex,
                PQueuePriorities = &queuePriority
            };
        }

        string[] deviceExtensions = [KhrSwapchain.ExtensionName];
        nint pDeviceExtensions = SilkMarshal.StringArrayToPtr(deviceExtensions);
        
        PhysicalDeviceFeatures enabledFeatures = new();

        DeviceCreateInfo deviceInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,

            QueueCreateInfoCount = (uint) uniqueQueues.Count,
            PQueueCreateInfos = queueCreateInfos,

            EnabledExtensionCount = (uint) deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**) pDeviceExtensions,

            PEnabledFeatures = &enabledFeatures
        };

        PhysicalDeviceDynamicRenderingFeatures dynamicRendering = new()
        {
            SType = StructureType.PhysicalDeviceDynamicRenderingFeatures,
            DynamicRendering = true
        };
        deviceInfo.PNext = &dynamicRendering;

        PhysicalDeviceSynchronization2Features sync2 = new()
        {
            SType = StructureType.PhysicalDeviceSynchronization2Features,
            Synchronization2 = true
        };
        dynamicRendering.PNext = &sync2;
        
        GLog.Log("Creating device.");
        _vk.CreateDevice(_physicalDevice, &deviceInfo, null, out _device).Check("Create device");
        
        GLog.Log("Getting device queues.");
        _vk.GetDeviceQueue(_device, _queues.GraphicsIndex, 0, out _queues.Graphics);
        _vk.GetDeviceQueue(_device, _queues.PresentIndex, 0, out _queues.Present);
    }

    public override CommandList CreateCommandList()
    {
        return new VulkanCommandList(_vk, _device, in _queues);
    }

    public override void ExecuteCommandList(CommandList cl)
    {
        VulkanCommandList vulkanCL = (VulkanCommandList) cl;
        Debug.Assert(vulkanCL.CurrentCommandBuffer.Handle != 0,
            "Cannot execute: Command list has not had any commands submitted to it!");
        Debug.Assert(vulkanCL.CurrentFence.Handle != 0, "Cannot execute: Command list has not been ended!");

        CommandBuffer cb = vulkanCL.CurrentCommandBuffer;

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &cb
        };

        _vk.QueueSubmit(_queues.Graphics, 1, &submitInfo, vulkanCL.CurrentFence);
        vulkanCL.SignalSubmitted();
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _vk.DeviceWaitIdle(_device).Check("Wait for device idle");
        _vk.DestroyDevice(_device, null);
    }
}