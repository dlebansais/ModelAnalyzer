namespace Verifier;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using ModelAnalyzer;
using Newtonsoft.Json;
using ProcessCommunication;

internal class Program
{
    private const int MaxDepth = 2;

    public static void Main()
    {
        string Location = Assembly.GetExecutingAssembly().Location;
        Environment.CurrentDirectory = Path.GetDirectoryName(Location);

        Channel FromClientChannel = new(Channel.ClientToServerGuid, Mode.Receive);
        FromClientChannel.Open();

        if (!FromClientChannel.IsOpen)
            return;

        TimeSpan Timeout = TimeSpan.FromSeconds(60);
        Stopwatch Watch = new();
        Watch.Start();

        while (Watch.Elapsed < Timeout)
        {
            byte[]? Data = FromClientChannel.Read();
            if (Data is not null)
            {
                ProcessData(Data);
                Watch.Restart();
            }
            else
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }

        FromClientChannel.Close();
    }

    private static void ProcessData(byte[] data)
    {
        Channel ToClientChannel = new(Channel.ServerToClientGuid, Mode.Send);
        ToClientChannel.Open();

        if (ToClientChannel.IsOpen)
        {
            int DecodedCount = 0;
            int VerifiedCount = 0;
            int Offset = 0;
            while (Offset + sizeof(int) <= data.Length)
            {
                int EncodedStringLength = BitConverter.ToInt32(data, Offset);

                if (Offset + EncodedStringLength <= data.Length)
                {
                    byte[] EncodedString = new byte[EncodedStringLength - sizeof(int)];
                    Array.Copy(data, Offset + sizeof(int), EncodedString, 0, EncodedString.Length);

                    string JsonString = Encoding.UTF8.GetString(EncodedString);
                    ClassModel? ClassModel = JsonConvert.DeserializeObject(JsonString) as ClassModel;
                    if (ClassModel is not null)
                    {
                        DecodedCount++;

                        try
                        {
                            using Verifier Verifier = new()
                            {
                                MaxDepth = MaxDepth,
                                ClassName = ClassModel.Name,
                                FieldTable = ClassModel.FieldTable,
                                MethodTable = ClassModel.MethodTable,
                                InvariantList = ClassModel.InvariantList,
                            };

                            Verifier.Verify();

                            ClassModel.IsInvariantViolated = Verifier.IsInvariantViolated;

                            VerifiedCount += 10;
                        }
                        catch
                        {
                        }
                    }
                }

                Offset += EncodedStringLength;
            }

            byte[] Data = new byte[10 + DecodedCount + VerifiedCount];

            if (Data.Length <= ToClientChannel.GetFreeLength())
                ToClientChannel.Write(Data);

            ToClientChannel.Close();
        }
    }
}
