namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AnalysisLogger;
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
    /// <param name="waitIfAsync">True if the method should wait until verification is completed.</param>
    /// <exception cref="ArgumentException">Class name empty.</exception>
    public IClassModel GetClassModel(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration, bool waitIfAsync = false)
    {
        ClearLogs();

        string ClassName = classDeclaration.Identifier.ValueText;
        if (ClassName == string.Empty)
            throw new ArgumentException("Class name must not be empty.");

        Log($"Getting model for class '{ClassName}', {(waitIfAsync ? "waiting for a delayed analysis" : "not waiting for delayed analysis")}.");

        GetClassModelInternal(compilationContext, classDeclaration, out IClassModel ClassModel, out bool IsVerifyingAsynchronously);

        if (IsVerifyingAsynchronously && waitIfAsync)
        {
            Log("Waiting for the delayed analysis.");

            ClassModel.InvariantViolationVerified.WaitOne(Timeout.InfiniteTimeSpan);

            Log("Analysis complete.");

            return ClassModel;
        }
        else
        {
            Log("Synchronous analysis complete.");

            return ClassModel;
        }
    }

    /// <summary>
    /// Gets the model of a class.
    /// </summary>
    /// <param name="compilationContext">The compilation context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <exception cref="ArgumentException">Class name empty.</exception>
    public async Task<IClassModel> GetClassModelAsync(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration)
    {
        ClearLogs();

        string ClassName = classDeclaration.Identifier.ValueText;
        if (ClassName == string.Empty)
            throw new ArgumentException("Class name must not be empty.");

        Log($"Getting model for class '{ClassName}' asynchronously.");

        GetClassModelInternal(compilationContext, classDeclaration, out IClassModel ClassModel, out bool IsVerifyingAsynchronously);

        if (IsVerifyingAsynchronously)
        {
            Log("Waiting for the delayed analysis.");

            return await Task.Run(() =>
            {
                ClassModel.InvariantViolationVerified.WaitOne(Timeout.InfiniteTimeSpan);

                Log($"Analysis of '{ClassName}' complete.");

                return ClassModel;
            });
        }
        else
        {
            Log($"Analysis of '{ClassName}' complete.");

            return ClassModel;
        }
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
                    if (!ClassModel.Unsupported.IsEmpty || ClassModel.InvariantViolationVerified.WaitOne(0))
                        ToRemoveClassList.Add(ClassName);
            }

            foreach (string ClassName in ToRemoveClassList)
            {
                Log($"Removing class '{ClassName}'.");

                Context.ClassModelTable.Remove(ClassName);
                Context.ClassNameWithInvariantViolation.Remove(ClassName);
            }
        }
    }

    private void GetClassModelInternal(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration, out IClassModel classModel, out bool isVerifyingAsynchronously)
    {
        isVerifyingAsynchronously = false;

        string ClassName = classDeclaration.Identifier.ValueText;
        Debug.Assert(ClassName != string.Empty);

        lock (Context.Lock)
        {
            Log($"Compilation Context: {compilationContext}");

            // Compare this compilation context with the previous one. They will be different if their hash code is not the same, or if the new context is an asynchronous request.
            bool IsNewCompilationContext = !Context.LastCompilationContext.IsCompatibleWith(compilationContext);
            bool ClassModelMustBeUpdated = IsNewCompilationContext || !Context.ClassModelTable.ContainsKey(ClassName);

            if (ClassModelMustBeUpdated)
            {
                ClassDeclarationParser Parser = new(classDeclaration) { Logger = Logger };
                Parser.Parse();

                ClassModel NewClassModel = new ClassModel()
                {
                    Name = ClassName,
                    FieldTable = Parser.FieldTable,
                    MethodTable = Parser.MethodTable,
                    InvariantList = Parser.InvariantList,
                    Unsupported = Parser.Unsupported,
                };

                classModel = NewClassModel;

                Context.UpdateClassModel(NewClassModel, out bool IsAdded);
                Log(IsAdded ? $"Class model for '{ClassName}' is new and has been added." : $"Updated model for class '{ClassName}'.");

                ScheduleAsynchronousVerification();

                if (StartMode == SynchronizedThreadStartMode.Auto)
                {
                    Log("Starting the verification thread.");
                    StartVerification();
                }

                isVerifyingAsynchronously = true;
                Context.LastCompilationContext = compilationContext with { IsAsyncRunRequested = true };
            }
            else
            {
                classModel = Context.ClassModelTable[ClassName];
                Context.LastCompilationContext = compilationContext;
            }
        }
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

    /// <summary>
    /// Checks whether the invariant of a class is violated.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public bool IsInvariantViolated(IClassModel classModel)
    {
        return Context.GetIsInvariantViolated(classModel.Name);
    }

    private SynchronizedVerificationContext Context;
}
