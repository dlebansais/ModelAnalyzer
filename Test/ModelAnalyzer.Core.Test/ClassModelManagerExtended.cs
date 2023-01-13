namespace ClassModelManager.Test;

using System;
using ModelAnalyzer;

internal class ClassModelManagerExtended : ClassModelManager, IDisposable
{
    public ClassModelManagerExtended()
        : base()
    {
    }

    public void FakeFinalize()
    {
        Dispose(false);
    }
}
