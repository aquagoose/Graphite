namespace Graphite;

public interface IBackendBase
{
    public Instance CreateInstance(ref readonly InstanceInfo info);
}