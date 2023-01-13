namespace Core.Test;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Local"/> class.
/// </summary>
public class LocalTest
{
    [Test]
    [Category("Core")]
    public void Local_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_0
{
    public void Write()
    {
        bool X;
        int Y;
        double Z;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        IList<IMethod> Methods = ClassModel.GetMethods();

        Assert.That(Methods.Count, Is.EqualTo(1));

        IMethod FirstMethod = Methods.First();

        IList<ILocal> Locals = FirstMethod.GetLocals();

        Assert.That(Locals.Count, Is.EqualTo(3));

        ILocal FirstLocal = Locals.First();

        Assert.That(FirstLocal.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstLocal.Type, Is.EqualTo(ExpressionType.Boolean));

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreLocal_0
  public void Write()
  {
    bool X
    int Y
    double Z
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Local_BadType()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_1
{
    public void Write()
    {
        string X;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Locals.Count, Is.EqualTo(1));

        IUnsupportedLocal UnsupportedLocal = ClassModel.Unsupported.Locals[0];
        Assert.That(UnsupportedLocal.Name.Text, Is.EqualTo("*"));
        Assert.That(UnsupportedLocal.Type, Is.EqualTo(ExpressionType.Other));
        Assert.That(UnsupportedLocal.Initializer, Is.Null);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreLocal_1
  public void Write()
  {
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Local_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_2
{
    public void Write()
    {
        [Serializable]
        int X;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Locals.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Local_UnsupportedModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_3
{
    public void Write()
    {
        const int X = 0;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Locals.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Local_DuplicateLocalName()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_5
{
    public void Write()
    {
        int X;
        int X;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Local_ValidInitializer()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_6
{
    public void Write()
    {
        bool X = true;
        int Y = 0;
        double Z = 1.1;
        double K = 1;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreLocal_6
  public void Write()
  {
    bool X = true
    int Y = 0
    double Z = 1.1
    double K = 1
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Local_UnsupportedInitializer()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_7
{
    public void Write()
    {
        int X = 1 + 1;
        int Y = false;
        bool Z = 0;
        double K = false;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Expressions.Count, Is.EqualTo(4));
    }

    [Test]
    [Category("Core")]
    public void Local_InvalidLocalTestedAsDestination()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_8
{
    public void Write(int y)
    {
        string X;
        int Y;

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
    public void Local_InvalidLocalTestedAsSource()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_9
{
    int Y;

    public void Write(int y)
    {
        string X;
        int Z;

        Y = Z;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
    }

    [Test]
    [Category("Core")]
    public void Local_NameCollisionWithField()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_11
{
    int X;

    public void Write()
    {
        int X;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Locals.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Local_NameCollisionWithParameter()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_12
{
    public void Write(int x)
    {
        int x;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Locals.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Local_NameCollisionWithProperty()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_13
{
    public int X { get; set; }

    public void Write()
    {
        int X;
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.Locals.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void Local_WithClassType()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_14
{
}

class Program_CoreLocal_15
{
    public void Write()
    {
        Program_CoreLocal_14 X;
    }
}
");

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclarationList[0]);

        List<IClassModel> ClassModelList = TestHelper.ToClassModel(ClassDeclarationList, TokenReplacement);

        foreach (IClassModel ClassModel in ClassModelList)
            Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }

    [Test]
    [Category("Core")]
    public void Local_WithClassTypeNullInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_16
{
}

class Program_CoreLocal_17
{
    public void Write()
    {
        Program_CoreLocal_16 X = null;
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
    public void Local_NullableWithClassTypeInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_18
{
}

class Program_CoreLocal_19
{
    public void Write()
    {
        Program_CoreLocal_18? X = null;
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
    public void Local_WithClassTypeObjectInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_20
{
}

class Program_CoreLocal_21
{
    public void Write()
    {
        Program_CoreLocal_20 X = new();
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

        IList<IMethod> Methods = ClassModel1.GetMethods();
        Assert.That(Methods.Count, Is.EqualTo(1));

        IMethod FirstMethod = Methods[0];

        IList<ILocal> Locals = FirstMethod.GetLocals();

        Assert.That(Locals.Count, Is.EqualTo(1));

        ILocal FirstLocal = Locals.First();

        Assert.That(FirstLocal.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstLocal.Type.Name, Is.EqualTo("Program_CoreLocal_20"));
        Assert.That(FirstLocal.Type.IsNullable, Is.False);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreLocal_21
  public void Write()
  {
    Program_CoreLocal_20 X = new Program_CoreLocal_20()
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Local_NullableWithClassTypeObjectInitializer()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_22
{
}

class Program_CoreLocal_23
{
    public void Write()
    {
        Program_CoreLocal_22? X = new();
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

        IList<IMethod> Methods = ClassModel1.GetMethods();
        Assert.That(Methods.Count, Is.EqualTo(1));

        IMethod FirstMethod = Methods[0];

        IList<ILocal> Locals = FirstMethod.GetLocals();

        Assert.That(Locals.Count, Is.EqualTo(1));

        ILocal FirstLocal = Locals.First();

        Assert.That(FirstLocal.Name.Text, Is.EqualTo("X"));
        Assert.That(FirstLocal.Type.Name, Is.EqualTo("Program_CoreLocal_22"));
        Assert.That(FirstLocal.Type.IsNullable, Is.True);

        string? ClassModelString = ClassModel1.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreLocal_23
  public void Write()
  {
    Program_CoreLocal_22? X = new Program_CoreLocal_22()
  }
"));
    }

    [Test]
    [Category("Core")]
    public void Local_Cycle()
    {
        List<ClassDeclarationSyntax> ClassDeclarationList = TestHelper.FromSourceCode(@"
using System;

class Program_CoreLocal_24
{
    public void Write()
    {
        Program_CoreLocal_25 X = new();
    }
}

class Program_CoreLocal_25
{
    public void Write()
    {
        Program_CoreLocal_24 X = new();
    }
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
