namespace Verifier;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using AnalysisLogger;
using Microsoft.Extensions.Logging;
using ModelAnalyzer;
using Newtonsoft.Json;
using ProcessCommunication;

internal class Program
{
    private const int MaxDepth = 2;

    private static FileLogger Logger = new FileLogger("C:\\Projects\\Temp\\verifier.txt");

    private static void Log(string message)
    {
        Logger.Log(message);
        Debug.WriteLine(message);
    }

    private static void Log(LogLevel level, string message)
    {
        Logger.Log(level, message);
        Debug.WriteLine(message);
    }

    public static void Main()
    {
        /*
        string JsonString = "{\"Name\":\"Other\",\"FieldTable\":{\"IsSealed\":true,\"List\":[{\"Key\":{\"Name\":\"XY\"},\"Value\":{\"FieldName\":{\"Name\":\"XY\"},\"Name\":\"XY\"}}]},\"MethodTable\":{\"IsSealed\":true,\"List\":[{\"Key\":{\"Name\":\"Read\"},\"Value\":{\"MethodName\":{\"Name\":\"Read\"},\"IsSupported\":true,\"HasReturnValue\":true,\"ParameterTable\":{\"IsSealed\":true,\"List\":[]},\"RequireList\":[],\"StatementList\":[{\"$type\":\"ModelAnalyzer.AssignmentStatement, ModelAnalyzer.Data\",\"Destination\":{\"$type\":\"ModelAnalyzer.Field, ModelAnalyzer.Data\",\"FieldName\":{\"Name\":\"XY\"},\"Name\":\"XY\"},\"Expression\":{\"$type\":\"ModelAnalyzer.LiteralIntValueExpression, ModelAnalyzer.Data\",\"Value\":0}},{\"$type\":\"ModelAnalyzer.ReturnStatement, ModelAnalyzer.Data\",\"Expression\":{\"$type\":\"ModelAnalyzer.VariableValueExpression, ModelAnalyzer.Data\",\"Variable\":{\"$type\":\"ModelAnalyzer.Field, ModelAnalyzer.Data\",\"FieldName\":{\"Name\":\"XY\"},\"Name\":\"XY\"}}}],\"EnsureList\":[],\"Name\":\"Read\"}},{\"Key\":{\"Name\":\"Write\"},\"Value\":{\"MethodName\":{\"Name\":\"Write\"},\"IsSupported\":true,\"HasReturnValue\":false,\"ParameterTable\":{\"IsSealed\":true,\"List\":[{\"Key\":{\"Name\":\"x\"},\"Value\":{\"ParameterName\":{\"Name\":\"x\"},\"Name\":\"x\"}}]},\"RequireList\":[{\"BooleanExpression\":{\"$type\":\"ModelAnalyzer.ComparisonExpression, ModelAnalyzer.Data\",\"Left\":{\"$type\":\"ModelAnalyzer.VariableValueExpression, ModelAnalyzer.Data\",\"Variable\":{\"$type\":\"ModelAnalyzer.Parameter, ModelAnalyzer.Data\",\"ParameterName\":{\"Name\":\"x\"},\"Name\":\"x\"}},\"Operator\":0,\"Right\":{\"$type\":\"ModelAnalyzer.LiteralIntValueExpression, ModelAnalyzer.Data\",\"Value\":1}}}],\"StatementList\":[{\"$type\":\"ModelAnalyzer.AssignmentStatement, ModelAnalyzer.Data\",\"Destination\":{\"$type\":\"ModelAnalyzer.Field, ModelAnalyzer.Data\",\"FieldName\":{\"Name\":\"XY\"},\"Name\":\"XY\"},\"Expression\":{\"$type\":\"ModelAnalyzer.BinaryArithmeticExpression, ModelAnalyzer.Data\",\"Left\":{\"$type\":\"ModelAnalyzer.VariableValueExpression, ModelAnalyzer.Data\",\"Variable\":{\"$type\":\"ModelAnalyzer.Parameter, ModelAnalyzer.Data\",\"ParameterName\":{\"Name\":\"x\"},\"Name\":\"x\"}},\"Operator\":1,\"Right\":{\"$type\":\"ModelAnalyzer.LiteralIntValueExpression, ModelAnalyzer.Data\",\"Value\":1}}}],\"EnsureList\":[{\"BooleanExpression\":{\"$type\":\"ModelAnalyzer.ComparisonExpression, ModelAnalyzer.Data\",\"Left\":{\"$type\":\"ModelAnalyzer.VariableValueExpression, ModelAnalyzer.Data\",\"Variable\":{\"$type\":\"ModelAnalyzer.Field, ModelAnalyzer.Data\",\"FieldName\":{\"Name\":\"XY\"},\"Name\":\"XY\"}},\"Operator\":3,\"Right\":{\"$type\":\"ModelAnalyzer.LiteralIntValueExpression, ModelAnalyzer.Data\",\"Value\":0}}}],\"Name\":\"Write\"}}]},\"InvariantList\":[{\"BooleanExpression\":{\"$type\":\"ModelAnalyzer.ComparisonExpression, ModelAnalyzer.Data\",\"Left\":{\"$type\":\"ModelAnalyzer.VariableValueExpression, ModelAnalyzer.Data\",\"Variable\":{\"$type\":\"ModelAnalyzer.Field, ModelAnalyzer.Data\",\"FieldName\":{\"Name\":\"XY\"},\"Name\":\"XY\"}},\"Operator\":0,\"Right\":{\"$type\":\"ModelAnalyzer.LiteralIntValueExpression, ModelAnalyzer.Data\",\"Value\":0}}}],\"Unsupported\":{\"IsEmpty\":true,\"InvalidDeclaration\":false,\"HasUnsupporteMember\":false,\"InternalFields\":[],\"InternalMethods\":[],\"InternalParameters\":[],\"InternalRequires\":[],\"InternalEnsures\":[],\"InternalStatements\":[],\"InternalExpressions\":[],\"InternalInvariants\":[]}}";
        ClassModel ClassModel = JsonConvert.DeserializeObject<ClassModel>(JsonString, new JsonSerializerSettings() { Error = ErrorHandler, TypeNameHandling = TypeNameHandling.Auto })!;

        using Verifier Verifier = new()
        {
            MaxDepth = MaxDepth,
            ClassName = ClassModel.Name,
            Logger = Logger,
            FieldTable = ClassModel.FieldTable,
            MethodTable = ClassModel.MethodTable,
            InvariantList = ClassModel.InvariantList,
        };

        Verifier.Verify();
        */
        Log("Entering main()");

        string Location = Assembly.GetExecutingAssembly().Location;
        Environment.CurrentDirectory = Path.GetDirectoryName(Location);

        Channel FromClientChannel = new(Channel.ClientToServerGuid, Mode.Receive);
        FromClientChannel.Open();

        if (!FromClientChannel.IsOpen)
        {
            Log("Channel already open, exiting");
            return;
        }

        TimeSpan Timeout = TimeSpan.FromSeconds(60);
        Stopwatch Watch = new();
        Watch.Start();

        while (Watch.Elapsed < Timeout)
        {
            byte[]? Data = FromClientChannel.Read();
            if (Data is not null)
            {
                Log($"Data received {Data.Length} bytes");

                ProcessData(Data);
                Watch.Restart();
            }
            else
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }

        Log("Timeout, exiting");

        FromClientChannel.Close();

        Log("Done");
    }

    private static void ErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
    {
        Log(LogLevel.Error, args.ErrorContext.Error.Message);
    }

    private static void ProcessData(byte[] data)
    {
        Channel ToClientChannel = new(Channel.ServerToClientGuid, Mode.Send);
        ToClientChannel.Open();

        if (ToClientChannel.IsOpen)
        {
            Log($"Client channel opened");

            int Offset = 0;
            while (Converter.TryDecodeString(data, ref Offset, out string JsonString))
            {
                Log(JsonString);

                ClassModel? ClassModel = JsonConvert.DeserializeObject<ClassModel>(JsonString, new JsonSerializerSettings() { Error = ErrorHandler, TypeNameHandling = TypeNameHandling.Auto });
                if (ClassModel is not null)
                {
                    Log($"Class model decoded");

                    bool IsInvariantViolated = false;

                    try
                    {
                        using Verifier Verifier = new()
                        {
                            MaxDepth = MaxDepth,
                            ClassName = ClassModel.Name,
                            Logger = Logger,
                            FieldTable = ClassModel.FieldTable,
                            MethodTable = ClassModel.MethodTable,
                            InvariantList = ClassModel.InvariantList,
                        };

                        Verifier.Verify();

                        IsInvariantViolated = Verifier.IsInvariantViolated;
                        Log($"Class model verified: {(IsInvariantViolated ? "invariant violated" : "no invariant violation")}");
                    }
                    catch
                    {
                    }

                    VerificationResult Result = new() { ClassName = ClassModel.Name, IsInvariantViolated = IsInvariantViolated };

                    JsonString = JsonConvert.SerializeObject(Result, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    byte[] EncodedString = Converter.EncodeString(JsonString);

                    if (EncodedString.Length <= ToClientChannel.GetFreeLength())
                    {
                        ToClientChannel.Write(EncodedString);
                        Log("Ack sent");
                    }
                    else
                        Log("No room for ack");
                }
            }

            ToClientChannel.Close();
        }
        else
            Log($"Failed to open client channel, {ToClientChannel.LastError}");
    }
}
