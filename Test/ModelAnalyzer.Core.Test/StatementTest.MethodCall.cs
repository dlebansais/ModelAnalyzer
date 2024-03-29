﻿namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Statement"/> class.
/// </summary>
public partial class StatementTest
{
    [Test]
    [Category("Core")]
    public void Statement_MethodCall()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_1
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
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethodCallStatement_1
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

class Program_CoreMethodCallStatement_2
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

class Program_CoreMethodCallStatement_3
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

class Program_CoreMethodCallStatement_4
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

class Program_CoreMethodCallStatement_5
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
    public void Statement_MethodCallInvalidName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_6
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

class Program_CoreMethodCallStatement_7
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

class Program_CoreMethodCallStatement_8
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

class Program_CoreMethodCallStatement_9
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

class Program_CoreMethodCallStatement_10
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

class Program_CoreMethodCallStatement_11
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

class Program_CoreMethodCallStatement_12
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

class Program_CoreMethodCallStatement_13
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
    public void Statement_MethodCallMultiple()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_14
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
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethodCallStatement_14
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
    public void Statement_PublicMethodCall()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_15
{
    public void WriteY()
    {
    }
}

class Program_CoreMethodCallStatement_16
{
    public void WriteX()
    {
        Program_CoreMethodCallStatement_15 X = new();

        X.WriteY();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethodCallStatement_16
  public void WriteX()
  {
    Program_CoreMethodCallStatement_15 X = new Program_CoreMethodCallStatement_15()

    X.WriteY();
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_InvalidCallPath()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_17
{
    public void WriteZ()
    {
    }
}

class Program_CoreMethodCallStatement_18
{
    Program_CoreMethodCallStatement_17 Y = new();
}

class Program_CoreMethodCallStatement_19
{
    public void WriteX()
    {
        Program_CoreMethodCallStatement_18 X = new();

        X.Y.WriteZ();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

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
    public void Statement_InvalidLastMethod()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_20
{
    public void WriteY()
    {
    }
}

class Program_CoreMethodCallStatement_21
{
    public void WriteX()
    {
        Program_CoreMethodCallStatement_20 X = new();

        X.OtherMethod();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel1.Unsupported.Statements.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Statement_InvalidMiddleProperty()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_22
{
    public int Z { get; set; }
}

class Program_CoreMethodCallStatement_23
{
    public int Y { get; set; }
}

class Program_CoreMethodCallStatement_24
{
    public void WriteX()
    {
        Program_CoreMethodCallStatement_23 X = new();

        X.Y.Z();
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

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
    public void Statement_PrivateStaticMethodCall()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_25
{
    public void Write1(int x)
    {
        Write2(x);
        Program_CoreMethodCallStatement_25.Write2(x);
    }

    static void Write2(int x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreMethodCallStatement_25
  public void Write1(int x)
  {
    Program_CoreMethodCallStatement_25.Write2(x);
    Program_CoreMethodCallStatement_25.Write2(x);
  }

  static void Write2(int x)
  {
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Statement_PrivateStaticBadMethodName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_26
{
    public void Write1(int x)
    {
        Write2(x);
        Program_CoreMethodCallStatement_26.OtherMethod(x);
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void Statement_PrivateStaticBadClassName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_27
{
    public void Write(int x)
    {
        nameof(x).OtherMethod(x);
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void Statement_PrivateStaticWrongClassName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreMethodCallStatement_28
{
    public void Write(int x)
    {
        OtherClass.OtherMethod(x);
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }
}
