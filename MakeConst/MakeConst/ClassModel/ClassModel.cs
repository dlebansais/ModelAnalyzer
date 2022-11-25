namespace DemoAnalyzer;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

public record ClassModel
{
    public required string Name { get; init; }
    public required bool IsSupported { get; init; }
    public required Dictionary<FieldName, IField> FieldTable { get; init; }
    public required Dictionary<MethodName, IMethod> MethodTable { get; init; }
    public required List<IInvariant> InvariantList { get; init; }

    public static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
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

    public static ClassModel FromClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        string Name = classDeclaration.Identifier.ValueText;
        bool IsSupported = false;
        Dictionary<FieldName, IField> FieldTable = new();
        Dictionary<MethodName, IMethod> MethodTable = new();
        List<IInvariant> InvariantList = new();

        if (Name != string.Empty && IsClassDeclarationSupported(classDeclaration))
        {
            IsSupported = true;
            CheckUnsupportedMembers(classDeclaration, ref IsSupported);
            FieldTable = ParseFields(classDeclaration, ref IsSupported);
            MethodTable = ParseMethods(classDeclaration, ref IsSupported);
            InvariantList = ParseInvariants(classDeclaration, ref IsSupported);
        }

        return new ClassModel
        {
            Name = Name,
            IsSupported = IsSupported,
            FieldTable = FieldTable,
            MethodTable = MethodTable,
            InvariantList = InvariantList,
        };
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

    private static List<IInvariant> ParseInvariants(ClassDeclarationSyntax classDeclaration, ref bool isSupported)
    {
        List<IInvariant> InvariantList = new();

        SyntaxToken LastToken = classDeclaration.GetLastToken();
        var Location = LastToken.GetLocation();

        if (LastToken.HasTrailingTrivia)
        {
            SyntaxTriviaList TrailingTrivia = LastToken.TrailingTrivia;
            AddInvariantsInTrivia(InvariantList, TrailingTrivia, ref isSupported);
            Location = TrailingTrivia.Last().GetLocation();
        }

        var NextToken = classDeclaration.SyntaxTree.GetRoot().FindToken(Location.SourceSpan.End);

        if (NextToken.HasLeadingTrivia)
            AddInvariantsInTrivia(InvariantList, NextToken.LeadingTrivia, ref isSupported);

        return InvariantList;
    }

    private static void AddInvariantsInTrivia(List<IInvariant> invariantList, SyntaxTriviaList triviaList, ref bool isSupported)
    {
        foreach (SyntaxTrivia Trivia in triviaList)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                string Pattern = $"// {Modeling.Invariant}";

                if (Comment.StartsWith(Pattern))
                {
                    Location Location = Trivia.GetLocation();
                    UnsupportedInvariant NewInvariant = new() { Text = Comment.Substring(Pattern.Length), Location = Location };
                    invariantList.Add(NewInvariant);
                    isSupported = false;
                }
            }
    }

    private static void CheckUnsupportedMembers(ClassDeclarationSyntax classDeclaration, ref bool isSupported)
    {
        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is not FieldDeclarationSyntax && Member is not MethodDeclarationSyntax)
            {
                isSupported = false;
                break;
            }
    }

    private static Dictionary<FieldName, IField> ParseFields(ClassDeclarationSyntax classDeclaration, ref bool isSupported)
    {
        Dictionary<FieldName, IField> FieldTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is FieldDeclarationSyntax FieldDeclaration)
                AddField(FieldDeclaration, FieldTable, ref isSupported);
    
        return FieldTable;
    }

    private static void AddField(FieldDeclarationSyntax fieldDeclaration, Dictionary<FieldName, IField> fieldTable, ref bool isSupported)
    {
        VariableDeclarationSyntax Declaration = fieldDeclaration.Declaration;

        bool IsFieldSupported = (fieldDeclaration.AttributeLists.Count == 0 &&
                                 fieldDeclaration.Modifiers.All(modifier => modifier.IsKind(SyntaxKind.PrivateKeyword)) &&
                                 IsTypeSupported(Declaration.Type, out _));

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
        {
            FieldName Name = new(Variable.Identifier.ValueText);
            IField NewField;

            if (IsFieldSupported &&
                (Variable.ArgumentList is not BracketedArgumentListSyntax BracketedArgumentList || BracketedArgumentList.Arguments.Count == 0) &&
                Variable.Initializer is null)
            {
                NewField = new Field { Name = Name };
            }
            else
            {
                Location Location = Variable.Identifier.GetLocation();
                NewField = new UnsupportedField { Name = Name, Location = Location };
                isSupported = false;
            }

            fieldTable.Add(NewField.Name, NewField);
        }
    }

    private static Dictionary<MethodName, IMethod> ParseMethods(ClassDeclarationSyntax classDeclaration, ref bool isSupported)
    {
        Dictionary<MethodName, IMethod> MethodTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is MethodDeclarationSyntax MethodDeclaration)
                AddMethod(MethodDeclaration, MethodTable, ref isSupported);

        return MethodTable;
    }

    private static void AddMethod(MethodDeclarationSyntax methodDeclaration, Dictionary<MethodName, IMethod> methodTable, ref bool isSupported)
    {
        bool IsMethodSupported = (IsTypeSupported(methodDeclaration.ReturnType, out bool IsVoidReturn) &&
                                  methodDeclaration.AttributeLists.Count == 0 &&
                                  methodDeclaration.Modifiers.All(modifier => modifier.IsKind(SyntaxKind.PrivateKeyword) || modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword)));

        MethodName Name = new(methodDeclaration.Identifier.ValueText);
        List<string> ParameterNameList = new();
        IMethod NewMethod;

        if (AreAllMethodParametersSupported(methodDeclaration, ParameterNameList))
        {
            Dictionary<string, Parameter> ParameterTable = new();
            foreach (string ParameterName in ParameterNameList)
                ParameterTable.Add(ParameterName, new Parameter() { Name = ParameterName });

            NewMethod = new Method { Name = Name, HasReturnValue = !IsVoidReturn, ParameterTable = ParameterTable };
        }
        else
        {
            Location Location = methodDeclaration.Identifier.GetLocation();
            NewMethod = new UnsupportedMethod { Name = Name, Location = Location };
            isSupported = false;
        }

        methodTable.Add(NewMethod.Name, NewMethod);
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

    public void Verify()
    {
        bool IsInvariantViolated = false;

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
            if (Entry.Key.Name == "XYZ")
            {
                IsInvariantViolated = true;
                break;
            }

        ClassModelManager.Instance.SetIsInvariantViolated(Name, IsInvariantViolated);
    }

    public override string ToString()
    {
        string Result = @$"{Name}
";

        foreach (KeyValuePair<FieldName, IField> FieldEntry in FieldTable)
            if (FieldEntry.Value is Field Field)
                Result += @$"  int {Field.Name.Name}
";

        foreach (KeyValuePair<MethodName, IMethod> MethodEntry in MethodTable)
            if (MethodEntry.Value is Method Method)
            {
                string Parameters = string.Empty;

                foreach (KeyValuePair<string, Parameter> ParameterEntry in Method.ParameterTable)
                {
                    if (Parameters.Length > 0)
                        Parameters += ", ";

                    Parameters += ParameterEntry.Key;
                }

                string ReturnString = Method.HasReturnValue ? "int" : "void";
                Result += @$"  {ReturnString} {Method.Name.Name}({Parameters})
";

                foreach (Invariant Invariant in InvariantList)
                    Result += @$"  * {Invariant.FieldName} {Invariant.Operator} {Invariant.Constant}
";
            }

        return Result;
    }
}
