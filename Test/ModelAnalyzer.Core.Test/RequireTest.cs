namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Require"/> class.
/// </summary>
public class RequireTest
{
    [Test]
    [Category("Core")]
    public void Require_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_0
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        IReadOnlyList<IMethod> Methods = ClassModel.GetMethods();

        Assert.That(Methods.Count, Is.EqualTo(1));

        IMethod FirstMethod = Methods.First();

        IReadOnlyList<IRequire> Requires = FirstMethod.GetRequires();

        Assert.That(Requires.Count, Is.EqualTo(1));

        IRequire FirstRequire = Requires.First();

        Assert.That(FirstRequire.Text, Is.EqualTo("x >= 0"));
        Assert.That(FirstRequire.Location.IsInSource, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_0
  int X
  void Write(int x)
  # require x >= 0
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_ComplexRequire()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_1
{
    int X;

    void Write(int x)
    // Require: x >= 0 || x >= (0 + 1) || (x + 1) <= 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_1
  int X
  void Write(int x)
  # require ((x >= 0) || (x >= (0 + 1))) || ((x + 1) <= 0)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_ExpressionNotBoolean()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_2
{
    int X;

    void Write(int x)
    // Require: 0
    // Require: X
    // Require: X + X
    // Require: -X
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);

        IUnsupportedRequire UnsupportedRequire = ClassModel.Unsupported.Requires[0];
        Assert.That(UnsupportedRequire.Text, Is.EqualTo("0"));

        TestUnsupportedRequireExpression(ClassModel, 0, "0");
        TestUnsupportedRequireExpression(ClassModel, 1, "X");
        TestUnsupportedRequireExpression(ClassModel, 2, "X + X");
        TestUnsupportedRequireExpression(ClassModel, 3, "-X");

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_2
  int X
  void Write(int x)
  {
    X = x;
  }
"));
    }

    private void TestUnsupportedRequireExpression(IClassModel classModel, int index, string unsupportedExpressionText)
    {
        Assert.That(classModel.Unsupported.Requires.Count, Is.GreaterThan(index));

        IUnsupportedRequire UnsupportedRequire = classModel.Unsupported.Requires[index];
        Assert.That(UnsupportedRequire.Text, Is.EqualTo(unsupportedExpressionText));
    }

    [Test]
    [Category("Core")]
    public void Require_InvalidExpression()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_3
{
    int X;

    void Write(int x)
    // Require: *
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);

        IUnsupportedRequire UnsupportedRequire = ClassModel.Unsupported.Requires[0];
        Assert.That(UnsupportedRequire.Text, Is.EqualTo("*"));
    }

    [Test]
    [Category("Core")]
    public void Require_TooManyInstructions()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_4
{
    int X;

    void Write(int x)
    // Require: X == 0; break;
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Requires.Count, Is.EqualTo(1));

        IUnsupportedRequire UnsupportedRequire = ClassModel.Unsupported.Requires[0];
        Assert.That(UnsupportedRequire.Text, Is.EqualTo("X == 0; break;"));
    }

    [Test]
    [Category("Core")]
    public void Require_NoKeyword()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_5
{
    int X;

    void Write(int x)
    // Require
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_5
  int X
  void Write(int x)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_MultipleMethods()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_6
{
    int X;

    void Write1(int x)
    // Require: x >= 0
    {
        X = x;
    }

    void Write2(int x)
    // Require: x >= 0
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_6
  int X
  void Write1(int x)
  # require x >= 0
  {
    X = x;
  }
  void Write2(int x)
  # require x >= 0
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_UnsupportedMember()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_7
{
    int X;

    void Write(int x)
    // Require: x >= 0
    {
        X = x;
    }

    int Getter { get; }
    // Require: X >= 0
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.HasUnsupporteMember, Is.True);
        Assert.That(ClassModel.Unsupported.Requires.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_7
  int X
  void Write(int x)
  # require x >= 0
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_ExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_8
{
    int X;

    int Read() => X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_8
  int X
  int Read()
  {
    return X;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_UnknownField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_9
{
    int X;

    void Write(int x)
    // Require: Y == x
    {
        X = x;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_9
  int X
  void Write(int x)
  {
    X = x;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Require_Missplaced()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreRequire_10
{
    int X;

    void Write(int x)
    {
        X = x;
    }
    // Require: X == x
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Requires.Count, Is.EqualTo(1));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreRequire_10
  int X
  void Write(int x)
  {
    X = x;
  }
"));
    }
}
