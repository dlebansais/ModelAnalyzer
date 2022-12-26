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
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_AddOrIncrement()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 0)));

        TestObject.AddOrIncrementName(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 1)));

        string TestName2 = "Test2";

        TestObject.AddOrIncrementName(TestName2);

        Assert.That(TestObject.ContainsName(TestName2), Is.True);
        Assert.That(TestObject.GetAlias(TestName2), Is.EqualTo(new AliasName(TestName2, 0)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Increment()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 0)));

        TestObject.IncrementNameAlias(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Clone()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 0)));

        TestObject.IncrementNameAlias(TestName);

        Assert.That(TestObject.ContainsName(TestName), Is.True);
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 1)));

        AliasTable CloneTable = TestObject.Clone();

        Assert.That(CloneTable.ContainsName(TestName), Is.True);
        Assert.That(CloneTable.GetAlias(TestName), Is.EqualTo(new AliasName(TestName, 1)));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_AliasDifference()
    {
        string TestName1 = "Test1";
        string TestName2 = "Test2";
        string TestName3 = "Test3";

        AliasTable TestObject1 = new();
        TestObject1.AddName(TestName1);
        TestObject1.AddName(TestName2);

        AliasTable TestObject2 = new();
        TestObject2.AddName(TestName1);
        TestObject2.AddName(TestName2);

        List<AliasName> Difference;

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(0));

        TestObject1.AddName(TestName3);

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(1));
        Assert.That(Difference[0], Is.EqualTo(new AliasName(TestName3, 0)));

        TestObject1.IncrementNameAlias(TestName1);
        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(2));
        Assert.That(Difference.Exists(aliasName => aliasName == new AliasName(TestName3, 0)), Is.True);
        Assert.That(Difference.Exists(aliasName => aliasName == new AliasName(TestName1, 1)), Is.True);

        Difference = TestObject2.GetAliasDifference(TestObject1);

        Assert.That(Difference.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Verification")]
    public void AliasTable_Merge()
    {
        string TestName1 = "Test1";
        string TestName2 = "Test2";
        string TestName3 = "Test3";

        AliasTable TestObject1 = new();
        TestObject1.AddName(TestName1);
        TestObject1.AddName(TestName2);

        AliasTable TestObject2 = new();
        TestObject2.AddName(TestName1);
        TestObject2.AddName(TestName2);

        Assert.That(TestObject1.ContainsName(TestName1), Is.True);
        Assert.That(TestObject1.GetAlias(TestName1), Is.EqualTo(new AliasName(TestName1, 0)));
        Assert.That(TestObject1.ContainsName(TestName2), Is.True);
        Assert.That(TestObject1.GetAlias(TestName2), Is.EqualTo(new AliasName(TestName2, 0)));

        Assert.That(TestObject2.ContainsName(TestName1), Is.True);
        Assert.That(TestObject2.GetAlias(TestName1), Is.EqualTo(new AliasName(TestName1, 0)));
        Assert.That(TestObject2.ContainsName(TestName2), Is.True);
        Assert.That(TestObject2.GetAlias(TestName2), Is.EqualTo(new AliasName(TestName2, 0)));

        List<string> UpdatedNameList;

        TestObject1.Merge(TestObject2, out UpdatedNameList);

        Assert.That(UpdatedNameList.Count, Is.EqualTo(0));

        TestObject1.IncrementNameAlias(TestName1);

        TestObject1.Merge(TestObject2, out UpdatedNameList);

        Assert.That(UpdatedNameList.Count, Is.EqualTo(1));
        Assert.That(UpdatedNameList[0], Is.EqualTo(TestName1));
        Assert.That(TestObject1.GetAlias(TestName1), Is.EqualTo(new AliasName(TestName1, 2)));

        TestObject2.AddName(TestName3);

        TestObject1.Merge(TestObject2, out UpdatedNameList);

        Assert.That(UpdatedNameList.Count, Is.EqualTo(1));
        Assert.That(UpdatedNameList[0], Is.EqualTo(TestName1));
        Assert.That(TestObject1.GetAlias(TestName1), Is.EqualTo(new AliasName(TestName1, 3)));
    }
}
