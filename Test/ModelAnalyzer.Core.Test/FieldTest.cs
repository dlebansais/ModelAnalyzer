namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
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
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        IList<IField> Fields = ClassModel.GetFields();

        Assert.That(Fields.Count, Is.EqualTo(3));

        IField FirstField = Fields.First();

        Assert.That(FirstField.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstField.Type, Is.EqualTo(ExpressionType.Boolean));

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
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));

        IUnsupportedField UnsupportedField = ClassModel.Unsupported.Fields[0];
        Assert.That(UnsupportedField.Name.Text, Is.EqualTo("X"));
        Assert.That(UnsupportedField.Type, Is.EqualTo(ExpressionType.Other));
        Assert.That(UnsupportedField.Initializer, Is.Null);

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
").First();

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
    public void Field_InvalidFieldResult()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_9
{
    int Result;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Field_NameCollisionWithProperty()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_10
{
    public int X { get; protected set; }
    int X;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Field_WithClassType()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_11
{
}

class Program_CoreField_12
{
    Program_CoreField_11 X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        foreach (IClassModel ClassModel in ClassModelList)
            Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Field_WithClassTypeNullInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_13
{
}

class Program_CoreField_14
{
    Program_CoreField_13 X = null;
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
    public void Field_NullableWithClassTypeInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_15
{
}

class Program_CoreField_16
{
    Program_CoreField_15? X = null;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        IList<IField> Fields = ClassModel1.GetFields();

        Assert.That(Fields.Count, Is.EqualTo(1));

        IField FirstField = Fields.First();

        Assert.That(FirstField.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstField.Type.TypeName, Is.EqualTo(ClassName.FromSimpleString("Program_CoreField_15")));
        Assert.That(FirstField.Type.IsNullable, Is.True);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_16
  Program_CoreField_15? X = null
"));
    }

    [Test]
    [Category("Core")]
    public void Field_WithClassTypeObjectInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_17
{
}

class Program_CoreField_18
{
    Program_CoreField_17 X = new();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        IList<IField> Fields = ClassModel1.GetFields();

        Assert.That(Fields.Count, Is.EqualTo(1));

        IField FirstField = Fields.First();

        Assert.That(FirstField.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstField.Type.TypeName, Is.EqualTo(ClassName.FromSimpleString("Program_CoreField_17")));
        Assert.That(FirstField.Type.IsNullable, Is.False);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_18
  Program_CoreField_17 X = new Program_CoreField_17()
"));
    }

    [Test]
    [Category("Core")]
    public void Field_NullableWithClassTypeObjectInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_19
{
}

class Program_CoreField_20
{
    Program_CoreField_19? X = new();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);

        IList<IField> Fields = ClassModel1.GetFields();

        Assert.That(Fields.Count, Is.EqualTo(1));

        IField FirstField = Fields.First();

        Assert.That(FirstField.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstField.Type.TypeName, Is.EqualTo(ClassName.FromSimpleString("Program_CoreField_19")));
        Assert.That(FirstField.Type.IsNullable, Is.True);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreField_20
  Program_CoreField_19? X = new Program_CoreField_19()
"));
    }

    [Test]
    [Category("Core")]
    public void Field_InvalidObjectInitializerWithArgument()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_21
{
}

class Program_CoreField_22
{
    Program_CoreField_21 X = new(0);
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
    public void Field_InvalidObjectInitializerWithInitializers()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_23
{
    public int X { get; set; }
}

class Program_CoreField_24
{
    Program_CoreField_23 X = new() { X = 0 };
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
    public void Field_Cycle()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_25
{
    Program_CoreField_26 X;
}

class Program_CoreField_26
{
    Program_CoreField_25 X;
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty && ClassModel1.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void Field_UnknownReference()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_27
{
}

class Program_CoreField_28
{
    Foo X = new();
    Foo? Y = new();
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel1.Unsupported.Fields.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Field_NullablePredefinedType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreField_29
{
    int? X;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));
    }
}
