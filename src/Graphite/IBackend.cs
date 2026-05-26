namespace Graphite;

public interface IBackend : IBackendBase
{
    public static abstract string Name { get; }
}