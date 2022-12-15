namespace ModelAnalyzer;

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a context for synchronized verification of a class model.
/// </summary>
internal class SynchronizedVerificationContext : ISynchronizedContext<ModelVerification, ClassModel>
{
    /// <inheritdoc/>
    public object Lock => ((ICollection)VerificationList).SyncRoot;

    /// <inheritdoc/>
    public void CloneAndRemove(out IDictionary<ModelVerification, ClassModel> cloneTable)
    {
        cloneTable = new Dictionary<ModelVerification, ClassModel>();

        Dictionary<ClassModel, ClassModel> ClonedClassModelTable = new();

        foreach (ModelVerification ModelVerification in VerificationList)
        {
            ClassModel OriginalModel = (ClassModel)ModelVerification.ClassModel;
            ClassModel ClonedModel;

            if (!ClonedClassModelTable.ContainsKey(OriginalModel))
            {
                ClonedModel = OriginalModel with { };
                ClonedClassModelTable.Add(OriginalModel, ClonedModel);
            }
            else
                ClonedModel = ClonedClassModelTable[OriginalModel];

            cloneTable.Add(ModelVerification, ClonedModel);
        }

        VerificationList.Clear();
    }

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
    /// Finds a model verification by the class name. Returns <see langword="null"/> if not found.
    /// </summary>
    /// <param name="className">The class name.</param>
    public ModelVerification? FindByName(string className)
    {
        foreach (ModelVerification Item in VerificationList)
            if (Item.ClassModel.Name == className)
                return Item;

        return null;
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
    /// Gets the list of verification objects.
    /// </summary>
    public List<ModelVerification> VerificationList { get; } = new();

    /// <summary>
    /// Gets or sets the last compilation context.
    /// </summary>
    public CompilationContext LastCompilationContext { get; set; } = CompilationContext.Default;
}
