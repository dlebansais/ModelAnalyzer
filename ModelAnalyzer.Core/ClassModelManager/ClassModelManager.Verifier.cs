namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FileExtractor;
using Newtonsoft.Json;
using ProcessCommunication;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public partial class ClassModelManager : IDisposable
{
    private Channel InitChannel()
    {
        Channel Channel = new Channel(ReceiveChannelGuid, Mode.Receive);
        Channel.Open();

        return Channel;
    }

    /// <summary>
    /// Gets or sets the delay before reading the result of a verification.
    /// </summary>
    public static TimeSpan DelayBeforeReadingVerificationResult { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Starts verifying classes. Only needed if <see cref="StartMode"/> is <see cref="VerificationProcessStartMode.Auto"/>.
    /// </summary>
    /// <param name="classModel">The class classModel.</param>
    private IClassModel WaitForVerification(IClassModel classModel)
    {
        ClassName ClassName = classModel.ClassName;

        if (!classModel.Unsupported.IsEmpty)
        {
            Log($"Model for class '{ClassName}' contains unsupported elements, aborting verification.");
            return classModel;
        }

        if (!FromServerChannel.IsOpen)
        {
            Log("Channel not open to receive verification results, aborting.");
            return classModel;
        }

        if (StartMode == VerificationProcessStartMode.Manual)
            ScheduleAsynchronousVerification();

        Log($"Starting the verification loop on channel {FromServerChannel.Guid}.");

        Thread.Sleep(DelayBeforeReadingVerificationResult);

        Stopwatch Watch = new();
        Watch.Start();

        do
        {
            UpdateVerificationEvents();
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
        while (!GetVerifiedModel(Watch, ref classModel));

        return classModel;
    }

    private bool GetVerifiedModel(Stopwatch watch, ref IClassModel classModel)
    {
        if (watch.Elapsed >= Timeouts.VerificationAcknowledgeTimeout)
        {
            Log($"Verification loop ended on timeout.");
            return true;
        }

        ClassName ClassName = classModel.ClassName;

        CheckVerificationStatus(ClassName, out bool IsFound, out bool IsVerified, out IReadOnlyList<IInvariantViolation> InvariantViolations, out IReadOnlyList<IRequireViolation> RequireViolations, out IReadOnlyList<IEnsureViolation> EnsureViolations, out IReadOnlyList<IAssumeViolation> AssumeViolations);

        if (!IsFound)
        {
            Log($"Class '{ClassName}' no longer in the list of models.");
            return true;
        }

        if (IsVerified)
        {
            Log($"Verification loop completed, class {ClassName} verified, invariant violation: {InvariantViolations.Count}, require violation: {RequireViolations.Count}, ensure violation: {EnsureViolations.Count}, assume violation: {AssumeViolations.Count}.");

            classModel = ((ClassModel)classModel) with { InvariantViolations = InvariantViolations, RequireViolations = RequireViolations, EnsureViolations = EnsureViolations, AssumeViolations = AssumeViolations };
            return true;
        }

        return false;
    }

    private void CheckVerificationStatus(ClassName className, out bool isFound, out bool isVerified, out IReadOnlyList<IInvariantViolation> invariantViolations, out IReadOnlyList<IRequireViolation> requireViolations, out IReadOnlyList<IEnsureViolation> ensureViolations, out IReadOnlyList<IAssumeViolation> assumeViolations)
    {
        invariantViolations = null!;
        requireViolations = null!;
        ensureViolations = null!;
        assumeViolations = null!;

        lock (Context.Lock)
        {
            ClassModelTable ClassModelTable = Context.VerificationState.ModelExchange.ClassModelTable;

            if (ClassModelTable.ContainsKey(className))
            {
                ClassModel ClassModel = ClassModelTable[className];
                isFound = true;
                VerificationState VerificationState = Context.VerificationState;
                isVerified = VerificationState.VerificationResultTable.ContainsKey(className);

                if (isVerified)
                {
                    invariantViolations = ClassModel.InvariantViolations;
                    requireViolations = ClassModel.RequireViolations;
                    ensureViolations = ClassModel.EnsureViolations;
                    assumeViolations = ClassModel.AssumeViolations;
                }
            }
            else
            {
                isFound = false;
                isVerified = false;
            }
        }
    }

    private void UpdateVerificationEvents()
    {
        byte[]? Data = FromServerChannel.Read();

        if (Data is null)
        {
            Log("No data read from verifier.");
            return;
        }

        Log($"{Data.Length} bytes read from verifier.");
        Log(FromServerChannel.GetStats("From Server"));

        int Offset = 0;
        while (Converter.TryDecodeString(Data, ref Offset, out string JsonString))
        {
            Log(JsonString);

            UpdateVerificationEvents(JsonString);
        }
    }

    private void UpdateVerificationEvents(string jsonString)
    {
        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassNameConverter());
        Settings.Converters.Add(new ClassModelTableConverter());

        VerificationResult? VerificationResult = JsonConvert.DeserializeObject<VerificationResult>(jsonString, Settings);
        if (VerificationResult is not null)
            UpdateVerificationEvent(VerificationResult);
        else
            Log("Failed to deserialize the verification result.");
    }

    private void UpdateVerificationEvent(VerificationResult verificationResult)
    {
        Log($"Verification result decoded: {verificationResult}");

        ClassName ClassName = verificationResult.ClassName;

        lock (Context.Lock)
        {
            ClassModelTable ClassModelTable = Context.VerificationState.ModelExchange.ClassModelTable;

            if (ClassModelTable.ContainsKey(ClassName))
            {
                UpdateVerificationEventForClass(ClassName, verificationResult);
                return;
            }
        }

        Log($"Class '{ClassName}' no longer in the list of models, verification result lost.");
    }

    private void UpdateVerificationEventForClass(ClassName className, VerificationResult verificationResult)
    {
        ClassModelTable ClassModelTable = Context.VerificationState.ModelExchange.ClassModelTable;
        ClassModel OldClassModel = ClassModelTable[className];
        ClassModel NewClassModel = WithFilledViolationLists(ClassModelTable, MethodCallStatementList, FunctionCallExpressionList, ArithmeticExpressionList, OldClassModel, verificationResult);
        ClassModelTable[className] = NewClassModel;

        Dictionary<ClassName, VerificationResult> VerificationResultTable = new(Context.VerificationState.VerificationResultTable);

        if (VerificationResultTable.ContainsKey(className))
            VerificationResultTable[className] = verificationResult;
        else
            VerificationResultTable.Add(className, verificationResult);

        Context.VerificationState = Context.VerificationState with { VerificationResultTable = VerificationResultTable };
    }

    private static ClassModel WithFilledViolationLists(ClassModelTable classModelTable, List<MethodCallStatementEntry> methodCallStatementList, List<FunctionCallExpressionEntry> functionCallExpressionList, List<ArithmeticExpressionEntry> arithmeticExpressionList, ClassModel oldClassModel, VerificationResult verificationResult)
    {
        string MethodName = verificationResult.MethodName;
        Method? SelectedMethod = null;

        foreach (var Entry in oldClassModel.MethodTable)
            if (Entry.Key.Text == MethodName)
            {
                SelectedMethod = Entry.Value;
                break;
            }

        if (verificationResult.ErrorType == VerificationErrorType.InvariantError)
            return WithFilledInvariantViolationLists(oldClassModel, verificationResult);
        else if (verificationResult.ErrorType == VerificationErrorType.RequireError)
            return WithFilledRequireViolationLists(classModelTable, methodCallStatementList, functionCallExpressionList, oldClassModel, verificationResult);
        else if (verificationResult.ErrorType == VerificationErrorType.EnsureError && SelectedMethod is not null)
            return WithFilledEnsureViolationLists(oldClassModel, verificationResult, SelectedMethod);
        else if (verificationResult.ErrorType == VerificationErrorType.AssumeError)
            return WithFilledAssumeViolationLists(classModelTable, arithmeticExpressionList, oldClassModel, verificationResult);
        else
            return oldClassModel;
    }

    private static ClassModel WithFilledInvariantViolationLists(ClassModel oldClassModel, VerificationResult verificationResult)
    {
        LocationId LocationId = verificationResult.LocationId;
        List<IInvariantViolation> InvariantViolations = new();
        IReadOnlyList<Invariant> InvariantList = oldClassModel.InvariantList;

        Debug.Assert(LocationId != LocationId.None);

        foreach (Invariant Invariant in InvariantList)
            if (((Expression)Invariant.BooleanExpression).LocationId == LocationId)
            {
                InvariantViolation NewInvariantViolation = new() { Invariant = Invariant };
                InvariantViolations.Add(NewInvariantViolation);
            }

        ClassModel NewClassModel = oldClassModel with { InvariantViolations = InvariantViolations.AsReadOnly() };

        return NewClassModel;
    }

    private static ClassModel WithFilledRequireViolationLists(ClassModelTable classModelTable, List<MethodCallStatementEntry> methodCallStatementList, List<FunctionCallExpressionEntry> functionCallExpressionList, ClassModel oldClassModel, VerificationResult verificationResult)
    {
        LocationId LocationId = verificationResult.LocationId;
        List<IRequireViolation> RequireViolations = new();
        RequireViolation? NewRequireViolation = null;

        Debug.Assert(LocationId != LocationId.None);

        foreach (MethodCallStatementEntry Entry in methodCallStatementList)
            if (((Statement)Entry.Statement).LocationId == LocationId)
            {
                Method Method = GetCalledMethod(classModelTable, Entry.Statement);
                NewRequireViolation = new() { Method = Method, Text = verificationResult.ErrorText, NameLocation = Entry.Statement.NameLocation, Statement = Entry.Statement as IStatement, Expression = null, Require = null };
                break;
            }

        foreach (FunctionCallExpressionEntry Entry in functionCallExpressionList)
            if (((Expression)Entry.Expression).LocationId == LocationId)
            {
                Method Method = GetCalledMethod(classModelTable, Entry.Expression);
                NewRequireViolation = new() { Method = Method, Text = verificationResult.ErrorText, NameLocation = Entry.Expression.NameLocation, Statement = null, Expression = Entry.Expression as IExpression, Require = null };
                break;
            }

        if (NewRequireViolation is null)
        {
            string MethodName = verificationResult.MethodName;
            Method? SelectedMethod = null;

            foreach (var Entry in oldClassModel.MethodTable)
                if (Entry.Key.Text == MethodName)
                {
                    SelectedMethod = Entry.Value;
                    break;
                }

            Debug.Assert(SelectedMethod is not null);
            List<Require> RequireList = SelectedMethod!.RequireList;

            foreach (Require Require in RequireList)
                if (((Expression)Require.BooleanExpression).LocationId == LocationId)
                {
                    NewRequireViolation = new() { Method = SelectedMethod!, Text = verificationResult.ErrorText, NameLocation = Require.Location, Expression = null, Statement = null, Require = Require };
                    break;
                }
        }

        if (NewRequireViolation is not null)
        {
            Debug.Assert(NewRequireViolation.Statement is not null || NewRequireViolation.Expression is not null || NewRequireViolation.Require is not null);
            RequireViolations.Add(NewRequireViolation);
        }

        ClassModel NewClassModel = oldClassModel with { RequireViolations = RequireViolations.AsReadOnly() };

        return NewClassModel;
    }

    private static ClassModel WithFilledEnsureViolationLists(ClassModel oldClassModel, VerificationResult verificationResult, Method method)
    {
        LocationId LocationId = verificationResult.LocationId;
        List<IEnsureViolation> EnsureViolations = new();
        List<Ensure> EnsureList = method.EnsureList;

        Debug.Assert(LocationId != LocationId.None);

        foreach (Ensure Ensure in EnsureList)
            if (((Expression)Ensure.BooleanExpression).LocationId == LocationId)
            {
                EnsureViolation NewEnsureViolation = new() { Method = method, Ensure = Ensure };
                EnsureViolations.Add(NewEnsureViolation);
            }

        ClassModel NewClassModel = oldClassModel with { EnsureViolations = EnsureViolations.AsReadOnly() };

        return NewClassModel;
    }

    private static ClassModel WithFilledAssumeViolationLists(ClassModelTable classModelTable, List<ArithmeticExpressionEntry> arithmeticExpressionList, ClassModel oldClassModel, VerificationResult verificationResult)
    {
        LocationId LocationId = verificationResult.LocationId;
        Debug.Assert(LocationId != LocationId.None);

        List<IAssumeViolation> AssumeViolations = new();

        foreach (ArithmeticExpressionEntry Entry in arithmeticExpressionList)
            if (((Expression)Entry.Expression).LocationId == LocationId)
            {
                Method? Method = null;

                if (verificationResult.MethodName != string.Empty)
                    Method = GetCalledMethod(classModelTable, verificationResult.ClassName, verificationResult.MethodName);

                AssumeViolation NewViolation = new() { Method = Method, Text = verificationResult.ErrorText, Location = Entry.Expression.Location };
                AssumeViolations.Add(NewViolation);
                break;
            }

        ClassModel NewClassModel = oldClassModel with { AssumeViolations = AssumeViolations.AsReadOnly() };

        return NewClassModel;
    }

    private static Method GetCalledMethod(ClassModelTable classModelTable, ICall call)
    {
        ClassName CalledClassName = call is IPublicCall ? call.ClassName : call.CallerClassName;

        return GetCalledMethod(classModelTable, CalledClassName, call.MethodName.Text);
    }

    private static Method GetCalledMethod(ClassModelTable classModelTable, ClassName className, string methodName)
    {
        Debug.Assert(className != ClassName.Empty);
        Debug.Assert(classModelTable.ContainsKey(className));

        ClassModel ClassModel = classModelTable[className];

        return GetCalledMethod(ClassModel, methodName);
    }

    private static Method GetCalledMethod(ClassModel classModel, string methodName)
    {
        Method? SelectedMethod = null;

        foreach (var Entry in classModel.MethodTable)
            if (Entry.Key.Text == methodName)
            {
                SelectedMethod = Entry.Value;
                break;
            }

        Debug.Assert(SelectedMethod is not null);

        return SelectedMethod!;
    }

    private void ScheduleAsynchronousVerification()
    {
        Extractor.Extract();
        Log($"Extracted count: {Extractor.ExtractedPathTable.Count}");
        Log($"Extracted error: {Extractor.LastExceptionMessage}");
        string VerifierFilePath = Extractor.GetExtractedPath(Extractor.VerifierFileName);

        Logger.Log($"Starting the verification process at {VerifierFilePath}.");

        try
        {
            ProcessStartInfo ProcessStartInfo = new ProcessStartInfo();
            ProcessStartInfo.FileName = VerifierFilePath;
            ProcessStartInfo.UseShellExecute = false;
            ProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(VerifierFilePath);
            Process CreatedProcess = Process.Start(ProcessStartInfo);

            Log($"CreatedProcess: {CreatedProcess} {CreatedProcess.Id} {CreatedProcess.ProcessName}");
        }
        catch (Exception exception)
        {
            Logger.LogException(exception);
        }

        Log("Creating the channel to send class models.");

        using Channel ToServerChannel = new Channel(Channel.ClientToServerGuid, Mode.Send);

        TimeSpan Timeout = Timeouts.VerifierProcessLaunchTimeout;
        Stopwatch Watch = new();
        Watch.Start();

        while (Watch.Elapsed < Timeout)
        {
            ToServerChannel.Open();
            if (ToServerChannel.IsOpen)
                break;

            Thread.Sleep(TimeSpan.FromMilliseconds(500));
        }

        if (ToServerChannel.IsOpen)
        {
            Log("Channel opened.");

            SendClassModelDataForVerification(ToServerChannel);
            ToServerChannel.Close();
        }
        else
            Log("Could not open the channel.");

        UpdateVerificationEvents();
    }

    private void SendClassModelDataForVerification(Channel channel)
    {
        lock (Context.Lock)
        {
            VerificationState VerificationState = Context.VerificationState;

            if (VerificationState.IsVerificationRequestSent)
                Log($"Skipping complete verification, it's being done.");
            else
            {
                bool AnyEmptyUnsupported = false;

                foreach (KeyValuePair<ClassName, ClassModel> Entry in VerificationState.ModelExchange.ClassModelTable)
                {
                    ClassName ClassName = Entry.Key;
                    ClassModel ClassModel = Entry.Value;

                    if (!ClassModel.Unsupported.IsEmpty)
                        Log($"Skipping complete verification for class '{ClassName}', it has unsupported elements.");
                    else
                        AnyEmptyUnsupported = true;
                }

                if (AnyEmptyUnsupported)
                    Context.VerificationState = VerificationState with { IsVerificationRequestSent = true };
            }
        }

        if (Context.VerificationState.IsVerificationRequestSent)
        {
            ModelExchange ModelExchange = Context.VerificationState.ModelExchange;
            SendData(channel, ModelExchange);
        }
    }

    private void SendData(Channel channel, ModelExchange modelExchange)
    {
        JsonSerializerSettings Settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        Settings.Converters.Add(new ClassNameConverter());
        Settings.Converters.Add(new ClassModelTableConverter());

        string JSonString = JsonConvert.SerializeObject(modelExchange, Settings);
        byte[] EncodedString = Converter.EncodeString(JSonString);

        Log(channel.GetStats("To Server"));

        if (EncodedString.Length <= channel.GetFreeLength())
        {
            TimeSpan Timeout = Timeouts.VerificationBusyTimeout;
            Stopwatch Watch = new();
            Watch.Start();

            for (; ;)
            {
                if (channel.GetUsedLength() <= EncodedString.Length * 5)
                    break;

                if (Watch.Elapsed >= Timeout)
                {
                    Log("Ignoring the busy state.");
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }

            channel.Write(EncodedString);

            Log($"Data send {EncodedString.Length} bytes.");
        }
        else
            Log($"Unable to send data, buffer full.");
    }

    private Channel FromServerChannel;
    private List<MethodCallStatementEntry> MethodCallStatementList = new();
    private List<FunctionCallExpressionEntry> FunctionCallExpressionList = new();
    private List<ArithmeticExpressionEntry> ArithmeticExpressionList = new();
}
