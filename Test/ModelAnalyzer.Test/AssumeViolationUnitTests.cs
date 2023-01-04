namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<AssumeViolationAnalyzer>;

[TestFixture]
public class AssumeViolationUnitTests
{
    string ForSynchronousTestOnly = InvariantViolationAnalyzer.ForSynchronousTestOnly;

    [Test]
    [Category("Analyzer")]
    public async Task ValidRemainderOperandInteger_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_Integer_0
{{
    public int X {{ get; set; }}

    public void Remainder(int x, int y)
    // Require: x > 0
    // Require: y > 0
    {{
        X = x % y;
    }}
    // Ensure: X >= 0
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task AssumeViolationRemainderInteger_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_Integer_1|]
{{
    int X;

    public void Remainder(int x, int y)
    {{
        X = x % y;
    }}
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidDivideOperandInteger_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class {ForSynchronousTestOnly}_Integer_2
{{
    public int X {{ get; set; }}

    public void Remainder(int x, int y)
    // Require: x > 0
    // Require: y > 0
    {{
        X = x / y;
    }}
    // Ensure: X >= 0
}}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task AssumeViolationDivideInteger_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@$"
using System;

class [|{ForSynchronousTestOnly}_Integer_3|]
{{
    int X;

    public void Remainder(int x, int y)
    {{
        X = x / y;
    }}
}}
");
    }
}
