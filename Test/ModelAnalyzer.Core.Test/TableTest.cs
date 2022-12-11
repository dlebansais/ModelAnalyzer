namespace ModelAnalyzer.Core.Test;

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class TableTest
{
    [Test]
    [Category("Core")]
    public void BasicTest()
    {
        NameAndItemTable<string, int> TestTable = new();

        Assert.IsFalse(TestTable.IsSealed);
        Assert.That(TestTable.ContainsItem("*"), Is.False);
    }

    [Test]
    [Category("Core")]
    public void TableTest_AddItem()
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

        // Test the IEnumerable interface.
        IsFound = false;
        foreach (var Item in (IEnumerable)TestTable)
            if (Item is KeyValuePair<string, int> Entry && Entry.Key == "*" && Entry.Value == 0)
                IsFound = true;

        Assert.IsTrue(IsFound);
    }

    [Test]
    [Category("Core")]
    public void TableTest_Sealing()
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
