namespace Libz3Extractor;

using System;
using System.IO;
using System.Reflection;

public class Extractor
{
    public static bool Extract()
    {
        try
        {
            Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
            string Location = ExecutingAssembly.Location;

            // Ok, we're inside the system, let's be hacky.
            string AnalyzerRoot = Path.GetDirectoryName(Path.GetDirectoryName(Location));

            // Assume a folder architecture with each file in a separate directory.
            foreach (string Directory in Directory.GetDirectories(AnalyzerRoot))
            {
                string DllDirectory = Path.Combine(AnalyzerRoot, Directory);
                string MicrosoftZ3FilePath = Path.Combine(DllDirectory, "Microsoft.Z3.dll");

                // Look for the directory where Microsoft.Z3.dll is.
                if (File.Exists(MicrosoftZ3FilePath))
                {
                    string Libz3FilePath = Path.Combine(DllDirectory, "libz3.dll");

                    // Get a stream to the dll loaded inside us.
                    using Stream ResourceStream = ExecutingAssembly.GetManifestResourceStream("Libz3Extractor.libz3.dll");

                    // Create a file stream in the same folder as Microsoft.Z3.dll.
                    using FileStream DllStream = new(Libz3FilePath, FileMode.Create, FileAccess.Write);

                    // Copy the stream to have a new dll ready to load.
                    ResourceStream.CopyTo(DllStream);
                    ResourceStream.Flush();

                    return true;
                }
            }
        }
        catch
        {
        }

        return false;
    }
}