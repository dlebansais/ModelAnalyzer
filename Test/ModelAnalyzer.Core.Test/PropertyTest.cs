namespace ModelAnalyzer.Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Property"/> class.
/// </summary>
public class PropertyTest
{
    [Test]
    [Category("Core")]
    public void Property_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_0
{
    public bool X { get; set; }
    public int Y { get; set; }
    public double Z { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        IList<IProperty> Properties = ClassModel.GetProperties();

        Assert.That(Properties.Count, Is.EqualTo(3));

        IProperty FirstProperty = Properties.First();

        Assert.That(FirstProperty.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstProperty.Type, Is.EqualTo(ExpressionType.Boolean));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreProperty_0
  public bool X { get; set; }
  public int Y { get; set; }
  public double Z { get; set; }
"));
    }

    [Test]
    [Category("Core")]
    public void Property_BadType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_1
{
    public string X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));

        IUnsupportedProperty UnsupportedProperty = ClassModel.Unsupported.Properties[0];
        Assert.That(UnsupportedProperty.Name.Text, Is.EqualTo("*"));
        Assert.That(UnsupportedProperty.Type, Is.EqualTo(ExpressionType.Other));
        Assert.That(UnsupportedProperty.Initializer, Is.Null);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreProperty_1
"));
    }

    [Test]
    [Category("Core")]
    public void Property_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_2
{
    [Serializable]
    public int X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_UnsupportedModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_3
{
    protected int X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_MissingModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_4
{
    int X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_InternalModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_5
{
    internal int X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreProperty_5
  public int X { get; set; }
"));
    }

    [Test]
    [Category("Core")]
    public void Property_DuplicatePropertyName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_6
{
    public int X { get; set; }
    public int X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Property_ValidInitializer()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_7
{
    public bool X { get; set; } = true;
    public int Y { get; set; } = 0;
    public double Z { get; set; } = 1.1;
    public double K { get; set; } = 1;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreProperty_7
  public bool X { get; set; } = true
  public int Y { get; set; } = 0
  public double Z { get; set; } = 1.1
  public double K { get; set; } = 1
"));
    }

    [Test]
    [Category("Core")]
    public void Property_UnsupportedInitializer()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_8
{
    public int X { get; set; } = 1 + 1;
    public int Y { get; set; } = false;
    public bool Z { get; set; } = 0;
    public double K { get; set; } = false;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(4));
    }

    [Test]
    [Category("Core")]
    public void Property_InvalidPropertyTestedAsDestination()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_9
{
    public string X { get; set; }
    public int Y { get; set; }

    public void Write(int y)
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
    public void Property_InvalidPropertyResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_10
{
    public int Result { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_ExplicitInterface()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_11
{
    int ITest.X { get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_Body()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_12
{
    public int X
    {
        get { return 0; }
        set { }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_ExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_13
{
    public int Result { get; set; } => 0;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_AccessorExpressionBody()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_14
{
    public int Result { get => 0; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_MissingAccessor()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_15
{
    public int Result { get; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_WrongAccessorOrder()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_16
{
    public int Result { set; get; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_NoAccessor()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_17
{
    public int Result => 0;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_AccessorWithModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_18
{
    public int Result { get; protected set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_AccessorAttribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_19
{
    public int X { [System.Runtime.CompilerServices.CompilerGenerated]get; set; }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }
}
