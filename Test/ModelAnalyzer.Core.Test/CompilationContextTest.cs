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

        Assert.That(Context1, Is.Not.EqualTo(Context2));
        Assert.That(Context1, Is.Not.EqualTo(Context3));
        Assert.That(Context2, Is.Not.EqualTo(Context3));
    }
}
