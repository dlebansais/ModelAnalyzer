namespace Miscellaneous.Test;

using ModelAnalyzer;
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
        ClassName ClassName = ClassName.FromSimpleString("Test");
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new(TestObjectName, ExpressionType.Integer) { Initializer = null, ClassName = ClassName };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        VariableAlias TestObject = new(TestVariable);

        Assert.That(TestObject.Variable, Is.EqualTo(TestVariable));
        Assert.That(TestObject, Is.EqualTo(new VariableAlias(TestVariable, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_Increment()
    {
        ClassName ClassName = ClassName.FromSimpleString("Test");
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new(TestObjectName, ExpressionType.Integer) { Initializer = null, ClassName = ClassName };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        VariableAlias TestObject = new(TestVariable);

        TestObject = TestObject.Incremented();

        Assert.That(TestObject.Variable, Is.EqualTo(TestVariable));
        Assert.That(TestObject, Is.EqualTo(new VariableAlias(TestVariable, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeNoIncrement()
    {
        ClassName ClassName = ClassName.FromSimpleString("Test");
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new(TestObjectName, ExpressionType.Integer) { Initializer = null, ClassName = ClassName };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        VariableAlias TestObject1 = new(TestVariable);
        VariableAlias TestObject2 = new(TestVariable);

        TestObject2 = TestObject2.Merged(TestObject1, out bool IsUpdated);

        Assert.That(IsUpdated, Is.False);
        Assert.That(TestObject1, Is.EqualTo(new VariableAlias(TestVariable, 0)));
        Assert.That(TestObject2, Is.EqualTo(new VariableAlias(TestVariable, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeSymetric()
    {
        ClassName ClassName = ClassName.FromSimpleString("Test");
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new(TestObjectName, ExpressionType.Integer) { Initializer = null, ClassName = ClassName };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        VariableAlias TestObject1 = new(TestVariable);
        VariableAlias TestObject2 = new(TestVariable);

        TestObject1 = TestObject1.Merged(TestObject2, out bool IsUpdated);

        Assert.That(IsUpdated, Is.False);
        Assert.That(TestObject1, Is.EqualTo(new VariableAlias(TestVariable, 0)));
        Assert.That(TestObject2, Is.EqualTo(new VariableAlias(TestVariable, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasName_MergeOneIncrement()
    {
        ClassName ClassName = ClassName.FromSimpleString("Test");
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new(TestObjectName, ExpressionType.Integer) { Initializer = null, ClassName = ClassName };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        VariableAlias TestObject1 = new(TestVariable);
        VariableAlias TestObject2 = new(TestVariable);

        TestObject1 = TestObject1.Incremented();
        TestObject2 = TestObject2.Merged(TestObject1, out bool IsUpdated);

        Assert.That(IsUpdated, Is.True);
        Assert.That(TestObject1, Is.EqualTo(new VariableAlias(TestVariable, 1)));
        Assert.That(TestObject2, Is.EqualTo(new VariableAlias(TestVariable, 1)));
    }
}
