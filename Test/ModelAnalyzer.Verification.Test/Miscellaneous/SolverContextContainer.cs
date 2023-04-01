namespace Miscellaneous.Test;

using System;
using CodeProverBinding;

internal class SolverContextContainer : IDisposable
{
    public SolverContextExtended TestObject { get; } = new(new Binder(Prover.Default));

    public void Dispose()
    {
        // Not disposing of TestObject is intentional, this triggers the destructor.
    }
}