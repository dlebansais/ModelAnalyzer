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

class Program_InvariantViolation_0
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

class [|Program_InvariantViolation_1|]
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

class Program_InvariantViolation_2
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

class [|Program_InvariantViolation_3|]
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

class Program_InvariantViolation_4
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

class [|Program_InvariantViolation_5|]
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

class [|Program_InvariantViolation_6|]
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

    [TestMethod]
    public async Task UnconstrainedInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class [|Program_InvariantViolation_7|]
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
// Invariant: X == 0
");
    }

    [TestMethod]
    public async Task ConstrainedInvariant_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_InvariantViolation_8
{
    int X;

    void Write(int x)
    // Require: x == 0
    {
        X = x;
    }
}
// Invariant: X == 0
");
    }
}
