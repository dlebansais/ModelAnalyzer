namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadFieldUnitTests
{

    [Test]
    [Category("Analyzer")]
    public async Task FieldShouldNotHaveAttribute_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadField_0
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    int [|X|]MA0002;
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task FieldShouldNotHaveModifier_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadField_1
{
    public int [|X|]MA0002;
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task FieldPredefinedTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadField_2
{
    string [|X|]MA0002;
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task FieldTypeIsNotSupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadField_3
{
    Program_BadField_3 [|X|]MA0002;
}
");
    }
}
