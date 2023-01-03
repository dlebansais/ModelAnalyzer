namespace ModelAnalyzer.Verification.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

internal class VerifierExtended : Verifier, IDisposable
{
    [SetsRequiredMembers]
    public VerifierExtended()
        : base()
    {
        MaxDepth = 0;
        ClassName = string.Empty;
        PropertyTable = new();
        FieldTable = new();
        MethodTable = new();
        InvariantList = new List<Invariant>().AsReadOnly();
    }

    public void FakeFinalize()
    {
        Dispose(false);
    }
}
