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
    public object Lock => ((ICollection)ClassModelTable).SyncRoot;

    /// <summary>
    /// Adds a class model.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public void AddClassModel(ClassModel classModel)
    {
        string ClassName = classModel.Name;

        Debug.Assert(!ClassModelTable.ContainsKey(ClassName));

        ClassModelTable.Add(ClassName, classModel);
    }

    /// <summary>
    /// Updates an existing class model.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public void UpdateClassModel(ClassModel classModel)
    {
        string ClassName = classModel.Name;

        Debug.Assert(ClassModelTable.ContainsKey(ClassName));

        ClassModelTable[ClassName] = classModel;
    }

    /// <summary>
    /// Gets the table of class models.
    /// </summary>
    public Dictionary<string, ClassModel> ClassModelTable { get; } = new();

    /// <summary>
    /// Gets or sets the last compilation context.
    /// </summary>
    public CompilationContext LastCompilationContext { get; set; } = CompilationContext.Default;
}
