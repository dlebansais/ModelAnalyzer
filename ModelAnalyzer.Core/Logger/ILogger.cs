namespace DemoAnalyzer;

using System;

public interface ILogger
{
    void Clear();
    void Log(string message);
    void LogException(Exception e);
}
