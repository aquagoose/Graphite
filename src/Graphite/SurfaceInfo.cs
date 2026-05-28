namespace Graphite;

/// <summary>
/// Info to describe a <see cref="Surface"/>.
/// </summary>
public record struct SurfaceInfo
{
    /// <summary>
    /// The type of surface.
    /// </summary>
    public SurfaceType Type;

    /// <summary>
    /// The window pointer.
    /// </summary>
    public nint Window;

    /// <summary>
    /// The display pointer.
    /// </summary>
    public nint Display;

    public SurfaceInfo(SurfaceType type, IntPtr window, IntPtr display)
    {
        Type = type;
        Window = window;
        Display = display;
    }
}