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
    private const int MaxDepth = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelManager"/> class.
    /// </summary>
    public ClassModelManager()
    {
        Context = new SynchronizedVerificationContext();
        SynchronizedThread = new(Context, ExecuteVerification);
        VerificationThread = InitThread();
        FromServerChannel = InitChannel();

        // StartThread();
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

        GetClassModelInternal(compilationContext, classDeclaration, out IClassModel ClassModel, out _, out bool IsVerifyingAsynchronously);

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

        GetClassModelInternal(compilationContext, classDeclaration, out IClassModel ClassModel, out _, out bool IsVerifyingAsynchronously);

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

    /// <summary>
    /// Starts verifying classes. Only needed if <see cref="StartMode"/> is <see cref="SynchronizedThreadStartMode.Auto"/>.
    /// </summary>
    public void StartVerification()
    {
        Log("Starting the verification thread.");
        SynchronizedThread.Start();
    }

    private void GetClassModelInternal(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration, out IClassModel classModel, out ModelVerification modelVerification, out bool isVerifyingAsynchronously)
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
            ModelVerification? ExistingModelVerification = Context.FindByName(ClassName);

            if (ClassModelMustBeUpdated || ExistingModelVerification is null)
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

                modelVerification = new() { ClassModel = NewClassModel };
                classModel = NewClassModel;

                Context.UpdateClassModel(NewClassModel, out bool IsAdded);
                Log(IsAdded ? $"Class model for '{ClassName}' is new and has been added." : $"Updated model for class '{ClassName}'.");

                Context.VerificationList.Add(modelVerification);

                if (StartMode == SynchronizedThreadStartMode.Auto)
                {
                    Log("Starting the verification thread.");
                    SynchronizedThread.Start();
                }

                ScheduleAsynchronousVerification();

                isVerifyingAsynchronously = true;
                Context.LastCompilationContext = compilationContext with { IsAsyncRunRequested = true };
            }
            else
            {
                modelVerification = ExistingModelVerification;
                classModel = Context.ClassModelTable[ClassName];
                Context.LastCompilationContext = compilationContext;
            }
        }
    }

    private void ExecuteVerification(IDictionary<ModelVerification, ClassModel> cloneTable)
    {
        Log("Executing verification started.");

        foreach (KeyValuePair<ModelVerification, ClassModel> Entry in cloneTable)
        {
            ModelVerification ModelVerification = Entry.Key;
            ClassModel ClassModel = Entry.Value;
            string ClassName = ClassModel.Name;

            if (!ClassModel.Unsupported.IsEmpty)
                Log($"Skipping complete verification for class '{ClassName}', it has unsupported elements.");
            else
            {
                using Verifier Verifier = new()
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

#if DEBUG
        // Simulate an analysis that takes time.
        Thread.Sleep(TimeSpan.FromSeconds(1));
#endif

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
