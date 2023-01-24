namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a class name.
/// </summary>
public class ClassName
{
    /// <summary>
    /// Gets the empty class name.
    /// </summary>
    public static ClassName Empty { get; } = new() { Namespace = new List<string>(), Text = string.Empty };

    /// <summary>
    /// Gets the class' namespace.
    /// </summary>
    required public List<string> Namespace { get; init; }

    /// <summary>
    /// Gets the class name within the namespace.
    /// </summary>
    required public string Text { get; init; }

    /// <summary>
    /// Gets the class name as a name path.
    /// </summary>
    public List<string> ToNamePath()
    {
        List<string> NamePath = new();
        NamePath.AddRange(Namespace);
        NamePath.Add(Text);

        return NamePath;
    }

    /// <summary>
    /// Creates a class name from a simple string.
    /// </summary>
    /// <param name="text">The string.</param>
    public static ClassName FromSimpleString(string text)
    {
        return new ClassName() { Namespace = new List<string>(), Text = text };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Join(".", ToNamePath());
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is ClassName Other &&
               Namespace.SequenceEqual(Other.Namespace) &&
               Text == Other.Text;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int Result = 0;
        foreach (string Item in Namespace)
            Result ^= Item.GetHashCode();

        Result ^= Text.GetHashCode();

        return Result;
    }

    /// <summary>
    /// Compares two instances of <see cref="ClassName"/>.
    /// </summary>
    /// <param name="classname1">The first instance.</param>
    /// <param name="classname2">The second instance.</param>
    public static bool operator ==(ClassName classname1, ClassName classname2)
    {
        return classname1.Namespace.SequenceEqual(classname2.Namespace) && classname1.Text == classname2.Text;
    }

    /// <summary>
    /// Compares two instances of <see cref="ClassName"/>.
    /// </summary>
    /// <param name="classname1">The first instance.</param>
    /// <param name="classname2">The second instance.</param>
    public static bool operator !=(ClassName classname1, ClassName classname2)
    {
        return !classname1.Namespace.SequenceEqual(classname2.Namespace) || classname1.Text != classname2.Text;
    }
}
