namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Statement"/> class.
/// </summary>
public class StatementTest
{
    [Test]
    [Category("Core")]
    public void Statement_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_0
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
    public void Statement_Assignment()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_1
{
    int X, Y;

    void Write(int x)
    {
        X = x + 1;
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
    public void Statement_AssignmentInvalidDestination()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_2
{
    void Write(int x)
    {
        x = x + 1;
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
    public void Statement_If()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_3
{
    int X;
    int Y;

    void Write(int x, int y)
    {
        if (x == 0)
        {
            X = x;
            Y = y;
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_3
  int X
  int Y

  void Write(int x, int y)
  {
    if (x == 0)
    {
      X = x;
      Y = y;
    }
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_IfWithElse()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_4
{
    int X;
    int Y;

    void Write(int x, int y)
    {
        if (x == 0)
        {
            X = 0;
        }
        else
        {
            X = x;
            Y = y;
        }
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_4
  int X
  int Y

  void Write(int x, int y)
  {
    if (x == 0)
      X = 0;
    else
    {
      X = x;
      Y = y;
    }
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_Return()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_5
{
    int X;

    int Read()
    {
        int Y;
        return X;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_5
  int X

  int Read()
  {
    int Y
    return X;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_ReturnWithoutValue()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_6
{
    void Write()
    {
        return;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_6
  void Write()
  {
    return;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_ExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_7
{
    int X;
    int Read() => X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_7
  int X

  int Read()
  {
    return X;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_UnsupporteChecked()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_8
{
    void Write(int x)
    {
        checked
        {
        }
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
    public void Statement_UnsupportedSingleInstruction()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_9
{
    void Write(int x)
    {
        if (x == 0)
            break;
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
    public void Statement_UnsupportedInvalidDestination()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_10
{
    int X;

    void Write(int x, int y)
    {
        x = y;
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
    public void Statement_UnsupportedExpressionDestination()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_11
{
    void Write(int x, int y)
    {
        x[0] = y;
    }

    int[] X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_InvalidReturnExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_12
{
    int Write(int x)
    {
        return x % 10;
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
    public void Statement_InvalidExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_13
{
    int X;
    int Read() => X % 10;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_InvalidSimpleExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_14
{
    void Write(int x)
    {
        x;
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
    public void Statement_MethodCall()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_15
{
    public void Write1(int x)
    {
        Write2(x);
    }

    void Write2(int x)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_15
  public void Write1(int x)
  {
    Write2(x);
  }

  void Write2(int x)
  {
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_MethodCallBadMethod()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_16
{
    public void Write1(int x)
    {
        x[0]();
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
    public void Statement_MethodCallNamedArgument()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_17
{
    public void Write1()
    {
        Write2(x: 0);
    }

    void Write2(int x)
    {
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
    public void Statement_MethodCallRefArgument()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_18
{
    public void Write1(int x)
    {
        Write2(ref x);
    }

    void Write2(int x)
    {
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
    public void Statement_MethodCallInvalidExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_19
{
    public void Write1(int x)
    {
        Write2(nameof(x));
    }

    void Write2(int x)
    {
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
    public void Statement_BadExpressionStatement()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_20
{
    void Write(int x)
    {
        x++;
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
    public void Statement_MethodCallInvalidName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_21
{
    public void Write1(int x)
    {
        Write2(x);
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
    public void Statement_MethodCallInvalidRecursive()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_22
{
    public void Write1(int x)
    {
        Write1(x);
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
    public void Statement_MethodCallTooManyArguments()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_23
{
    public void Write1(int x)
    {
        Write2(x, x);
    }

    void Write2(int x)
    {
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
    public void Statement_MethodCallMatchingArguments()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_24
{
    public void Write1(bool x, int y, double z)
    {
        Write2(x, y, z);
        Write2(true, 1, 1.0);
        Write2(true, 1, 1);
    }

    void Write2(bool x, int y, double z)
    {
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Statement_MethodCallNonMatchingArguments()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_25
{
    public void Write1()
    {
        Write2(true);
    }

    void Write2(int x)
    {
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
    public void Statement_MethodCallCircular()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_26
{
    public void Write1()
    {
        Write2();
    }

    void Write2()
    {
        Write1();
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
    public void Statement_ReturnResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_27
{
    int Read()
    {
        int Result = 1;

        return Result;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_27
  int Read()
  {
    int Result = 1
    return Result;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_ReturnNotResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_28
{
    int Read()
    {
        int Result = 0;

        return 0;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(1));
    }
}
