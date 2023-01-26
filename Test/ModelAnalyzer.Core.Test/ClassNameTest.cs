namespace Miscellaneous.Test;

using System.Collections.Generic;
using Core.Test;
using ModelAnalyzer;
using Newtonsoft.Json;
using NUnit.Framework;

/// <summary>
/// Tests for the <see cref="ClassNameTest"/> class.
/// </summary>
public class ClassNameTest
{
    [Test]
    [Category("Core")]
    public void ClassNameTest_BasicTest()
    {
        ClassName TestName0 = ClassName.Empty;

        Assert.That(TestName0.Namespace.Count, Is.EqualTo(0));
        Assert.That(TestName0.Text, Is.Empty);
        Assert.That(TestName0.ToString(), Is.Empty);

        ClassName TestName1 = ClassName.FromSimpleString(string.Empty);

        Assert.That(TestName1.Namespace.Count, Is.EqualTo(0));
        Assert.That(TestName1.Text, Is.Empty);
        Assert.That(TestName1.ToString(), Is.Empty);

        Assert.That(TestName1, Is.EqualTo(ClassName.Empty));

        string TestString = "test";
        ClassName TestName2 = ClassName.FromSimpleString(TestString);

        Assert.That(TestName2.Namespace.Count, Is.EqualTo(0));
        Assert.That(TestName2.Text, Is.EqualTo(TestString));
        Assert.That(TestName2.ToString(), Is.EqualTo(TestString));
    }

    [Test]
    [Category("Core")]
    public void ClassNameTest_WithPath()
    {
        string TestString = "test";
        string NamespaceString0 = "namespace0";
        string NamespaceString1 = "namespace1";

        ClassName TestName0 = new() { Namespace = new List<string>() { NamespaceString0 }, Text = TestString };

        Assert.That(TestName0.Namespace.Count, Is.EqualTo(1));
        Assert.That(TestName0.Namespace[0], Is.EqualTo(NamespaceString0));
        Assert.That(TestName0.Text, Is.EqualTo(TestString));
        Assert.That(TestName0.ToString(), Is.EqualTo($"{NamespaceString0}.{TestString}"));

        ClassName TestName1 = new() { Namespace = new List<string>() { NamespaceString0, NamespaceString1 }, Text = TestString };

        Assert.That(TestName1.Namespace.Count, Is.EqualTo(2));
        Assert.That(TestName1.Namespace[0], Is.EqualTo(NamespaceString0));
        Assert.That(TestName1.Namespace[1], Is.EqualTo(NamespaceString1));
        Assert.That(TestName1.Text, Is.EqualTo(TestString));
        Assert.That(TestName1.ToString(), Is.EqualTo($"{NamespaceString0}.{NamespaceString1}.{TestString}"));
    }

    [Test]
    [Category("Core")]
    public void ClassNameTest_Json()
    {
        string TestString = "test";
        string NamespaceString0 = "namespace0";
        string JsonString;
        ClassName? Result;

        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassNameConverter());

        ClassName TestName0 = ClassName.Empty;

        JsonString = JsonConvert.SerializeObject(TestName0, Settings);

        Result = JsonConvert.DeserializeObject<ClassName>(JsonString, Settings);

        Assert.That(Result, Is.Not.Null);
        Assert.That(Result!, Is.EqualTo(TestName0));

        ClassName TestName1 = ClassName.FromSimpleString(TestString);

        JsonString = JsonConvert.SerializeObject(TestName1, Settings);

        Result = JsonConvert.DeserializeObject<ClassName>(JsonString, Settings);

        Assert.That(Result, Is.Not.Null);
        Assert.That(Result!, Is.EqualTo(TestName1));

        ClassName TestName2 = new() { Namespace = new List<string>() { NamespaceString0 }, Text = TestString };

        JsonString = JsonConvert.SerializeObject(TestName2, Settings);

        Result = JsonConvert.DeserializeObject<ClassName>(JsonString, Settings);

        Assert.That(Result, Is.Not.Null);
        Assert.That(Result!, Is.EqualTo(TestName2));
        Assert.That(Result!.GetHashCode(), Is.EqualTo(TestName2.GetHashCode()));
        Assert.That(Result! == TestName2, Is.True);
        Assert.That(Result! != TestName2, Is.False);
    }
}
