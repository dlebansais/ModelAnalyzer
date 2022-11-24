namespace DemoAnalyzer;

using System.Collections.Generic;

public class ClassModelManager
{
    public static ClassModelManager Instance = new();

    private Dictionary<string, ClassModel> ClassTable = new();

    public void Update(ClassModel classModel)
    {
        bool IsClassChanged = false;

        lock (ClassTable)
        {
            foreach (KeyValuePair<string, ClassModel> Entry in ClassTable)
                if (Entry.Key == classModel.Name)
                {
                    ClassModel ExistingClassModel = Entry.Value;
                    if (!ExistingClassModel.Equals(classModel))
                    {
                        ClassTable[classModel.Name] = classModel;
                        IsClassChanged = true;
                    }

                    break;
                }
        }

        if (IsClassChanged)
        {
        }
    }
}
