﻿namespace DemoAnalyzer;

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

public class ClassModelManager
{
    public static ClassModelManager Instance { get; } = new();

    private Dictionary<string, ClassModel> ClassTable = new();
    private int LastHashCode;
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
        Logger.Log("StartThread()");

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

            Thread.Sleep(TimeSpan.FromSeconds(1));

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
            Logger.LogException(e);
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

    public (ClassModel ClassModel, bool IsThreadStarted) GetClassModel(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        int HashCode = context.Compilation.GetHashCode();
        ClassModel Result;
        bool IsThreadStarted = false;

        lock (ClassTable)
        {
            string ClassName = classDeclaration.Identifier.ValueText;

            if (ClassName == string.Empty || LastHashCode != HashCode || !ClassTable.ContainsKey(ClassName))
            {
                Result = ClassModel.FromClassDeclaration(classDeclaration);

                if (ClassName != string.Empty)
                {
                    UpdateClassModel(Result);

                    if (LastHashCode != HashCode)
                    {
                        LastHashCode = HashCode;
                        ScheduleThreadStart();
                        IsThreadStarted = true;
                    }
                }
            }
            else
                Result = ClassTable[ClassName];
        }

        return (Result, IsThreadStarted);
    }

    public void UpdateClassModel(ClassModel classModel)
    {
        string ClassName = classModel.Name;

        if (!ClassTable.ContainsKey(ClassName))
        {
            if (ClassTable.Count == 0)
                Logger.Clear();

            ClassTable.Add(ClassName, classModel);
            ViolationTable.Add(ClassName, false);
        }
        else
        {
            ClassTable[ClassName] = classModel;
        }
    }

    public void RemoveMissingClasses(List<string> existingClassList)
    {
        lock (ClassTable)
        {
            List<string> ToRemoveClassList = new();

            foreach (KeyValuePair<string, ClassModel> Entry in ClassTable)
                if (!existingClassList.Contains(Entry.Key))
                    ToRemoveClassList.Add(Entry.Key);

            foreach (string Key in ToRemoveClassList)
            {
                ClassTable.Remove(Key);
                ViolationTable.Remove(Key);
            }
        }
    }

    public static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;

        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                if (Comment.StartsWith($"// {Modeling.None}"))
                    return true;
            }

        return false;
    }
}
