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
    Action [|X|]MA0002;
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task FieldValidClasType_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadField_4
{
}

class Program_BadField_5
{
    Program_BadField_4 X;
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task FieldInvalidClasType_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

struct Program_BadField_4
{
}

class Program_BadField_5
{
    Program_BadField_4 [|X|]MA0002;
}
");
    }
}
