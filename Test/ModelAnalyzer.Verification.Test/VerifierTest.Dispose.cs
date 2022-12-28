namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    [Test]
    [Category("Core")]
    public void VerifierTest_Dispose()
    {
        using (VerifierExtended TestObject = new VerifierExtended())
        {
        }
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_DoubleDispose()
    {
        using (VerifierExtended TestObject = new VerifierExtended())
        {
            TestObject.Dispose();
        }
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_FakeFinalize()
    {
        using (VerifierExtended TestObject = new VerifierExtended())
        {
            TestObject.FakeFinalize();
        }
    }

    [Test]
    [Category("Core")]
    public void VerifierTest_Destructor()
    {
        using VerifierContainer Container = new();
    }
}
