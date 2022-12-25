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

        Assert.IsFalse(TestTable.IsSealed);
        Assert.That(TestTable.ContainsItem("*"), Is.False);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_AddItem()
    {
        NameAndItemTable<string, int> TestTable = new();

        TestTable.AddItem("*", 0);

        Assert.IsFalse(TestTable.IsSealed);
        Assert.That(TestTable.ContainsItem("*"), Is.True);

        bool IsFound = false;
        foreach (KeyValuePair<string, int> Entry in TestTable)
            if (Entry.Key == "*" && Entry.Value == 0)
                IsFound = true;

        Assert.IsTrue(IsFound);
    }

    [Test]
    [Category("Core")]
    public void NameAndItemTable_Sealing()
    {
        NameAndItemTable<string, int> TestTable = new();

        TestTable.AddItem("*", 0);

        Assert.IsFalse(TestTable.IsSealed);
        Assert.That(TestTable.ContainsItem("*"), Is.True);

        TestTable.Seal();

        Assert.IsTrue(TestTable.IsSealed);

        Assert.Throws<InvalidOperationException>(() => TestTable.AddItem("+", 0));
    }
}
