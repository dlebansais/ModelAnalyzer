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

        Channel Channel = new(Channel.SharedGuid, Mode.Server);

        if (!Channel.Open())
            return;

        TimeSpan Timeout = TimeSpan.FromSeconds(60);
        Stopwatch Watch = new();
        Watch.Start();

        while (Watch.Elapsed < Timeout)
        {
            byte[]? Data = Channel.Read();
            if (Data is not null)
            {
                ProcessData(Data);
                Watch.Restart();
            }
            else
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }

        Channel.Close();
    }

    private static void ProcessData(byte[] data)
    {
    }
}
