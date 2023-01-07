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
        VerificationState VerificationState = new()
        {
            ModelExchange = new ModelExchange() { ClassModelTable = new Dictionary<string, ClassModel>(), ReceiveChannelGuid = receiveChannelGuid },
            IsVerificationRequestSent = false,
            VerificationResult = VerificationResult.Default,
        };

        Context = new SynchronizedVerificationContext() { VerificationState = VerificationState };
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
    /// Gets models of a classes.
    /// </summary>
    /// <param name="compilationContext">The compilation context.</param>
    /// <param name="classDeclarationList">The list of class declaration.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <exception cref="ArgumentException">Empty class name.</exception>
    public IDictionary<ClassDeclarationSyntax, IClassModel> GetClassModels(CompilationContext compilationContext, List<ClassDeclarationSyntax> classDeclarationList, IModel semanticModel)
    {
        string ClassNames = string.Empty;

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
        {
            string ClassName = ClassDeclaration.Identifier.ValueText;
            if (ClassName == string.Empty)
                throw new ArgumentException("Class name must not be empty.");

            if (ClassNames != string.Empty)
                ClassNames += ", ";

            ClassNames += ClassName;
        }

        // We clear logs only after the constructor has exited.
        ClearLogs();

        Log($"Getting model for class '{ClassNames}'.");

        return GetClassModelListInternal(compilationContext, classDeclarationList, semanticModel);
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

            foreach (string ClassName in Context.GetClassModelNameList())
                if (!existingClassList.Contains(ClassName))
                    ToRemoveClassList.Add(ClassName);

            foreach (string ClassName in ToRemoveClassList)
            {
                Log($"Removing class '{ClassName}'.");

                Context.RemoveClass(ClassName);
            }
        }
    }

    private IDictionary<ClassDeclarationSyntax, IClassModel> GetClassModelListInternal(CompilationContext compilationContext, List<ClassDeclarationSyntax> classDeclarationList, IModel semanticModel)
    {
        Dictionary<ClassDeclarationSyntax, IClassModel> Result = new();

        lock (Context.Lock)
        {
            Log($"Compilation context: {compilationContext}");

            // Compare this compilation context with the previous one. They will be different if their hash code is not the same, or if the new context is an asynchronous request.
            bool IsNewCompilationContext = !Context.LastCompilationContext.IsCompatibleWith(compilationContext);
            bool ClassModelAlreadyExistForAll = classDeclarationList.TrueForAll(classDeclaration => Context.ContainsClass(classDeclaration.Identifier.ValueText));

            if (IsNewCompilationContext || !ClassModelAlreadyExistForAll)
            {
                VerificationState OldVerificationState = Context.VerificationState;
                ModelExchange OldClassModelExchange = OldVerificationState.ModelExchange;
                Dictionary<string, ClassModel> OldClassModelTable = OldClassModelExchange.ClassModelTable;
                Dictionary<string, ClassModel> NewClassModelTable = new();

                foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
                {
                    string ClassName = ClassDeclaration.Identifier.ValueText;
                    Debug.Assert(ClassName != string.Empty);

                    ClassDeclarationParser Parser = new(ClassDeclaration, semanticModel) { Logger = Logger };
                    Parser.Parse();

                    ClassModel NewClassModel;

                    if (OldClassModelTable.ContainsKey(ClassName))
                    {
                        Log($"Updating model for class '{ClassName}'.");

                        ClassModel OldClassModel = OldClassModelTable[ClassName];

                        NewClassModel = OldClassModel with
                        {
                            PropertyTable = Parser.PropertyTable,
                            FieldTable = Parser.FieldTable,
                            MethodTable = Parser.MethodTable,
                            InvariantList = Parser.InvariantList,
                            Unsupported = Parser.Unsupported,
                        };
                    }
                    else
                    {
                        Log($"Adding new model for class '{ClassName}'.");

                        NewClassModel = new ClassModel()
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
                    }

                    NewClassModelTable.Add(ClassName, NewClassModel);
                    Result.Add(ClassDeclaration, NewClassModel);
                }

                ModelExchange NewClassModelExchange = OldClassModelExchange with
                {
                    ClassModelTable = NewClassModelTable,
                };

                VerificationState NewVerificationState = OldVerificationState with
                {
                    ModelExchange = NewClassModelExchange,
                    IsVerificationRequestSent = false,
                    VerificationResult = VerificationResult.Default,
                };

                Context.VerificationState = NewVerificationState;

                if (StartMode == VerificationProcessStartMode.Auto)
                    ScheduleAsynchronousVerification();
            }
            else
            {
                Dictionary<string, ClassModel> ClassModelTable = Context.VerificationState.ModelExchange.ClassModelTable;

                foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
                {
                    string ClassName = ClassDeclaration.Identifier.ValueText;

                    Debug.Assert(ClassName != string.Empty);
                    Debug.Assert(ClassModelTable.ContainsKey(ClassName));

                    Result.Add(ClassDeclaration, ClassModelTable[ClassName]);
                }
            }

            Context.LastCompilationContext = compilationContext;
        }

        return Result;
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void ClearLogs()
    {
        lock (Context.Lock)
        {
            if (Context.VerificationState.ModelExchange.ClassModelTable.Count == 0)
                Logger.Clear();
        }
    }

    private SynchronizedVerificationContext Context;
}
