namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections.Generic;

internal class SynchronizedThreadExtended : SynchronizedThread<ModelVerification, ClassModel>, IDisposable
{
    public SynchronizedThreadExtended()
        : base(InitContext(), InitCallback)
    {
    }

    public void FakeFinalize()
    {
        Dispose(false);
    }

    private static SynchronizedVerificationContext InitContext()
    {
        return new SynchronizedVerificationContext();
    }

    private static void InitCallback(IDictionary<ModelVerification, ClassModel> parameter)
    {
    }
}
