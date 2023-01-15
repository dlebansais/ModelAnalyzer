namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadExpressionUnitTests
{
    [Test]
    [Category("Analyzer")]
    public async Task ExpressionIsUnsupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_0
{
    int X;

    void Write(int x)
    {
        X = [|nameof(X).Length|]MA0009;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidBinaryExpression_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_1
{
    bool X;

    void Write(bool x)
    {
        X = x [|^|]MA0009 x;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidOperator_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_2
{
    int X;

    void Write(int x)
    {
        X = (x + x) - ((x * x) / x);
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithBadConstantInInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_3
{
    bool X;

    bool Read()
    {
        return X;
    }

    void Write(bool x)
    {
        if (x != false)
            X = x;
    }
}
// Invariant: X [|^|]MA0009 X
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithBadExpressionInInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_4
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|typeof(X)|]MA0009
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithBadOperatorInInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_5
{
    bool X;

    bool Read()
    {
        return X;
    }

    void Write(bool x)
    {
        if (x != false)
            X = x;
    }
}
// Invariant: X [|^|]MA0009 false
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ClassWithUnknownFieldInInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_6
{
    int X;

    int Read()
    {
        return X;
    }

    void Write(int x)
    {
        if (x >= 0)
            X = x;
    }
}
// Invariant: [|Y|]MA0009 >= 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task MemberAccessGoodPath_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadExpression_7
{
    public int Y { get; set; }
}

class Program_BadExpression_8
{
    public int Write()
    {
        Program_BadExpression_7 X = new();
        return X.Y;
    }
    // Ensure: Result == 0
}
");
    }
}
