namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvariantViolationAnalyzer>;

[TestFixture]
public class InvariantViolationUnitTests
{
    string ForSynchronousTestOnly = InvariantViolationAnalyzer.ForSynchronousTestOnly;

    [Test]
    [Category("Analyzer")]
    public async Task ValidInitialState_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_0
{{
    int X;
}}
// Invariant: X == 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InitialStateViolateInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_1|]
{{
    int X;
}}
// Invariant: X == 1
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInvariantAfterAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_2
{{
    int X;

    public void Write()
    {{
        X = 1;
    }}
}}
// Invariant: X == 0 || X == 1
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidInvariantAfterAssignment_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_3|]
{{
    int X;

    public void Write()
    {{
        X = 1;
    }}
}}
// Invariant: X == 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInvariantAfterConditional_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_4
{{
    int X;

    public void Write()
    {{
        if (X == 0)
            X = 1;
        else
            X = 2;
    }}
}}
// Invariant: X == 0 || X == 1 || X == 2
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidInvariantAfterConditional1_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_5|]
{{
    int X;

    public void Write()
    {{
        if (X == 0)
            X = 2;
        else
            X = 1;
    }}
}}
// Invariant: X == 0 || X == 1
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidInvariantAfterConditional2_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_6|]
{{
    int X;

    public void Write()
    {{
        if (X == 1)
            X = 1;
        else
            X = 2;
    }}
}}
// Invariant: X == 0 || X == 1
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task UnconstrainedInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_7|]
{{
    int X;

    void Write(int x)
    {{
        X = x;
    }}
}}
// Invariant: X == 0
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ConstrainedInvariant_BoolNoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_8
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
    public async Task ConstrainedInvariant_IntNoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_9
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
    public async Task ConstrainedInvariant_DoubleNoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_10
{{
    double X;

    void Write(double x)
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
    public async Task AsyncTest_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

// This class name is not set to ForSynchronousTestOnly_xx on purpose.
class Program_InvariantViolation_11
{{
    int X;

    void Write(int x)
    {{
        X = x;
    }}
}}
// Invariant: X == 0
");
    }
}
