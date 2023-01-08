namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadPropertyUnitTests
{

    [Test]
    [Category("Analyzer")]
    public async Task PropertyShouldNotHaveAttribute_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadProperty_0
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    public int [|X|]MA0010 { get; set; }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task PropertyShouldNotHaveModifier_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadProperty_1
{
    static int [|X|]MA0010 { get; set; }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task PropertyPredefinedTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadProperty_2
{
    public string [|X|]MA0010 { get; set; }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task PropertyTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadProperty_3
{
    public Action [|X|]MA0010 { get; set; }
}
");
    }
}
