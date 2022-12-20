namespace ModelAnalyzer;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class CompilationContextHelper
{
    public static CompilationContext ToCompilationContext(ClassDeclarationSyntax classDeclaration, bool isAsyncRunRequested)
    {
        return new CompilationContext(classDeclaration, isAsyncRunRequested);
    }
}
