namespace Libz3Extractor;

using System;
using System.IO;
using System.Reflection;

public class Extractor
{
    public static bool Extract()
    {
        if (!Directory.Exists("C:\\Projects\\Temp"))
            return false;

        using FileStream LogStream = new("C:\\Projects\\Temp\\log.txt", FileMode.Create, FileAccess.Write);
        using StreamWriter LogWriter = new(LogStream);

        try
        {
            LogWriter.WriteLine("Started");

            Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
            string Location = ExecutingAssembly.Location;

            LogWriter.WriteLine(Location);

            // Ok, we're inside the system, let's be hacky.
            string AnalyzerRoot = Path.GetDirectoryName(Path.GetDirectoryName(Location));

            LogWriter.WriteLine(AnalyzerRoot);

            // Assume a folder architecture with each file in a separate directory.
            foreach (string Directory in Directory.GetDirectories(AnalyzerRoot))
            {
                LogWriter.WriteLine(Directory);

                string DllDirectory = Path.Combine(AnalyzerRoot, Directory);
                string MicrosoftZ3FilePath = Path.Combine(DllDirectory, "Microsoft.Z3.dll");

                LogWriter.WriteLine(MicrosoftZ3FilePath);

                // Look for the directory where Microsoft.Z3.dll is.
                if (File.Exists(MicrosoftZ3FilePath))
                {
                    LogWriter.WriteLine("Found");

                    string Libz3FilePath = Path.Combine(DllDirectory, "libz3.dll");

                    LogWriter.WriteLine(Libz3FilePath);

                    // Get a stream to the dll loaded inside us.
                    string[] ResourceNames = ExecutingAssembly.GetManifestResourceNames();
                    using Stream ResourceStream = ExecutingAssembly.GetManifestResourceStream("libz3.dll");

                    foreach (string Name in ResourceNames)
                        LogWriter.WriteLine($"Resource: {Name}");

                    // Create a file stream in the same folder as Microsoft.Z3.dll.
                    using FileStream DllStream = new(Libz3FilePath, FileMode.Create, FileAccess.Write);

                    LogWriter.WriteLine("File created");

                    // Copy the stream to have a new dll ready to load.
                    ResourceStream.CopyTo(DllStream);
                    ResourceStream.Flush();

                    LogWriter.WriteLine("Flushed");

                    return true;
                }
            }
        }
        catch (Exception e)
        {
            LogWriter.WriteLine(e.Message);
        }
        return false;
    }
}