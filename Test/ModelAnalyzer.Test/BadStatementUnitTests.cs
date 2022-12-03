﻿namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<BadStatementAnalyzer>;

[TestClass]
public class BadStatementUnitTests
{
    [TestMethod]
    public async Task StatementIsUnsupported_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;
    int Read()
    {
        [|throw new NotImplementedException();|]
    }
}
");
    }

    [TestMethod]
    public async Task InvalidAssignmentDestination_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;
    void Write(int x)
    {
        [|x = 0;|]
    }
}
");
    }

    [TestMethod]
    public async Task ValidReturn_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;

    int Read()
    {
        return X;
    }
}
");
    }

    [TestMethod]
    public async Task InvalidReturn_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;

    int Read()
    {
        [|return X;|]
        X = 0;
    }
}
");
    }

    [TestMethod]
    public async Task InvalidNestedReturns_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;

    int Read()
    {
        if (X == 0)
            [|return X;|]
        else
            [|return X + 1;|]
    }
}
");
    }
}