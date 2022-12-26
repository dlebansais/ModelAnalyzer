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
        FieldName TestObjectName = new() { Name = "Test" };
        Field TestField = new Field() { FieldName = TestObjectName, VariableType = ExpressionType.Integer, Initializer = null };
        AliasTable TestObject = new();
        TestObject.AddVariable(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_AddOrIncrement()
    {
        FieldName TestObjectName = new() { Name = "Test" };
        Field TestField = new Field() { FieldName = TestObjectName, VariableType = ExpressionType.Integer, Initializer = null };
        AliasTable TestObject = new();
        TestObject.AddVariable(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 0)));

        TestObject.AddOrIncrement(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 1)));

        FieldName TestObjectName2 = new() { Name = "Test2" };
        Field TestField2 = new Field() { FieldName = TestObjectName2, VariableType = ExpressionType.Integer, Initializer = null };

        TestObject.AddOrIncrement(TestField2);

        Assert.That(TestObject.ContainsVariable(TestField2), Is.True);
        Assert.That(TestObject.GetAlias(TestField2), Is.EqualTo(new AliasName(TestField2, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Increment()
    {
        FieldName TestObjectName = new() { Name = "Test" };
        Field TestField = new Field() { FieldName = TestObjectName, VariableType = ExpressionType.Integer, Initializer = null };
        AliasTable TestObject = new();
        TestObject.AddVariable(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 0)));

        TestObject.IncrementNameAlias(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Clone()
    {
        FieldName TestObjectName = new() { Name = "Test" };
        Field TestField = new Field() { FieldName = TestObjectName, VariableType = ExpressionType.Integer, Initializer = null };
        AliasTable TestObject = new();
        TestObject.AddVariable(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 0)));

        TestObject.IncrementNameAlias(TestField);

        Assert.That(TestObject.ContainsVariable(TestField), Is.True);
        Assert.That(TestObject.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 1)));

        AliasTable CloneTable = TestObject.Clone();

        Assert.That(CloneTable.ContainsVariable(TestField), Is.True);
        Assert.That(CloneTable.GetAlias(TestField), Is.EqualTo(new AliasName(TestField, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_AliasDifference()
    {
        FieldName TestObjectName1 = new() { Name = "Test1" };
        Field TestField1 = new Field() { FieldName = TestObjectName1, VariableType = ExpressionType.Integer, Initializer = null };
        FieldName TestObjectName2 = new() { Name = "Test2" };
        Field TestField2 = new Field() { FieldName = TestObjectName2, VariableType = ExpressionType.Integer, Initializer = null };
        FieldName TestObjectName3 = new() { Name = "Test3" };
        Field TestField3 = new Field() { FieldName = TestObjectName3, VariableType = ExpressionType.Integer, Initializer = null };

        AliasTable TestObject1 = new();
        TestObject1.AddVariable(TestField1);
        TestObject1.AddVariable(TestField2);

        AliasTable TestObject2 = new();
        TestObject2.AddVariable(TestField1);
        TestObject2.AddVariable(TestField2);

        List<AliasName> Difference;

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(0));

        TestObject1.AddVariable(TestField3);

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(1));
        Assert.That(Difference[0], Is.EqualTo(new AliasName(TestField3, 0)));

        TestObject1.IncrementNameAlias(TestField1);
        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(2));
        Assert.That(Difference.Exists(aliasName => aliasName == new AliasName(TestField3, 0)), Is.True);
        Assert.That(Difference.Exists(aliasName => aliasName == new AliasName(TestField1, 1)), Is.True);

        Difference = TestObject2.GetAliasDifference(TestObject1);

        Assert.That(Difference.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Merge()
    {
        FieldName TestObjectName1 = new() { Name = "Test1" };
        Field TestField1 = new Field() { FieldName = TestObjectName1, VariableType = ExpressionType.Integer, Initializer = null };
        FieldName TestObjectName2 = new() { Name = "Test2" };
        Field TestField2 = new Field() { FieldName = TestObjectName2, VariableType = ExpressionType.Integer, Initializer = null };
        FieldName TestObjectName3 = new() { Name = "Test3" };
        Field TestField3 = new Field() { FieldName = TestObjectName3, VariableType = ExpressionType.Integer, Initializer = null };

        AliasTable TestObject1 = new();
        TestObject1.AddVariable(TestField1);
        TestObject1.AddVariable(TestField2);

        AliasTable TestObject2 = new();
        TestObject2.AddVariable(TestField1);
        TestObject2.AddVariable(TestField2);

        Assert.That(TestObject1.ContainsVariable(TestField1), Is.True);
        Assert.That(TestObject1.GetAlias(TestField1), Is.EqualTo(new AliasName(TestField1, 0)));
        Assert.That(TestObject1.ContainsVariable(TestField2), Is.True);
        Assert.That(TestObject1.GetAlias(TestField2), Is.EqualTo(new AliasName(TestField2, 0)));

        Assert.That(TestObject2.ContainsVariable(TestField1), Is.True);
        Assert.That(TestObject2.GetAlias(TestField1), Is.EqualTo(new AliasName(TestField1, 0)));
        Assert.That(TestObject2.ContainsVariable(TestField2), Is.True);
        Assert.That(TestObject2.GetAlias(TestField2), Is.EqualTo(new AliasName(TestField2, 0)));

        List<IVariable> UpdatedVariableList;

        TestObject1.Merge(TestObject2, out UpdatedVariableList);

        Assert.That(UpdatedVariableList.Count, Is.EqualTo(0));

        TestObject1.IncrementNameAlias(TestField1);

        TestObject1.Merge(TestObject2, out UpdatedVariableList);

        Assert.That(UpdatedVariableList.Count, Is.EqualTo(1));
        Assert.That(UpdatedVariableList[0], Is.EqualTo(TestField1));
        Assert.That(TestObject1.GetAlias(TestField1), Is.EqualTo(new AliasName(TestField1, 2)));

        TestObject2.AddVariable(TestField3);

        TestObject1.Merge(TestObject2, out UpdatedVariableList);

        Assert.That(UpdatedVariableList.Count, Is.EqualTo(1));
        Assert.That(UpdatedVariableList[0], Is.EqualTo(TestField1));
        Assert.That(TestObject1.GetAlias(TestField1), Is.EqualTo(new AliasName(TestField1, 3)));
    }
}
