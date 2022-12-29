namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<EnsureViolationAnalyzer>;

[TestFixture]
public class EnsureViolationUnitTests
{
    string ForSynchronousTestOnly = InvariantViolationAnalyzer.ForSynchronousTestOnly;

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialStateBoolean_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_Boolean_0
{{
    bool X;
    void Write(bool x)
    // Require: x == false
    {{
        X = x;
    }}
    // Ensure: X == false
}}
// Invariant: X == false
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureViolationBoolean_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_Boolean_1
{{
    bool X;
    void Write(bool x)
    {{
        X = x;
    }}
    [|// Ensure: X == false|]
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialStateInteger_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_Integer_0
{{
    int X;
    void Write(int x)
    // Require: x == 0
    {{
        X = x;
    }}
    // Ensure: X == 0
}}
// Invariant: X == 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureViolationInteger_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_Integer_1
{{
    int X;
    void Write(int x)
    {{
        X = x;
    }}
    [|// Ensure: X == 0|]
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialStateFloatingPoint_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_FloatingPoint_0
{{
    double X;
    void Write(double x)
    // Require: x == 0.0
    {{
        X = x;
    }}
    // Ensure: X == 0.0
}}
// Invariant: X == 0.0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task EnsureViolationFloatingPoint_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_FloatingPoint_1
{{
    double X;
    void Write(double x)
    {{
        X = x;
    }}
    [|// Ensure: X == 0.0|]
}}
");
    }
}
