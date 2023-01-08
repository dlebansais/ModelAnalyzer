namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadParameterUnitTests
{
    [Test]
    [Category("Analyzer")]
    public async Task ParameterShouldNotHaveAttribute_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_0
{
    void Write([|[System.Runtime.InteropServices.In] int x|]MA0004)
    {
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ParameterShouldNotHaveModifier_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_1
{
    void Write([|ref int x|]MA0004)
    {
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ParameterPredefinedTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_2
{
    void Write([|string p|]MA0004)
    {
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ParameterTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadParameter_3
{
    void Write([|Action p|]MA0004)
    {
    }
}
");
    }
}
