using AnalysisLogger;

namespace DemoAnalyzer;

public static class Initialization
{
    public static IAnalysisLogger Logger { get; } = new NullLogger();
    public static ClassModelManager Manager { get; } = new ClassModelManager() { Logger = Logger };
}
