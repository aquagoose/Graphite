namespace Graphite;

/// <summary>
/// A list of commands that can be executed by a <see cref="Device"/>.
/// </summary>
public abstract class CommandList : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="CommandList"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// Begin accepting commands. You <b>MUST</b> call this before executing any commands.
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// Finish accepting commands and prepare for execution. You <b>MUST</b> call this before submitting for execution.
    /// </summary>
    /// <remarks>Once ended, calling <see cref="Begin"/> will reset the command list.</remarks>
    public abstract void End();

    /// <summary>
    /// Dispose of this <see cref="CommandList"/>.
    /// </summary>
    public abstract void Dispose();
}