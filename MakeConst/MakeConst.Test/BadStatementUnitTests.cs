namespace DemoAnalyzer.Test;

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
}
