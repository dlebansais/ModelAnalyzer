namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassModelAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ClassModel";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ClassModelAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var ClassDeclaration = (ClassDeclarationSyntax)context.Node;
            AnalyzeClass(context, ClassDeclaration);
        }
        catch (Exception e)
        {
            Logger.Log(e.Message);
            Logger.Log(e.StackTrace);
        }
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        // Check whether the class can be modeled
        if (IsClassIgnoredForModeling(classDeclaration))
            return;

        string Name = classDeclaration.Identifier.ValueText;
        if (Name == string.Empty)
            return;

        List<string> InvariantList = ParseInvariants(classDeclaration);
        ClassModel ClassModel = new() { Name = Name, InvariantList = InvariantList };

        if (IsClassDeclarationSupported(classDeclaration) && AreAllMembersSupported(classDeclaration, ClassModel))
        {
            ClassModelManager.Instance.Update(ClassModel);
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
    }

    private static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;

        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                if (Comment.StartsWith($"// {Modeling.None}"))
                    return true;
            }

        return false;
    }

    private static List<string> ParseInvariants(ClassDeclarationSyntax classDeclaration)
    {
        List<string> InvariantList = new();

        SyntaxToken LastToken = classDeclaration.GetLastToken();
        var Location = LastToken.GetLocation();

        if (LastToken.HasTrailingTrivia)
        {
            SyntaxTriviaList TrailingTrivia = LastToken.TrailingTrivia;
            AddInvariantsInTrivia(InvariantList, TrailingTrivia);
            Location = TrailingTrivia.Last().GetLocation();
        }

        var NextToken = classDeclaration.SyntaxTree.GetRoot().FindToken(Location.SourceSpan.End);

        if (NextToken.HasLeadingTrivia)
            AddInvariantsInTrivia(InvariantList, NextToken.LeadingTrivia);

        return InvariantList;
    }

    private static void AddInvariantsInTrivia(List<string> invariantList, SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Pattern))
                    invariantList.Add(Comment.Substring(Pattern.Length));
            }
    }

    private static bool IsClassDeclarationSupported(ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in classDeclaration.Modifiers)
        {
            SyntaxKind Kind = Modifier.Kind();
            if (Kind != SyntaxKind.PrivateKeyword && Kind != SyntaxKind.PublicKeyword && Kind != SyntaxKind.InternalKeyword && Kind != SyntaxKind.PartialKeyword)
                return false;
        }

        if (classDeclaration.BaseList is BaseListSyntax BaseList && BaseList.Types.Count > 0)
            return false;

        if (classDeclaration.TypeParameterList is TypeParameterListSyntax TypeParameterList && TypeParameterList.Parameters.Count > 0)
            return false;

        if (classDeclaration.ConstraintClauses.Count > 0)
            return false;

        return true;
    }

    private static bool AreAllMembersSupported(ClassDeclarationSyntax classDeclaration, ClassModel classModel)
    {
        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (!IsMemberSupported(Member, classModel))
                return false;

        return true;
    }

    private static bool IsMemberSupported(MemberDeclarationSyntax member, ClassModel classModel)
    {
        switch (member)
        {
            case FieldDeclarationSyntax AsField:
                return IsFieldSupported(AsField, classModel);
            case MethodDeclarationSyntax AsMethod:
                return IsMethodSupported(AsMethod, classModel);
            default:
                return false;
        }
    }

    private static bool IsFieldSupported(FieldDeclarationSyntax field, ClassModel classModel)
    {
        if (field.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in field.Modifiers)
        {
            SyntaxKind Kind = Modifier.Kind();
            if (Kind != SyntaxKind.PrivateKeyword)
                return false;
        }

        if (!IsTypeSupported(field.Declaration.Type, out bool IsVoid))
            return false;

        VariableDeclarationSyntax Declaration = field.Declaration;

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
        {
            string Name = Variable.Identifier.ValueText;

            if (Variable.ArgumentList is BracketedArgumentListSyntax BracketedArgumentList && BracketedArgumentList.Arguments.Count > 0)
                return false;

            if (Variable.Initializer is not null)
                return false;

            Field NewField = new() { Name = new FieldName(Name) };
            classModel.FieldTable.Add(NewField.Name, NewField);
        }

        return true;
    }

    private static bool IsMethodSupported(MethodDeclarationSyntax method, ClassModel classModel)
    {
        if (method.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in method.Modifiers)
        {
            SyntaxKind Kind = Modifier.Kind();
            if (Kind != SyntaxKind.PrivateKeyword && Kind != SyntaxKind.PublicKeyword && Kind != SyntaxKind.InternalKeyword)
                return false;
        }

        if (!IsTypeSupported(method.ReturnType, out bool IsVoidReturn))
            return false;

        List<string> ParameterNameList = new();

        if (!AreAllMethodParametersSupported(method, ParameterNameList))
            return false;

        string Name = method.Identifier.ValueText;

        Dictionary<string, Parameter> ParameterTable = new();
        foreach (string ParameterName in ParameterNameList)
            ParameterTable.Add(ParameterName, new Parameter() { Name = ParameterName });

        Method NewMethod = new() { Name = new MethodName(Name), HasReturnValue = !IsVoidReturn, ParameterTable = ParameterTable };
        classModel.MethodTable.Add(NewMethod.Name, NewMethod);

        return true;
    }

    private static bool AreAllMethodParametersSupported(MethodDeclarationSyntax method, List<string> parameterNameList)
    {
        foreach (ParameterSyntax Parameter in method.ParameterList.Parameters)
            if (!IsParameterSupported(Parameter, parameterNameList))
                return false;

        return true;
    }

    private static bool IsParameterSupported(ParameterSyntax parameter, List<string> parameterNameList)
    {
        if (parameter.AttributeLists.Count > 0)
            return false;

        if (parameter.Modifiers.Count > 0)
            return false;

        if (!IsTypeSupported(parameter.Type, out _))
            return false;

        string Name = parameter.Identifier.ValueText;
        parameterNameList.Add(Name);

        return true;
    }

    private static bool IsTypeSupported(TypeSyntax? type, out bool isVoid)
    {
        isVoid = false;

        if (type is not PredefinedTypeSyntax PredefinedType)
            return false;

        SyntaxKind TypeKind = PredefinedType.Keyword.Kind();
        if (TypeKind != SyntaxKind.IntKeyword && TypeKind != SyntaxKind.VoidKeyword)
            return false;

        isVoid = TypeKind == SyntaxKind.VoidKeyword;
        return true;
    }
}
