namespace Miscellaneous.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelAnalyzer;

internal class TestHelper
{
    private static List<string> ClassNameList = new();

    public static List<ClassDeclarationSyntax> FromSourceCode(string sourceCode, bool isClassNameRepeated = false)
    {
        CSharpParseOptions Options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText(sourceCode, Options);
        var Diagnostics = SyntaxTree.GetDiagnostics();
        List<Diagnostic> ErrorList = new();

        foreach (Diagnostic Diagnostic in Diagnostics)
            if (Diagnostic.Severity == DiagnosticSeverity.Error && Diagnostic.Id != "CS1029")
                ErrorList.Add(Diagnostic);

        Debug.Assert(ErrorList.Count == 0);

        CompilationUnitSyntax Root = SyntaxTree.GetCompilationUnitRoot();
        Debug.Assert(Root.Members.Count > 0);

        List<ClassDeclarationSyntax> ClassDeclarationList = new();

        if (Root.Members[0] is ClassDeclarationSyntax)
        {
            foreach (ClassDeclarationSyntax ClassDeclaration in Root.Members)
                ClassDeclarationList.Add(ClassDeclaration);
        }
        else
        {
            foreach (ClassDeclarationSyntax ClassDeclaration in ((NamespaceDeclarationSyntax)Root.Members[0]).Members)
                ClassDeclarationList.Add(ClassDeclaration);
        }

        foreach (ClassDeclarationSyntax ClassDeclaration in ClassDeclarationList)
        {
            string ClassName = ClassDeclaration.Identifier.ValueText;

            Debug.Assert(!ClassNameList.Contains(ClassName) || isClassNameRepeated);
            ClassNameList.Add(ClassName);
        }

        return ClassDeclarationList;
    }

    public static TokenReplacement BeginReplaceToken(ClassDeclarationSyntax classDeclaration)
    {
        return BeginReplaceToken(classDeclaration, classDeclaration => default, SyntaxKind.None);
    }

    public static TokenReplacement BeginReplaceToken(ClassDeclarationSyntax classDeclaration, Func<ClassDeclarationSyntax, SyntaxToken> locator, SyntaxKind newKind)
    {
        TokenReplacement Result;
        SyntaxToken Token = locator(classDeclaration);

        if (Token != default)
            Result = new TokenReplacement(Token, newKind);
        else
            Result = TokenReplacement.None;

        return Result;
    }

    public static IClassModel ToClassModel(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement, bool waitIfAsync = false, Action<ClassModelManager>? managerHandler = null)
    {
        List<IClassModel> ClassModelList = ToClassModel(new List<ClassDeclarationSyntax>() { classDeclaration }, tokenReplacement, waitIfAsync, managerHandler);
        return ClassModelList.First();
    }

    public static List<IClassModel> ToClassModel(List<ClassDeclarationSyntax> classDeclarationList, TokenReplacement tokenReplacement, bool waitIfAsync = false, Action<ClassModelManager>? managerHandler = null)
    {
        tokenReplacement.Replace();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = classDeclarationList.Count > 1 ? VerificationProcessStartMode.Manual : VerificationProcessStartMode.Auto };

        MadeUpSemanticModel SemanticModel = new();

        var Result = Manager.GetClassModels(CompilationContext.GetAnother(), classDeclarationList, SemanticModel);

        List<IClassModel> ClassModelList = new(Result.Values);

        if (managerHandler is not null)
            managerHandler(Manager);

        if (waitIfAsync)
        {
            List<IClassModel> VerifiedClassModelList = new();

            foreach (IClassModel ClassModel in ClassModelList)
                VerifiedClassModelList.Add(Manager.GetVerifiedModel(ClassModel));

            ClassModelList = VerifiedClassModelList;
        }

        tokenReplacement.Restore();

        Manager.RemoveMissingClasses(new List<string>());

        return ClassModelList;
    }

    public static async Task<IClassModel> ToClassModelAsync(ClassDeclarationSyntax classDeclaration, TokenReplacement tokenReplacement, Action<ClassModelManager>? managerHandler = null)
    {
        List<IClassModel> ClassModelList = await ToClassModelAsync(new List<ClassDeclarationSyntax>() { classDeclaration }, tokenReplacement, managerHandler);
        return ClassModelList.First();
    }

    public static async Task<List<IClassModel>> ToClassModelAsync(List<ClassDeclarationSyntax> classDeclarationList, TokenReplacement tokenReplacement, Action<ClassModelManager>? managerHandler = null)
    {
        tokenReplacement.Replace();

        using ClassModelManager Manager = new() { Logger = TestInitialization.Logger, StartMode = classDeclarationList.Count > 1 ? VerificationProcessStartMode.Manual : VerificationProcessStartMode.Auto };

        List<Task<IClassModel>> TaskList = new();
        CompilationContext CompilationContext = CompilationContext.Default;

        ClassDeclarationSyntax? PreviousClassDeclaration = null;
        List<IClassModel> ClassModelList = new();
        MadeUpSemanticModel SemanticModel = new();

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
        {
            if (PreviousClassDeclaration != ClassDeclaration)
                CompilationContext = CompilationContext.GetAnother();

            ClassModelList.Add(Manager.GetClassModels(CompilationContext, new List<ClassDeclarationSyntax>() { ClassDeclaration }, SemanticModel).First().Value);

            PreviousClassDeclaration = ClassDeclaration;
        }

        if (managerHandler is not null)
            managerHandler(Manager);

        ClassModelList = await Task.Run(async () =>
        {
            List<IClassModel> VerifiedClassModelList = new();

            foreach (IClassModel ClassModel in ClassModelList)
                VerifiedClassModelList.Add(await Manager.GetVerifiedModelAsync(ClassModel));

            return VerifiedClassModelList;
        });

        tokenReplacement.Restore();

        return ClassModelList;
    }

    public static void ExecuteClassModelTest(List<ClassDeclarationSyntax> classDeclarationList, TokenReplacement tokenReplacement, Action<List<ClassDeclarationSyntax>> handler)
    {
        tokenReplacement.Replace();

        handler(classDeclarationList);

        tokenReplacement.Restore();
    }
}
