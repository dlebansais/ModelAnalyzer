namespace FileExtractor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class Extractor
{
    public const string Libz3FileName = "libz3.dll";
    public const string VerifierFileName = "Verifier.exe";

    // We must put libz3 inside Verifier.exe, otherwise the analyzer will not load.
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
                string ThisFileFilePath = Path.Combine(DllDirectory, "FileExtractor.dll");

                // Look for the directory where FileExtractor.dll is.
                if (File.Exists(ThisFileFilePath))
                {
                    ExtractFile(ExecutingAssembly, DllDirectory, Libz3FileName);
                    ExtractFile(ExecutingAssembly, DllDirectory, VerifierFileName);
                }
            }
        }
        catch (Exception exception)
        {
            LastExceptionMessage = exception.Message;
        }
    }

    private static void ExtractFile(Assembly executingAssembly, string directoryPath, string fileName)
    {
        string DestinationPath = Path.Combine(directoryPath, fileName);

        try
        {
            if (!File.Exists(DestinationPath))
            {
                // Get a stream to the file loaded inside us.
                using Stream ResourceStream = executingAssembly.GetManifestResourceStream($"FileExtractor.{fileName}");

                // Create a file stream in the new folder.
                using FileStream DllStream = new(DestinationPath, FileMode.Create, FileAccess.Write);

                // Copy the stream to have a new file ready to use.
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

    public static Dictionary<string, string> ExtractedPathTable { get; } = new();
    public static string LastExceptionMessage { get; private set; } = string.Empty;
}
