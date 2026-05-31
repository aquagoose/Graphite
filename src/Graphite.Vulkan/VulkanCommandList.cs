using System.Diagnostics;
using Silk.NET.Vulkan;

namespace Graphite.Vulkan;

internal sealed unsafe class VulkanCommandList : CommandList
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkDevice _device;
    private readonly CommandPool _pool;

    private readonly Queue<Fence> _availableFences;
    private readonly Queue<CommandBuffer> _availableCommandBuffers;
    private readonly List<(CommandBuffer cb, Fence fence)> _submittedCommandBuffers;

    /// <summary>
    /// The currently executing command buffer. If not executing, its handle will be 0/null.
    /// </summary>
    public CommandBuffer CurrentCommandBuffer;

    /// <summary>
    /// The currently active fence. If the command list has not been ended, its handle will be 0/null.
    /// </summary>
    public Fence CurrentFence;
    
    public VulkanCommandList(Vk vk, VkDevice device, ref readonly Queues queues)
    {
        _vk = vk;
        _device = device;

        _availableFences = [];
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

    public override void Begin()
    {
        Debug.Assert(CurrentCommandBuffer.Handle == 0, "Cannot begin: CommandList must be executed first!");
        
        // try to get the next available command buffer
        // if none are available, create a new one
        if (!_availableCommandBuffers.TryDequeue(out CommandBuffer cb))
        {
            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _pool,
                CommandBufferCount = 1,
                Level = CommandBufferLevel.Primary
            };
            
            GLog.Log("Allocating new command buffer.");
            _vk.AllocateCommandBuffers(_device, &allocInfo, out cb).Check("Allocate command buffer");
        }

        CurrentCommandBuffer = cb;

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit, // CLs are only allowed to be submitted once
        };

        _vk.BeginCommandBuffer(CurrentCommandBuffer, &beginInfo).Check("Begin command buffer");
    }
    
    public override void End()
    {
        Debug.Assert(CurrentCommandBuffer.Handle != 0, "Cannot end: Must call Begin() first!");
        _vk.EndCommandBuffer(CurrentCommandBuffer).Check("End command buffer");

        if (!_availableFences.TryDequeue(out Fence fence))
        {
            FenceCreateInfo fenceInfo = new()
            {
                SType = StructureType.FenceCreateInfo
            };
            
            GLog.Log("Creating fence.");
            _vk.CreateFence(_device, &fenceInfo, null, out fence).Check("Create fence");
        }

        CurrentFence = fence;
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        foreach ((_, Fence fence) in _submittedCommandBuffers)
            _vk.DestroyFence(_device, fence, null);
        
        foreach (Fence fence in _availableFences)
            _vk.DestroyFence(_device, fence, null);
        
        // no need to free the command buffers manually as destroying the cmd pool will do it automatically
        _vk.DestroyCommandPool(_device, _pool, null);
    }

    internal void SignalSubmitted()
    {
        _submittedCommandBuffers.Add((CurrentCommandBuffer, CurrentFence));
        CurrentCommandBuffer = new CommandBuffer();
        CurrentFence = new Fence();
        
        for (int i = 0; i < _submittedCommandBuffers.Count; i++)
        {
            (CommandBuffer cb, Fence fence) = _submittedCommandBuffers[i];
            if (_vk.GetFenceStatus(_device, fence) != Result.Success)
                continue;
            
            // return the command buffer and fence back to the available pool
            _availableCommandBuffers.Enqueue(cb);
            _availableFences.Enqueue(fence);
            _submittedCommandBuffers.RemoveAt(i--);
        }
    }
}