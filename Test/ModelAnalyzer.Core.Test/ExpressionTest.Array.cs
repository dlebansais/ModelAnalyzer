namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Ensure"/> class.
/// </summary>
public partial class ExpressionTest
{
    [Test]
    [Category("Core")]
    public void Expression_ArrayElement()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_0
{
    public void Write()
    {
        int[] X = new int[2];
        int Y;

        Y = X[0];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidMiddleElement()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_1
{
    public int Z { get; set; }
}

class Program_CoreExpression_Array_2
{
    public int[] Y { get; set; } = new int[0];
}

class Program_CoreExpression_Array_3
{
    public int ReadX()
    {
        Program_CoreExpression_Array_2 X = new();

        return X.Y.Z.SomeMethod();
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
        Assert.That(ClassModel2.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidElementType()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_4
{
    void Write()
    {
        int[] X = new char[2];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidArrayInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_5
{
    void Write()
    {
        int[] X = new int[] { 0 };
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidExpressionWithArray()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_6
{
    void Write()
    {
        int X = 0;
        X = (X + X)[0];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidElementIndex()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_7
{
    void Write()
    {
        int[] X = new int[1];
        int Y;

        Y = X[Y == 0 ? 0 : 1];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidElementIndexArray()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_8
{
    void Write()
    {
        int[] X = new int[1];
        int Y;

        Y = X[X[Y]];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidComplexElementIndex()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_9
{
    public int[] Y { get; set; } = new int[1];
}

class Program_CoreExpression_Array_10
{
    void Write()
    {
        Program_CoreExpression_Array_9 X = new();
        int Z;

        Z = X[X.Y];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(2));

        IClassModel ClassModel1 = ClassModelList[1];

        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel1.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidArrayInitializerIndex()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_11
{
    void Write()
    {
        int X;
        int[] Y = new int[X == 0 ? 0 : 1];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_ArrayTooManyRanks()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_12
{
    void Write()
    {
        int[][] X = new int[][2];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_ArrayTooManySizes()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_13
{
    void Write()
    {
        int[,] X = new int[1, 1];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_ArrayVariableRank()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_14
{
    void Write()
    {
        int X = 1;
        int[] Y = new int[Y];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_ArrayUnsupportedRank()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_Array_15
{
    void Write()
    {
        int[] X = new int[1.0];
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList.First());

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel0.Unsupported.Expressions.Count, Is.EqualTo(1));
    }
}
