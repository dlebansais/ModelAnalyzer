using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace ModelAnalyzer.Test
{
    internal static class CSharpVerifierHelper
    {
        /// <summary>
        /// By default, the compiler reports diagnostics for nullable reference types at
        /// <see cref="DiagnosticSeverity.Warning"/>, and the analyzer test framework defaults to only validating
        /// diagnostics at <see cref="DiagnosticSeverity.Error"/>. This map contains all compiler diagnostic IDs
        /// related to nullability mapped to <see cref="ReportDiagnostic.Error"/>, which is then used to enable all
        /// of these warnings for default validation during analyzer and code fix tests.
        /// </summary>
        internal static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
            var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            // Workaround for https://github.com/dotnet/roslyn/issues/41610
            nullableWarnings = nullableWarnings
                .SetItem("CS8632", ReportDiagnostic.Error)
                .SetItem("CS8669", ReportDiagnostic.Error);

            return nullableWarnings;
        }

        internal static string ExtractDiagnosticId(string source, out string diagnosticId)
        {
            string FirstDiagnosticId = string.Empty;
            string ExtractedSource = source;
            string ExtractedDiagnosticId;

            do
            {
                ExtractedSource = ExtractDiagnosticIdOnce(ExtractedSource, out ExtractedDiagnosticId);

                if (ExtractedDiagnosticId != string.Empty)
                {
                    if (FirstDiagnosticId == string.Empty)
                        FirstDiagnosticId = ExtractedDiagnosticId;
                    else if (FirstDiagnosticId != ExtractedDiagnosticId)
                    {
                        diagnosticId = string.Empty;
                        return source;
                    }
                }
            }
            while (ExtractedDiagnosticId != string.Empty);

            diagnosticId = FirstDiagnosticId;
            return ExtractedSource;
        }

        private static string ExtractDiagnosticIdOnce(string source, out string diagnosticId)
        {
            string EndTagPattern = "|]";
            string StartRulePattern = "MA";
            string Pattern = EndTagPattern + StartRulePattern;
            const int RuleIdLength = 4;

            int PatternIndex = source.IndexOf(Pattern);

            if (PatternIndex >= 0 && PatternIndex + Pattern.Length + RuleIdLength <= source.Length)
            {
                diagnosticId = source.Substring(PatternIndex + EndTagPattern.Length, StartRulePattern.Length + RuleIdLength);
                source = source.Substring(0, PatternIndex + EndTagPattern.Length) + source.Substring(PatternIndex + Pattern.Length + RuleIdLength);
            }
            else
                diagnosticId = string.Empty;

            return source;
        }

        internal static bool TrySelectDiagnosticDescriptor(DiagnosticAnalyzer[] analyzers, string diagnosticId, out DiagnosticDescriptor diagnosticDescriptor)
        {
            if (diagnosticId != string.Empty)
            {
                foreach (var analyzer in analyzers)
                    foreach (var descriptor in analyzer.SupportedDiagnostics)
                        if (descriptor.Id == diagnosticId)
                        {
                            diagnosticDescriptor = descriptor;
                            return true;
                        }
            }

            diagnosticDescriptor = null;
            return false;
        }
    }
}
