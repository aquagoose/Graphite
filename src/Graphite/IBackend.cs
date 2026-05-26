namespace Graphite;

public interface IBackend
{
    public static abstract string Name { get; }

    public Instance CreateInstance(in InstanceInfo info);
}