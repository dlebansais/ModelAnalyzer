namespace DemoAnalyzer;

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
            ModelVerification.WaitForUpToDate(Timeout.InfiniteTimeSpan);
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
                ModelVerification.WaitForUpToDate(Timeout.InfiniteTimeSpan);

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
            ModelVerification? ExistingModelVerification = FindInQueue(ClassName);

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
                    UpdateClassModel(NewClassModel);

                    if (IsNewCompilationContext)
                    {
                        ScheduleThreadStart();
                        isVerifyingAsynchronously = true;
                    }
                }
            }
            else
                modelVerification = ExistingModelVerification;
        }
    }

    private ModelVerification? FindInQueue(string className)
    {
        foreach (ModelVerification Item in Context.VerificationList)
            if (Item.ClassModel.Name == className)
                return Item;

        return null;
    }

    private void UpdateClassModel(ClassModel classModel)
    {
        string ClassName = classModel.Name;

        if (!Context.ClassModelTable.ContainsKey(ClassName))
        {
            Log($"Class '{ClassName}' is new, adding the class model.");

            Context.ClassModelTable.Add(ClassName, classModel);
        }
        else
        {
            Log($"Updating model for class '{ClassName}'.");

            Context.ClassModelTable[ClassName] = classModel;
        }
    }

    private void ScheduleThreadStart()
    {
        lock (Context.Lock)
        {
            if (ModelThread is null)
            {
                Log($"Starting the verification thread.");

                ThreadShouldBeRestarted = false;
                ModelThread = new Thread(new ThreadStart(ExecuteThread));
                ModelThread.Start();
            }
            else
                ThreadShouldBeRestarted = true;
        }
    }

    private void ExecuteThread()
    {
        Log("Verification thread entered.");

        try
        {
            Dictionary<ModelVerification, ClassModel> CloneTable = new();

            lock (Context.Lock)
            {
                foreach (ModelVerification ModelVerification in Context.VerificationList)
                {
                    ClassModel Original = (ClassModel)ModelVerification.ClassModel;
                    ClassModel Clone = Original with { };

                    Log($"*** Class {Original.Name} cloned.");
                    CloneTable.Add(ModelVerification, Clone);
                }

                Context.VerificationList.Clear();
            }

            foreach (KeyValuePair<ModelVerification, ClassModel> Entry in CloneTable)
            {
                ModelVerification ModelVerification = Entry.Key;
                ClassModel ClassModel = Entry.Value;
                string ClassName = ClassModel.Name;

                if (ClassModel.Unsupported.IsEmpty)
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

                ModelVerification.SetUpToDate();
            }

            // Simulate an analysis that takes time.
            Thread.Sleep(TimeSpan.FromSeconds(1));

            bool Restart = false;

            lock (Context.Lock)
            {
                ModelThread = null;

                if (ThreadShouldBeRestarted)
                    Restart = true;
            }

            if (Restart)
            {
                Log($"Restarting the verification thread.");

                ThreadShouldBeRestarted = false;
                ModelThread = new Thread(new ThreadStart(ExecuteThread));
                ModelThread.Start();
            }
        }
        catch (Exception exception)
        {
            Logger.LogException(exception);
        }

        Log("Verification thread exited.");
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

    private SynchronizedVerificationContext Context = new();
    private Thread? ModelThread = null;
    private bool ThreadShouldBeRestarted;
}
