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
        GetClassModelInternal(context, classDeclaration, out ModelVerification ModelVerification, out bool IsVerifyingAsynchronously);

        if (IsVerifyingAsynchronously && waitIfAsync)
            ModelVerification.WaitForUpToDate(Timeout.InfiniteTimeSpan);

        return ModelVerification.ClassModel;
    }

    /// <summary>
    /// Gets the model of a class.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    public async Task<IClassModel> GetClassModelAsync(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        GetClassModelInternal(context, classDeclaration, out ModelVerification ModelVerification, out bool IsVerifyingAsynchronously);

        if (IsVerifyingAsynchronously)
        {
            return await Task.Run(() =>
            {
                ModelVerification.WaitForUpToDate(Timeout.InfiniteTimeSpan);
                return ModelVerification.ClassModel;
            });
        }
        else
            return ModelVerification.ClassModel;
    }

    /// <summary>
    /// Removes classes that no longer exist.
    /// </summary>
    /// <param name="existingClassList">The list of existing classes.</param>
    public void RemoveMissingClasses(List<string> existingClassList)
    {
        lock (ModelVerificationTable)
        {
            List<string> ToRemoveClassList = new();

            foreach (KeyValuePair<string, ModelVerification> Entry in ModelVerificationTable)
                if (!existingClassList.Contains(Entry.Key))
                    ToRemoveClassList.Add(Entry.Key);

            foreach (string Key in ToRemoveClassList)
                ModelVerificationTable.Remove(Key);
        }
    }

    private void GetClassModelInternal(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, out ModelVerification modelVerification, out bool isVerifyingAsynchronously)
    {
        isVerifyingAsynchronously = false;

        int HashCode = context.Compilation.GetHashCode();
        bool IsNewCompilationContext = LastHashCode != HashCode;
        LastHashCode = HashCode;

        string ClassName = classDeclaration.Identifier.ValueText;

        lock (ModelVerificationTable)
        {
            if (ClassName == string.Empty || IsNewCompilationContext || !ModelVerificationTable.ContainsKey(ClassName))
            {
                ClassDeclarationParser Parser = new(classDeclaration) { Logger = Logger };

                IClassModel NewClassModel = new ClassModel()
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
                    UpdateClassModel(modelVerification);

                    if (IsNewCompilationContext)
                    {
                        ScheduleThreadStart();
                        isVerifyingAsynchronously = true;
                    }
                }
            }
            else
                modelVerification = ModelVerificationTable[ClassName];
        }
    }

    private void UpdateClassModel(ModelVerification modelVerification)
    {
        string ClassName = modelVerification.ClassModel.Name;

        if (!ModelVerificationTable.ContainsKey(ClassName))
        {
            if (ModelVerificationTable.Count == 0)
                ClearLogs();

            ModelVerificationTable.Add(ClassName, modelVerification);
        }
        else
        {
            ModelVerificationTable[ClassName] = modelVerification;
        }
    }

    private void ScheduleThreadStart()
    {
        lock (ModelVerificationTable)
        {
            if (ModelThread is null)
                StartThread();
            else
                ThreadShouldBeRestarted = true;
        }
    }

    private void StartThread()
    {
        Log("StartThread()");

        ThreadShouldBeRestarted = false;
        ModelThread = new Thread(new ThreadStart(ExecuteThread));
        ModelThread.Start();
    }

    private void ExecuteThread()
    {
        try
        {
            Dictionary<ModelVerification, ClassModel> CloneTable = new();

            lock (ModelVerificationTable)
            {
                foreach (KeyValuePair<string, ModelVerification> Entry in ModelVerificationTable)
                {
                    ModelVerification ModelVerification = Entry.Value;
                    ClassModel Original = (ClassModel)ModelVerification.ClassModel;
                    ClassModel Clone = Original with { };

                    CloneTable.Add(ModelVerification, Clone);
                }
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

            Thread.Sleep(TimeSpan.FromSeconds(1));

            bool Restart = false;

            lock (ModelVerificationTable)
            {
                ModelThread = null;

                if (ThreadShouldBeRestarted)
                    Restart = true;
            }

            if (Restart)
                StartThread();
        }
        catch (Exception exception)
        {
            Logger.LogException(exception);
        }
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void ClearLogs()
    {
    }

    private Dictionary<string, ModelVerification> ModelVerificationTable = new();
    private int LastHashCode;
    private Thread? ModelThread = null;
    private bool ThreadShouldBeRestarted;
}
