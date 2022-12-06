namespace DemoAnalyzer;

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a context for synchronized verification of a class model.
/// </summary>
internal class SynchronizedVerificationContext : ISynchronizedContext<ModelVerification, IClassModel>
{
    /// <inheritdoc/>
    public object Lock => ((ICollection)VerificationList).SyncRoot;

    /// <inheritdoc/>
    public void CloneAndRemove(out List<ModelVerification> syncList, out List<IClassModel> itemList)
    {
        syncList = new List<ModelVerification>(VerificationList);
        VerificationList.Clear();

        itemList = new List<IClassModel>();
        List<ClassModel> ClonedClassModelList = new();

        foreach (ModelVerification ModelVerification in syncList)
        {
            ClassModel OriginalModel = (ClassModel)ModelVerification.ClassModel;

            if (!ClonedClassModelList.Contains(OriginalModel))
            {
                ClonedClassModelList.Add(OriginalModel);

                ClassModel CloneModel = OriginalModel with { };
                itemList.Add(CloneModel);
            }
        }
    }

    /// <summary>
    /// Gets the table of class models.
    /// </summary>
    public Dictionary<string, ClassModel> ClassModelTable { get; } = new();

    /// <summary>
    /// Gets the list of verification objects.
    /// </summary>
    public List<ModelVerification> VerificationList { get; } = new();

    /// <summary>
    /// Gets or sets the last compilation context hash code.
    /// </summary>
    public int LastHashCode { get; set; }
}
