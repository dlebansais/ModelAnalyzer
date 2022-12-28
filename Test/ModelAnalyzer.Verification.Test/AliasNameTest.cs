namespace ModelAnalyzer.Verification.Test;

using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="VariableAlias"/> class.
/// </summary>
public class AliasNameTest
{
    [Test]
    [Category("Verification")]
    public void AliasName_BasicTest()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        VariableAlias TestObject = new(TestField);

        Assert.That(TestObject.Variable, Is.EqualTo(TestField));
        Assert.That(TestObject, Is.EqualTo(new VariableAlias(TestField, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_Increment()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        VariableAlias TestObject = new(TestField);

        TestObject = TestObject.Incremented();

        Assert.That(TestObject.Variable, Is.EqualTo(TestField));
        Assert.That(TestObject, Is.EqualTo(new VariableAlias(TestField, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeNoIncrement()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        VariableAlias TestObject1 = new(TestField);
        VariableAlias TestObject2 = new(TestField);

        TestObject2 = TestObject2.Merged(TestObject1, out bool IsUpdated);

        Assert.That(IsUpdated, Is.False);
        Assert.That(TestObject1, Is.EqualTo(new VariableAlias(TestField, 0)));
        Assert.That(TestObject2, Is.EqualTo(new VariableAlias(TestField, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeSymetric()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        VariableAlias TestObject1 = new(TestField);
        VariableAlias TestObject2 = new(TestField);

        TestObject1 = TestObject1.Merged(TestObject2, out bool IsUpdated);

        Assert.That(IsUpdated, Is.False);
        Assert.That(TestObject1, Is.EqualTo(new VariableAlias(TestField, 0)));
        Assert.That(TestObject2, Is.EqualTo(new VariableAlias(TestField, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeOneIncrement()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        VariableAlias TestObject1 = new(TestField);
        VariableAlias TestObject2 = new(TestField);

        TestObject1 = TestObject1.Incremented();
        TestObject2 = TestObject2.Merged(TestObject1, out bool IsUpdated);

        Assert.That(IsUpdated, Is.True);
        Assert.That(TestObject1, Is.EqualTo(new VariableAlias(TestField, 1)));
        Assert.That(TestObject2, Is.EqualTo(new VariableAlias(TestField, 1)));
    }
}
