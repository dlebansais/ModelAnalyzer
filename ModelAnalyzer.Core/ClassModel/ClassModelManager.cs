namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnalysisLogger;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public class ClassModelManager
{
    private const int MaxDepth = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelManager"/> class.
    /// </summary>
    public ClassModelManager()
    {
        Context = new SynchronizedVerificationContext();
        SynchronizedThread = new(Context, ExecuteVerification);
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Checks whether a class is ignored for modeling.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
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

    /// <summary>
    /// Gets the model of a class.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="waitIfAsync">True if the method should wait until verification is completed.</param>
    public IClassModel GetClassModel(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, bool waitIfAsync = false)
    {
        ClearLogs();

        string ClassName = classDeclaration.Identifier.ValueText;
        Log($"Getting model for class '{ClassName}', {(waitIfAsync ? "waiting for a delayed analysis" : "not waiting for delayed analysis")}.");

        GetClassModelInternal(context, classDeclaration, out ModelVerification ModelVerification, out bool IsVerifyingAsynchronously);

        if (IsVerifyingAsynchronously && waitIfAsync)
        {
            Log("Waiting for the delayed analysis.");
            ModelVerification.WaitForUpToDate(Timeout.InfiniteTimeSpan, out _);
        }

        Log("Analysis complete.");

        return ModelVerification.ClassModel;
    }

    /// <summary>
    /// Gets the model of a class.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    public async Task<IClassModel> GetClassModelAsync(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        ClearLogs();

        string ClassName = classDeclaration.Identifier.ValueText;
        Log($"Getting model for class '{ClassName}' asynchronously.");

        GetClassModelInternal(context, classDeclaration, out ModelVerification ModelVerification, out bool IsVerifyingAsynchronously);

        if (IsVerifyingAsynchronously)
        {
            Log("Waiting for the delayed analysis.");

            return await Task.Run(() =>
            {
                ModelVerification.WaitForUpToDate(Timeout.InfiniteTimeSpan, out _);

                Log($"Analysis of '{ClassName}' complete.");

                return ModelVerification.ClassModel;
            });
        }
        else
        {
            Log($"Analysis of '{ClassName}' complete.");

            return ModelVerification.ClassModel;
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
            List<IClassModel> VerifiedClassList = new();
            List<string> ToRemoveClassList = new();

            // Don't remove classes still being analyzed yet.
            foreach (ModelVerification ModelVerification in Context.VerificationList)
                VerifiedClassList.Add(ModelVerification.ClassModel);

            foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
            {
                string ClassName = Entry.Key;
                IClassModel ClassModel = Entry.Value;

                if (!existingClassList.Contains(ClassName) && VerifiedClassList.Contains(ClassModel))
                    ToRemoveClassList.Add(ClassName);
            }

            foreach (string ClassName in ToRemoveClassList)
            {
                Log($"Removing class '{ClassName}'.");

                Context.ClassModelTable.Remove(ClassName);
            }
        }
    }

    private void GetClassModelInternal(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, out ModelVerification modelVerification, out bool isVerifyingAsynchronously)
    {
        isVerifyingAsynchronously = false;

        string ClassName = classDeclaration.Identifier.ValueText;

        lock (Context.Lock)
        {
            int HashCode = context.Compilation.GetHashCode();
            bool IsNewCompilationContext = Context.LastHashCode != HashCode;
            Context.LastHashCode = HashCode;

            bool ClassModelMustBeUpdated = ClassName == string.Empty || IsNewCompilationContext || !Context.ClassModelTable.ContainsKey(ClassName);
            ModelVerification? ExistingModelVerification = Context.FindByName(ClassName);

            if (ClassModelMustBeUpdated || ExistingModelVerification is null)
            {
                ClassDeclarationParser Parser = new(classDeclaration) { Logger = Logger };

                ClassModel NewClassModel = new ClassModel()
                {
                    Name = ClassName,
                    Manager = this,
                    FieldTable = Parser.FieldTable,
                    MethodTable = Parser.MethodTable,
                    InvariantList = Parser.InvariantList,
                    Unsupported = Parser.Unsupported,
                };

                modelVerification = new() { ClassModel = NewClassModel };

                if (ClassName != string.Empty)
                {
                    Context.UpdateClassModel(NewClassModel, out bool IsAdded);
                    Log(IsAdded ? $"Class model for '{ClassName}' is new and has been added." : $"Updated model for class '{ClassName}'.");

                    if (IsNewCompilationContext)
                    {
                        Context.VerificationList.Add(modelVerification);

                        Log("Starting the verification thread.");
                        SynchronizedThread.Start();
                        isVerifyingAsynchronously = true;
                    }
                }
            }
            else
                modelVerification = ExistingModelVerification;
        }
    }

    private void ExecuteVerification(IDictionary<ModelVerification, ClassModel> cloneTable)
    {
        Log("Executing verification started.");

        List<ClassModel> VerifiedClassModelList = new();

        foreach (KeyValuePair<ModelVerification, ClassModel> Entry in cloneTable)
        {
            ModelVerification ModelVerification = Entry.Key;
            ClassModel ClassModel = Entry.Value;
            string ClassName = ClassModel.Name;

            if (VerifiedClassModelList.Contains(ClassModel))
                Log($"Not redoing verification for class '{ClassName}'.");
            else
            {
                VerifiedClassModelList.Add(ClassModel);

                if (!ClassModel.Unsupported.IsEmpty)
                    Log($"Skipping complete verification for class '{ClassName}', it has unsupported elements.");
                else
                {
                    Verifier Verifier = new()
                    {
                        MaxDepth = MaxDepth,
                        ClassName = ClassName,
                        FieldTable = ClassModel.FieldTable,
                        MethodTable = ClassModel.MethodTable,
                        InvariantList = ClassModel.InvariantList,
                        Logger = Logger,
                    };

                    Verifier.Verify();

                    ((ClassModel)ModelVerification.ClassModel).IsInvariantViolated = Verifier.IsInvariantViolated;
                }
            }

            ModelVerification.SetUpToDate();
        }

        // Simulate an analysis that takes time.
        Thread.Sleep(TimeSpan.FromSeconds(1));

        Log("Executing verification completed.");
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
    private SynchronizedThread<ModelVerification, ClassModel> SynchronizedThread;
}
