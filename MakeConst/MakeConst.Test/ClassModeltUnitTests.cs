namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<ClassModelAnalyzer, ClassModelCodeFixProvider>;

[TestClass]
public class ClassModeltUnitTests
{
    [TestMethod]
    public async Task ClassWithNoMembers_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
}
");
    }

    [TestMethod]
    public async Task ClassCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    static void Main()
    {
    }
}
", @"
using System;

// No model
class Program
{
    static void Main()
    {
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
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithAttributeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

[Serializable]
class [|Program|]
{
}
", @"
using System;

// No model
[Serializable]
class Program
{
}
");
    }

    [TestMethod]
    public async Task ClassWithPublicModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class Program
{
}
");
    }

    [TestMethod]
    public async Task ClassWithInternalModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

internal class Program
{
}
");
    }

    [TestMethod]
    public async Task ClassWithPartialModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

partial class Program
{
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedModifierCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

static class [|Program|]
{
}
", @"
using System;

// No model
static class Program
{
}
");
    }

    [TestMethod]
    public async Task ClassWithBaseTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|] : IDisposable
{
    public void Dispose()
    {
    }
}
", @"
using System;

// No model
class Program : IDisposable
{
    public void Dispose()
    {
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithTypeParameterCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]<T>
{
}
", @"
using System;

// No model
class Program<T>
{
}
");
    }

    [TestMethod]
    public async Task ClassWithTypeConstraintCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]<T>
where T : class
{
}
", @"
using System;

// No model
class Program<T>
where T : class
{
}
");
    }

    [TestMethod]
    public async Task ClassWithFieldMember_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;
}
");
    }

    [TestMethod]
    public async Task ClassWithTwoFieldMembers_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;
    int Y;
}
");
    }

    [TestMethod]
    public async Task ClassWithFieldAttributeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    [ThreadStatic]
    int X;
}
", @"
using System;

// No model
class Program
{
    [ThreadStatic]
    int X;
}
");
    }

    [TestMethod]
    public async Task ClassWithPrivateFieldModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    private int X;
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedFieldModifierCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    static int X;
}
", @"
using System;

// No model
class Program
{
    static int X;
}
");
    }

    [TestMethod]
    public async Task ClassWithPublicFielCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    public int X;
}
", @"
using System;

// No model
class Program
{
    public int X;
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedFieldTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    Program X;
}
", @"
using System;

// No model
class Program
{
    Program X;
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedFieldPredefinedTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    string X;
}
", @"
using System;

// No model
class Program
{
    string X;
}
");
    }

    [TestMethod]
    public async Task ClassWithMethodMemberNoParameters_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int Read()
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithMethodMemberWithNoReturn_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    void Write()
    {
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithMethodAttributeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    [MTAThread]
    int Read()
    {
        return 0;
    }
}
", @"
using System;

// No model
class Program
{
    [MTAThread]
    int Read()
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithPrivateMethodMember_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    private int Read()
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithPublicMethodMember_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    public int Read()
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithInternalMethodMember_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    internal int Read()
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedMethodModifierCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    static int Read()
    {
        return 0;
    }
}
", @"
using System;

// No model
class Program
{
    static int Read()
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedReturnTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    Program Read()
    {
        return null!;
    }
}
", @"
using System;

// No model
class Program
{
    Program Read()
    {
        return null!;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedReturnPredefinedTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    string Read()
    {
        return null!;
    }
}
", @"
using System;

// No model
class Program
{
    string Read()
    {
        return null!;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithMethodMemberWithOneParameter_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int Read(int x)
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithMethodMemberWithTwoParameters_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int Read(int x, int y)
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithParameterAttributeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    void Write([System.Runtime.InteropServices.In] int x)
    {
    }
}
", @"
using System;

// No model
class Program
{
    void Write([System.Runtime.InteropServices.In] int x)
    {
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithParameterModifierCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    void Write(ref int x)
    {
    }
}
", @"
using System;

// No model
class Program
{
    void Write(ref int x)
    {
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedParameterTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    void Write(Program p)
    {
    }
}
", @"
using System;

// No model
class Program
{
    void Write(Program p)
    {
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedParameterPredefinedTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program|]
{
    void Write(string p)
    {
    }
}
", @"
using System;

// No model
class Program
{
    void Write(string p)
    {
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithValidInstructions_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    int X;

    int Read(int x, int y)
    {
        X = x + y;
        if (X >= x || 0 < y)
            X = y;

        return X;
    }
}
");
    }
}
