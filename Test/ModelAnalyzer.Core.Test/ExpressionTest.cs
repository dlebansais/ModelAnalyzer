namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Ensure"/> class.
/// </summary>
public class ExpressionTest
{
    [Test]
    [Category("Core")]
    public void Expression_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_0
{
    int X;

    void Write(int x)
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryArithmetic()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_1
{
    int X;

    void Write(int x)
    {
        X = x + 1 + x;
        X = x - 1 - x;
        X = x * 1 * x;
        X = x / 1 / x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryArithmetic_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_2
{
    int X;

    void Write(int x)
    {
        X = x + 1; // Token '+' is replaced with ';'.
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryArithmeticOperator, SyntaxKind.SemicolonToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryLogical()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_3
{
    void Write(int x)
    {
        if ((x == 0) && (x == 0 || x == 1) && (x == 0 || x == 1 || x == 2))
            if (true || false)
                if (true && true)
                {
                }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryLogical_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_4
{
    int X;

    void Write(int x)
    {
        if (x == 0 && x == 0) // Token '&&' is replaced with 'do'.
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryLogicalOperator, SyntaxKind.DoKeyword);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryLogical()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_5
{
    void Write(int x)
    {
        if (!(x == 0))
            if (!true)
            {
            }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryLogical_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_6
{
    int X;

    void Write(int x)
    {
        if (!(x == 0)) // Token '!' is replaced with '%'.
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateUnaryLogicalOperator, SyntaxKind.PercentToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_Parenthesized()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_7
{
    int X;

    void Write(int x)
    {
        X = ((x + 1) + (x - 1)) - ((x - 1) - (x + 1) + (-(x + 1)));
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_InAssertion()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_8
{
    int X;

    public void Write(int x)
    // Require: x >= 0
    // Require: (x + 1) >= (0)
    // Require: true
    // Require: (x + 1) >= -1
    {
        X = x;
    }
    // Ensure: (X) >= 0
    // Ensure: X >= 0 && (X >= 1)
    // Ensure: !(X < 0)
    // Ensure: X >= 0 && !(X >= 1)
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_8
  int X

  public void Write(int x)
  # require x >= 0
  # require (x + 1) >= 0
  # require true
  # require (x + 1) >= -1
  {
    X = x;
  }
  # ensure X >= 0
  # ensure (X >= 0) && (X >= 1)
  # ensure !(X < 0)
  # ensure (X >= 0) && (!(X >= 1))
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_Unsupported()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_9
{
    int X;

    void Write(int x)
    {
        if (x is string)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_Unsupported_Nested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_10
{
    int X;

    void Write(int x)
    {
        if (x is string || x is float)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidFieldName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_11
{
    int X;

    void Write(int x)
    {
        if (Y == 0)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidLocalName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_12
{
    void Write(int x)
    {
        int X;

        if (Y == 0)
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_Unsupported_Parenthesized()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_13
{
    int X;

    void Write(int x)
    {
        if ((x is string))
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_Unsupported_Literal()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_14
{
    int X;

    void Write(int x)
    {
        if (x == ""*"")
        {
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryArithmetic()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_15
{
    int X;

    void Write()
    {
        X = -1;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryArithmetic_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_16
{
    int X;

    void Write(int x)
    {
        X = -1; // Token '-' is replaced with '+'.
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateUnaryArithmeticOperator, SyntaxKind.PlusToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryArithmetic_InvalidOperand()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_17
{
    int X;

    void Write(int x)
    {
        X = -sizeof(X);
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_SourceAndDestinationNotCompatible()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_18
{
    int X;

    void Write(int x, double y)
    {
        X = x + y;
        X = y + x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Expression_Double()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_19
{
    double X;

    void Write()
    {
        X = 1.1 + 2.2;
        X = 1;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_ResultInsideEnsure()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_20
{
    int X;

    public int Write()
    {
        return X;
    }
    // Ensure: Result == 0
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_ResultOutsideEnsure()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_21
{
    int X;

    bool Write()
    {
        return Result == 0;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FieldSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_22
{
    int X;

    void Write()
    {
        int Y;

        Y = X;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_ParameterSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_23
{
    void Write(int x)
    {
        int Y;

        Y = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCall()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_24
{
    public int Write1(int x)
    {
        return Write2(x);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_24
  public int Write1(int x)
  {
    return Write2(x);
  }

  int Write2(int x)
  {
    return x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallBadMethod()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_25
{
    public int Write1(int x)
    {
        return x[0]();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallNamedArgument()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_26
{
    public int Write1()
    {
        return Write2(x: 0);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallRefArgument()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_27
{
    public int Write1(int x)
    {
        return Write2(ref x);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallInvalidExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_28
{
    public int Write1(int x)
    {
        return Write2(x[0]);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallInvalidName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_29
{
    public int Write1(int x)
    {
        return Write2(x);
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallInvalidRecursive()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_30
{
    public int Write1(int x)
    {
        return Write1(x);
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallTooManyArguments()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_31
{
    public int Write1(int x)
    {
        return Write2(x, x);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallTooManyArgumentsNested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_32
{
    public int Write1(int x)
    {
        if (false)
            return Write2(x, x);
        else
            return Write2(x);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallMatchingArguments()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_33
{
    int X;

    public int Write1(bool x, int y, double z)
    {
        X = Write2(x, y, z);
        X = Write2(true, 1, 1.0);
        X = Write2(true, 1, 1);
    }

    int Write2(bool x, int y, double z)
    {
        return 0;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallNonMatchingArguments()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_34
{
    public int Write1()
    {
        return Write2(true);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallCircular()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_35
{
    public int Write1()
    {
        return Write2();
    }

    int Write2()
    {
        return Write1();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallCircularNested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_36
{
    public int Write1()
    {
        if (false)
            return Write2();

        return 0;
    }

    int Write2()
    {
        return Write1();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallComposite()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_37
{
    public int Write1(int x)
    {
        return Write2(x) + Write2(x);
    }

    int Write2(int x)
    {
        return x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_37
  public int Write1(int x)
  {
    return Write2(x) + Write2(x);
  }

  int Write2(int x)
  {
    return x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallInInvariant()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_38
{
    int X;

    public void Write(int x)
    {
        X = x;
    }

    int Read(int x)
    {
        return x;
    }
}
// Invariant: Read(X) >= 0 || Read(X) < 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_38
  int X

  public void Write(int x)
  {
    X = x;
  }

  int Read(int x)
  {
    return x;
  }
  * (Read(X) >= 0) || (Read(X) < 0)
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_39
{
    public int Write1() => Write2();

    int Write2() => Write1();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Expression_FunctionCallInvalidInvariantExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_40
{
    int X;

    int Write(int x, int y)
    {
        return 0;
    }
}
// Invariant: Write(X) == 0
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_Remainder_InvalidLeft()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_41
{
    int X;

    void Write(int x)
    {
        X = 1.1 % 2;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_Remainder_InvalidRight()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_42
{
    int X;

    void Write(int x)
    {
        X = 1 % 2.1;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    private SyntaxToken LocateBinaryArithmeticOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }

    private SyntaxToken LocateUnaryArithmeticOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        PrefixUnaryExpressionSyntax UnaryExpression = (PrefixUnaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = UnaryExpression.OperatorToken;

        return Operator;
    }

    private SyntaxToken LocateBinaryLogicalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }

    private SyntaxToken LocateUnaryLogicalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        PrefixUnaryExpressionSyntax UnaryExpression = (PrefixUnaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = UnaryExpression.OperatorToken;

        return Operator;
    }
}
