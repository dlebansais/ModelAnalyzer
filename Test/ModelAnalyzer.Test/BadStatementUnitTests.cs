﻿namespace ModelAnalyzer.Test;

using System.Threading.Tasks;
using NUnit.Framework;
using VerifyCS = CSharpAnalyzerVerifier<InvalidElementAnalyzer>;

[TestFixture]
public class BadStatementUnitTests
{
    [Test]
    [Category("Analyzer")]
    public async Task StatementIsUnsupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadStatement_0
{
    int X;
    int Read()
    {
        [|throw new NotImplementedException();|]MA0008
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidAssignmentDestination_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadStatement_1
{
    int X;
    void Write(int x)
    {
        [|x = 0;|]MA0008
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task ValidReturn_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadStatement_2
{
    int X;

    int Read()
    {
        return X;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidReturn_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadStatement_3
{
    int X;

    int Read()
    {
        [|return X;|]MA0008
        X = 0;
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidNestedReturns_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadStatement_4
{
    int X;

    int Read()
    {
        if (X == 0)
            [|return X;|]MA0008
        else
            [|return X + 1;|]MA0008
    }
}
");
    }

    [Test]
    [Category("Analyzer")]
    public async Task InvalidRecursiveCall_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_BadStatement_5
{
    public void Write()
    {
        [|Write|]MA0008();
    }
}
");
    }
}
