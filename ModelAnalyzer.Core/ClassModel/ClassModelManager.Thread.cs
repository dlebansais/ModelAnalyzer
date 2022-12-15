namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public partial class ClassModelManager : IDisposable
{
    private Thread InitThread()
    {
        VerificationThreadShutdown = false;
        RunVerification = false;
        Thread NewThread = new Thread(new ThreadStart(ExecuteThread));
        return NewThread;
    }

    private void CleanupThread()
    {
        RunVerification = false;
        VerificationThreadShutdown = true;
        VerificationThread.Join();
    }

    private void RequestVerification()
    {
        lock (Context.Lock)
        {
            RunVerification = true;
        }
    }

    private void ExecuteThread()
    {
        while (!VerificationThreadShutdown)
        {
            List<ClassModel> ClonedClassModelList = new();
            bool IsVerificationRequested = false;

            lock (Context.Lock)
            {
                if (RunVerification)
                {
                    RunVerification = false;
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
                ClassModel.InvariantViolationVerified.Set();
            }
        }

#if DEBUG
        // Simulate an analysis that takes time.
        Thread.Sleep(TimeSpan.FromSeconds(1));
#endif

        Log("Executing verification completed.");
    }

    private Thread VerificationThread;
    private bool VerificationThreadShutdown;
    private bool RunVerification;
}
