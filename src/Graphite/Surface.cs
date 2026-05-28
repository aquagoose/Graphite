namespace Graphite;

/// <summary>
/// A surface that can be used in a <see cref="Swapchain"/>.
/// </summary>
public abstract class Surface : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Surface"/> is disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// Dispose of this <see cref="Surface"/>.
    /// </summary>
    public abstract void Dispose();
}