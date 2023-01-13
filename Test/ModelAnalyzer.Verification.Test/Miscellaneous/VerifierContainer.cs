namespace Miscellaneous.Test;

using System;

internal class VerifierContainer : IDisposable
{
    public VerifierExtended TestObject { get; } = new();

    public void Dispose()
    {
        // Not disposing of TestObject is intentional, this triggers the destructor.
    }
}