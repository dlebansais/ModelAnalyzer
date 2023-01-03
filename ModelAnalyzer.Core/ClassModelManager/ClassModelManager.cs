namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AnalysisLogger;
using FileExtractor;
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
        : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelManager"/> class.
    /// </summary>
    /// <param name="receiveChannelGuid">The guid of the channel receiving ack from the verifier.</param>
    public ClassModelManager(Guid receiveChannelGuid)
    {
        ReceiveChannelGuid = receiveChannelGuid;

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
    public VerificationProcessStartMode StartMode { get; set; }

    /// <summary>
    /// Gets the guid of the channel receiving ack from the verifier.
    /// </summary>
    public Guid ReceiveChannelGuid { get; }

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

            foreach (KeyValuePair<string, VerificationState> Entry in Context.ClassModelTable)
            {
                string ClassName = Entry.Key;

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

    private ClassModel GetClassModelInternal(CompilationContext compilationContext, ClassDeclarationSyntax classDeclaration)
    {
        string ClassName = classDeclaration.Identifier.ValueText;
        Debug.Assert(ClassName != string.Empty);

        VerificationState NewVerificationState;

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
                    VerificationState OldVerificationState = Context.ClassModelTable[ClassName];
                    ClassModelExchange OldClassModelExchange = OldVerificationState.ClassModelExchange;
                    ClassModel OldClassModel = OldClassModelExchange.ClassModel;

                    ClassModel NewClassModel = OldClassModel with
                    {
                        PropertyTable = Parser.PropertyTable,
                        FieldTable = Parser.FieldTable,
                        MethodTable = Parser.MethodTable,
                        InvariantList = Parser.InvariantList,
                        Unsupported = Parser.Unsupported,
                    };

                    ClassModelExchange NewClassModelExchange = OldClassModelExchange with
                    {
                        ClassModel = NewClassModel,
                    };

                    NewVerificationState = OldVerificationState with
                    {
                        ClassModelExchange = NewClassModelExchange,
                        IsVerificationRequestSent = false,
                        VerificationResult = VerificationResult.Default,
                    };

                    Context.UpdateClassModel(NewVerificationState);

                    Log($"Updated model for class '{ClassName}'.");
                }
                else
                {
                    ClassModel NewClassModel = new ClassModel()
                    {
                        Name = ClassName,
                        PropertyTable = Parser.PropertyTable,
                        FieldTable = Parser.FieldTable,
                        MethodTable = Parser.MethodTable,
                        InvariantList = Parser.InvariantList,
                        Unsupported = Parser.Unsupported,
                        InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
                        RequireViolations = new List<IRequireViolation>().AsReadOnly(),
                        EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
                        AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
                    };

                    ClassModelExchange NewClassModelExchange = new()
                    {
                        ClassModel = NewClassModel,
                        ReceiveChannelGuid = ReceiveChannelGuid,
                    };

                    NewVerificationState = new VerificationState()
                    {
                        ClassModelExchange = NewClassModelExchange,
                        IsVerificationRequestSent = false,
                        VerificationResult = VerificationResult.Default,
                    };

                    Context.AddClassModel(NewVerificationState);

                    Log($"Class model for '{ClassName}' is new and has been added.");
                }

                if (StartMode == VerificationProcessStartMode.Auto)
                    ScheduleAsynchronousVerification();
            }
            else
                NewVerificationState = Context.ClassModelTable[ClassName];

            Context.LastCompilationContext = compilationContext;
        }

        return NewVerificationState.ClassModelExchange.ClassModel;
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
