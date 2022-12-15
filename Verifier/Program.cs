namespace Verifier;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using ProcessCommunication;

internal class Program
{
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
            byte[] Data = new byte[10];

            if (Data.Length <= ToClientChannel.GetFreeLength())
                ToClientChannel.Write(Data);

            ToClientChannel.Close();
        }
    }
}
