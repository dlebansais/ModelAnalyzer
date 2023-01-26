namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<RequireViolationAnalyzer>;

[TestFixture]
public class RequireViolationUnitTests
{
    string ForSynchronousTestOnly = InvariantViolationAnalyzer.ForSynchronousTestOnly;

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialStateBoolean_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_RequireViolation_Boolean_0
{{
    bool X;

    public void Write(bool x)
    // Require: x == false
    {{
        X = x;
    }}
}}
// Invariant: X == false
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task RequireViolationBoolean_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_RequireViolation_Boolean_1
{{
    bool X;

    public void Write(bool x)
    // Require: x == false
    [|// Require: x != false|]
    {{
        X = x;
    }}
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialStateInteger_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_RequireViolation_Integer_0
{{
    int X;

    public void Write(int x)
    // Require: x == 0
    {{
        X = x;
    }}
}}
// Invariant: X == 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task RequireViolationInteger_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_RequireViolation_Integer_1
{{
    int X;

    public void Write(int x)
    // Require: x == 0
    [|// Require: x != 0|]
    {{
        X = x;
    }}
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialStateFloatingPoint_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_RequireViolation_FloatingPoint_0
{{
    double X;

    public void Write(double x)
    // Require: x == 0.0
    {{
        X = x;
    }}
}}
// Invariant: X == 0.0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task RequireViolationFloatingPoint_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_RequireViolation_FloatingPoint_1
{{
    double X;

    public void Write(double x)
    // Require: x == 0.0
    [|// Require: x != 0.0|]
    {{
        X = x;
    }}
}}
");
    }
}
