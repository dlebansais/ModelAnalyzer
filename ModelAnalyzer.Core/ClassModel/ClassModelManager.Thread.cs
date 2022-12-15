namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ProcessCommunication;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public partial class ClassModelManager : IDisposable
{
    private Thread InitThread()
    {
        IsVerificationThreadShutdownRequested = false;
        IsAsynchronousVerificationScheduled = false;
        Thread NewThread = new Thread(new ThreadStart(ExecuteThread));
        return NewThread;
    }

    private Channel InitChannel()
    {
        Channel Channel = new Channel(Channel.ServerToClientGuid, Mode.ReceiveShared);
        Channel.Open();

        return Channel;
    }

    /// <summary>
    /// Starts the verification thread.
    /// </summary>
    public void StartThread()
    {
        VerificationThread.Start();
    }

    private void CleanupThread()
    {
        IsAsynchronousVerificationScheduled = false;
        IsVerificationThreadShutdownRequested = true;
        VerificationThread.Join();
    }

    private void ScheduleAsynchronousVerification()
    {
        lock (Context.Lock)
        {
            IsAsynchronousVerificationScheduled = true;
        }

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
                    string JSonString = JsonConvert.SerializeObject(ClassModel);
                    byte[] EncodedString = Encoding.UTF8.GetBytes(JSonString);

                    if (EncodedString.Length <= ToServerChannel.GetFreeLength())
                    {
                        ToServerChannel.Write(EncodedString);

                        Log($"Data send for class '{ClassName}'.");
                    }
                    else
                        Log($"Unable to send data for class '{ClassName}', buffer full.");
                }

                ClassModel.InvariantViolationVerified.Set();
            }

            ToServerChannel.Close();
            Log("Channel closed.");
        }

        if (FromServerChannel.IsOpen)
        {
            byte[]? Data = FromServerChannel.Read();

            if (Data is null)
                Log("No data read from verifier.");
            else
                Log($"{Data.Length} bytes read from verifier.");
        }
        else
            Log($"verifier channel not open, last error={FromServerChannel.LastError}.");
    }

    private void ExecuteThread()
    {
        Log("Entering ExecuteThread.");

        while (!IsVerificationThreadShutdownRequested)
        {
            List<ClassModel> ClonedClassModelList = new();
            bool IsVerificationRequested = false;

            lock (Context.Lock)
            {
                if (IsAsynchronousVerificationScheduled)
                {
                    IsAsynchronousVerificationScheduled = false;
                    IsVerificationRequested = true;

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
            }

            if (IsVerificationRequested)
            {
                ExecuteVerification(ClonedClassModelList);

                lock (Context.Lock)
                {
                    foreach (ClassModel ClassModel in ClonedClassModelList)
                        if (ClassModel.IsInvariantViolated && !Context.ClassNameWithInvariantViolation.Contains(ClassModel.Name))
                            Context.ClassNameWithInvariantViolation.Add(ClassModel.Name);
                        else if (!ClassModel.IsInvariantViolated && Context.ClassNameWithInvariantViolation.Contains(ClassModel.Name))
                            Context.ClassNameWithInvariantViolation.Remove(ClassModel.Name);
                }
            }
            else
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }

        Log("Leaving ExecuteThread.");
    }

    private void ExecuteVerification(List<ClassModel> clonedClassModelList)
    {
        Log("Executing verification started.");

        foreach (ClassModel ClassModel in clonedClassModelList)
        {
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

                ClassModel.IsInvariantViolated = Verifier.IsInvariantViolated;
            }

            ClassModel.InvariantViolationVerified.Set();
        }

#if DEBUG
        // Simulate an analysis that takes time.
        Thread.Sleep(TimeSpan.FromSeconds(1));
#endif

        Log("Executing verification completed.");
    }

    private Thread VerificationThread;
    private bool IsVerificationThreadShutdownRequested;
    private bool IsAsynchronousVerificationScheduled;
    private Channel FromServerChannel;
}
