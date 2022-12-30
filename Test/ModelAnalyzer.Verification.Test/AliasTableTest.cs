namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="AliasTable"/> class.
/// </summary>
public class AliasTableTest
{
    [Test]
    [Category("Verification")]
    public void AliasTable_BasicTest()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        AliasTable TestObject = new();
        TestObject.AddVariable(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_AddOrIncrement()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        AliasTable TestObject = new();
        TestObject.AddVariable(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 0)));

        TestObject.AddOrIncrement(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 1)));

        FieldName TestObjectName2 = new() { Text = "Test2" };
        Field TestField2 = new Field() { Name = TestObjectName2, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable2 = new Variable(TestField2.Name, TestField2.Type);

        TestObject.AddOrIncrement(TestVariable2);

        Assert.That(TestObject.ContainsVariable(TestVariable2), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable2), Is.EqualTo(new VariableAlias(TestVariable2, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Increment()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        AliasTable TestObject = new();
        TestObject.AddVariable(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 0)));

        TestObject.IncrementAlias(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Clone()
    {
        FieldName TestObjectName = new() { Text = "Test" };
        Field TestField = new Field() { Name = TestObjectName, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable = new Variable(TestField.Name, TestField.Type);
        AliasTable TestObject = new();
        TestObject.AddVariable(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 0)));

        TestObject.IncrementAlias(TestVariable);

        Assert.That(TestObject.ContainsVariable(TestVariable), Is.True);
        Assert.That(TestObject.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 1)));

        AliasTable CloneTable = TestObject.Clone();

        Assert.That(CloneTable.ContainsVariable(TestVariable), Is.True);
        Assert.That(CloneTable.GetAlias(TestVariable), Is.EqualTo(new VariableAlias(TestVariable, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_AliasDifference()
    {
        FieldName TestObjectName1 = new() { Text = "Test1" };
        Field TestField1 = new Field() { Name = TestObjectName1, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable1 = new Variable(TestField1.Name, TestField1.Type);
        FieldName TestObjectName2 = new() { Text = "Test2" };
        Field TestField2 = new Field() { Name = TestObjectName2, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable2 = new Variable(TestField2.Name, TestField2.Type);
        FieldName TestObjectName3 = new() { Text = "Test3" };
        Field TestField3 = new Field() { Name = TestObjectName3, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable3 = new Variable(TestField3.Name, TestField3.Type);

        AliasTable TestObject1 = new();
        TestObject1.AddVariable(TestVariable1);
        TestObject1.AddVariable(TestVariable2);

        AliasTable TestObject2 = new();
        TestObject2.AddVariable(TestVariable1);
        TestObject2.AddVariable(TestVariable2);

        List<VariableAlias> Difference;

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(0));

        TestObject1.AddVariable(TestVariable3);

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(1));
        Assert.That(Difference[0], Is.EqualTo(new VariableAlias(TestVariable3, 0)));

        TestObject1.IncrementAlias(TestVariable1);
        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(2));
        Assert.That(Difference.Exists(aliasName => aliasName == new VariableAlias(TestVariable3, 0)), Is.True);
        Assert.That(Difference.Exists(aliasName => aliasName == new VariableAlias(TestVariable1, 1)), Is.True);

        Difference = TestObject2.GetAliasDifference(TestObject1);

        Assert.That(Difference.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Merge()
    {
        FieldName TestObjectName1 = new() { Text = "Test1" };
        Field TestField1 = new Field() { Name = TestObjectName1, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable1 = new Variable(TestField1.Name, TestField1.Type);
        FieldName TestObjectName2 = new() { Text = "Test2" };
        Field TestField2 = new Field() { Name = TestObjectName2, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable2 = new Variable(TestField2.Name, TestField2.Type);
        FieldName TestObjectName3 = new() { Text = "Test3" };
        Field TestField3 = new Field() { Name = TestObjectName3, Type = ExpressionType.Integer, Initializer = null };
        Variable TestVariable3 = new Variable(TestField3.Name, TestField3.Type);

        AliasTable TestObject1 = new();
        TestObject1.AddVariable(TestVariable1);
        TestObject1.AddVariable(TestVariable2);

        AliasTable TestObject2 = new();
        TestObject2.AddVariable(TestVariable1);
        TestObject2.AddVariable(TestVariable2);

        Assert.That(TestObject1.ContainsVariable(TestVariable1), Is.True);
        Assert.That(TestObject1.GetAlias(TestVariable1), Is.EqualTo(new VariableAlias(TestVariable1, 0)));
        Assert.That(TestObject1.ContainsVariable(TestVariable2), Is.True);
        Assert.That(TestObject1.GetAlias(TestVariable2), Is.EqualTo(new VariableAlias(TestVariable2, 0)));

        Assert.That(TestObject2.ContainsVariable(TestVariable1), Is.True);
        Assert.That(TestObject2.GetAlias(TestVariable1), Is.EqualTo(new VariableAlias(TestVariable1, 0)));
        Assert.That(TestObject2.ContainsVariable(TestVariable2), Is.True);
        Assert.That(TestObject2.GetAlias(TestVariable2), Is.EqualTo(new VariableAlias(TestVariable2, 0)));

        List<Variable> UpdatedVariableList;

        TestObject1.Merge(TestObject2, out UpdatedVariableList);

        Assert.That(UpdatedVariableList.Count, Is.EqualTo(0));

        TestObject1.IncrementAlias(TestVariable1);

        TestObject1.Merge(TestObject2, out UpdatedVariableList);

        Assert.That(UpdatedVariableList.Count, Is.EqualTo(1));
        Assert.That(UpdatedVariableList[0], Is.EqualTo(TestVariable1));
        Assert.That(TestObject1.GetAlias(TestVariable1), Is.EqualTo(new VariableAlias(TestVariable1, 2)));

        TestObject2.AddVariable(TestVariable3);

        TestObject1.Merge(TestObject2, out UpdatedVariableList);

        Assert.That(UpdatedVariableList.Count, Is.EqualTo(1));
        Assert.That(UpdatedVariableList[0], Is.EqualTo(TestVariable1));
        Assert.That(TestObject1.GetAlias(TestVariable1), Is.EqualTo(new VariableAlias(TestVariable1, 3)));
    }
}
