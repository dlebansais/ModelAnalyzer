namespace DemoAnalyzer;

using System.Collections.Immutable;
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
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Check whether the class can be modeled
        if (IsClassIgnoredForModeling(classDeclaration))
            return;

        if (IsClassDeclarationSupported(classDeclaration) && AreAllMembersSupported(classDeclaration))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
    }

    private static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;

        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                string Comment = Trivia.ToFullString();
                if (Comment.StartsWith($"// {Modeling.None}"))
                    return true;
            }

        return false;
    }

    private static bool IsClassDeclarationSupported(ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in classDeclaration.Modifiers)
        {
            SyntaxKind Kind = Modifier.Kind();
            if (Kind != SyntaxKind.PrivateKeyword && Kind != SyntaxKind.PublicKeyword && Kind != SyntaxKind.PartialKeyword)
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

    private static bool AreAllMembersSupported(ClassDeclarationSyntax classDeclaration)
    {
        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (!IsMemberSupported(Member))
                return false;

        return true;
    }

    private static bool IsMemberSupported(MemberDeclarationSyntax member)
    {
        switch (member)
        {
            case FieldDeclarationSyntax AsField:
                return IsFieldSupported(AsField);
            case MethodDeclarationSyntax AsMethod:
                return IsMethodSupported(AsMethod);
            default:
                return false;
        }
    }

    private static bool IsFieldSupported(FieldDeclarationSyntax field)
    {
        if (field.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in field.Modifiers)
        {
            SyntaxKind Kind = Modifier.Kind();
            if (Kind != SyntaxKind.PrivateKeyword)
                return false;
        }

        if (!IsTypeSupported(field.Declaration.Type))
            return false;

        return true;
    }

    private static bool IsMethodSupported(MethodDeclarationSyntax method)
    {
        if (method.AttributeLists.Count > 0)
            return false;

        foreach (SyntaxToken Modifier in method.Modifiers)
        {
            SyntaxKind Kind = Modifier.Kind();
            if (Kind != SyntaxKind.PrivateKeyword && Kind != SyntaxKind.PublicKeyword)
                return false;
        }

        if (!IsTypeSupported(method.ReturnType))
            return false;

        if (!AreAllMethodParametersSupported(method))
            return false;

        return true;
    }

    private static bool AreAllMethodParametersSupported(MethodDeclarationSyntax method)
    {
        foreach (ParameterSyntax Parameter in method.ParameterList.Parameters)
            if (!IsParameterSupported(Parameter))
                return false;

        return true;
    }

    private static bool IsParameterSupported(ParameterSyntax parameter)
    {
        if (parameter.AttributeLists.Count > 0)
            return false;

        if (parameter.Modifiers.Count > 0)
            return false;

        if (!IsTypeSupported(parameter.Type))
            return false;

        return true;
    }

    private static bool IsTypeSupported(TypeSyntax type)
    {
        if (type is not PredefinedTypeSyntax AsPredefinedType)
            return false;

        SyntaxKind TypeKind = AsPredefinedType.Keyword.Kind();
        if (TypeKind != SyntaxKind.IntKeyword && TypeKind != SyntaxKind.VoidKeyword)
            return false;

        return true;
    }
}
