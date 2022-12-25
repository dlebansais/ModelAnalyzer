namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

public class CallSequenceTest
{
    [Test]
    [Category("Verifier")]
    public void BasicTest()
    {
        CallSequence TestObject = new();

        Assert.That(TestObject.IsEmpty);
        Assert.That(TestObject.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    [Category("Verifier")]
    public void CallSequenceTest_AddedCall()
    {
        CallSequence TestObject0 = new();
        string MethodName1 = "Test1";
        string MethodName2 = "Test2";

        Method Method1 = new() { MethodName = new MethodName() { Name = MethodName1 }, ParameterTable = new(), EnsureList = new(), RequireList = new(), StatementList = new(), ReturnType = ExpressionType.Void };

        CallSequence TestObject1 = TestObject0.WithAddedCall(Method1);

        Assert.That(!TestObject1.IsEmpty);
        Assert.That(TestObject1.ToString(), Is.EqualTo(MethodName1));

        Method Method2 = new() { MethodName = new MethodName() { Name = MethodName2 }, ParameterTable = new(), EnsureList = new(), RequireList = new(), StatementList = new(), ReturnType = ExpressionType.Void };

        CallSequence TestObject2 = TestObject1.WithAddedCall(Method2);

        Assert.That(!TestObject2.IsEmpty);
        Assert.That(TestObject2.ToString(), Is.EqualTo($"{MethodName1}, {MethodName2}"));

        Assert.That(TestObject0.IsEmpty);
        Assert.That(TestObject0.ToString(), Is.EqualTo(string.Empty));
    }
}
