﻿namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    public void Expression_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_0
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
    public void Expression_BinaryArithmetic()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_1
{
    int X;

    void Write(int x)
    {
        X = x + 1 + x;
        X = x - 1 - x;
        X = x * 1 * x;
        X = x / 1 / x;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryArithmetic_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_2
{
    int X;

    void Write(int x)
    {
        X = x + 1; // Token '+' is replaced with ';'.
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryArithmeticOperator, SyntaxKind.SemicolonToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    private SyntaxToken LocateBinaryArithmeticOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryLogical()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_3
{
    void Write(int x)
    {
        if ((x == 0) && (x == 0 || x == 1) && (x == 0 || x == 1 || x == 2))
            if (true || false)
                if (true && true)
                {
                }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_BinaryLogical_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_4
{
    int X;

    void Write(int x)
    {
        if (x == 0 && x == 0) // Token '&&' is replaced with 'do'.
        {
        }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateBinaryLogicalOperator, SyntaxKind.DoKeyword);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    private SyntaxToken LocateBinaryLogicalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        BinaryExpressionSyntax BinaryExpression = (BinaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = BinaryExpression.OperatorToken;

        return Operator;
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryLogical()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_5
{
    void Write(int x)
    {
        if (!(x == 0))
            if (!true)
            {
            }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryLogical_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_6
{
    int X;

    void Write(int x)
    {
        if (!(x == 0)) // Token '!' is replaced with '%'.
        {
        }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateUnaryLogicalOperator, SyntaxKind.PercentToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    private SyntaxToken LocateUnaryLogicalOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        IfStatementSyntax IfStatement = (IfStatementSyntax)Block.Statements[0];
        PrefixUnaryExpressionSyntax UnaryExpression = (PrefixUnaryExpressionSyntax)IfStatement.Condition;
        SyntaxToken Operator = UnaryExpression.OperatorToken;

        return Operator;
    }

    [Test]
    [Category("Core")]
    public void Expression_Parenthesized()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_7
{
    int X;

    void Write(int x)
    {
        X = ((x + 1) + (x - 1)) - ((x - 1) - (x + 1) + (-(x + 1)));
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_InAssertion()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_8
{
    public int X { get; set; }

    public void Write(int x)
    // Require: x >= 0
    // Require: (x + 1) >= (0)
    // Require: true
    // Require: (x + 1) >= -1
    {
        X = x;
    }
    // Ensure: (X) >= 0
    // Ensure: X >= 0 && (X >= 1)
    // Ensure: !(X < 0)
    // Ensure: X >= 0 && !(X >= 1)
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_8
  public int X { get; set; }

  public void Write(int x)
  # require x >= 0
  # require (x + 1) >= 0
  # require true
  # require (x + 1) >= -1
  {
    X = x;
  }
  # ensure X >= 0
  # ensure (X >= 0) && (X >= 1)
  # ensure !(X < 0)
  # ensure (X >= 0) && (!(X >= 1))
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_Unsupported()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_9
{
    int X;

    void Write(int x)
    {
        if (x is string)
        {
        }
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
    public void Expression_Unsupported_Nested()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_10
{
    int X;

    void Write(int x)
    {
        if (x is string || x is float)
        {
        }
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(2));
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidFieldName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_11
{
    int X;

    void Write(int x)
    {
        if (Y == 0)
        {
        }
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
    public void Expression_InvalidLocalName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_12
{
    void Write(int x)
    {
        int X;

        if (Y == 0)
        {
        }
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
    public void Expression_Unsupported_Parenthesized()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_13
{
    int X;

    void Write(int x)
    {
        if ((x is string))
        {
        }
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
    public void Expression_Unsupported_Literal()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_14
{
    int X;

    void Write(int x)
    {
        if (x == ""*"")
        {
        }
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
    public void Expression_UnaryArithmetic()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_15
{
    int X;

    void Write()
    {
        X = -1;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryArithmetic_InvalidOperator()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_16
{
    int X;

    void Write(int x)
    {
        X = -1; // Token '-' is replaced with '+'.
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration, LocateUnaryArithmeticOperator, SyntaxKind.PlusToken);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    private SyntaxToken LocateUnaryArithmeticOperator(ClassDeclarationSyntax classDeclaration)
    {
        MethodDeclarationSyntax Method = (MethodDeclarationSyntax)classDeclaration.Members[1];
        BlockSyntax Block = Method.Body!;
        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        AssignmentExpressionSyntax AssignmentExpression = (AssignmentExpressionSyntax)ExpressionStatement.Expression;
        PrefixUnaryExpressionSyntax UnaryExpression = (PrefixUnaryExpressionSyntax)AssignmentExpression.Right;
        SyntaxToken Operator = UnaryExpression.OperatorToken;

        return Operator;
    }

    [Test]
    [Category("Core")]
    public void Expression_UnaryArithmetic_InvalidOperand()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_17
{
    int X;

    void Write(int x)
    {
        X = -sizeof(X);
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
    public void Expression_SourceAndDestinationNotCompatible()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_18
{
    int X;

    void Write(int x, double y)
    {
        X = x + y;
        X = y + x;
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
    public void Expression_Double()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_19
{
    double X;

    void Write()
    {
        X = 1.1 + 2.2;
        X = 1;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_ResultInsideEnsure()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_20
{
    int X;

    public int Write()
    {
        return X;
    }
    // Ensure: Result == 0
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_ResultOutsideEnsure()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_21
{
    int X;

    bool Write()
    {
        return Result == 0;
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
    public void Expression_FieldSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_22
{
    int X;

    void Write()
    {
        int Y;

        Y = X;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_ParameterSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_23
{
    void Write(int x)
    {
        int Y;

        Y = x;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Expression_Remainder_InvalidLeft()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_41
{
    int X;

    void Write(int x)
    {
        X = 1.1 % 2;
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
    public void Expression_Remainder_InvalidRight()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_42
{
    int X;

    void Write(int x)
    {
        X = 1 % 2.1;
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
    public void Expression_Null()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_43
{
}

class Program_CoreExpression_44
{
    Program_CoreExpression_43? X;
    Program_CoreExpression_43? Y;

    public int Read()
    {
        int Z;

        X = null;
        Y = new Program_CoreExpression_43();

        if (X == null && Y == null)
            Z = 0;
        else
            Z = 1;

        return Z;
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
    }

    [Test]
    [Category("Core")]
    public void Expression_NewObjectWithArgument()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_45
{
}

class Program_CoreExpression_46
{
    public void Write()
    {
        Program_CoreExpression_45 Z;

        Z = new Program_CoreExpression_45(0);
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
        Assert.That(ClassModel1.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_NewObjectWithInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_47
{
    public int X { get; set; }
}

class Program_CoreExpression_48
{
    public void Write()
    {
        Program_CoreExpression_47 Z;

        Z = new Program_CoreExpression_47() { X = 0 };
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
        Assert.That(ClassModel1.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_NewObjectWithUnknownType()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_49
{
}

class Program_CoreExpression_50
{
    public void Write()
    {
        Program_CoreExpression_49 Z;

        Z = new Foo();
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
        Assert.That(ClassModel1.Unsupported.Expressions.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Expression_WrongTypeAssignment()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_51
{
}

class Program_CoreExpression_52
{
}

class Program_CoreExpression_53
{
    public void Write()
    {
        Program_CoreExpression_51 X = new();
        Program_CoreExpression_52 Y = new();

        Y = X;
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
    }

    [Test]
    [Category("Core")]
    public void Expression_PathExpression()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_54
{
    public int Y { get; set; }
}

class Program_CoreExpression_55
{
    public void Write()
    {
        Program_CoreExpression_54 X = new();
        int N;

        N = X.Y;
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
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_55
  public void Write()
  {
    Program_CoreExpression_54 X = new Program_CoreExpression_54()
    int N

    N = X.Y;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_LongPathExpression()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_56
{
    public double Z { get; set; }
}

class Program_CoreExpression_57
{
    public Program_CoreExpression_56 Y { get; set; } = new();
}

class Program_CoreExpression_58
{
    public Program_CoreExpression_57 X { get; set; } = new();
}

class Program_CoreExpression_59
{
    public void Write()
    {
        Program_CoreExpression_58 A = new();
        double D;

        D = A.X.Y.Z;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(4));

        IClassModel ClassModel0 = ClassModelList[0];
        IClassModel ClassModel1 = ClassModelList[1];
        IClassModel ClassModel2 = ClassModelList[2];
        IClassModel ClassModel3 = ClassModelList[3];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel1.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel2.Unsupported.IsEmpty, Is.True);
        Assert.That(ClassModel3.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel3.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreExpression_59
  public void Write()
  {
    Program_CoreExpression_58 A = new Program_CoreExpression_58()
    double D

    D = A.X.Y.Z;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Expression_UnknownFirstVariableInPath()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_60
{
    public int Y { get; set; }
}

class Program_CoreExpression_61
{
    public void Write()
    {
        int N;

        N = Z.Y;
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

    [Test]
    [Category("Core")]
    public void Expression_UnknownSecondVariableInPath()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_62
{
    public int Y { get; set; }
}

class Program_CoreExpression_63
{
    public void Write()
    {
        Program_CoreExpression_62 X = new();
        int N;

        N = X.Z;
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

    [Test]
    [Category("Core")]
    public void Expression_InvalidSecondVariableInPath()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_64
{
    public int Z { get; set; }
}

class Program_CoreExpression_65
{
    public int Y { get; set; }
}

class Program_CoreExpression_66
{
    public void Write()
    {
        Program_CoreExpression_65 X = new();
        int N;

        N = X.Y.Z;
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
    }

    [Test]
    [Category("Core")]
    public void Expression_InvalidSecondClassInPath()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_67
{
    public void Write()
    {
        int X;
        
        X = nameof(X).Length;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);
        Assert.That(ClassModelList.Count, Is.EqualTo(1));

        IClassModel ClassModel0 = ClassModelList[0];

        Assert.That(ClassModel0.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void Expression_UnsupportedType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreExpression_68
{
    Tuple<int, int> X;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Fields.Count, Is.EqualTo(1));
    }
}
