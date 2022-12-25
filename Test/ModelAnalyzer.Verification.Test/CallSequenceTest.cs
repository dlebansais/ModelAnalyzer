namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="CallSequence"/> class.
/// </summary>
public class CallSequenceTest
{
    [Test]
    [Category("Verification")]
    public void CallSequence_BasicTest()
    {
        CallSequence TestObject = new();

        Assert.That(TestObject.IsEmpty);
        Assert.That(TestObject.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    [Category("Verification")]
    public void CallSequence_AddedCall()
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

    [Test]
    [Category("Verification")]
    public void CallSequence_Enumerate()
    {
        CallSequence TestObject = new();
        string MethodName = "Test";

        Method Method = new() { MethodName = new MethodName() { Name = MethodName }, ParameterTable = new(), EnsureList = new(), RequireList = new(), StatementList = new(), ReturnType = ExpressionType.Void };

        TestObject = TestObject.WithAddedCall(Method);

        Assert.That(!TestObject.IsEmpty);
        Assert.That(TestObject.ToString(), Is.EqualTo(MethodName));

        List<Method> MethodList = new();
        foreach (var Item in TestObject)
            MethodList.Add(Item);

        Assert.That(MethodList.Count, Is.EqualTo(1));
        Assert.That(MethodList[0], Is.EqualTo(Method));
    }
}
