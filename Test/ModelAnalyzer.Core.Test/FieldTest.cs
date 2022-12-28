namespace ModelAnalyzer.Core.Test;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Field"/> class.
/// </summary>
public class FieldTest
{
    [Test]
    [Category("Core")]
    public void Field_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_0
{
    bool X;
    int Y;
    double Z;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_0
  bool X
  int Y
  double Z
"));
    }

    [Test]
    [Category("Core")]
    public void Field_BadType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_1
{
    string X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));

        IUnsupportedField UnsupportedField = ClassModel.Unsupported.Fields[0];
        Assert.That(UnsupportedField.Name.Text, Is.EqualTo("*"));
        Assert.That(UnsupportedField.Type, Is.EqualTo(ExpressionType.Other));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_1
"));
    }

    [Test]
    [Category("Core")]
    public void Field_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_2
{
    [Serializable]
    int X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Field_UnsupportedModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_3
{
    public int X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Field_Modifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_4
{
    private int X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_4
  int X
"));
    }

    [Test]
    [Category("Core")]
    public void Field_DuplicateFieldName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_5
{
    int X;
    int X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Field_ValidInitializer()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_6
{
    bool X = true;
    int Y = 0;
    double Z = 1.1;
    double K = 1;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_6
  bool X = true
  int Y = 0
  double Z = 1.1
  double K = 1
"));
    }

    [Test]
    [Category("Core")]
    public void Field_UnsupportedInitializer()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_7
{
    int X = 1 + 1;
    int Y = false;
    bool Z = 0;
    double K = false;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(4));
    }

    [Test]
    [Category("Core")]
    public void Field_InvalidFieldTestedAsDestination()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_8
{
    string X;
    int Y;

    void Write(int y)
    {
        Y = y;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }
}
