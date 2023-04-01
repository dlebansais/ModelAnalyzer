namespace Miscellaneous.Test;

using CodeProverBinding;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="SolverContext"/> class.
/// </summary>
public partial class SolverContextTest
{
    [Test]
    [Category("Verification")]
    public void SolverContextTest_Dispose()
    {
        using (SolverContextExtended TestObject = new SolverContextExtended(new Binder(Prover.Default)))
        {
        }
    }

    [Test]
    [Category("Verification")]
    public void SolverContextTest_DoubleDispose()
    {
        using (SolverContextExtended TestObject = new SolverContextExtended(new Binder(Prover.Default)))
        {
            TestObject.Dispose();
        }
    }

    [Test]
    [Category("Verification")]
    public void SolverContextTest_FakeFinalize()
    {
        using (SolverContextExtended TestObject = new SolverContextExtended(new Binder(Prover.Default)))
        {
            TestObject.FakeFinalize();
        }
    }

    [Test]
    [Category("Verification")]
    public void SolverContextTest_Destructor()
    {
        using SolverContextContainer Container = new();
    }
}
