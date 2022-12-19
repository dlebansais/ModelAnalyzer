namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Libz3Extractor;
using Newtonsoft.Json;
using ProcessCommunication;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public partial class ClassModelManager : IDisposable
{
    private Channel InitChannel()
    {
        Channel Channel = new Channel(Channel.ServerToClientGuid, Mode.ReceiveShared);
        Channel.Open();

        return Channel;
    }

    /// <summary>
    /// Starts verifying classes. Only needed if <see cref="StartMode"/> is <see cref="SynchronizedThreadStartMode.Auto"/>.
    /// </summary>
    /// <param name="classModel">The class classModel.</param>
    private IClassModel WaitForVerification(IClassModel classModel)
    {
        string ClassName = classModel.Name;

        if (!classModel.Unsupported.IsEmpty)
        {
            Log($"Model for class '{ClassName}' contains unsupported elements, aborting verification.");
            return classModel;
        }

        if (!FromServerChannel.IsOpen)
        {
            Log("Channel not open to receive verification results, aborting.");
            return classModel;
        }

        if (StartMode == SynchronizedThreadStartMode.Manual)
            ScheduleAsynchronousVerification();

        Log("Starting the verification loop.");

        TimeSpan Timeout = TimeSpan.FromSeconds(5);
        Stopwatch Watch = new();
        Watch.Start();

        for (; ;)
        {
            if (Watch.Elapsed >= Timeout)
            {
                Log($"Verification loop ended on timeout.");
                return classModel;
            }

            CheckVerificationStatus(ClassName, out bool IsFound, out bool IsVerified, out bool IsInvariantViolated);

            if (!IsFound)
            {
                Log($"Class '{ClassName}' no longer in the list of models.");
                return classModel;
            }

            if (IsVerified)
            {
                Log($"Verification loop completed, class {ClassName} verified, invariant violation: {IsInvariantViolated}.");
                return ((ClassModel)classModel) with { IsVerified = true, IsInvariantViolated = IsInvariantViolated };
            }

            UpdateVerificationEvents();
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    private void CheckVerificationStatus(string className, out bool isFound, out bool isVerified, out bool isInvariantViolated)
    {
        lock (Context.Lock)
        {
            if (Context.ClassModelTable.ContainsKey(className))
            {
                isFound = true;

                ClassModel ClassModel = Context.ClassModelTable[className];
                isVerified = ClassModel.IsVerified;
                isInvariantViolated = ClassModel.IsInvariantViolated;
            }
            else
            {
                isFound = false;
                isVerified = false;
                isInvariantViolated = false;
            }
        }
    }

    private void UpdateVerificationEvents()
    {
        byte[]? Data = FromServerChannel.Read();

        if (Data is null)
        {
            Log("No data read from verifier.");
            return;
        }

        Log($"{Data.Length} bytes read from verifier.");

        int Offset = 0;
        while (Converter.TryDecodeString(Data, ref Offset, out string JsonString))
        {
            Log($"New offset: {Offset}.");

            Log(JsonString);

            VerificationResult? VerificationResult = JsonConvert.DeserializeObject<VerificationResult>(JsonString, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            if (VerificationResult is not null)
                UpdateVerificationEvent(VerificationResult);
            else
                Log("Failed to deserialize the verification result.");
        }
    }

    private void UpdateVerificationEvent(VerificationResult verificationResult)
    {
        string ClassName = verificationResult.ClassName;
        bool IsInvariantViolated = verificationResult.IsError;

        Log($"Verification result decoded: {verificationResult}");

        lock (Context.Lock)
        {
            foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
            {
                ClassModel ClassModel = Entry.Value;

                if (ClassModel.Name == ClassName)
                {
                    Context.ClassModelTable[ClassName] = ClassModel with { IsVerified = true, IsInvariantViolated = IsInvariantViolated };

                    if (Interlocked.Decrement(ref VerificationRequestCount) == 0)
                        VerifierCallWatch.Stop();

                    return;
                }
            }
        }

        Log($"Class '{ClassName}' no longer in the list of models, verification result lost.");
    }

    private void ScheduleAsynchronousVerification()
    {
        Extractor.Extract();
        Log($"Extracted count: {Extractor.ExtractedPathTable.Count}");
        Log($"Extracted error: {Extractor.LastExceptionMessage}");
        string VerifierFilePath = Extractor.GetExtractedPath(Extractor.VerifierFileName);

        Logger.Log($"Starting the verification process at {VerifierFilePath}.");

        try
        {
            ProcessStartInfo ProcessStartInfo = new ProcessStartInfo();
            ProcessStartInfo.FileName = VerifierFilePath;
            ProcessStartInfo.UseShellExecute = false;
            ProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(VerifierFilePath);
            Process CreatedProcess = Process.Start(ProcessStartInfo);

            Log($"CreatedProcess: {CreatedProcess} {CreatedProcess.Id} {CreatedProcess.ProcessName}");
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }

        Log("Creating the channel to send class models.");

        Channel ToServerChannel = new Channel(Channel.ClientToServerGuid, Mode.Send);

        TimeSpan Timeout = TimeSpan.FromSeconds(5);
        Stopwatch Watch = new();
        Watch.Start();

        while (Watch.Elapsed < Timeout)
        {
            ToServerChannel.Open();
            if (ToServerChannel.IsOpen)
                break;

            Thread.Sleep(TimeSpan.FromMilliseconds(500));
        }

        if (ToServerChannel.IsOpen)
        {
            Log("Channel opened.");

            SendClassModelDataForVerification(ToServerChannel);
            ToServerChannel.Close();
        }
        else
            Log("Could not open the channel.");

        UpdateVerificationEvents();
    }

    private void SendClassModelDataForVerification(Channel channel)
    {
        List<ClassModel> ToVerifyList = new();

        lock (Context.Lock)
        {
            foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
            {
                ClassModel ClassModel = Entry.Value;
                string ClassName = ClassModel.Name;

                if (!ClassModel.Unsupported.IsEmpty)
                    Log($"Skipping complete verification for class '{ClassName}', it has unsupported elements.");
                else if (ClassModel.IsVerified)
                    Log($"Skipping complete verification for class '{ClassName}', it's already verified.");
                else
                    ToVerifyList.Add(ClassModel);
            }
        }

        foreach (ClassModel ClassModel in ToVerifyList)
        {
            string ClassName = ClassModel.Name;

            string JSonString = JsonConvert.SerializeObject(ClassModel, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            byte[] EncodedString = Converter.EncodeString(JSonString);

            if (EncodedString.Length <= channel.GetFreeLength())
            {
                channel.Write(EncodedString);

                Log($"Data send {EncodedString.Length} bytes for class '{ClassName}'.");

                Interlocked.Increment(ref VerificationRequestCount);
                VerifierCallWatch.Restart();
            }
            else
                Log($"Unable to send data for class '{ClassName}', buffer full.");
        }
    }

    private static Stopwatch VerifierCallWatch = new();
    private static int VerificationRequestCount;
    private Channel FromServerChannel;
}
