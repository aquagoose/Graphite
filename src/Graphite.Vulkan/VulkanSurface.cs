using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphite.Vulkan;

internal sealed unsafe class VulkanSurface : Surface
{
    public override bool IsDisposed { get; protected set; }

    private readonly Vk _vk;
    private readonly VkInstance _instance;

    private readonly KhrSurface _khrSurface;
    private readonly KhrWin32Surface? _khrWin32Surface;
    private readonly KhrWaylandSurface? _khrWaylandSurface;
    private readonly KhrXlibSurface? _khrXlibSurface;
    private readonly KhrXcbSurface? _khrXcbSurface;
    
    public readonly SurfaceKHR Surface;
    
    public VulkanSurface(Vk vk, VkInstance instance, ref readonly SurfaceInfo info)
    {
        _vk = vk;
        _instance = instance;

        if (!_vk.TryGetInstanceExtension(_instance, out _khrSurface))
            throw new Exception("Failed to get KhrSurface extension.");

        switch (info.Type)
        {
            case SurfaceType.Win32:
            {
                if (!_vk.TryGetInstanceExtension(_instance, out _khrWin32Surface))
                    throw new Exception("Failed to get KhrWin32Surface extension.");

                Win32SurfaceCreateInfoKHR surfaceInfo = new()
                {
                    SType = StructureType.Win32SurfaceCreateInfoKhr,
                    Hwnd = info.Window,
                    Hinstance = info.Display
                };

                GLog.Log("Creating Win32 surface.");
                _khrWin32Surface!.CreateWin32Surface(_instance, &surfaceInfo, null, out Surface)
                    .Check("Create Win32 surface");
                break;
            }
            case SurfaceType.Wayland:
            {
                if (!_vk.TryGetInstanceExtension(_instance, out _khrWaylandSurface))
                    throw new Exception("Failed to get KhrWaylandSurface extension.");

                WaylandSurfaceCreateInfoKHR surfaceInfo = new()
                {
                    SType = StructureType.WaylandSurfaceCreateInfoKhr,
                    Surface = (nint*) info.Window,
                    Display = (nint*) info.Display
                };

                GLog.Log("Creating Wayland surface.");
                _khrWaylandSurface!.CreateWaylandSurface(_instance, &surfaceInfo, null, out Surface)
                    .Check("Create Wayland surface");
                break;
            }
            case SurfaceType.Xlib:
            {
                if (!_vk.TryGetInstanceExtension(_instance, out _khrXlibSurface))
                    throw new Exception("Failed to get KhrXlibSurface extension.");

                XlibSurfaceCreateInfoKHR surfaceInfo = new()
                {
                    SType = StructureType.XlibSurfaceCreateInfoKhr,
                    Window = info.Window,
                    Dpy = (nint*) info.Display
                };

                GLog.Log("Creating Xlib surface.");
                _khrXlibSurface!.CreateXlibSurface(_instance, &surfaceInfo, null, out Surface)
                    .Check("Create Xlib surface");
                break;
            }
            case SurfaceType.Xcb:
            {
                if (!_vk.TryGetInstanceExtension(_instance, out _khrXcbSurface))
                    throw new Exception("Failed to get KhrXcbSurface extension.");

                XcbSurfaceCreateInfoKHR surfaceInfo = new()
                {
                    SType = StructureType.XcbSurfaceCreateInfoKhr,
                    Window = info.Window,
                    Connection = (nint*) info.Display
                };

                GLog.Log("Creating Xcb surface.");
                _khrXcbSurface!.CreateXcbSurface(_instance, &surfaceInfo, null, out Surface)
                    .Check("Create Xcb surface");
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _khrSurface.DestroySurface(_instance, Surface, null);
        _khrSurface.Dispose();
        
        _khrWin32Surface?.Dispose();
        _khrWaylandSurface?.Dispose();
        _khrXlibSurface?.Dispose();
        _khrXcbSurface?.Dispose();
    }
}