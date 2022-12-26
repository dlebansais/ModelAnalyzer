namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="AliasName"/> class.
/// </summary>
public class AliasNameTest
{
    [Test]
    [Category("Verification")]
    public void AliasName_BasicTest()
    {
        string TestObjectName = "Test";
        AliasName TestObject = new(TestObjectName);

        Assert.That(TestObject.VariableName, Is.EqualTo(TestObjectName));
        Assert.That(TestObject, Is.EqualTo(new AliasName(TestObjectName, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_Increment()
    {
        string TestObjectName = "Test";
        AliasName TestObject = new(TestObjectName);

        TestObject = TestObject.Incremented();

        Assert.That(TestObject.VariableName, Is.EqualTo(TestObjectName));
        Assert.That(TestObject, Is.EqualTo(new AliasName(TestObjectName, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeNoIncrement()
    {
        string TestObjectName = "Test";
        AliasName TestObject1 = new(TestObjectName);
        AliasName TestObject2 = new(TestObjectName);

        TestObject2 = TestObject2.Merged(TestObject1, out bool IsUpdated);

        Assert.That(IsUpdated, Is.False);
        Assert.That(TestObject1, Is.EqualTo(new AliasName(TestObjectName, 0)));
        Assert.That(TestObject2, Is.EqualTo(new AliasName(TestObjectName, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeSymetric()
    {
        string TestObjectName = "Test";
        AliasName TestObject1 = new(TestObjectName);
        AliasName TestObject2 = new(TestObjectName);

        TestObject1 = TestObject1.Merged(TestObject2, out bool IsUpdated);

        Assert.That(IsUpdated, Is.False);
        Assert.That(TestObject1, Is.EqualTo(new AliasName(TestObjectName, 0)));
        Assert.That(TestObject2, Is.EqualTo(new AliasName(TestObjectName, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeOneIncrement()
    {
        string TestObjectName = "Test";
        AliasName TestObject1 = new(TestObjectName);
        AliasName TestObject2 = new(TestObjectName);

        TestObject1 = TestObject1.Incremented();
        TestObject2 = TestObject2.Merged(TestObject1, out bool IsUpdated);

        Assert.That(IsUpdated, Is.True);
        Assert.That(TestObject1, Is.EqualTo(new AliasName(TestObjectName, 1)));
        Assert.That(TestObject2, Is.EqualTo(new AliasName(TestObjectName, 1)));
    }
}
