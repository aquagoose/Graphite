namespace Graphite;

/// <summary>
/// Represents various supported <see cref="Surface"/> types.
/// </summary>
public enum SurfaceType
{
    /// <summary>
    /// Windows/Win32.
    /// </summary>
    Win32,
    
    /// <summary>
    /// Wayland
    /// </summary>
    Wayland,
    
    /// <summary>
    /// X11 with XLib
    /// </summary>
    Xlib,
    
    /// <summary>
    /// X11 with X Connection Bus
    /// </summary>
    Xcb
}