global using VkInstance = Silk.NET.Vulkan.Instance;
global using VkDevice = Silk.NET.Vulkan.Device;
using Silk.NET.Vulkan;

namespace Graphite.Vulkan;

internal static class VulkanUtils
{
    public static void Check(this Result result, string operation)
    {
        if (result != Result.Success)
            throw new Exception($"Vulkan operation \"{operation}\" failed: {result}");
    }
}