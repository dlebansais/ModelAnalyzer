namespace DemoAnalyzer;

using System;

internal partial record ClassModel : IClassModel
{
    private const int MaxDepth = 1;

    /// <summary>
    /// Verifies a class model.
    /// </summary>
    public void Verify()
    {
        if (Unsupported.IsEmpty)
        {
            Verifier Verifier = new() { MaxDepth = MaxDepth, ClassName = Name, Logger = Logger, FieldTable = FieldTable, MethodTable = MethodTable, InvariantList = InvariantList };
            Verifier.Verify();

            Manager.SetIsInvariantViolated(Name, Verifier.IsInvariantViolated);
        }

        Logger.Log("Pulsing event");
        PulseEvent.Set();
    }

    /// <inheritdoc/>
    public void WaitForThreadCompleted()
    {
        bool IsCompleted = PulseEvent.WaitOne(TimeSpan.FromSeconds(2));

        Logger.Log($"Wait on event done, IsCompleted={IsCompleted}");
    }
}
