namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Threading;
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
    required public IAnalysisLogger Logger { get; init; }

    private Dictionary<string, ModelVerification> ModelVerificationTable = new();
    private int LastHashCode;
    private Dictionary<string, bool> ViolationTable = new();
    private Thread? ModelThread = null;
    private bool ThreadShouldBeRestarted;

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
                        Logger = Logger,
                        FieldTable = ClassModel.FieldTable,
                        MethodTable = ClassModel.MethodTable,
                        InvariantList = ClassModel.InvariantList,
                    };

                    Verifier.Verify();

                    SetIsInvariantViolated(ClassName, Verifier.IsInvariantViolated);
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

    /// <summary>
    /// Checks whether the invariant of a class is violated.
    /// </summary>
    /// <param name="name">The class name.</param>
    public bool IsInvariantViolated(string name)
    {
        lock (ViolationTable)
        {
            return ViolationTable.ContainsKey(name) && ViolationTable[name] == true;
        }
    }

    /// <summary>
    /// Sets whether the invariant of a class is violated.
    /// </summary>
    /// <param name="name">The class name.</param>
    /// <param name="isInvariantViolated">Whether the invariant is violated.</param>
    private void SetIsInvariantViolated(string name, bool isInvariantViolated)
    {
        lock (ViolationTable)
        {
            if (ViolationTable.ContainsKey(name))
                ViolationTable[name] = isInvariantViolated;
        }
    }

    /// <summary>
    /// Gets the model of a class.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="classDeclaration">The class declaration.</param>
    public IModelVerification GetClassModel(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        int HashCode = context.Compilation.GetHashCode();
        ModelVerification Result;
        bool IsVerifyingAsynchronously = false;
        string ClassName = classDeclaration.Identifier.ValueText;

        lock (ModelVerificationTable)
        {
            if (ClassName == string.Empty || LastHashCode != HashCode || !ModelVerificationTable.ContainsKey(ClassName))
            {
                ClassDeclarationParser Parser = new(classDeclaration) { Logger = Logger };

                IClassModel NewClassModel = new ClassModel()
                {
                    Name = ClassName,
                    Manager = this,
                    Logger = Logger,
                    FieldTable = Parser.FieldTable,
                    MethodTable = Parser.MethodTable,
                    InvariantList = Parser.InvariantList,
                    Unsupported = Parser.Unsupported,
                };

                Result = new ModelVerification() { ClassModel = NewClassModel };

                if (ClassName != string.Empty)
                {
                    UpdateClassModel(Result);

                    if (LastHashCode != HashCode)
                    {
                        LastHashCode = HashCode;
                        ScheduleThreadStart();
                        IsVerifyingAsynchronously = true;
                    }
                }

                if (!IsVerifyingAsynchronously)
                    Result.SetUpToDate();
            }
            else
                Result = ModelVerificationTable[ClassName];
        }

        return Result;
    }

    /// <summary>
    /// Updates a class model.
    /// </summary>
    /// <param name="modelVerification">The model verification.</param>
    private void UpdateClassModel(ModelVerification modelVerification)
    {
        string ClassName = modelVerification.ClassModel.Name;

        if (!ModelVerificationTable.ContainsKey(ClassName))
        {
            if (ModelVerificationTable.Count == 0)
                ClearLogs();

            ModelVerificationTable.Add(ClassName, modelVerification);
            ViolationTable.Add(ClassName, false);
        }
        else
        {
            ModelVerificationTable[ClassName] = modelVerification;
        }
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
            {
                ModelVerificationTable.Remove(Key);
                ViolationTable.Remove(Key);
            }
        }
    }

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

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void ClearLogs()
    {
    }
}
