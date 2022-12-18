namespace Libz3Extractor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class Extractor
{
    public const string Libz3FileName = "libz3.dll";
    public const string VerifierFileName = "Verifier.exe";

    // We must put libz3.dll in a directory not known to Visual Studio, otherwise the analyzer will not load.
    public static void Extract()
    {
        if (ExtractedPathTable.Count > 0)
            return;

        try
        {
            Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
            string Location = ExecutingAssembly.Location;

            // Ok, we're inside the system, let's be hacky.
            string AnalyzerRoot = Path.GetDirectoryName(Path.GetDirectoryName(Location));

            // Assume a folder architecture with each file in a separate directory.
            foreach (string Folder in Directory.GetDirectories(AnalyzerRoot))
            {
                string DllDirectory = Path.Combine(AnalyzerRoot, Folder);
                string MicrosoftZ3FilePath = Path.Combine(DllDirectory, "Microsoft.Z3.dll");

                // Look for the directory where Microsoft.Z3.dll is.
                if (File.Exists(MicrosoftZ3FilePath))
                {
                    // Create a subdirectory where to put libz3.dll.
                    string WinLocation = Path.Combine(DllDirectory, "win");
                    if (!Directory.Exists(WinLocation))
                        Directory.CreateDirectory(WinLocation);

                    string NativeLocation = Path.Combine(WinLocation, "native");
                    if (!Directory.Exists(NativeLocation))
                        Directory.CreateDirectory(NativeLocation);

                    // Make sure the new directory is in the path.
                    string PathEnvironmentVariableName = "PATH";
                    string PathEnvironmentVariable = Environment.GetEnvironmentVariable(PathEnvironmentVariableName);

                    if (!PathEnvironmentVariable.Contains(NativeLocation))
                        Environment.SetEnvironmentVariable(PathEnvironmentVariableName, PathEnvironmentVariable + $";{NativeLocation}");

                    ExtractFile(ExecutingAssembly, NativeLocation, Libz3FileName);
                    ExtractFile(ExecutingAssembly, DllDirectory, VerifierFileName);
                }
            }
        }
        catch
        {
        }
    }

    private static void ExtractFile(Assembly executingAssembly, string directoryPath, string fileName)
    {
        string DestinationPath = Path.Combine(directoryPath, fileName);

        try
        {
            if (!File.Exists(DestinationPath))
            {
                // Get a stream to the dll loaded inside us.
                using Stream ResourceStream = executingAssembly.GetManifestResourceStream($"Libz3Extractor.{fileName}");

                // Create a file stream in the new folder.
                using FileStream DllStream = new(DestinationPath, FileMode.Create, FileAccess.Write);

                // Copy the stream to have a new dll ready to load.
                ResourceStream.CopyTo(DllStream);
                ResourceStream.Flush();
            }
        }
        catch (IOException)
        {
        }

        ExtractedPathTable.Add(fileName, DestinationPath);
    }

    public static string GetExtractedPath(string fileName)
    {
        return ExtractedPathTable[fileName];
    }

    private static Dictionary<string, string> ExtractedPathTable = new();
}
