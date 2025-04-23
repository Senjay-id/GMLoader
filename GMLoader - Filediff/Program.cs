using Serilog.Events;
using Serilog;
using Standart.Hash.xxHash;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Config.Net;
public interface IConfig
{
    public string ExportTextureConfigOutputPath { get; }
    public string ExportBackgroundTextureConfigOutputPath { get; }
}

class Program
{
    public static List<string> invalidFileStreamNames { get; set; }
    public static int invalidFileStream { get; set; }
    public static string exportTextureConfigOutputPath { get; set; }
    public static string exportBackgroundTextureConfigOutputPath { get; set; }

    [STAThread]
    static void Main()
    {
        try
        {
            if (File.Exists("GMLoader - Filediff.log"))
                File.Delete("GMLoader - Filediff.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File("GMLoader - Filediff.log")
                .CreateLogger();

            if (!File.Exists("..\\GMLoader.ini"))
            {
                Log.Information("Missing GMLoader.ini file \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            IConfig config = new ConfigurationBuilder<IConfig>()
               .UseIniFile("..\\GMLoader.ini")
               .Build();

            exportTextureConfigOutputPath = config.ExportTextureConfigOutputPath;
            exportBackgroundTextureConfigOutputPath = config.ExportBackgroundTextureConfigOutputPath;

            Console.WriteLine("Select the vanilla folder");
            string firstFolderPath = SelectFolder("Select the vanilla folder");
            if (string.IsNullOrEmpty(firstFolderPath)) return;

            Console.WriteLine("Select the modded folder\n");
            string secondFolderPath = SelectFolder("Select the modded folder");
            if (string.IsNullOrEmpty(secondFolderPath)) return;

            string outputFolderPath = Path.Combine(Environment.CurrentDirectory, "filediff_output");
            if (Directory.Exists(outputFolderPath))
                Directory.Delete(outputFolderPath, true);
            mkDir(outputFolderPath);

            invalidFileStreamNames = new List<string>();
            invalidFileStream = 0;

            CompareAndCopyFiles(firstFolderPath, secondFolderPath, outputFolderPath);

            if (invalidFileStream > 0)
            {
                Log.Information("");
                Log.Error("Error, invalid file stream length for the file below:");
                foreach (string name in invalidFileStreamNames)
                {
                    Log.Error(name);
                }
                Log.Error("The invalid file stream could be caused by empty file, please check if the file is indeed empty in UMT");
                Log.Information("");
            }

            Log.Information("Merging sprite configuration files");
            string spriteConfigFileName = "MyModdedSpriteConfig.yaml";
            string backgroundSpriteConfigFilename = "MyModdedBackgroundSpriteConfig.yaml";
            string spriteConfigRelativePath = Path.Combine(outputFolderPath, Path.GetFileName(exportTextureConfigOutputPath));
            string backgroundSpriteConfigRelativePath = Path.Combine(spriteConfigRelativePath, "backgrounds");
            mkDir(spriteConfigRelativePath);
            mkDir(backgroundSpriteConfigRelativePath);
            string[] spriteConfigFiles = Directory.GetFiles(spriteConfigRelativePath, "*.yaml", SearchOption.TopDirectoryOnly);
            string[] BackgroundSpriteConfigFIles = Directory.GetFiles(backgroundSpriteConfigRelativePath, "*.yaml", SearchOption.TopDirectoryOnly);
            
            if (spriteConfigFiles.Length != 0)
            {
                using (StreamWriter writer = new StreamWriter(spriteConfigFileName))
                {
                    foreach (string file in spriteConfigFiles)
                    {
                        Log.Information($"Merging {Path.GetFileName(file)}");

                        string fileContent = File.ReadAllText(file);
                        writer.WriteLine(fileContent);
                        //writer.WriteLine(); // Add blank line between files
                    }
                }
                foreach (string file in spriteConfigFiles)
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted: {Path.GetFileName(file)}");
                }
            }
            if (BackgroundSpriteConfigFIles.Length != 0)
            {
                using (StreamWriter writer = new StreamWriter(backgroundSpriteConfigFilename))
                {
                    foreach (string file in BackgroundSpriteConfigFIles)
                    {
                        Log.Information($"Merging {Path.GetFileName(file)}");

                        string fileContent = File.ReadAllText(file);
                        writer.WriteLine(fileContent);
                        //writer.WriteLine(); // Add blank line between files
                    }
                    foreach (string file in BackgroundSpriteConfigFIles)
                    {
                        File.Delete(file);
                        Log.Information($"Deleted: {Path.GetFileName(file)}");
                    }
                }
            }

            File.Copy(spriteConfigFileName, Path.Combine(spriteConfigRelativePath, spriteConfigFileName));
            File.Copy(backgroundSpriteConfigFilename, Path.Combine(backgroundSpriteConfigRelativePath, backgroundSpriteConfigFilename));

            Console.WriteLine($"All files have been copied to the output folder at: {outputFolderPath}");
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
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

    static void CompareAndCopyFiles(string vanillaFolder, string moddedFolder, string outputFolder)
    {
        var vanillaFiles = Directory.GetFiles(vanillaFolder, "*.*", SearchOption.AllDirectories);
        var moddedFiles = Directory.GetFiles(moddedFolder, "*.*", SearchOption.AllDirectories);
        var regex = new Regex(@"^(.*?)(?=_f[0-9])", RegexOptions.Compiled);

        var vanillaFileNames = new ConcurrentDictionary<string, string>();

        // Populate the dictionary with vanilla file names
        Parallel.ForEach(vanillaFiles, vanillaFile =>
        {
            var match = regex.Match(Path.GetFileName(vanillaFile));
            if (match.Success)
            {
                string baseName = match.Groups[1].Value;
                //Log.Information($"Storing basename: {baseName} which equals to {vanillaFile}");
                vanillaFileNames[baseName] = vanillaFile;
            }
            else if (vanillaFile.Contains("backgrounds") || vanillaFile.Contains("nostrip"))
            {
                //Log.Warning($"{vanillaFile} ends with backgrounds or nostrip");
                string fileName = Path.GetFileName(vanillaFile);
                vanillaFileNames[fileName] = vanillaFile;
            }
            else
            {
                vanillaFileNames[Path.GetFileName(vanillaFile)] = vanillaFile; // Store all other vanilla files
            }
        });

        // Process modded files and compare with vanilla
        Parallel.ForEach(moddedFiles, moddedFile =>
        {
            string fileName = Path.GetFileName(moddedFile);
            var match = regex.Match(Path.GetFileName(moddedFile));
            if (match.Success)
            {
                fileName = match.Groups[1].Value;
                //Log.Information($"Storing fileName: {fileName} which equals to {moddedFile}");
            }
            
            string relativePath = Path.GetRelativePath(moddedFolder, moddedFile);
            string outputFilePath = Path.Combine(outputFolder, relativePath);

            // Always copy spriteData regardless of hash
            if (fileName.Equals("data.json", StringComparison.OrdinalIgnoreCase))
            {
                Log.Information($"Copying {moddedFile}");
                mkDir(Path.GetDirectoryName(outputFilePath));
                File.Copy(moddedFile, outputFilePath, true);
                return;
            }

            if (vanillaFileNames.TryGetValue(fileName, out string vanillaFile))
            {
                ulong vanillaHash = ComputeFileHash3(vanillaFile);
                ulong modHash = ComputeFileHash3(moddedFile);

                // Compare the files
                if (vanillaHash != modHash)
                {
                    //Log.Information($"{vanillaFile} and {moddedFile} hash is not equal");
                    Log.Information($"File hash is different, Copying {Path.GetFileName(moddedFile)}");
                    mkDir(Path.GetDirectoryName(outputFilePath));
                    File.Copy(moddedFile, outputFilePath, true);
                }
            }
            else
            {
                Log.Information($"Vanilla file not found. Copying {Path.GetFileName(moddedFile)}");
                mkDir(Path.GetDirectoryName(outputFilePath));
                File.Copy(moddedFile, outputFilePath, true);
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

    static ulong ComputeFileHash3(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            if (stream.Length == 0)
            {
                Log.Error($"Error, {Path.GetFileName(filePath)} stream length is 0");
                invalidFileStreamNames.Add(Path.GetFileName(filePath));
                invalidFileStream++;
                return 0; // or some other default value
            }
            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, (int)stream.Length);
            return xxHash3.ComputeHash(fileBytes, (int)stream.Length, 0);
        }
    }
}
