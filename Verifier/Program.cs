namespace Verifier;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using AnalysisLogger;
using Microsoft.Extensions.Logging;
using ModelAnalyzer;
using Newtonsoft.Json;
using ProcessCommunication;

internal class Program
{
    private const int MaxDepth = 2;

#if DEBUG
    private static IAnalysisLogger Logger = new FileLogger((EnvironmentVariable)"MODEL_VERIFIER_LOG_PATH", "verifier.txt");
#else
    private static IAnalysisLogger Logger = new NullLogger();
#endif

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
                ClassModel? ClassModel = JsonConvert.DeserializeObject<ClassModel>(JsonString, new JsonSerializerSettings() { Error = ErrorHandler, TypeNameHandling = TypeNameHandling.Auto });
                if (ClassModel is not null)
                {
                    Log($"Class model decoded\n{ClassModel}");
                    ProcessClassModel(ToClientChannel, ClassModel);
                }
            }

            ToClientChannel.Close();
        }
        else
            Log($"Failed to open client channel, {ToClientChannel.LastError}");
    }

    private static void ProcessClassModel(Channel toClientChannel, ClassModel classModel)
    {
        string ClassName = classModel.Name;
        VerificationResult VerificationResult;

        try
        {
            using Verifier Verifier = new()
            {
                MaxDepth = MaxDepth,
                ClassName = ClassName,
                FieldTable = classModel.FieldTable,
                MethodTable = classModel.MethodTable,
                InvariantList = classModel.InvariantList,
                Logger = Logger,
            };

            Verifier.Verify();

            VerificationResult = Verifier.VerificationResult;
            Log($"Class model verified: {VerificationResult}");
        }
        catch (Exception exception)
        {
            Logger.LogException(exception);
            VerificationResult = VerificationResult.Default with { ErrorType = VerificationErrorType.Exception, ClassName = ClassName, MethodName = string.Empty, ErrorIndex = -1 };
        }

        string JsonString = JsonConvert.SerializeObject(VerificationResult, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        byte[] EncodedString = Converter.EncodeString(JsonString);

        if (EncodedString.Length <= toClientChannel.GetFreeLength())
        {
            toClientChannel.Write(EncodedString);
            Log("Ack sent");
        }
        else
            Log("No room for ack");
    }
}
