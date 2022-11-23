namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<ClassModelAnalyzer, ClassModelCodeFixProvider>;

[TestClass]
public class ClassModeltUnitTests
{
    [TestMethod]
    public async Task ClassCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    static void Main()
    {
        const int i = 0;
        Console.WriteLine(i);
    }
}
", @"
using System;

// No model
class Program
{
    static void Main()
    {
        const int i = 0;
        Console.WriteLine(i);
    }
}
");
    }

    [TestMethod]
    public async Task ClassNotModeled_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

// No model
class Program
{
    static void Main()
    {
        int i = 0;
        Console.WriteLine(i++);
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithCommentBeforeNotModeled_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

// Some comment
// No model
class Program
{
    static void Main()
    {
        int i = 0;
        Console.WriteLine(i++);
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithCommentAfterNotModeled_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

// No model
// Some comment
class Program
{
    static void Main()
    {
        int i = 0;
        Console.WriteLine(i++);
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithCommentCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

// Some comment
class [|Program|]
{
    static void Main()
    {
        const int i = 0;
        Console.WriteLine(i);
    }
}
", @"
using System;

// Some comment
// No model
class Program
{
    static void Main()
    {
        const int i = 0;
        Console.WriteLine(i);
    }
}
");
    }
}
