namespace ModelAnalyzer.Core.Test;

using System;

public class ClassModelManagerExtended : ClassModelManager, IDisposable
{
    public void FakeFinalize()
    {
        Dispose(false);
    }
}
