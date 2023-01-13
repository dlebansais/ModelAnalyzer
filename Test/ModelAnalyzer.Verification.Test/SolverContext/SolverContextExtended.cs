namespace ModelAnalyzer.Verification.Test;

using System;
using System.Diagnostics.CodeAnalysis;

internal class SolverContextExtended : SolverContext, IDisposable
{
    [SetsRequiredMembers]
    public SolverContextExtended()
        : base()
    {
    }

    public void FakeFinalize()
    {
        Dispose(false);
    }
}
