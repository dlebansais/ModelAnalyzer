namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    [Test]
    [Category("Verification")]
    public void VerifierTest_Dispose()
    {
        using (VerifierExtended TestObject = new VerifierExtended())
        {
        }
    }

    [Test]
    [Category("Verification")]
    public void VerifierTest_DoubleDispose()
    {
        using (VerifierExtended TestObject = new VerifierExtended())
        {
            TestObject.Dispose();
        }
    }

    [Test]
    [Category("Verification")]
    public void VerifierTest_FakeFinalize()
    {
        using (VerifierExtended TestObject = new VerifierExtended())
        {
            TestObject.FakeFinalize();
        }
    }

    [Test]
    [Category("Verification")]
    public void VerifierTest_Destructor()
    {
        using VerifierContainer Container = new();
    }
}
