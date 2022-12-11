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

    public static FileLogger Logger { get; } = new();
}
