namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections.Generic;
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
        NameAndItemTable<string, int> TestTable = new();

        Assert.That(TestTable.ContainsItem("*"), Is.False);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_AddItem()
    {
        NameAndItemTable<string, int> TestTable = new();

        TestTable.AddItem("*", 0);

        Assert.That(TestTable.ContainsItem("*"), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<string, int> Entry in TestTable)
            if (Entry.Key == "*" && Entry.Value == 0)
                IsFound = true;

        Assert.That(IsFound, Is.True);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_ReadOnlyFieldTable()
    {
        FieldTable TestTable = new();
        FieldName TestFieldName = new FieldName() { Text = "*" };
        Field TestField = new Field() { FieldName = TestFieldName, VariableType = ExpressionType.Other, Initializer = null };

        TestTable.AddItem(TestFieldName, TestField);

        Assert.That(TestTable.ContainsItem(TestFieldName), Is.True);

        ReadOnlyFieldTable ReadOnlyTestTable = TestTable.ToReadOnly();

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
            MethodName = TestMethodName,
            ParameterTable = ReadOnlyParameterTable.Empty,
            EnsureList = new(),
            RequireList = new(),
            StatementList = new(),
            ReturnType = ExpressionType.Void,
        };

        TestTable.AddItem(TestMethodName, TestMethod);

        Assert.That(TestTable.ContainsItem(TestMethodName), Is.True);

        ReadOnlyMethodTable ReadOnlyTestTable = TestTable.ToReadOnly();

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
        Parameter TestParameter = new Parameter() { ParameterName = TestParameterName, VariableType = ExpressionType.Other };

        TestTable.AddItem(TestParameterName, TestParameter);

        Assert.That(TestTable.ContainsItem(TestParameterName), Is.True);

        ReadOnlyParameterTable ReadOnlyTestTable = TestTable.ToReadOnly();

        Assert.That(ReadOnlyTestTable, Is.Not.EqualTo(ReadOnlyParameterTable.Empty));
        Assert.That(ReadOnlyTestTable.ContainsItem(TestParameterName), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<ParameterName, Parameter> Entry in ReadOnlyTestTable)
            if (Entry.Key == TestParameterName && Entry.Value == TestParameter)
                IsFound = true;

        Assert.That(IsFound, Is.True);
    }
}
