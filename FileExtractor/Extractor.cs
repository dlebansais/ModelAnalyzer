namespace FileExtractor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
/// Provides tool to extract files from resources.
/// </summary>
public static class Extractor
{
    /// <summary>
    /// The libz3 library name.
    /// </summary>
    public const string Libz3FileName = "libz3.dll";

    /// <summary>
    /// The verifier exe name.
    /// </summary>
    public const string VerifierFileName = "Verifier.exe";

    /// <summary>
    /// Extracts files in resources into a subfolder of this module.
    /// </summary>
    public static void Extract()
    {
        if (ExtractedPathTable.Count > 0)
            return;

        // We must put libz3 along with Verifier.exe in a directory Windows doesn't know about, otherwise the analyzer will not load.
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
                    // Create a #@$*! subdirectory where to put files. F*** Windows.
                    string WinLocation = Path.Combine(DllDirectory, "win");
                    if (!Directory.Exists(WinLocation))
                        Directory.CreateDirectory(WinLocation);

                    ExtractFile(ExecutingAssembly, WinLocation, Libz3FileName);
                    ExtractFile(ExecutingAssembly, WinLocation, VerifierFileName);
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

    /// <summary>
    /// Gets the path where a file was extracted.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    public static string GetExtractedPath(string fileName)
    {
        return ExtractedPathTable[fileName];
    }

    /// <summary>
    /// Resets extraction so that a call to <see cref="Extract"/> will extract files again.
    /// </summary>
    public static void Reset()
    {
        ExtractedPathTable.Clear();
        LastExceptionMessage = string.Empty;
    }

    /// <summary>
    /// Gets the last exception message.
    /// </summary>
    public static string LastExceptionMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the table of extracted files.
    /// </summary>
    public static Dictionary<string, string> ExtractedPathTable { get; } = new();
}
