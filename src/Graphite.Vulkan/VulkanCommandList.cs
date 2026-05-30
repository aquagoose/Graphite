using Silk.NET.Vulkan;

namespace Graphite.Vulkan;

internal sealed unsafe class VulkanCommandList : CommandList
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkDevice _device;
    private readonly CommandPool _pool;
    
    private readonly Queue<CommandBuffer> _availableCommandBuffers;
    private readonly List<(CommandBuffer cb, Fence fence)> _submittedCommandBuffers;
    
    public VulkanCommandList(Vk vk, VkDevice device, ref readonly Queues queues)
    {
        _vk = vk;
        _device = device;
        
        _availableCommandBuffers = [];
        _submittedCommandBuffers = [];

        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queues.GraphicsIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };
        
        GLog.Log("Creating command pool.");
        _vk.CreateCommandPool(_device, &poolInfo, null, out _pool).Check("Create command pool");
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        foreach ((_, Fence fence) in _submittedCommandBuffers)
            _vk.DestroyFence(_device, fence, null);
        
        _vk.DestroyCommandPool(_device, _pool, null);
    }
}