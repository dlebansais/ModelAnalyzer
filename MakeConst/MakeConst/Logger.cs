namespace DemoAnalyzer;

using System.IO;
using System.Threading;

public static class Logger
{
    private static readonly Mutex Mutex = new();

    public static void Log(string message)
    {
        lock (Mutex)
        {
            WriteLogSync(message);
        }
    }

    private static void WriteLogSync(string message)
    {
        using FileStream Stream = new("C:\\Projects\\Temp\\analyzer.txt", FileMode.Append, FileAccess.Write);
        using StreamWriter Writer = new(Stream);
        Writer.WriteLine(message);
    }
}
