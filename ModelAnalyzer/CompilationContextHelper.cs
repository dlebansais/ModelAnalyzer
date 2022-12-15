namespace ModelAnalyzer;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class CompilationContextHelper
{
    public static CompilationContext ToCompilationContext(string diagnosticId, ClassDeclarationSyntax classDeclaration, bool isAsyncRunRequested)
    {
        return new CompilationContext(diagnosticId, classDeclaration, isAsyncRunRequested);
    }
}
