namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents the set of preloaded classes.
/// </summary>
internal static class Preloaded
{
    private const string MathSqrt = @"{
  ""ClassName"": ""Math"",
  ""Methods"": [
    {
      ""Name"": ""Sqrt"",
      ""ReturnTypeName"": ""double"",
      ""Parameters"": [
        {
          ""Name"": ""d"",
          ""TypeName"": ""double""
        }
      ],
      ""Requires"": [
        ""d >= 0""
      ],
      ""Ensures"": [
        ""Result >= 0"",
        ""Result * Result == d"",
      ]
    },
    {
      ""Name"": ""ForTestOnly_DoNotUse"",
      ""ReturnTypeName"": ""void"",
    }
  ]
}";

    /// <summary>
    /// Gets the table of preloaded classes.
    /// </summary>
    public static ClassModelTable GetClasses()
    {
        ClassModelTable PreloadedClasses = new();

        string[] StoredJsonClasses = new string[]
        {
            MathSqrt,
        };

        foreach (string JsonString in StoredJsonClasses)
        {
            ClassModel ClassModel = JsonStringToClassModel(JsonString);
            PreloadedClasses.Add(ClassModel.ClassName, ClassModel);
        }

        return PreloadedClasses;
    }

    /// <summary>
    /// Creates a class model from a JSON string.
    /// </summary>
    /// <param name="jsonString">The string in JSON format.</param>
    private static ClassModel JsonStringToClassModel(string jsonString)
    {
        PreloadedClassModel PreloadedClassModel = JsonStringToPreloadedClassModel(jsonString);
        ClassModel NewClassModel = CreateClassModel(PreloadedClassModel);

        return NewClassModel;
    }

    /// <summary>
    /// Creates a preloaded class model from a JSON string.
    /// </summary>
    /// <param name="jsonString">The string in JSON format.</param>
    private static PreloadedClassModel JsonStringToPreloadedClassModel(string jsonString)
    {
        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassNameConverter());

        PreloadedClassModel? PreloadedClassModel = JsonConvert.DeserializeObject<PreloadedClassModel>(jsonString, Settings);

        Debug.Assert(PreloadedClassModel is not null);

        return PreloadedClassModel!;
    }

    /// <summary>
    /// Creates a class model from a JSON string.
    /// </summary>
    /// <param name="preloadedClassModel">The preloaded class model.</param>
    private static ClassModel CreateClassModel(PreloadedClassModel preloadedClassModel)
    {
        MethodTable NewMethodTable = new();

        foreach (PreloadedMethod Item in preloadedClassModel.Methods)
        {
            Method NewMethod = CreateMethod(preloadedClassModel.ClassName, Item);
            NewMethodTable.AddItem(NewMethod);
        }

        ClassModel NewClassModel = new()
        {
            ClassName = preloadedClassModel.ClassName,
            PropertyTable = ReadOnlyPropertyTable.Empty,
            FieldTable = ReadOnlyFieldTable.Empty,
            MethodTable = NewMethodTable.AsReadOnly(),
            InvariantList = new List<Invariant>().AsReadOnly(),
            Unsupported = new Unsupported(),
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        return NewClassModel;
    }

    /// <summary>
    /// Creates a method from the preloaded value.
    /// </summary>
    /// <param name="className">The name of the class containing the method.</param>
    /// <param name="preloadedMethod">The preloaded method.</param>
    private static Method CreateMethod(ClassName className, PreloadedMethod preloadedMethod)
    {
        ParameterTable ParameterTable = new();

        foreach (PreloadedParameter Item in preloadedMethod.Parameters)
        {
            Parameter NewParameter = CreateParameter(Item);
            ParameterTable.AddItem(NewParameter);
        }

        List<Require> RequireList = new();

        foreach (string RequireString in preloadedMethod.Requires)
        {
            Require NewRequire = CreateRequire(RequireString);
            RequireList.Add(NewRequire);
        }

        List<Ensure> EnsureList = new();

        foreach (string EnsureString in preloadedMethod.Ensures)
        {
            Ensure NewEnsure = CreateEnsure(EnsureString);
            EnsureList.Add(NewEnsure);
        }

        ExpressionType ReturnType = ConvertTypeName(preloadedMethod.ReturnTypeName);
        Debug.Assert(ReturnType != ExpressionType.Other);
        Debug.Assert(ReturnType.IsSimple);

        Local? ResultLocal;

        if (ReturnType == ExpressionType.Void)
            ResultLocal = null;
        else
            ResultLocal = new Local()
            {
                Name = new LocalName() { Text = "Result" },
                Type = ReturnType,
                Initializer = null,
            };

        BlockScope NewBlock = new() { LocalTable = ReadOnlyLocalTable.Empty, IndexLocal = null, StatementList = new List<Statement>() };
        Method NewMethod = new()
        {
            Name = new MethodName() { Text = preloadedMethod.Name },
            ClassName = className,
            AccessModifier = AccessModifier.Public,
            IsStatic = true,
            IsPreloaded = true,
            ReturnType = ReturnType,
            ParameterTable = ParameterTable.AsReadOnly(),
            RequireList = RequireList,
            RootBlock = NewBlock,
            ResultLocal = ResultLocal,
            EnsureList = EnsureList,
        };

        return NewMethod;
    }

    /// <summary>
    /// Creates a parameter from the preloaded value.
    /// </summary>
    /// <param name="preloadedParameter">The preloaded parameter.</param>
    private static Parameter CreateParameter(PreloadedParameter preloadedParameter)
    {
        Parameter NewParameter = new Parameter()
        {
            Name = new ParameterName() { Text = preloadedParameter.Name },
            Type = ConvertTypeName(preloadedParameter.TypeName),
        };

        return NewParameter;
    }

    /// <summary>
    /// Creates a require clause from a string.
    /// </summary>
    /// <param name="requireString">The require clause as string.</param>
    private static Require CreateRequire(string requireString)
    {
        Require NewRequire = new Require()
        {
            Text = requireString,
            Location = Location.None,
            BooleanExpression = new LiteralBooleanValueExpression() { Value = true },
        };

        return NewRequire;
    }

    /// <summary>
    /// Creates an ensure clause from a string.
    /// </summary>
    /// <param name="ensureString">The ensure clause as string.</param>
    private static Ensure CreateEnsure(string ensureString)
    {
        Ensure NewEnsure = new Ensure()
        {
            Text = ensureString,
            Location = Location.None,
            BooleanExpression = new LiteralBooleanValueExpression() { Value = true },
        };

        return NewEnsure;
    }

    /// <summary>
    /// Converts a string to a simple type.
    /// </summary>
    /// <param name="typeName">The name of the type to convert.</param>
    private static ExpressionType ConvertTypeName(string typeName)
    {
        Dictionary<string, ExpressionType> TypeTable = new()
        {
            { "void", ExpressionType.Void },
            { "bool", ExpressionType.Boolean },
            { "int", ExpressionType.Integer },
            { "double", ExpressionType.FloatingPoint },
        };

        Debug.Assert(TypeTable.ContainsKey(typeName));

        return TypeTable[typeName];
    }
}
