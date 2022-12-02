namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<InvariantViolationAnalyzer>;

[TestClass]
public class InvariantViolationUnitTests
{
    [TestMethod]
    public async Task ValidInitialState_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program1
{
    int X;
}
// Invariant: X == 0
");
    }

    [TestMethod]
    public async Task InitialStateViolateInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class [|Program2|]
{
    int X;
}
// Invariant: X == 1
");
    }

    [TestMethod]
    public async Task ValidInvariantAfterAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program3
{
    int X;

    public void Write()
    {
        X = 1;
    }
}
// Invariant: X == 0 || X == 1
");
    }

    [TestMethod]
    public async Task InvalidInvariantAfterAssignment_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class [|Program4|]
{
    int X;

    public void Write()
    {
        X = 1;
    }
}
// Invariant: X == 0
");
    }

    [TestMethod]
    public async Task ValidInvariantAfterConditional_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program5
{
    int X;

    public void Write()
    {
        if (X == 0)
            X = 1;
        else
            X = 2;
    }
}
// Invariant: X == 0 || X == 1
");
    }

    [TestMethod]
    public async Task InvalidInvariantAfterConditional1_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class [|Program6|]
{
    int X;

    public void Write()
    {
        if (X == 0)
            X = 2;
        else
            X = 1;
    }
}
// Invariant: X == 0 || X == 1
");
    }

    [TestMethod]
    public async Task InvalidInvariantAfterConditional2_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class [|Program7|]
{
    int X;

    public void Write()
    {
        if (X == 1)
            X = 1;
        else
            X = 2;
    }
}
// Invariant: X == 0 || X == 1
");
    }
}
