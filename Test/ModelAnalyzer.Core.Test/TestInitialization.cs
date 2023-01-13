namespace Miscellaneous.Test;

using AnalysisLogger;
using ModelAnalyzer;
using NUnit.Framework;

[SetUpFixture]
public class TestInitialization
{
    [OneTimeSetUp]
    public void Init()
    {
        TextBuilder.NewLine = @"
";
    }

#if DEBUG
    public static IAnalysisLogger Logger { get; } = new FileLogger((EnvironmentVariable)"MODEL_ANALYZER_LOG_PATH", "analyzer.txt");
#else
    public static IAnalysisLogger Logger { get; } = new NullLogger();
#endif
}
