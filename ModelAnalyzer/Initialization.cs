namespace ModelAnalyzer;

using AnalysisLogger;

public static class Initialization
{
#if DEBUG
    public static IAnalysisLogger Logger { get; } = new FileLogger();
#else
    public static IAnalysisLogger Logger { get; } = new NullLogger();
#endif
    public static ClassModelManager Manager { get; } = new ClassModelManager() { Logger = Logger };
    public static bool ExtractionStatus { get; } = Libz3Extractor.Extractor.Extract();
}
