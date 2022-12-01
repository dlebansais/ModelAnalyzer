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
}
