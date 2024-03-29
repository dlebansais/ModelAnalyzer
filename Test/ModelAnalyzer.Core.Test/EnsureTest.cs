﻿namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Ensure"/> class.
/// </summary>
public class EnsureTest
{
    [Test]
    [Category("Core")]
    public void Ensure_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_0
{
    public int X { get; set; }

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        IList<IMethod> Methods = ClassModel.GetMethods();

        Assert.That(Methods.Count, Is.EqualTo(1));

        IMethod FirstMethod = Methods.First();
        Assert.That(FirstMethod.Name.Text, Is.EqualTo("Write"));

        IList<IEnsure> Ensures = FirstMethod.GetEnsures();

        Assert.That(Ensures.Count, Is.EqualTo(1));

        IEnsure FirstEnsure = Ensures.First();

        Assert.That(FirstEnsure.Text, Is.EqualTo("X == x"));
        Assert.That(FirstEnsure.Location.IsInSource, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_0
  public int X { get; set; }

  public void Write(int x)
  {
    X = x;
  }
  # ensure X == x
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_ComplexEnsure()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_1
{
    public int X { get; set; }

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x || X != (x + 1) || (X + 1) != x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Ensure_ExpressionNotBoolean()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_2
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: 0
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        IUnsupportedEnsure UnsupportedEnsure = ClassModel.Unsupported.Ensures[0];
        Assert.That(UnsupportedEnsure.Text, Is.EqualTo("0"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_TooManyInstructions()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_3
{
    public int X { get; set; }

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == 0; break;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        IUnsupportedEnsure UnsupportedEnsure = ClassModel.Unsupported.Ensures[0];
        Assert.That(UnsupportedEnsure.Text, Is.EqualTo("X == 0; break;"));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_3
  public int X { get; set; }

  public void Write(int x)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_NoKeyword()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_4
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
    // Ensure
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_4
  int X

  public void Write(int x)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_InvalidExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_5
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: *
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        IUnsupportedEnsure UnsupportedEnsure = ClassModel.Unsupported.Ensures[0];
        Assert.That(UnsupportedEnsure.Text, Is.EqualTo("*"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_MultipleMethods()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_6
{
    public int X { get; set; }

    public void Write1(int x)
    {
        X = x;
    }
    // Ensure: X == x

    public void Write2(int x)
    {
        X = x;
    }
    // Ensure: X == x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_6
  public int X { get; set; }

  public void Write1(int x)
  {
    X = x;
  }
  # ensure X == x

  public void Write2(int x)
  {
    X = x;
  }
  # ensure X == x
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_UnsupportedMember()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_7
{
    public int X { get; set; }

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x

    public delegate void Event();
    // Ensure: X == x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.HasUnsupporteMember, Is.True);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_7
  public int X { get; set; }

  public void Write(int x)
  {
    X = x;
  }
  # ensure X == x
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_UnknownField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_8
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: Y == x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_8
  int X

  public void Write(int x)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_Missplaced()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_9
{
    int X;

    public void Write(int x)
    // Ensure: X == x
    {
        X = x;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Ensures.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_9
  int X

  public void Write(int x)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_LocalNotAllowed()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_10
{
    public void Write(int x)
    {
        int X;

        X = x;
    }
    // Ensure: X == x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreEnsure_10
  public void Write(int x)
  {
    int X

    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Ensure_FieldNotAllowed()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_11
{
    int X;

    public void Write(int x)
    {
        X = x;
    }
    // Ensure: X == x
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Ensure_DirectResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_12
{
    public bool Write()
    {
        return true;
    }
    // Ensure: Result
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Ensure_InvalidFunctionCall()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreEnsure_13
{
    public void Write(int x)
    {
    }
    // Ensure: Write2() == 0

    int Write2(int x)
    {
        return x;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));

        IList<IMethod> Methods = ClassModel.GetMethods();
        Assert.That(Methods.Count, Is.EqualTo(2));

        IMethod FirstMethod = Methods.First();
        IList<IEnsure> Ensures = FirstMethod.GetEnsures();
        Assert.That(Ensures.Count, Is.EqualTo(0));
    }
}
