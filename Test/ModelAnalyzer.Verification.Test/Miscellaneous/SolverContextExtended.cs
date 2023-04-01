namespace Miscellaneous.Test;

using System;
using System.Diagnostics.CodeAnalysis;
using CodeProverBinding;
using ModelAnalyzer;

internal class SolverContextExtended : SolverContext, IDisposable
{
    [SetsRequiredMembers]
    public SolverContextExtended(Binder binder)
        : base(binder)
    {
    }

    public void FakeFinalize()
    {
        Dispose(false);
    }
}
