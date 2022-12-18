namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AnalysisLogger;
using Libz3Extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public partial class ClassModelManager : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelManager"/> class.
    /// </summary>
    public ClassModelManager()
    {
        Extractor.Extract();
        Context = new SynchronizedVerificationContext();
        FromServerChannel = InitChannel();
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Gets or sets the thread start mode.
    /// </summary>
    public SynchronizedThreadStartMode StartMode { get; set; }

    /// <summary>
    /// Checks whether a class is ignored for modeling.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    public static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
    {
        string ClassName = classDeclaration.Identifier.ValueText;

        return ClassName == string.Empty || IsClassWithNoModelPrefix(classDeclaration);
    }

    private static bool IsClassWithNoModelPrefix(ClassDeclarationSyntax classDeclaration)
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

    /// <summary>
    /// Gets the model of a class.
    /// </summary>
    /// <param name="compilationContext">The compilation context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <exception cref="ArgumentException">Class name empty.</exception>
    public IClassModel GetClassModel(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration)
    {
        string ClassName = classDeclaration.Identifier.ValueText;
        if (ClassName == string.Empty)
            throw new ArgumentException("Class name must not be empty.");

        // We clear logs only after the constructor has exited.
        ClearLogs();

        Log($"Getting model for class '{ClassName}'.");

        return GetClassModelInternal(compilationContext, classDeclaration);
    }

    /// <summary>
    /// Gets the verified version of a class model synchronously.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public IClassModel GetVerifiedModel(IClassModel classModel)
    {
        return WaitForVerification(classModel);
    }

    /// <summary>
    /// Gets the verified version of a class model synchronously.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public async Task<IClassModel> GetVerifiedModelAsync(IClassModel classModel)
    {
        return await Task.Run(() =>
        {
            return WaitForVerification(classModel);
        });
    }

    /// <summary>
    /// Removes classes that no longer exist.
    /// </summary>
    /// <param name="existingClassList">The list of existing classes.</param>
    public void RemoveMissingClasses(List<string> existingClassList)
    {
        Log("Cleaning up classes that no longer exist.");

        lock (Context.Lock)
        {
            List<string> ToRemoveClassList = new();

            foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
            {
                string ClassName = Entry.Key;
                IClassModel ClassModel = Entry.Value;

                if (!existingClassList.Contains(ClassName))
                    ToRemoveClassList.Add(ClassName);
            }

            foreach (string ClassName in ToRemoveClassList)
            {
                Log($"Removing class '{ClassName}'.");

                Context.ClassModelTable.Remove(ClassName);
            }
        }
    }

    /// <summary>
    /// Waits for the verifier to finish processing data.
    /// </summary>
    public static async Task SynchronizeWithVerifierAsync()
    {
        await Task.Run(SynchronizeWithVerifier);
    }

    private ClassModel GetClassModelInternal(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration)
    {
        string ClassName = classDeclaration.Identifier.ValueText;
        Debug.Assert(ClassName != string.Empty);

        ClassModel NewClassModel;

        lock (Context.Lock)
        {
            Log($"Compilation context: {compilationContext}");

            // Compare this compilation context with the previous one. They will be different if their hash code is not the same, or if the new context is an asynchronous request.
            bool IsNewCompilationContext = !Context.LastCompilationContext.IsCompatibleWith(compilationContext);
            bool ClassModelAlreadyExists = Context.ClassModelTable.ContainsKey(ClassName);

            if (IsNewCompilationContext || !ClassModelAlreadyExists)
            {
                ClassDeclarationParser Parser = new(classDeclaration) { Logger = Logger };
                Parser.Parse();

                if (ClassModelAlreadyExists)
                {
                    ClassModel OldClassModel = Context.ClassModelTable[ClassName];

                    NewClassModel = OldClassModel with
                    {
                        FieldTable = Parser.FieldTable,
                        MethodTable = Parser.MethodTable,
                        InvariantList = Parser.InvariantList,
                        Unsupported = Parser.Unsupported,
                        IsVerified = false,
                    };

                    Context.UpdateClassModel(NewClassModel);

                    Log($"Updated model for class '{ClassName}'.");
                }
                else
                {
                    NewClassModel = new ClassModel()
                    {
                        Name = ClassName,
                        FieldTable = Parser.FieldTable,
                        MethodTable = Parser.MethodTable,
                        InvariantList = Parser.InvariantList,
                        Unsupported = Parser.Unsupported,
                        IsVerified = false,
                        IsInvariantViolated = false,
                    };

                    Context.AddClassModel(NewClassModel);

                    Log($"Class model for '{ClassName}' is new and has been added.");
                }

                if (StartMode == SynchronizedThreadStartMode.Auto)
                    ScheduleAsynchronousVerification();
            }
            else
                NewClassModel = Context.ClassModelTable[ClassName];

            Context.LastCompilationContext = compilationContext;
        }

        return NewClassModel;
    }

    private static void SynchronizeWithVerifier()
    {
        while (VerificationRequestCount > 0 && VerifierCallWatch.Elapsed < TimeSpan.FromSeconds(5))
            Thread.Sleep(100);

        Interlocked.Exchange(ref VerificationRequestCount, 0);
        VerifierCallWatch.Stop();
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void ClearLogs()
    {
        lock (Context.Lock)
        {
            if (Context.ClassModelTable.Count == 0)
                Logger.Clear();
        }
    }

    private SynchronizedVerificationContext Context;
}
