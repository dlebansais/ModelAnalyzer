namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Threading;

public class ClassModelManager
{
    public static ClassModelManager Instance = new();

    private Dictionary<string, ClassModel> ClassTable = new();
    private Dictionary<string, bool> ViolationTable = new();
    private Thread? ModelThread = null;
    private bool ThreadShouldBeRestarted;

    private void ScheduleThreadStart()
    {
        lock (ClassTable)
        {
            if (ModelThread is null)
                StartThread();
            else
                ThreadShouldBeRestarted = true;
        }
    }

    private void StartThread()
    {
        ThreadShouldBeRestarted = false;
        ModelThread = new Thread(new ThreadStart(ExecuteThread));
        ModelThread.Start();
    }

    private void ExecuteThread()
    {
        try
        {
            List<ClassModel> ClassModelList = new();

            lock (ClassTable)
            {
                foreach (KeyValuePair<string, ClassModel> Entry in ClassTable)
                {
                    ClassModel Original = Entry.Value;
                    ClassModel Clone = Original with { };

                    ClassModelList.Add(Clone);
                }
            }

            foreach (ClassModel Item in ClassModelList)
                Item.Verify();

            Thread.Sleep(1000);

            bool Restart = false;

            lock (ClassTable)
            {
                ModelThread = null;

                if (ThreadShouldBeRestarted)
                    Restart = true;
            }

            if (Restart)
                StartThread();
        }
        catch (Exception e)
        {
            Logger.Log(e.Message);
            Logger.Log(e.StackTrace);
        }
    }

    public bool IsInvariantViolated(string name)
    {
        lock (ViolationTable)
        {
            return ViolationTable.ContainsKey(name) && ViolationTable[name] == true;
        }
    }

    public void SetIsInvariantViolated(string name, bool isInvariantViolated)
    {
        lock (ViolationTable)
        {
            if (ViolationTable.ContainsKey(name))
                ViolationTable[name] = isInvariantViolated;
        }
    }

    public void Update(ClassModel classModel)
    {
        bool IsClassChanged = false;

        lock (ClassTable)
        {
            bool IsFound = false;

            foreach (KeyValuePair<string, ClassModel> Entry in ClassTable)
                if (Entry.Key == classModel.Name)
                {
                    IsFound = true;
                    ClassModel ExistingClassModel = Entry.Value;

                    if (!ExistingClassModel.Equals(classModel))
                    {
                        ClassTable[classModel.Name] = classModel;
                        IsClassChanged = true;
                    }

                    break;
                }

            if (!IsFound)
            {
                if (ClassTable.Count == 0)
                    Logger.Clear();

                ClassTable.Add(classModel.Name, classModel);
                ViolationTable.Add(classModel.Name, false);
            }
        }

        if (IsClassChanged)
        {
            ScheduleThreadStart();
        }
    }
}
