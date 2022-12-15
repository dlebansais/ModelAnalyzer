namespace Libz3Extractor;

using System;
using System.IO;
using System.Reflection;

public class Extractor
{
    // We must put libz3.dll in a directory not known to Visual Studio, otherwise the analyzer will not load.
    public static void Extract()
    {
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

                    string Libz3FilePath = Path.Combine(NativeLocation, "libz3.dll");

                    // Get a stream to the dll loaded inside us.
                    using Stream ResourceStream = ExecutingAssembly.GetManifestResourceStream("Libz3Extractor.libz3.dll");

                    // Create a file stream in the new folder.
                    using FileStream DllStream = new(Libz3FilePath, FileMode.Create, FileAccess.Write);

                    // Copy the stream to have a new dll ready to load.
                    ResourceStream.CopyTo(DllStream);
                    ResourceStream.Flush();
                }
            }
        }
        catch
        {
        }
    }
}