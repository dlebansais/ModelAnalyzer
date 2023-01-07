namespace ModelAnalyzer;

using Microsoft.CodeAnalysis;

public static class CompilationContextHelper
{
    public static CompilationContext ToCompilationContext(SyntaxNode syntaxNode, bool isAsyncRunRequested)
    {
        return new CompilationContext(syntaxNode, isAsyncRunRequested);
    }
}
