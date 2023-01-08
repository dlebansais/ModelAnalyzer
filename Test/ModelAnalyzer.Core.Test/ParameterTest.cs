﻿namespace ModelAnalyzer.Core.Test;

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Parameter"/> class.
/// </summary>
public class ParameterTest
{
    [Test]
    [Category("Core")]
    public void Parameter_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_0
{
    int X;
    bool Y;

    void Write(int x, bool y)
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
    public void Parameter_InvalidParameterType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_2
{
    void Write(string x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));

        IUnsupportedParameter UnsupportedParameter = ClassModel.Unsupported.Parameters[0];
        Assert.That(UnsupportedParameter.Name.Text, Is.EqualTo("*"));
        Assert.That(UnsupportedParameter.Type, Is.EqualTo(ExpressionType.Other));
    }

    [Test]
    [Category("Core")]
    public void Parameter_DuplicateParameterName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_3
{
    void Write(int x, int x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreParameter_3
  void Write(int x)
  {
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Parameter_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_4
{
    void Write([System.Runtime.InteropServices.In]int x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Parameter_UnsupportedModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_5
{
    void Write(ref int x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Parameter_NameCollisionWithField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_6
{
    int X;

    void Write(int X)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Parameter_NameCollisionWithProperty()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_7
{
    public int X { get; set; }

    void Write(int X)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Parameter_InvalidParameterTestedAsSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_8
{
    int Y;

    void Write(string x, int y)
    {
        Y = y;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void Parameter_InvalidParameterName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParameter_9
{
    void Write(int Result)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Methods.Count, Is.EqualTo(0));
        Assert.That(ClassModel.Unsupported.Parameters.Count, Is.EqualTo(1));

        IUnsupportedParameter UnsupportedParameter = ClassModel.Unsupported.Parameters[0];
    }
}
