namespace Miscellaneous.Test;

using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="CompilationContext"/> class.
/// </summary>
public class CompilationContextTest
{
    [Test]
    [Category("Core")]
    public void CompilationContext_BasicTest()
    {
        CompilationContext Context1 = CompilationContext.Default;
        CompilationContext Context2 = CompilationContext.GetAnother();
        CompilationContext Context3 = CompilationContext.GetAnother();

        Assert.That(Context1.IsCompatibleWith(Context1), Is.True);
        Assert.That(Context2.IsCompatibleWith(Context2), Is.True);
        Assert.That(Context3.IsCompatibleWith(Context3), Is.True);

        Assert.That(Context1.IsCompatibleWith(Context2), Is.False);
        Assert.That(Context1.IsCompatibleWith(Context3), Is.False);
        Assert.That(Context2.IsCompatibleWith(Context3), Is.False);
        Assert.That(Context2.IsCompatibleWith(Context1), Is.False);
        Assert.That(Context3.IsCompatibleWith(Context1), Is.False);
        Assert.That(Context3.IsCompatibleWith(Context2), Is.False);
    }

    [Test]
    [Category("Core")]
    public void CompilationContext_HashCodeComparison()
    {
        CompilationContext Context1 = new CompilationContext(10, false);
        CompilationContext Context2 = new CompilationContext(10, false);
        CompilationContext Context3 = new CompilationContext(11, false);

        Assert.That(Context1.IsCompatibleWith(Context1), Is.True);
        Assert.That(Context2.IsCompatibleWith(Context2), Is.True);
        Assert.That(Context3.IsCompatibleWith(Context3), Is.True);

        Assert.That(Context1.IsCompatibleWith(Context2), Is.True);
        Assert.That(Context1.IsCompatibleWith(Context3), Is.False);
        Assert.That(Context2.IsCompatibleWith(Context3), Is.False);
        Assert.That(Context2.IsCompatibleWith(Context1), Is.True);
        Assert.That(Context3.IsCompatibleWith(Context1), Is.False);
        Assert.That(Context3.IsCompatibleWith(Context2), Is.False);
    }

    [Test]
    [Category("Core")]
    public void CompilationContext_IsAsyncRunRequestedComparison()
    {
        CompilationContext Context1 = new CompilationContext(10, false);
        CompilationContext Context2 = new CompilationContext(10, true);
        CompilationContext Context3 = new CompilationContext(11, true);

        Assert.That(Context1.IsCompatibleWith(Context2), Is.False);
        Assert.That(Context1.IsCompatibleWith(Context3), Is.False);
        Assert.That(Context2.IsCompatibleWith(Context3), Is.False);
        Assert.That(Context2.IsCompatibleWith(Context1), Is.True);
        Assert.That(Context3.IsCompatibleWith(Context1), Is.False);
        Assert.That(Context3.IsCompatibleWith(Context2), Is.False);
    }
}
