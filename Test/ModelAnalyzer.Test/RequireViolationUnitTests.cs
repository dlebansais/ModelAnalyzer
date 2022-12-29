﻿namespace ModelAnalyzer.Test;

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

class {ForSynchronousTestOnly}_Boolean_0
{{
    bool X;
    void Write(bool x)
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

class {ForSynchronousTestOnly}_Boolean_1
{{
    bool X;
    void Write(bool x)
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

class {ForSynchronousTestOnly}_Integer_0
{{
    int X;
    void Write(int x)
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

class {ForSynchronousTestOnly}_Integer_1
{{
    int X;
    void Write(int x)
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

class {ForSynchronousTestOnly}_FloatingPoint_0
{{
    double X;
    void Write(double x)
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

class {ForSynchronousTestOnly}_FloatingPoint_1
{{
    double X;
    void Write(double x)
    // Require: x == 0.0
    [|// Require: x != 0.0|]
    {{
        X = x;
    }}
}}
");
    }
}
