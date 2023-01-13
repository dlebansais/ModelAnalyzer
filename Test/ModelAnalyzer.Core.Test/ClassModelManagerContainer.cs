namespace ClassModelManager.Test;

using System;

internal class ClassModelManagerContainer : IDisposable
{
    public ClassModelManagerExtended TestObject { get; } = new();

    public void Dispose()
    {
        // Not disposing of TestObject is intentional, this triggers the destructor.
    }
}
