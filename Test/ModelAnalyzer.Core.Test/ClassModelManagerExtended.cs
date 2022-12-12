namespace ModelAnalyzer.Core.Test;

using System;

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
