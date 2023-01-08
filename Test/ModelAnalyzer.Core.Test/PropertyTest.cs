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

    [Test]
    [Category("Core")]
    public void Property_WithClassType()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_20
{
}

class Program_CoreProperty_21
{
    public Program_CoreProperty_20 X { get; set; }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        foreach (IClassModel ClassModel in ClassModelList)
            Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Property_WithClassTypeNullInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_22
{
}

class Program_CoreProperty_23
{
    public Program_CoreProperty_22 X { get; set; } = null;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel1.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Property_NullableWithClassTypeNullInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_24
{
}

class Program_CoreProperty_25
{
    public Program_CoreProperty_24? X { get; set; } = null;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Property_WithClassTypeObjectInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_26
{
}

class Program_CoreProperty_27
{
    public Program_CoreProperty_26 X { get; set; } = new();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        IList<IProperty> Properties = ClassModel1.GetProperties();

        Assert.That(Properties.Count, Is.EqualTo(1));

        IProperty FirstProperty = Properties.First();

        Assert.That(FirstProperty.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstProperty.Type.Name, Is.EqualTo("Program_CoreProperty_26"));
        Assert.That(FirstProperty.Type.IsNullable, Is.False);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreProperty_27
  public Program_CoreProperty_26 X { get; set; } = new Program_CoreProperty_26()
"));
    }

    [Test]
    [Category("Core")]
    public void Property_NullableWithClassTypeObjectInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_28
{
}

class Program_CoreProperty_29
{
    public Program_CoreProperty_28? X { get; set; } = new();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        IList<IProperty> Properties = ClassModel1.GetProperties();

        Assert.That(Properties.Count, Is.EqualTo(1));

        IProperty FirstProperty = Properties.First();

        Assert.That(FirstProperty.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstProperty.Type.Name, Is.EqualTo("Program_CoreProperty_28"));
        Assert.That(FirstProperty.Type.IsNullable, Is.True);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreProperty_29
  public Program_CoreProperty_28? X { get; set; } = new Program_CoreProperty_28()
"));
    }

    [Test]
    [Category("Core")]
    public void Property_Cycle()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreProperty_30
{
    public Program_CoreProperty_31 X { get; set; } = new();
}

class Program_CoreProperty_31
{
    public Program_CoreProperty_30 X { get; set; } = new();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty && ClassModel1.Unsupported.IsEmpty, Is.False);
    }
}
