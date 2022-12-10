namespace ModelAnalyzer.Core.Test;

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
}
