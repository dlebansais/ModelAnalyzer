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
        Thread.Sleep(DelayBeforeReadingVerificationResult);

        string ClassName = classModel.Name;

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

        Log("Starting the verification loop.");

        TimeSpan Timeout = Timeouts.VerificationAcknowledgeTimeout;
        Stopwatch Watch = new();
        Watch.Start();

        for (; ;)
        {
            if (Watch.Elapsed >= Timeout)
            {
                Log($"Verification loop ended on timeout.");
                return classModel;
            }

            CheckVerificationStatus(ClassName, out bool IsFound, out bool IsVerified, out IReadOnlyList<IInvariantViolation> InvariantViolations, out IReadOnlyList<IRequireViolation> RequireViolations, out IReadOnlyList<IEnsureViolation> EnsureViolations);

            if (!IsFound)
            {
                Log($"Class '{ClassName}' no longer in the list of models.");
                return classModel;
            }

            if (IsVerified)
            {
                Log($"Verification loop completed, class {ClassName} verified, invariant violation: {InvariantViolations.Count}, require violation: {RequireViolations.Count}, ensure violation: {EnsureViolations.Count}.");
                return ((ClassModel)classModel) with { InvariantViolations = InvariantViolations, RequireViolations = RequireViolations, EnsureViolations = EnsureViolations };
            }

            UpdateVerificationEvents();
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    private void CheckVerificationStatus(string className, out bool isFound, out bool isVerified, out IReadOnlyList<IInvariantViolation> invariantViolations, out IReadOnlyList<IRequireViolation> requireViolations, out IReadOnlyList<IEnsureViolation> ensureViolations)
    {
        invariantViolations = null!;
        requireViolations = null!;
        ensureViolations = null!;

        lock (Context.Lock)
        {
            if (Context.ClassModelTable.ContainsKey(className))
            {
                isFound = true;

                VerificationState VerificationState = Context.ClassModelTable[className];
                isVerified = VerificationState.VerificationResult != VerificationResult.Default;
                if (isVerified)
                {
                    invariantViolations = VerificationState.ClassModelExchange.ClassModel.InvariantViolations;
                    requireViolations = VerificationState.ClassModelExchange.ClassModel.RequireViolations;
                    ensureViolations = VerificationState.ClassModelExchange.ClassModel.EnsureViolations;
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

        int Offset = 0;
        while (Converter.TryDecodeString(Data, ref Offset, out string JsonString))
        {
            Log(JsonString);

            VerificationResult? VerificationResult = JsonConvert.DeserializeObject<VerificationResult>(JsonString, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            if (VerificationResult is not null)
                UpdateVerificationEvent(VerificationResult);
            else
                Log("Failed to deserialize the verification result.");
        }
    }

    private void UpdateVerificationEvent(VerificationResult verificationResult)
    {
        Log($"Verification result decoded: {verificationResult}");

        lock (Context.Lock)
        {
            foreach (KeyValuePair<string, VerificationState> Entry in Context.ClassModelTable)
                if (TryUpdateVerificationEventForClass(verificationResult, Entry.Value))
                    return;
        }

        Log($"Class no longer in the list of models, verification result lost.");
    }

    private bool TryUpdateVerificationEventForClass(VerificationResult verificationResult, VerificationState verificationState)
    {
        string ClassName = verificationResult.ClassName;

        if (verificationState.ClassModelExchange.ClassModel.Name == ClassName)
        {
            ClassModel OldClassModel = verificationState.ClassModelExchange.ClassModel;
            FillViolationLists(verificationResult, OldClassModel, out List<IInvariantViolation> InvariantViolations, out List<IRequireViolation> RequireViolations, out List<IEnsureViolation> EnsureViolations);
            ClassModel NewClassModel = OldClassModel with
            {
                InvariantViolations = InvariantViolations.AsReadOnly(),
                RequireViolations = RequireViolations.AsReadOnly(),
                EnsureViolations = EnsureViolations.AsReadOnly(),
            };

            ClassModelExchange OldClassModelExchange = verificationState.ClassModelExchange;
            ClassModelExchange NewClassModelExchange = OldClassModelExchange with { ClassModel = NewClassModel };

            Context.ClassModelTable[ClassName] = verificationState with { ClassModelExchange = NewClassModelExchange, VerificationResult = verificationResult };
            return true;
        }

        return false;
    }

    private void FillViolationLists(VerificationResult verificationResult, ClassModel classModel, out List<IInvariantViolation> invariantViolations, out List<IRequireViolation> requireViolations, out List<IEnsureViolation> ensureViolations)
    {
        invariantViolations = new List<IInvariantViolation>();
        requireViolations = new List<IRequireViolation>();
        ensureViolations = new List<IEnsureViolation>();

        int ErrorIndex = verificationResult.ErrorIndex;

        string MethodName = verificationResult.MethodName;
        Method? SelectedMethod = null;

        foreach (var Entry in classModel.MethodTable)
            if (Entry.Key.Text == MethodName)
            {
                SelectedMethod = Entry.Value;
                break;
            }

        if (verificationResult.ErrorType == VerificationErrorType.InvariantError)
        {
            List<Invariant> InvariantList = classModel.InvariantList;

            if (ErrorIndex >= 0 && ErrorIndex < InvariantList.Count)
                invariantViolations.Add(new InvariantViolation() { Invariant = InvariantList[ErrorIndex] });
        }
        else if (verificationResult.ErrorType == VerificationErrorType.RequireError && SelectedMethod is not null)
        {
            List<Require> RequireList = SelectedMethod.RequireList;

            if (ErrorIndex >= 0 && ErrorIndex < RequireList.Count)
                requireViolations.Add(new RequireViolation() { Method = SelectedMethod, Require = RequireList[ErrorIndex] });
        }
        else if (verificationResult.ErrorType == VerificationErrorType.EnsureError && SelectedMethod is not null)
        {
            List<Ensure> EnsureList = SelectedMethod.EnsureList;

            if (ErrorIndex >= 0 && ErrorIndex < EnsureList.Count)
                ensureViolations.Add(new EnsureViolation() { Method = SelectedMethod, Ensure = EnsureList[ErrorIndex] });
        }
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
        List<VerificationState> ToVerifyList = new();

        lock (Context.Lock)
        {
            foreach (KeyValuePair<string, VerificationState> Entry in Context.ClassModelTable)
            {
                VerificationState VerificationState = Entry.Value;
                ClassModel ClassModel = VerificationState.ClassModelExchange.ClassModel;
                string ClassName = ClassModel.Name;

                if (!ClassModel.Unsupported.IsEmpty)
                    Log($"Skipping complete verification for class '{ClassName}', it has unsupported elements.");
                else if (VerificationState.IsVerificationRequestSent)
                    Log($"Skipping complete verification for class '{ClassName}', it's being done.");
                else
                {
                    VerificationState = VerificationState with { IsVerificationRequestSent = true };
                    ToVerifyList.Add(VerificationState);
                }
            }

            foreach (VerificationState VerificationState in ToVerifyList)
                Context.ClassModelTable[VerificationState.ClassModelExchange.ClassModel.Name] = VerificationState;
        }

        foreach (VerificationState VerificationState in ToVerifyList)
        {
            ClassModelExchange ClassModelExchange = VerificationState.ClassModelExchange;
            ClassModel ClassModel = ClassModelExchange.ClassModel;
            string ClassName = ClassModel.Name;

            string JSonString = JsonConvert.SerializeObject(ClassModelExchange, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            byte[] EncodedString = Converter.EncodeString(JSonString);

            if (EncodedString.Length <= channel.GetFreeLength())
            {
                channel.Write(EncodedString);

                Log($"Data send {EncodedString.Length} bytes for class '{ClassName}'.");
            }
            else
                Log($"Unable to send data for class '{ClassName}', buffer full.");
        }
    }

    private Channel FromServerChannel;
}
