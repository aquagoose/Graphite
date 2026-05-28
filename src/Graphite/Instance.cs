using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Graphite;

public abstract class Instance : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Instance"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// Enumerate the supported <see cref="Adapter"/>s present on the system.
    /// </summary>
    /// <returns>An array of supported <see cref="Adapter"/>s.</returns>
    public abstract Adapter[] EnumerateAdapters();

    /// <summary>
    /// Create a <see cref="Surface"/>.
    /// </summary>
    /// <param name="info">The <see cref="SurfaceInfo"/> that describes the surface.</param>
    /// <returns>The created <see cref="Surface"/>.</returns>
    public abstract Surface CreateSurface(in SurfaceInfo info);
    
    /// <summary>
    /// Dispose of this <see cref="Instance"/>.
    /// </summary>
    public abstract void Dispose();
    
    private static List<IBackendBase> _registeredBackends;

    static Instance()
    {
        _registeredBackends = [];
    }

    public static void RegisterBackend<T>() where T : IBackend, new()
    {
        _registeredBackends.Add(new T());
    }

    public static Instance Create(in InstanceInfo info)
    {
        Debug.Assert(_registeredBackends.Count > 0);
        
        foreach (IBackendBase backend in _registeredBackends)
        {
            return backend.CreateInstance(in info);
        }

        throw new PlatformNotSupportedException("No supported backends were found!");
    }
}