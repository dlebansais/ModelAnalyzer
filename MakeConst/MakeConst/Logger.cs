namespace DemoAnalyzer;

using System;
using System.IO;
using System.Threading;

public static class Logger
{
    private static readonly Mutex Mutex = new();

    public static void Clear()
    {
        lock (Mutex)
        {
            ClearSync();
        }
    }

    private static void ClearSync()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i > 0)
                Thread.Sleep(10);

            try
            {
                WriteFile(FileMode.Create, $"Cleared {DateTime.Now}");
                return;
            }
            catch
            {
            }
        }
    }

    public static void Log(string message)
    {
        lock (Mutex)
        {
            WriteLogSync(message);
        }
    }

    public static void LogException(Exception e)
    {
        lock (Mutex)
        {
            WriteLogSync(e.Message);
            WriteLogSync(e.StackTrace);
        }
    }

    private static void WriteLogSync(string message)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i > 0)
                Thread.Sleep(10);

            try
            {
                WriteFile(FileMode.Append, message);
                return;
            }
            catch
            {
            }
        }
    }

    private static void WriteFile(FileMode mode, string message)
    {
        using FileStream Stream = new("C:\\Projects\\Temp\\analyzer.txt", mode, FileAccess.Write);
        using StreamWriter Writer = new(Stream);
        Writer.WriteLine(message);
    }
}
