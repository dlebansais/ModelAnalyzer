namespace ModelAnalyzer;

using System.Collections;
using System.Collections.Generic;
using Microsoft.Z3;

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
    /// Adds or updates a class model.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    /// <param name="isAdded"><see langword="true"/> upon return if added; otherwise, <see langword="false"/>.</param>
    public void UpdateClassModel(ClassModel classModel, out bool isAdded)
    {
        string ClassName = classModel.Name;

        if (!ClassModelTable.ContainsKey(ClassName))
        {
            ClassModelTable.Add(ClassName, classModel);
            isAdded = true;
        }
        else
        {
            ClassModelTable[ClassName] = classModel;
            isAdded = false;
        }
    }

    /// <summary>
    /// Checks whether the invariant of a class is violated.
    /// </summary>
    /// <param name="className">The class name.</param>
    public bool GetIsInvariantViolated(string className)
    {
        return ClassNameWithInvariantViolation.Contains(className);
    }

    /// <summary>
    /// Sets whether the invariant of a class is violated.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="isViolated">True if violated.</param>
    public void SetIsInvariantViolated(string className, bool isViolated)
    {
        if (isViolated && !ClassNameWithInvariantViolation.Contains(className))
            ClassNameWithInvariantViolation.Add(className);
        else if (!isViolated && ClassNameWithInvariantViolation.Contains(className))
            ClassNameWithInvariantViolation.Remove(className);
    }

    /// <summary>
    /// Gets the table of class models.
    /// </summary>
    public Dictionary<string, ClassModel> ClassModelTable { get; } = new();

    /// <summary>
    /// Gets the list of class names for wich the invariant is violated.
    /// </summary>
    public List<string> ClassNameWithInvariantViolation { get; } = new();

    /// <summary>
    /// Gets or sets the last compilation context.
    /// </summary>
    public CompilationContext LastCompilationContext { get; set; } = CompilationContext.Default;
}
