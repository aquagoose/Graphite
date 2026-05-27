using System.Runtime.CompilerServices;

namespace Graphite;

public static class GLog
{
    public static event OnDebugMessage MessageLogged;
    
    public static void LogMessage(Severity severity, string message, [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        MessageLogged(severity, message, line, file);
    }

    public static void Log(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => LogMessage(Severity.Verbose, message, line, path);

    public static void LogInfo(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => LogMessage(Severity.Info, message, line, path);
    
    public static void LogWarning(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => LogMessage(Severity.Warning, message, line, path);
    
    public static void LogError(string message, [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        => LogMessage(Severity.Error, message, line, path);
    
    public enum Severity
    {
        Verbose,
        Info,
        Warning,
        Error
    }
    
    public delegate void OnDebugMessage(Severity severity, string message, int line, string file);
}