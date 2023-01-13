namespace Miscellaneous.Test;

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="ClassDeclarationParser"/> class.
/// </summary>
public class ClassDeclarationParserTest
{
    [Test]
    [Category("Core")]
    public void ClassDeclarationParser_BasicTest()
    {
        ClassDeclarationSyntax ClassDeclaration = TestHelper.FromSourceCode(@"
using System;

class Program_CoreParser_0
{
    int X;

    void Write(int x)
    {
    }
}
").First();

        using TokenReplacement TokenReplacement = TestHelper.BeginReplaceToken(ClassDeclaration);

        IClassModel ClassModel = TestHelper.ToClassModel(ClassDeclaration, TokenReplacement);

        Assert.That(ClassModel.Unsupported.IsEmpty, Is.True);
    }
}
