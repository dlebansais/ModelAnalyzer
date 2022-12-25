﻿namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

public class AliasNameTest
{
    [Test]
    [Category("Verifier")]
    public void BasicTest()
    {
        string TestObjectName = "Test";
        AliasName TestObject = new() { VariableName = TestObjectName };

        Assert.That(TestObject.VariableName, Is.EqualTo(TestObjectName));
        Assert.That(TestObject.Alias, Is.EqualTo($"{TestObjectName}_0"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasNameTest_Increment()
    {
        string TestObjectName = "Test";
        AliasName TestObject = new() { VariableName = TestObjectName };

        TestObject.Increment();

        Assert.That(TestObject.VariableName, Is.EqualTo(TestObjectName));
        Assert.That(TestObject.Alias, Is.EqualTo($"{TestObjectName}_1"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasNameTest_MergeNoIncrement()
    {
        string TestObjectName = "Test";
        AliasName TestObject1 = new() { VariableName = TestObjectName };
        AliasName TestObject2 = new() { VariableName = TestObjectName };

        TestObject2.Merge(TestObject1, out bool IsUpdated);

        Assert.IsFalse(IsUpdated);
        Assert.That(TestObject1.Alias, Is.EqualTo($"{TestObjectName}_0"));
        Assert.That(TestObject2.Alias, Is.EqualTo($"{TestObjectName}_0"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasNameTest_MergeSymetric()
    {
        string TestObjectName = "Test";
        AliasName TestObject1 = new() { VariableName = TestObjectName };
        AliasName TestObject2 = new() { VariableName = TestObjectName };

        TestObject1.Merge(TestObject2, out bool IsUpdated);

        Assert.IsFalse(IsUpdated);
        Assert.That(TestObject1.Alias, Is.EqualTo($"{TestObjectName}_0"));
        Assert.That(TestObject2.Alias, Is.EqualTo($"{TestObjectName}_0"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasNameTest_MergeOneIncrement()
    {
        string TestObjectName = "Test";
        AliasName TestObject1 = new() { VariableName = TestObjectName };
        AliasName TestObject2 = new() { VariableName = TestObjectName };

        TestObject1.Increment();

        TestObject2.Merge(TestObject1, out bool IsUpdated);

        Assert.IsTrue(IsUpdated);
        Assert.That(TestObject1.Alias, Is.EqualTo($"{TestObjectName}_1"));
        Assert.That(TestObject2.Alias, Is.EqualTo($"{TestObjectName}_1"));
    }
}