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
        Libz3Extractor.Extractor.Extract();
        Manager = new ClassModelManager() { Logger = Logger, StartMode = SynchronizedThreadStartMode.Manual };
    }

    public static IAnalysisLogger Logger { get; }
    public static ClassModelManager Manager { get; }
}
