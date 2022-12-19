namespace ModelAnalyzer;

using AnalysisLogger;

public static class Initialization
{
    static Initialization()
    {
#if DEBUG
        Logger = new FileLogger((EnvironmentVariable)"MODEL_ANALYZER_LOG_PATH", "analyzer.txt");
#else
        Logger = new NullLogger();
#endif
        Manager = new ClassModelManager() { Logger = Logger, StartMode = VerificationProcessStartMode.Auto };
    }

    public static IAnalysisLogger Logger { get; }
    public static ClassModelManager Manager { get; }
}
