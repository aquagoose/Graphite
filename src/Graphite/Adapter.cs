namespace Graphite;

/// <summary>
/// An adapter is a device, whether physical or virtual, that can be issued render and compute commands.
/// </summary>
public readonly record struct Adapter
{
    /// <summary>
    /// The internal handle of the adapter. This is generally for internal use only and you should not store this.
    /// </summary>
    public readonly nint Handle;

    /// <summary>
    /// The index. Generally, adapters with a lower index value will have a higher priority.
    /// </summary>
    public readonly uint Index;

    /// <summary>
    /// The friendly name. For example, "Nvidia GTX 1080"
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The <see cref="AdapterType"/>, denoting if this is a integrated or dedicated adapter.
    /// </summary>
    public readonly AdapterType Type;

    /// <summary>
    /// The amount of video memory that the adapter has access to.
    /// </summary>
    public readonly ulong DedicatedMemory;

    public Adapter(IntPtr handle, uint index, string name, AdapterType type, ulong dedicatedMemory)
    {
        Handle = handle;
        Index = index;
        Name = name;
        Type = type;
        DedicatedMemory = dedicatedMemory;
    }

    public bool Equals(Adapter? other)
    {
        return Handle == other?.Handle;
    }
}