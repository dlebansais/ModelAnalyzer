namespace ModelAnalyzer;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a context for synchronized verification of a class model.
/// </summary>
internal class SynchronizedVerificationContext
{
    /// <summary>
    /// Gets the synchronization lock.
    /// </summary>
    public object Lock => ((ICollection)VerificationState.ModelExchange.ClassModelTable).SyncRoot;

    /// <summary>
    /// Checks whether the context contains the name of a class.
    /// </summary>
    /// <param name="className">The class name.</param>
    public bool ContainsClass(string className)
    {
        return VerificationState.ModelExchange.ClassModelTable.ContainsKey(className);
    }

    /// <summary>
    /// Removes a class by its name to clean up the list of classes that have been seen.
    /// </summary>
    /// <param name="className">The class name.</param>
    public void RemoveClass(string className)
    {
        Dictionary<string, ClassModel> ClassModelTable = VerificationState.ModelExchange.ClassModelTable;

        Debug.Assert(ClassModelTable.ContainsKey(className));

        ClassModelTable.Remove(className);
    }

    /// <summary>
    /// Gets the table of class models.
    /// </summary>
    public ICollection<string> GetClassModelNameList()
    {
        return VerificationState.ModelExchange.ClassModelTable.Keys;
    }

    /// <summary>
    /// Gets or sets the verification state.
    /// </summary>
    required public VerificationState VerificationState { get; set; }

    /// <summary>
    /// Gets or sets the last compilation context.
    /// </summary>
    public CompilationContext LastCompilationContext { get; set; } = CompilationContext.Default;
}
