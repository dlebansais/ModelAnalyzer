namespace ModelAnalyzer.Core.Test;

using AnalysisLogger;
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
    public static FileLogger Logger { get; } = new FileLogger((EnvironmentVariable)"MODEL_ANALYZER_LOG_PATH", "analyzer.txt");
#else
    public static FileLogger Logger { get; } = new NullLogger();
#endif
}
