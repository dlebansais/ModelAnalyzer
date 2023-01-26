namespace Core.Test;

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Miscellaneous.Test;
using ModelAnalyzer;
using Newtonsoft.Json;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="ClassModel"/> class.
/// </summary>
public class ClassModelTest
{
    [Test]
    [Category("Core")]
    public void ClassModel_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_0
{
    public int X { get; set; }
    double Y;
    bool Z() => false;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_0
  public int X { get; set; }

  double Y

  bool Z()
  {
    return false;
  }
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModel_Json()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_Json
{
    public int X { get; set; }
    double Y;
    bool Z() => false;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);
        string JsonString;
        ClassModelTable? Result;

        ClassModel ClassModel = (ClassModel)TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);
        ClassModelTable Table0 = new();
        Table0.Add(ClassModel.ClassName, ClassModel);

        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassModelTableConverter());

        JsonString = JsonConvert.SerializeObject(Table0, Settings);

        Result = JsonConvert.DeserializeObject<ClassModelTable>(JsonString, Settings);

        Assert.That(Result, Is.Not.Null);
        Assert.That(Result!.Count, Is.EqualTo(1));
    }

    [Test]
    [Category("Core")]
    public void ClassModel_InvalidClass_Attribute()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

[Serializable]
class Program_CoreClass_1
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModel_PublicModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

public class Program_CoreClass_2
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_2
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModel_PrivateModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

private class Program_CoreClass_3
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_3
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModel_InternalModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

internal class Program_CoreClass_4
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_4
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModel_PartialModifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

partial class Program_CoreClass_5
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);

        string? ClassModelString = ClassModel.ToString();
        Assert.That(ClassModelString, Is.EqualTo(@"Program_CoreClass_5
"));
    }

    [Test]
    [Category("Core")]
    public void ClassModel_Modifier()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

static class Program_CoreClass_6
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModel_ClassBase()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_7 : System.Action
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModel_InterfaceBase()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_8 : IDisposable
{
    public void Dispose()
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
    public void ClassModel_TypeParameter()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_9<T>
{
    T X;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModel_Constraint()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_10<T>
    where T : class
{
    T X;
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModel_GenericClassBase()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_11 : System.Action<int>
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }

    [Test]
    [Category("Core")]
    public void ClassModel_SimpleNameClassBase()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreClass_12 : Action
{
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.False);
        Assert.That(ClassModel.Unsupported.InvalidDeclaration, Is.True);
    }
}
