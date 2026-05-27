namespace Graphite;

/// <summary>
/// Represents various different types of adapter.
/// </summary>
public enum AdapterType
{
    /// <summary>
    /// The type of this adapter is not known.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Software adapter.
    /// </summary>
    Software,
    
    /// <summary>
    /// An integrated adapter, for example an iGPU.
    /// </summary>
    Integrated,
    
    /// <summary>
    /// A dedicated adapter, for example a dedicated graphics card.
    /// </summary>
    Dedicated
}