namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
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
        Channel Channel = new Channel(Channel.SharedGuid, Mode.Client);
        if (Channel.Open())
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
                    JsonSerializerOptions Options = new();
                    string JSonString = JsonSerializer.Serialize(ClassModel, Options);
                    byte[] EncodedString = Encoding.UTF8.GetBytes(JSonString);
                    Channel.Write(EncodedString);

                    Log($"Data send for class '{ClassName}'.");
                }

                ClassModel.InvariantViolationVerified.Set();
            }

            Channel.Close();
            Log("Channel closed.");
        }
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
}
