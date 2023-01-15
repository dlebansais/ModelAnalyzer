namespace Miscellaneous.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ModelAnalyzer;

internal class VerifierExtended : Verifier, IDisposable
{
    [SetsRequiredMembers]
    public VerifierExtended()
        : base(string.Empty)
    {
        MaxDepth = 0;
        ClassModelTable = new Dictionary<string, ClassModel>();
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
