using Serilog.Events;
using Serilog;
using xdelta3.net;
using System.Diagnostics;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            if (File.Exists("GMLoader - xdelta.log"))
                File.Delete("GMLoader - xdelta.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File("GMLoader - xdelta.log")
                .CreateLogger();

            Console.WriteLine("What would you like to do:");
            Console.WriteLine("1: Make a single xdelta patch");
            Console.WriteLine("2: Make xdelta patch for all files inside the folder\n");
            string choice = Console.ReadLine();

            while (choice != "1" && choice != "2")
            {
                Console.WriteLine("Please choose between 1 or 2");
                choice = Console.ReadLine();
            }

            string outputFolder = Path.Combine(Environment.CurrentDirectory, "xdelta_output");
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

            mkDir(outputFolder);

            if (choice == "1")
            {
                Log.Debug("User selected first choice");
                Console.WriteLine("\nSelect the vanilla file:\n");
                string vanillaFile = SelectFile("Select the vanilla file");
                if (string.IsNullOrEmpty(vanillaFile)) return;

                Console.WriteLine("\nSelect the modded file:\n");
                string modFile = SelectFile("Select the modded file");
                if (string.IsNullOrEmpty(modFile)) return;

                string vanillaFileName = Path.GetFileName(vanillaFile);
                string outputFile = Path.Combine(outputFolder, vanillaFileName + ".xdelta");

                makeDeltaPatch(vanillaFile, modFile, outputFile);
                Log.Information($"Patch file has been created at {outputFile}\n\nPress any key to close.");
                Console.ReadKey();
            }
            else if (choice == "2")
            {
                Log.Debug("User selected second choice");
                Console.WriteLine("\nSelect the vanilla folder:\n");
                string vanillaFolder = SelectFolder("Select the vanilla folder");
                if (string.IsNullOrEmpty(vanillaFolder)) return;

                Console.WriteLine("\nSelect the modded folder:\n");
                string moddedFolder = SelectFolder("Select the modded folder");
                if (string.IsNullOrEmpty(moddedFolder)) return;

                Stopwatch stopwatch = Stopwatch.StartNew();
                bulkDeltaPatch(vanillaFolder, moddedFolder, outputFolder);
                stopwatch.Stop();
                Log.Information($"Done, Elapsed time: {stopwatch.Elapsed.TotalSeconds:F2} seconds ({stopwatch.ElapsedMilliseconds} ms)");
                Log.Information($"Patch files has been created at {outputFolder}\n\nPress any key to close.");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    static void bulkDeltaPatch(string vanillaFolder, string moddedFolder, string outputFolder)
    {
        var vanillaFiles = Directory.GetFiles(vanillaFolder, "*.*", SearchOption.AllDirectories);
        var moddedFiles = Directory.GetFiles(moddedFolder, "*.*", SearchOption.AllDirectories);

        // Create a dictionary of modded file paths relative to the moddedFolder for quick lookup
        var moddedFileDict = moddedFiles.ToDictionary(
            file => Path.GetRelativePath(moddedFolder, file),
            file => file
        );

        Parallel.ForEach(vanillaFiles, vanillaFile =>
        {
            string relativePath = Path.GetRelativePath(vanillaFolder, vanillaFile);

            if (moddedFileDict.TryGetValue(relativePath, out string moddedFile))
            {
                string outputFile = Path.Combine(outputFolder, relativePath + ".xdelta");
                string outputFileDir = Path.GetDirectoryName(outputFile);

                mkDir(outputFileDir);

                try
                {
                    makeDeltaPatch(vanillaFile, moddedFile, outputFile);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to create patch for {relativePath} : {ex}");
                }
            }
            else
            {
                Log.Warning($"No corresponding modded file for: {relativePath}");
            }
        });
    }
    static void mkDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    static string SelectFile(string description)
    {
        using (var fileDialog = new OpenFileDialog())
        {
            fileDialog.Title = description;
            fileDialog.InitialDirectory = Environment.CurrentDirectory;
            fileDialog.Filter = "All files (*.*)|*.*"; // You can modify this filter based on your needs

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                return fileDialog.FileName;
            }
        }
        return null;
    }
    static string SelectFolder(string description)
    {
        using (var folderDialog = new FolderBrowserDialog())
        {
            folderDialog.Description = description;
            folderDialog.SelectedPath = Environment.CurrentDirectory;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                return folderDialog.SelectedPath;
            }
        }
        return null;
    }
    static void makeDeltaPatch(string sourceFilePath, string modifiedFilePath, string patchFilePath)
    {
        byte[] originalData = File.ReadAllBytes(sourceFilePath);
        byte[] modifiedData = File.ReadAllBytes(modifiedFilePath);

        var delta = Xdelta3Lib.Encode(source: originalData, target: modifiedData).ToArray();

        File.WriteAllBytes(patchFilePath, delta);
        Log.Information($"Created xdelta patch for {Path.GetFileName(sourceFilePath)}");
    }

}