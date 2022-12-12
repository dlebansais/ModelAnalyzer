namespace ModelAnalyzer.Core.Test;

using System;

internal class SynchronizedThreadContainer : IDisposable
{
    public SynchronizedThreadExtended TestObject { get; } = new();

    public void Dispose()
    {
        // Not disposing of TestObject is intentional, this triggers the destructor.
    }
}
