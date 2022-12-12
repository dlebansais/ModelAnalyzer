namespace ModelAnalyzer.Core.Test;

using System;
using System.Diagnostics.CodeAnalysis;

internal class VerifierExtended : Verifier, IDisposable
{
    [SetsRequiredMembers]
    public VerifierExtended()
        : base()
    {
        MaxDepth = 0;
        ClassName = string.Empty;
        FieldTable = new();
        MethodTable = new();
        InvariantList = new();
    }

    public void FakeFinalize()
    {
        Dispose(false);
    }
}
