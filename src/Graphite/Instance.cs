using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Graphite;

public abstract class Instance : IDisposable
{
    public abstract void Dispose();

    public static event OnDebugMessage DebugMessage;
    
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

    public static void Log(string message, DebugMessageSeverity severity = DebugMessageSeverity.Verbose,
        [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
    {
        DebugMessage(severity, message, line, file);
    }

    public delegate void OnDebugMessage(DebugMessageSeverity severity, string message, int line, string file);

    public enum DebugMessageSeverity
    {
        Verbose,
        Info,
        Warning,
        Error
    }
}