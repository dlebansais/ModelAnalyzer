namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using System.Linq;
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
").First();

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
    public int Z { get; set; }

    void Write(int x)
    {
        X = x + 1;
        Y = X;
        Z = x;
    }
}
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
    bool Write(int x)
    {
        return x is string;
    }
}
").First();

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
    bool X;
    bool Read() => X is string;
}
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_MethodCallTooManyArgumentsNested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_24
{
    public void Write1(int x)
    {
        if (false)
            Write2(x, x);
        else
            Write2(x);
    }

    void Write2(int x)
    {
    }
}
").First();

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

class Program_CoreStatement_25
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
").First();

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

class Program_CoreStatement_26
{
    public void Write1()
    {
        Write2(true);
    }

    void Write2(int x)
    {
    }
}
").First();

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

class Program_CoreStatement_27
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
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Statement_MethodCallCircularNested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_28
{
    public void Write1()
    {
        if (false)
            Write2();
    }

    void Write2()
    {
        Write1();
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Statement_ReturnResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_29
{
    int Read()
    {
        int Result = 1;

        return Result;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_29
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

class Program_CoreStatement_30
{
    int Read()
    {
        int Result = 0;

        return 0;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_ReturnExpressionWhenVoid()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_31
{
    void Read()
    {
        int Result = 0;

        return 0;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_MethodCallMultiple()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_32
{
    public void Write1(int x)
    {
        Write2(x);
        Write3(x);
    }

    void Write2(int x)
    {
    }

    void Write3(int x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_32
  public void Write1(int x)
  {
    Write2(x);
    Write3(x);
  }

  void Write2(int x)
  {
  }

  void Write3(int x)
  {
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_AssignmentOfNewObject()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_33
{
}

class Program_CoreStatement_34
{
    Program_CoreStatement_33 X = new();

    public void Write(int x)
    {
        Program_CoreStatement_33 Y;
        bool Z;

        Y = X;
        Z = Y == new Program_CoreStatement_33();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreStatement_34
  Program_CoreStatement_33 X = new Program_CoreStatement_33()

  public void Write(int x)
  {
    Program_CoreStatement_33 Y
    bool Z
    Y = X;
    Z = Y == new Program_CoreStatement_33();
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_AssignmentIncompatibleNullable()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_35
{
}

class Program_CoreStatement_36
{
}

class Program_CoreStatement_37
{
    Program_CoreStatement_35? X = new();
    Program_CoreStatement_36? Y = new();

    public void Write()
    {
        X = Y;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(3));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];
        IClassModel ClassModel2 = ClassModelList[2];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel2.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_AssignmentOfNewObjectNoArgument()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreStatement_38
{
}

class Program_CoreStatement_39
{
    public void Write(int x)
    {
        Program_CoreStatement_38? Y;

        Y = new Program_CoreStatement_38 { };
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.False);
    }
}
