namespace ModelAnalyzer;

using Microsoft.CodeAnalysis.Diagnostics;

public static class CompilationContextHelper
{
    public static CompilationContext ToCompilationContext(SyntaxNodeAnalysisContext context)
    {
        return new CompilationContext(context.Compilation.GetHashCode());
    }
}
