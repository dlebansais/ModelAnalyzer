namespace ModelAnalyzer;

using AnalysisLogger;

public static class Initialization
{
    static Initialization()
    {
#if DEBUG
        Logger = new FileLogger();
#else
        Logger = new NullLogger();
#endif
        Manager = new ClassModelManager() { Logger = Logger, StartMode = SynchronizedThreadStartMode.Auto };
    }

    public static IAnalysisLogger Logger { get; }
    public static ClassModelManager Manager { get; }
}
