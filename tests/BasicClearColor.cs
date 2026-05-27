#!/usr/bin/env -S dotnet --
#:project ../src/Graphite/Graphite.csproj
#:project ../src/Graphite.Vulkan/Graphite.Vulkan.csproj
#:package SDL3-CS@3.4.2

using Graphite;
using Graphite.Vulkan;
using SDL3;

if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events))
{
    Console.WriteLine($"Failed to initialize SDL: {SDL.GetError()}");
    return 1;
}

Instance.DebugMessage += (severity, message, line, file) => Console.WriteLine($"[{severity}] {message}"); 
Instance.RegisterBackend<VulkanBackend>();
Instance instance = Instance.Create(new InstanceInfo("Basic Clear Color"));

instance.Dispose();
return 0;