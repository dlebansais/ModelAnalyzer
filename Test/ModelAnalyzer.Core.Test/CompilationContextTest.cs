namespace ModelAnalyzer.Core.Test;

using NUnit.Framework;

public class CompilationContextTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        CompilationContext Context1 = CompilationContext.Default;
        CompilationContext Context2 = CompilationContext.GetAnother();
        CompilationContext Context3 = CompilationContext.GetAnother();

        Assert.IsTrue(Context1.IsCompatibleWith(Context1));
        Assert.IsTrue(Context2.IsCompatibleWith(Context2));
        Assert.IsTrue(Context3.IsCompatibleWith(Context3));

        Assert.IsFalse(Context1.IsCompatibleWith(Context2));
        Assert.IsFalse(Context1.IsCompatibleWith(Context3));
        Assert.IsFalse(Context2.IsCompatibleWith(Context3));
        Assert.IsFalse(Context2.IsCompatibleWith(Context1));
        Assert.IsFalse(Context3.IsCompatibleWith(Context1));
        Assert.IsFalse(Context3.IsCompatibleWith(Context2));
    }

    [Test]
    [Category("Core")]
    public void CompilationContextTestTest_HashCodeComparison()
    {
        CompilationContext Context1 = new CompilationContext(10, false);
        CompilationContext Context2 = new CompilationContext(10, false);
        CompilationContext Context3 = new CompilationContext(11, false);

        Assert.IsTrue(Context1.IsCompatibleWith(Context1));
        Assert.IsTrue(Context2.IsCompatibleWith(Context2));
        Assert.IsTrue(Context3.IsCompatibleWith(Context3));

        Assert.IsTrue(Context1.IsCompatibleWith(Context2));
        Assert.IsFalse(Context1.IsCompatibleWith(Context3));
        Assert.IsFalse(Context2.IsCompatibleWith(Context3));
        Assert.IsTrue(Context2.IsCompatibleWith(Context1));
        Assert.IsFalse(Context3.IsCompatibleWith(Context1));
        Assert.IsFalse(Context3.IsCompatibleWith(Context2));
    }

    [Test]
    [Category("Core")]
    public void CompilationContextTestTest_IsAsyncRunRequestedComparison()
    {
        CompilationContext Context1 = new CompilationContext(10, false);
        CompilationContext Context2 = new CompilationContext(10, true);
        CompilationContext Context3 = new CompilationContext(11, true);

        Assert.IsFalse(Context1.IsCompatibleWith(Context2));
        Assert.IsFalse(Context1.IsCompatibleWith(Context3));
        Assert.IsFalse(Context2.IsCompatibleWith(Context3));
        Assert.IsTrue(Context2.IsCompatibleWith(Context1));
        Assert.IsFalse(Context3.IsCompatibleWith(Context1));
        Assert.IsFalse(Context3.IsCompatibleWith(Context2));
    }
}
