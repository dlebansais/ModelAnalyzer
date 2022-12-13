namespace ModelAnalyzer;

using AnalysisLogger;

public static class Initialization
{
    public static IAnalysisLogger Logger { get; } = new FileLogger();
    public static ClassModelManager Manager { get; } = new ClassModelManager() { Logger = Logger };
    public static bool ExtractionStatus { get; } = Libz3Extractor.Extractor.Extract();
}
