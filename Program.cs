#region Using Directives
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using Newtonsoft.Json.Linq;
using Standart.Hash.xxHash;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
#endregion

namespace GMLoader;

public class GMLoaderProgram
{
    #region Properties
    public static UndertaleData Data { get; set; }
    private static ScriptOptions CliScriptOptions { get; set; }
    #endregion

    static void Main()
    {
        try
        {
            Console.Title = "GMLoader";

            if (File.Exists("GMLoader.log"))
            {
                File.Delete("GMLoader.log");
            }

            if (!File.Exists("GMLoader.ini"))
            {
                PrintMessage("Missing GMLoader.ini file \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
                .Build();

            #region Field
            string backupDataPath = configuration["universal:databackup"];
            string gameDataPath = configuration["universal:originaldata"];

            string importPreCSXPath = configuration["universal:importprecsx"];
            string compilePreCSXString = configuration["universal:compileprecsx"];

            string importBuiltInCSXPath = configuration["universal:importbuiltincsx"];
            string compileBuiltInCSXString = configuration["universal:compilebuiltincsx"];

            string importPostCSXPath = configuration["universal:importpostcsx"];
            string compilePostCSXString = configuration["universal:compilepostcsx"];

            string supportedHashVersion = configuration["gamespecific:supportedhashversion"];
            string autoGameStartString = configuration["universal:autogamestart"];
            string gameExecutable = configuration["gamespecific:gameexecutable"];

            bool compilePreCSX = bool.Parse(compilePreCSXString);
            bool compileBuiltInCSX = bool.Parse(compileBuiltInCSXString);
            bool compilePostCSX = bool.Parse(compilePostCSXString);

            bool autoGameStart = bool.Parse(autoGameStartString);
            ulong currentHash = 0;
            #endregion

            CreateDirectoryIfNotExists(importPreCSXPath);
            CreateDirectoryIfNotExists(importBuiltInCSXPath);
            CreateDirectoryIfNotExists(importPostCSXPath);

            string[] dirPreCSXFiles = Directory.GetFiles(importPreCSXPath, "*.csx");
            string[] dirBuiltInCSXFiles = Directory.GetFiles(importBuiltInCSXPath, "*.csx");
            string[] dirPostCSXFiles = Directory.GetFiles(importPostCSXPath, "*.csx");

            if (!compilePreCSX && !compileBuiltInCSX && !compilePostCSX)
            {
                PrintMessage("What's the point of using GMLoader if you disable CSX COMPILING REEEEEEEEEEE \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (dirBuiltInCSXFiles.Length == 0 && dirPreCSXFiles.Length == 0 && dirPostCSXFiles.Length == 0)
            {
                PrintMessage($"The CSX Script folder path is empty.\nAborting the process\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (!dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")) && !dirPreCSXFiles.Any(x => x.EndsWith(".csx")) && !dirPostCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"No CSX Script file found in the csx directory.\nAborting the process\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            PrintMessage(DateTime.Now.ToString() + "\n\nReading game data...\n");
            Console.ResetColor();

            if (File.Exists(backupDataPath))
            {
                currentHash = ComputeFileHash(backupDataPath);
            }

            Data = new UndertaleData();
            if (File.Exists(backupDataPath) && supportedHashVersion == currentHash.ToString())
            {
                using (var stream = new FileStream(backupDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);
            }
            else if (File.Exists(backupDataPath) && supportedHashVersion != currentHash.ToString())
            {
                PrintMessage("Game Data Hash Mismatch Error\nThis happens because loader is outdated or the data.win is modified.\nDelete backup.win and verify the integrity of game.\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                using (var stream = new FileStream(gameDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);
                File.Copy(gameDataPath, backupDataPath);
                PrintMessage("Backup of the data has been created");
            }

            Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content}";
            Console.ForegroundColor = ConsoleColor.Magenta;
            PrintMessage($"\nLoaded game {Data.GeneralInfo.Name.Content}");
            Console.ResetColor();

            //CSX Handling
            ScriptOptionsInitialize();

            //Compile users script before builtin scripts
            if (dirPreCSXFiles.Length != 0)
            {
                if (compilePreCSX)
                {
                    PrintMessage("\nLoading pre-CSX Scripts.");
                    foreach (string file in dirPreCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    PrintMessage("\nPre-Loading CSX script is disabled, skipping the process.");
                }
            }
            else if (compilePreCSX)
            {
                PrintMessage($"\nThe pre-CSX folder is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPreCSXPath))} , skipping the process.");
            }
            else if (compilePreCSX && !dirPreCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"\nNo pre-CSX script file found. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPreCSXPath))} , skipping the process.");
            }

            //Compile builtin scripts
            if (dirBuiltInCSXFiles.Length != 0)
            {
                if (compileBuiltInCSX)
                {
                    PrintMessage("\nLoading builtin-CSX scripts.");
                    foreach (string file in dirBuiltInCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    PrintMessage("\nLoading builtin-CSX script is disabled, skipping the process.");
                }
            }
            else if (compileBuiltInCSX)
            {
                PrintMessage($"\nThe builtin-CSX folder path is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath))} , skipping the process.");
            }
            else if (compileBuiltInCSX && !dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"\nNo builtin-CSX script file found at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath))} , skipping the process.");
            }

            //Compile users script after builtin scripts

            if (dirPostCSXFiles.Length != 0)
            {
                if (compilePostCSX)
                {
                    PrintMessage("\nLoading post-CSX Scripts.");
                    foreach (string file in dirPostCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    PrintMessage("\nLoading post-CSX script is disabled, skipping the process.");
                }
            }
            else if (compilePostCSX)
            {
                PrintMessage($"\nThe post-CSX folder is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPostCSXPath))} , skipping the process.");
            }
            else if (compilePostCSX && !dirPostCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"\nNo post-CSX script file found At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPostCSXPath))} , skipping the process.");
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            PrintMessage("\nRecompiling the data...");
            using (var stream = new FileStream(gameDataPath, FileMode.Create, FileAccess.ReadWrite))
                UndertaleIO.Write(stream, Data);
            Console.ResetColor();

            if (autoGameStart)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                PrintMessage("\nGame Data has been recompiled, Launching the game...");
                Process.Start(gameExecutable);
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                PrintMessage("\nGame Data has been recompiled, you can now launch the modded data through the game executable.\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            PrintMessage("An error occurred: " + e.Message);
            Console.ResetColor();
            Console.WriteLine("\nPress any key to close...");
            Console.ReadKey();
        }
    }

    #region Helper Methods

    public static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void PrintMessage(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText("GMLoader.log", "\n" + message);
    }

    public static ulong ComputeFileHash(string filePath)
    {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
            ulong hash = xxHash64.ComputeHash(fileStream);
            return hash;
        }
    }

    #endregion

    #region Script Handling

    private static void RunCSharpFile(string path)
    {
        string lines;
        try
        {
            lines = File.ReadAllText(path);
        }
        catch (Exception exc)
        {
            // rethrow as otherwise this will get interpreted as success
            PrintMessage(exc.Message);
            throw;
        }

        lines = $"#line 1 \"{path}\"\n" + lines;
        var ScriptPath = path;
        RunCSharpCode(lines, ScriptPath);
    }

    private static void RunCSharpCode(string code, string scriptFile = null)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        PrintMessage($"\nAttempting to execute '{Path.GetFileName(scriptFile)}'");
        Console.ResetColor();
        var ScriptErrorMessage = "";
        var ScriptExecutionSuccess = false;
        try
        {
            CSharpScript.EvaluateAsync(code, CliScriptOptions);
            ScriptExecutionSuccess = true;
            ScriptErrorMessage = "";
        }
        catch (Exception exc)
        {
            ScriptExecutionSuccess = false;
            ScriptErrorMessage = exc.ToString();
            //ScriptErrorType = "Exception";
            PrintMessage(exc.ToString());
        }

        //if (!FinishedMessageEnabled) return;

        if (ScriptExecutionSuccess)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            PrintMessage($"Finished executing '{Path.GetFileName(scriptFile)}'");
            Console.ResetColor();
        }
        else
        {
            PrintMessage(ScriptErrorMessage);
        }
    }

    private static void ScriptOptionsInitialize()
    {
        var references = new[]
        {
            typeof(UndertaleObject).Assembly,
            typeof(GMLoader.GMLoaderProgram).Assembly,
            typeof(ConfigurationBuilder).Assembly,
            typeof(IniConfigurationSource).Assembly,
            typeof(Newtonsoft.Json.Linq.JObject).Assembly,
            typeof(System.Text.Json.JsonSerializer).Assembly
        };

        CliScriptOptions = ScriptOptions.Default
            .WithReferences(references)
            .AddImports("UndertaleModLib", "UndertaleModLib.Models", "UndertaleModLib.Decompiler",
                "UndertaleModLib.Scripting", "UndertaleModLib.Compiler",
                "UndertaleModLib.Util", "GMLoader", "GMLoader.GMLoaderProgram", "System", "System.Linq", "System.IO", "System.Collections.Generic", "Microsoft.Extensions.Configuration", "System.Text.Json", "Newtonsoft.Json.Linq")
            // "WithEmitDebugInformation(true)" not only lets us to see a script line number which threw an exception,
            // but also provides other useful debug info when we run UMT in "Debug".
            .WithEmitDebugInformation(true);
    }

    #endregion

}
