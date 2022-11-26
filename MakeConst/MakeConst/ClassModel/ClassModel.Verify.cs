namespace DemoAnalyzer;

using System.Collections.Generic;

public partial record ClassModel
{
    public void Verify()
    {
        bool IsInvariantViolated = false;

        foreach (KeyValuePair<FieldName, IField> Entry in FieldTable)
            if (Entry.Key.Name == "XYZ")
            {
                IsInvariantViolated = true;
                break;
            }

        ClassModelManager.Instance.SetIsInvariantViolated(Name, IsInvariantViolated);
    }
}
