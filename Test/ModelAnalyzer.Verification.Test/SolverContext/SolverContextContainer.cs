namespace ModelAnalyzer.Verification.Test;

using System;

internal class SolverContextContainer : IDisposable
{
    public SolverContextExtended TestObject { get; } = new();

    public void Dispose()
    {
        // Not disposing of TestObject is intentional, this triggers the destructor.
    }
}