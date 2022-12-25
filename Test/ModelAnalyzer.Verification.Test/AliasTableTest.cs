namespace ModelAnalyzer.Verification.Test;

using System.Collections.Generic;
using NUnit.Framework;

public class AliasTableTest
{
    [Test]
    [Category("Verifier")]
    public void BasicTest()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_0"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasTableTest_AddOrIncrement()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_0"));

        TestObject.AddOrIncrementName(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_1"));

        string TestName2 = "Test2";

        TestObject.AddOrIncrementName(TestName2);

        Assert.That(TestObject.ContainsName(TestName2));
        Assert.That(TestObject.GetAlias(TestName2), Is.EqualTo($"{TestName2}_0"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasTableTest_Increment()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_0"));

        TestObject.IncrementNameAlias(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_1"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasTableTest_Clone()
    {
        string TestName = "Test";
        AliasTable TestObject = new();
        TestObject.AddName(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_0"));

        TestObject.IncrementNameAlias(TestName);

        Assert.That(TestObject.ContainsName(TestName));
        Assert.That(TestObject.GetAlias(TestName), Is.EqualTo($"{TestName}_1"));

        AliasTable CloneTable = TestObject.Clone();

        Assert.That(CloneTable.ContainsName(TestName));
        Assert.That(CloneTable.GetAlias(TestName), Is.EqualTo($"{TestName}_1"));
    }

    [Test]
    [Category("Verifier")]
    public void AliasTableTest_AliasDifference()
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

        List<string> Difference;

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(0));

        TestObject1.AddName(TestName3);

        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(1));
        Assert.That(Difference[0], Is.EqualTo($"{TestName3}_0"));

        TestObject1.IncrementNameAlias(TestName1);
        Difference = TestObject1.GetAliasDifference(TestObject2);

        Assert.That(Difference.Count, Is.EqualTo(2));
        Assert.That(Difference.Contains($"{TestName3}_0"));
        Assert.That(Difference.Contains($"{TestName1}_1"));

        Difference = TestObject2.GetAliasDifference(TestObject1);

        Assert.That(Difference.Count, Is.EqualTo(0));
    }

    [Test]
    [Category("Verifier")]
    public void AliasTableTest_Merge()
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

        Assert.That(TestObject1.ContainsName(TestName1));
        Assert.That(TestObject1.GetAlias(TestName1), Is.EqualTo($"{TestName1}_0"));
        Assert.That(TestObject1.ContainsName(TestName2));
        Assert.That(TestObject1.GetAlias(TestName2), Is.EqualTo($"{TestName2}_0"));

        Assert.That(TestObject2.ContainsName(TestName1));
        Assert.That(TestObject2.GetAlias(TestName1), Is.EqualTo($"{TestName1}_0"));
        Assert.That(TestObject2.ContainsName(TestName2));
        Assert.That(TestObject2.GetAlias(TestName2), Is.EqualTo($"{TestName2}_0"));

        List<string> UpdatedNameList;

        TestObject1.Merge(TestObject2, out UpdatedNameList);

        Assert.That(UpdatedNameList.Count, Is.EqualTo(0));

        TestObject1.IncrementNameAlias(TestName1);

        TestObject1.Merge(TestObject2, out UpdatedNameList);

        Assert.That(UpdatedNameList.Count, Is.EqualTo(1));
        Assert.That(UpdatedNameList[0], Is.EqualTo(TestName1));
        Assert.That(TestObject1.GetAlias(TestName1), Is.EqualTo($"{TestName1}_2"));

        TestObject2.AddName(TestName3);

        TestObject1.Merge(TestObject2, out UpdatedNameList);

        Assert.That(UpdatedNameList.Count, Is.EqualTo(1));
        Assert.That(UpdatedNameList[0], Is.EqualTo(TestName1));
        Assert.That(TestObject1.GetAlias(TestName1), Is.EqualTo($"{TestName1}_3"));
    }
}
