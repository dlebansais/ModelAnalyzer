namespace Miscellaneous.Test;

using System;
using System.Diagnostics.CodeAnalysis;
using ModelAnalyzer;

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
