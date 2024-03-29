﻿namespace Return.Test;

using System;
using ModelAnalyzer;
using NUnit.Framework;
using Verification.Test;

/// <summary>
/// Tests for the <see cref="Verifier"/> class.
/// </summary>
public partial class VerifierTest
{
    private const string ReturnSourceCodeBoolean1 = @"
using System;

class Program_Verifier_Boolean1
{
    bool X;

    public bool Read()
    {
        return X;
    }
}
";

    private const string ReturnSourceCodeBoolean2 = @"
using System;

class Program_Verifier_Boolean2
{
    bool X;

    public bool Read()
    {
        return X;
    }
    // Ensure: Result != false
}
";

    private const string ReturnSourceCodeBoolean3 = @"
using System;

class Program_Verifier_Boolean3
{
    public bool Read()
    {
        return false;
    }
    // Ensure: Result == false
}
";

    private const string ReturnSourceCodeBoolean4 = @"
using System;

class Program_Verifier_Boolean4
{
    public bool Read()
    {
        return true;
    }
    // Ensure: Result == true
}
";

    private const string ReturnSourceCodeBoolean5 = @"
using System;

class Program_Verifier_Boolean5
{
    public bool X { get; set; } = true;
    public bool Y { get; set; } = false;

    public bool Read()
    {
        return X && Y;
    }
    // Ensure: Result == Y
}
";

    private const string ReturnSourceCodeBoolean6 = @"
using System;

class Program_Verifier_Boolean6
{
    public bool X { get; set; }

    public void Read()
    {
        return;
    }
    // Ensure: X == false
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeBoolean1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeBoolean2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeBoolean3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeBoolean4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeBoolean5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Boolean6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeBoolean6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ReturnSourceCodeInteger1 = @"
using System;

class Program_Verifier_Integer1
{
    int X;

    public int Read()
    {
        return X;
    }
}
";

    private const string ReturnSourceCodeInteger2 = @"
using System;

class Program_Verifier_Integer2
{
    int X;

    public int Read()
    {
        return X;
    }
    // Ensure: Result != 0
}
";

    private const string ReturnSourceCodeInteger3 = @"
using System;

class Program_Verifier_Integer3
{
    public int Read()
    {
        return 0;
    }
    // Ensure: Result == 0
}
";

    private const string ReturnSourceCodeInteger4 = @"
using System;

class Program_Verifier_Integer4
{
    public int Read()
    {
        return 1;
    }
    // Ensure: Result == 1
}
";

    private const string ReturnSourceCodeInteger5 = @"
using System;

class Program_Verifier_Integer5
{
    public int X { get; set; } = 1;
    public int Y { get; set; } = 0;

    public int Read()
    {
        return X + Y;
    }
    // Ensure: Result == X
}
";

    private const string ReturnSourceCodeInteger6 = @"
using System;

class Program_Verifier_Integer6
{
    public int X { get; set; }

    public void Read()
    {
        return;
    }
    // Ensure: X == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_Integer1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeInteger1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeInteger2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeInteger3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeInteger4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeInteger5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_Integer6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeInteger6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    private const string ReturnSourceCodeFloatingPoint1 = @"
using System;

class Program_Verifier_FloatingPoint1
{
    double X;

    public double Read()
    {
        return X;
    }
}
";

    private const string ReturnSourceCodeFloatingPoint2 = @"
using System;

class Program_Verifier_FloatingPoint2
{
    double X;

    public double Read()
    {
        return X;
    }
    // Ensure: Result != 0.0
}
";

    private const string ReturnSourceCodeFloatingPoint3 = @"
using System;

class Program_Verifier_FloatingPoint3
{
    public double Read()
    {
        return 0.0;
    }
    // Ensure: Result == 0.0
}
";

    private const string ReturnSourceCodeFloatingPoint4 = @"
using System;

class Program_Verifier_FloatingPoint4
{
    public double Read()
    {
        return 1.0;
    }
    // Ensure: Result == 1.0
}
";

    private const string ReturnSourceCodeFloatingPoint5 = @"
using System;

class Program_Verifier_FloatingPoint5
{
    public double X { get; set; } = 1.0;
    public double Y { get; set; } = 0.0;

    public double Read()
    {
        return X + Y;
    }
    // Ensure: Result == X
}
";

    private const string ReturnSourceCodeFloatingPoint6 = @"
using System;

class Program_Verifier_FloatingPoint6
{
    public double X { get; set; }

    public void Read()
    {
        return;
    }
    // Ensure: X == 0.0
}
";

    private const string ReturnSourceCodeFloatingPoint7 = @"
using System;

class Program_Verifier_FloatingPoint7
{
    public double Read()
    {
        return 0;
    }
    // Ensure: Result == 0.0
}
";

    private const string ReturnSourceCodeFloatingPoint8 = @"
using System;

class Program_Verifier_FloatingPoint7
{
    public double Read()
    {
        return 0.0;
    }
    // Ensure: Result == 0
}
";

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint1_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint1, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint2_Error()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint2, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsError, Is.True);
        Assert.That(VerificationResult.ErrorType, Is.EqualTo(VerificationErrorType.EnsureError));
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint3_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint3, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint4_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint4, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint5_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint5, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint6_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint6, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint7_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint7, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }

    [Test]
    [Category("Verification")]
    public void Verifier_FloatingPoint8_Success()
    {
        Verifier TestObject = Tools.CreateVerifierFromSourceCode(ReturnSourceCodeFloatingPoint8, maxDepth: 1, maxDuration: TimeSpan.MaxValue);

        TestObject.Verify();

        VerificationResult VerificationResult = TestObject.VerificationResult;
        Assert.That(VerificationResult.IsSuccess, Is.True);
    }
}
