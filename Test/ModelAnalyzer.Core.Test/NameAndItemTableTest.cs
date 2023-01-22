namespace Miscellaneous.Test;

using System.Collections.Generic;
using Core.Test;
using ModelAnalyzer;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="NameAndItemTable{TName, TItem}"/> class.
/// </summary>
public class NameAndItemTableTest
{
    [Test]
    [Category("Core")]
    public void NameAndItemTable_BasicTest()
    {
        NameAndItemTable<ItemNameTest, ItemTest> TestTable = new();

        Assert.That(TestTable.ContainsItem("*"), Is.False);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_AddItem()
    {
        NameAndItemTable<ItemNameTest, ItemTest> TestTable = new();
        ItemTest TestObject = new() { Name = "*" };

        TestTable.AddItem(TestObject);

        Assert.That(TestTable.ContainsItem("*"), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<ItemNameTest, ItemTest> Entry in TestTable)
            if (Entry.Key == "*" && Entry.Value == TestObject)
                IsFound = true;

        Assert.That(IsFound, Is.True);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_ReadOnlyFieldTable()
    {
        FieldTable TestTable = new();
        FieldName TestFieldName = new FieldName() { Text = "*" };
        Field TestField = new Field() { Name = TestFieldName, Type = ExpressionType.Other, Initializer = null };

        TestTable.AddItem(TestField);

        Assert.That(TestTable.ContainsItem(TestFieldName), Is.True);

        ReadOnlyFieldTable ReadOnlyTestTable = TestTable.AsReadOnly();

        Assert.That(ReadOnlyTestTable, Is.Not.EqualTo(ReadOnlyFieldTable.Empty));
        Assert.That(ReadOnlyTestTable.ContainsItem(TestFieldName), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<FieldName, Field> Entry in ReadOnlyTestTable)
            if (Entry.Key == TestFieldName && Entry.Value == TestField)
                IsFound = true;

        Assert.That(IsFound, Is.True);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_ReadOnlyMethodTable()
    {
        MethodTable TestTable = new();
        MethodName TestMethodName = new MethodName() { Text = "*" };
        Method TestMethod = new Method()
        {
            Name = TestMethodName,
            ClassName = string.Empty,
            AccessModifier = AccessModifier.Public,
            IsStatic = false,
            ParameterTable = ReadOnlyParameterTable.Empty,
            EnsureList = new(),
            RequireList = new(),
            LocalTable = ReadOnlyLocalTable.Empty,
            StatementList = new(),
            ReturnType = ExpressionType.Void,
        };

        TestTable.AddItem(TestMethod);

        Assert.That(TestTable.ContainsItem(TestMethodName), Is.True);

        ReadOnlyMethodTable ReadOnlyTestTable = TestTable.AsReadOnly();

        Assert.That(ReadOnlyTestTable, Is.Not.EqualTo(ReadOnlyMethodTable.Empty));
        Assert.That(ReadOnlyTestTable.ContainsItem(TestMethodName), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<MethodName, Method> Entry in ReadOnlyTestTable)
            if (Entry.Key == TestMethodName && Entry.Value == TestMethod)
                IsFound = true;

        Assert.That(IsFound, Is.True);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_ReadOnlyParameterTable()
    {
        ParameterTable TestTable = new();
        ParameterName TestParameterName = new ParameterName() { Text = "*" };
        Parameter TestParameter = new Parameter() { Name = TestParameterName, Type = ExpressionType.Other };

        TestTable.AddItem(TestParameter);

        Assert.That(TestTable.ContainsItem(TestParameterName), Is.True);

        ReadOnlyParameterTable ReadOnlyTestTable = TestTable.AsReadOnly();

        Assert.That(ReadOnlyTestTable, Is.Not.EqualTo(ReadOnlyParameterTable.Empty));
        Assert.That(ReadOnlyTestTable.ContainsItem(TestParameterName), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<ParameterName, Parameter> Entry in ReadOnlyTestTable)
            if (Entry.Key == TestParameterName && Entry.Value == TestParameter)
                IsFound = true;

        Assert.That(IsFound, Is.True);
    }
}
