﻿namespace ModelAnalyzer.Test;

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

class {ForSynchronousTestOnly}_EnsureViolation_Boolean_0
{{
    public bool X {{ get; set; }}

    public void Write(bool x)
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

class {ForSynchronousTestOnly}_EnsureViolation_Boolean_1
{{
    public bool X {{ get; set; }}

    public void Write(bool x)
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

class {ForSynchronousTestOnly}_EnsureViolation_Integer_0
{{
    public int X {{ get; set; }}

    public void Write(int x)
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

class {ForSynchronousTestOnly}_EnsureViolation_Integer_1
{{
    public int X {{ get; set; }}

    public void Write(int x)
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

class {ForSynchronousTestOnly}_EnsureViolation_FloatingPoint_0
{{
    public double X {{ get; set; }}

    public void Write(double x)
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

class {ForSynchronousTestOnly}_EnsureViolation_FloatingPoint_1
{{
    public double X {{ get; set; }}

    public void Write(double x)
    {{
        X = x;
    }}
    [|// Ensure: X == 0.0|]
}}
");
    }
}
