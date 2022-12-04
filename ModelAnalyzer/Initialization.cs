using Microsoft.Extensions.Logging;

namespace DemoAnalyzer;

public static class Initialization
{
    public static ILogger Logger { get; } = new FileLogger();
    public static ClassModelManager Manager { get; } = new ClassModelManager() { Logger = Logger };
}
