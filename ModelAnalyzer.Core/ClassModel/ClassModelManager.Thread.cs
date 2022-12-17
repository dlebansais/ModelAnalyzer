namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
    public void StartVerification()
    {
        if (!FromServerChannel.IsOpen)
        {
            Log("Channel not open to receive verification results, aborting.");
            return;
        }

        Log("Starting the verification thread.");

        TimeSpan Timeout = TimeSpan.FromSeconds(5);
        Stopwatch Watch = new();
        Watch.Start();

        while (!IsAllClassesVerified() && Watch.Elapsed < Timeout)
        {
            UpdateVerificationEvents();
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    private bool IsAllClassesVerified()
    {
        bool Result = true;

        lock (Context.Lock)
        {
            foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
            {
                ClassModel ClassModel = Entry.Value;

                if (!ClassModel.InvariantViolationVerified.WaitOne(0))
                {
                    Result = false;
                    break;
                }
            }
        }

        return Result;
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
            Log(JsonString);

            VerificationResult? VerificationResult = JsonConvert.DeserializeObject<VerificationResult>(JsonString, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            if (VerificationResult is not null)
                UpdateVerificationEvent(VerificationResult);
        }
    }

    private void UpdateVerificationEvent(VerificationResult verificationResult)
    {
        string ClassName = verificationResult.ClassName;
        bool IsInvariantViolated = verificationResult.IsInvariantViolated;

        Log($"Verification result decoded for class '{ClassName}': {IsInvariantViolated}");

        lock (Context.Lock)
        {
            foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
            {
                ClassModel ClassModel = Entry.Value;

                if (ClassModel.Name == ClassName)
                {
                    Context.SetIsInvariantViolated(ClassName, IsInvariantViolated);
                    ClassModel.InvariantViolationVerified.Set();
                    break;
                }
            }
        }
    }

    private void ScheduleAsynchronousVerification()
    {
        Log("Creating the channel.");
        Channel ToServerChannel = new Channel(Channel.ClientToServerGuid, Mode.Send);
        ToServerChannel.Open();

        if (ToServerChannel.IsOpen)
        {
            Log("Channel opened.");

            List<ClassModel> ClonedClassModelList = new();

            lock (Context.Lock)
            {
                foreach (KeyValuePair<string, ClassModel> Entry in Context.ClassModelTable)
                {
                    ClassModel ClassModel = Entry.Value;

                    if (!ClassModel.InvariantViolationVerified.WaitOne(0))
                    {
                        ClassModel ClonedModel = ClassModel with { };
                        ClonedClassModelList.Add(ClonedModel);
                    }
                }
            }

            foreach (ClassModel ClassModel in ClonedClassModelList)
            {
                string ClassName = ClassModel.Name;

                if (!ClassModel.Unsupported.IsEmpty)
                    Log($"Skipping complete verification for class '{ClassName}', it has unsupported elements.");
                else
                {
                    string JSonString = JsonConvert.SerializeObject(ClassModel, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                    byte[] EncodedString = Converter.EncodeString(JSonString);

                    if (EncodedString.Length <= ToServerChannel.GetFreeLength())
                    {
                        ToServerChannel.Write(EncodedString);

                        Log($"Data send {EncodedString.Length} bytes for class '{ClassName}'.");
                    }
                    else
                        Log($"Unable to send data for class '{ClassName}', buffer full.");
                }

                ClassModel.InvariantViolationVerified.Set();
            }

            ToServerChannel.Close();
            Log("Channel closed.");
        }
    }

    private Channel FromServerChannel;
}
