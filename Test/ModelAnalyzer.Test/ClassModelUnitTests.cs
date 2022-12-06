namespace DemoAnalyzer.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<ClassModelAnalyzer, ClassModelCodeFixProvider>;

[TestClass]
public class ClassModelUnitTests
{
    [TestMethod]
    public async Task ClassWithNoMembers_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_ClassModel_0
{
}
");
    }

    [TestMethod]
    public async Task ClassCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program_ClassModel_1|]
{
    static void Main()
    {
    }
}
", @"
using System;

// No model
class Program_ClassModel_1
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
class Program_ClassModel_2
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
class Program_ClassModel_3
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
class Program_ClassModel_4
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
class [|Program_ClassModel_5|]
{
    static void Main()
    {
    }
}
", @"
using System;

// Some comment
// No model
class Program_ClassModel_5
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
class [|Program_ClassModel_6|]
{
}
", @"
using System;

// No model
[Serializable]
class Program_ClassModel_6
{
}
");
    }

    [TestMethod]
    public async Task ClassWithPublicModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class Program_ClassModel_7
{
}
");
    }

    [TestMethod]
    public async Task ClassWithInternalModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

internal class Program_ClassModel_8
{
}
");
    }

    [TestMethod]
    public async Task ClassWithPartialModifier_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

partial class Program_ClassModel_9
{
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedModifierCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

static class [|Program_ClassModel_10|]
{
}
", @"
using System;

// No model
static class Program_ClassModel_10
{
}
");
    }

    [TestMethod]
    public async Task ClassWithBaseTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program_ClassModel_11|] : IDisposable
{
    public void Dispose()
    {
    }
}
", @"
using System;

// No model
class Program_ClassModel_11 : IDisposable
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

class [|Program_ClassModel_12|]<T>
{
}
", @"
using System;

// No model
class Program_ClassModel_12<T>
{
}
");
    }

    [TestMethod]
    public async Task ClassWithTypeConstraintCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program_ClassModel_13|]<T>
where T : class
{
}
", @"
using System;

// No model
class Program_ClassModel_13<T>
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

class Program_ClassModel_14
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

class Program_ClassModel_15
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

class [|Program_ClassModel_16|]
{
    [ThreadStatic]
    int X;
}
", @"
using System;

// No model
class Program_ClassModel_16
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

class Program_ClassModel_17
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

class [|Program_ClassModel_18|]
{
    static int X;
}
", @"
using System;

// No model
class Program_ClassModel_18
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

class [|Program_ClassModel_19|]
{
    public int X;
}
", @"
using System;

// No model
class Program_ClassModel_19
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

class [|Program_ClassModel_20|]
{
    Program_ClassModel_20 X;
}
", @"
using System;

// No model
class Program_ClassModel_20
{
    Program_ClassModel_20 X;
}
");
    }

    [TestMethod]
    public async Task ClassWithUnsupportedFieldPredefinedTypeCouldHaveAModel_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class [|Program_ClassModel_21|]
{
    string X;
}
", @"
using System;

// No model
class Program_ClassModel_21
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

class Program_ClassModel_22
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

class Program_ClassModel_23
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

class [|Program_ClassModel_24|]
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
class Program_ClassModel_24
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

class Program_ClassModel_25
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

class Program_ClassModel_26
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

class Program_ClassModel_26
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

class [|Program_ClassModel_27|]
{
    static int Read()
    {
        return 0;
    }
}
", @"
using System;

// No model
class Program_ClassModel_27
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

class [|Program_ClassModel_28|]
{
    Program_ClassModel_28 Read()
    {
        return null!;
    }
}
", @"
using System;

// No model
class Program_ClassModel_28
{
    Program_ClassModel_28 Read()
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

class [|Program_ClassModel_29|]
{
    string Read()
    {
        return null!;
    }
}
", @"
using System;

// No model
class Program_ClassModel_29
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

class Program_ClassModel_30
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

class Program_ClassModel_31
{
    int Read(int x, int y)
    {
        return 0;
    }
}
");
    }

    [TestMethod]
    public async Task ClassWithValidInstructions_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program_ClassModel_32
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
