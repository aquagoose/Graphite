#!/usr/bin/env -S dotnet --
#:project ../src/Graphite/Graphite.csproj
#:project ../src/Graphite.Vulkan/Graphite.Vulkan.csproj
#:package SDL3-CS@3.4.2
#:package SDL3-CS.Native@3.4.2

using Graphite;
using Graphite.Vulkan;
using SDL3;

GLog.MessageLogged += (severity, message, line, file) => Console.WriteLine($"[{severity}] {message}"); 

if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
{
    Console.WriteLine($"Failed to initialize SDL: {SDL.GetError()}");
    return 1;
}

IntPtr window = SDL.CreateWindow("Basic Clear Color", 1280, 720, 0);
if (window == IntPtr.Zero)
{
    Console.WriteLine($"Failed to create window: {SDL.GetError()}");
    return 1;
}

Instance.RegisterBackend<VulkanBackend>();
Instance instance = Instance.Create(new InstanceInfo("Basic Clear Color"));

Adapter[] adapters = instance.EnumerateAdapters();
foreach (Adapter adapter in adapters)
    Console.WriteLine(adapter);

SurfaceInfo surfaceInfo;
uint windowProps = SDL.GetWindowProperties(window);
if (OperatingSystem.IsWindows())
{
    nint hwnd = SDL.GetPointerProperty(windowProps, SDL.Props.WindowWin32HWNDPointer, 0);
    nint hinstance = SDL.GetPointerProperty(windowProps, SDL.Props.WindowWin32InstancePointer, 0);
    surfaceInfo = new SurfaceInfo(SurfaceType.Win32, hwnd, hinstance);
}
else
{
    string? driver = SDL.GetCurrentVideoDriver();
    switch (driver)
    {
        case "wayland":
        {
            nint wlsurface = SDL.GetPointerProperty(windowProps, SDL.Props.WindowWaylandSurfacePointer, 0);
            nint display = SDL.GetPointerProperty(windowProps, SDL.Props.WindowWaylandDisplayPointer, 0);
            surfaceInfo = new SurfaceInfo(SurfaceType.Wayland, wlsurface, display);
            break;
        }
        
        default:
            throw new PlatformNotSupportedException($"Video driver \"{driver}\" is not supported.");
    }
}

Surface surface = instance.CreateSurface(in surfaceInfo);
Device device = instance.CreateDevice(surface);

bool alive = true;
while (alive)
{
    while (SDL.PollEvent(out SDL.Event winEvent))
    {
        switch ((SDL.EventType) winEvent.Type)
        {
            case SDL.EventType.Quit:
            case SDL.EventType.WindowCloseRequested:
                alive = false;
                break;
        }
    }
}

device.Dispose();
surface.Dispose();
instance.Dispose();

SDL.DestroyWindow(window);
SDL.Quit();
return 0;