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

class {ForSynchronousTestOnly}_InvariantViolation_0
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

class {ForSynchronousTestOnly}_InvariantViolation_1
{{
    int X;
}}
[|// Invariant: X == 1|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInvariantAfterAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_2
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

class {ForSynchronousTestOnly}_InvariantViolation_3
{{
    int X;

    public void Write()
    {{
        X = 1;
    }}
}}
[|// Invariant: X == 0|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidInvariantAfterConditional_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_4
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

class {ForSynchronousTestOnly}_InvariantViolation_5
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
[|// Invariant: X == 0 || X == 1|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidInvariantAfterConditional2_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_6
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
[|// Invariant: X == 0 || X == 1|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task UnconstrainedInvariant_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_7
{{
    int X;

    public void Write(int x)
    {{
        X = x;
    }}
}}
[|// Invariant: X == 0|]
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ConstrainedInvariant_BoolNoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_8
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
    public async Task ConstrainedInvariant_IntNoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_9
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
    public async Task ConstrainedInvariant_DoubleNoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_InvariantViolation_10
{{
    double X;

    public void Write(double x)
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

    public void Write(int x)
    {{
        X = x;
    }}
}}
// Invariant: X == 0
");
    }
}
