#region Using Directives
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using Newtonsoft.Json.Linq;
using Serilog;
using Standart.Hash.xxHash;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Models;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using Serilog.Events;
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
            if (File.Exists("GMLoader.log"))
                File.Delete("GMLoader.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File("GMLoader.log")
                .CreateLogger();

            Console.Title = "GMLoader";

            if (!File.Exists("GMLoader.ini"))
            {
                Log.Information("Missing GMLoader.ini file \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
                .Build();

            #region Config
            string backupDataPath = configuration["universal:databackup"];
            string gameDataPath = configuration["universal:originaldata"];
            string modsPath = configuration["universal:modsdirectory"];

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
            mkDir(modsPath);
            mkDir(importPreCSXPath);
            mkDir(importBuiltInCSXPath);
            mkDir(importPostCSXPath);

            string modsPathAbsoluteDir = Path.GetFullPath(modsPath);
            if (Directory.Exists(modsPath))
            {
                Log.Debug($"Scanning the filetree of {modsPathAbsoluteDir}");
                Log.Debug($"{Path.GetFileName(modsPathAbsoluteDir)}");
                PrintFileTree(modsPath, "", true);
            }

            string[] dirPreCSXFiles = Directory.GetFiles(importPreCSXPath, "*.csx");
            string[] dirBuiltInCSXFiles = Directory.GetFiles(importBuiltInCSXPath, "*.csx");
            string[] dirPostCSXFiles = Directory.GetFiles(importPostCSXPath, "*.csx");

            if (!compilePreCSX && !compileBuiltInCSX && !compilePostCSX)
            {
                Log.Information("What's the point of using GMLoader if you disable CSX COMPILING AAAAAAAAAAAAAAAAA \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (dirBuiltInCSXFiles.Length == 0 && dirPreCSXFiles.Length == 0 && dirPostCSXFiles.Length == 0)
            {
                Log.Information($"The CSX Script folder path is empty.\nAborting the process\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (!dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")) && !dirPreCSXFiles.Any(x => x.EndsWith(".csx")) && !dirPostCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                Log.Information($"No CSX Script file found in the csx directory.\nAborting the process\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Log.Information("Reading game data...");

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
                Log.Information("\nGame Data Hash Mismatch Error.\nThis happens because modloader is outdated or the data.win is modified.\n\nDelete backup.win and verify the integrity of game.\n\nIf your using MO2, check the overwrite folder and delete backup.win\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                using (var stream = new FileStream(gameDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);
                File.Copy(gameDataPath, backupDataPath);
                Log.Information($"Backup of the data has been created at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, backupDataPath))}");
            }

            Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content}";
            Log.Information($"Loaded game {Data.GeneralInfo.Name.Content}");

            //CSX Handling
            ScriptOptionsInitialize();

            //Compile users script before builtin scripts
            if (dirPreCSXFiles.Length != 0)
            {
                if (compilePreCSX)
                {
                    Log.Information("Loading pre-CSX Scripts.");
                    foreach (string file in dirPreCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    Log.Information("Pre-Loading CSX script is disabled, skipping the process.");
                }
            }
            else if (compilePreCSX)
            {
                Log.Debug($"The pre-CSX folder is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPreCSXPath))} , skipping the process.");
            }
            else if (compilePreCSX && !dirPreCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                Log.Debug($"No pre-CSX script file found. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPreCSXPath))} , skipping the process.");
            }

            //Compile builtin scripts
            if (dirBuiltInCSXFiles.Length != 0)
            {
                if (compileBuiltInCSX)
                {
                    Log.Information("Loading builtin-CSX scripts.");
                    foreach (string file in dirBuiltInCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    Log.Information("Loading builtin-CSX script is disabled, skipping the process.");
                }
            }
            else if (compileBuiltInCSX)
            {
                Log.Information($"The builtin-CSX folder path is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath))} , skipping the process.");
            }
            else if (compileBuiltInCSX && !dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                Log.Information($"No builtin-CSX script file found at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath))} , skipping the process.");
            }

            //Compile users script after builtin scripts

            if (dirPostCSXFiles.Length != 0)
            {
                if (compilePostCSX)
                {
                    Log.Information("Loading post-CSX Scripts.");
                    foreach (string file in dirPostCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    Log.Information("Loading post-CSX script is disabled, skipping the process.");
                }
            }
            else if (compilePostCSX)
            {
                Log.Debug($"The post-CSX folder is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPostCSXPath))} , skipping the process.");
            }
            else if (compilePostCSX && !dirPostCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                Log.Debug($"No post-CSX script file found At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPostCSXPath))} , skipping the process.");
            }

            Log.Information("Recompiling the data...");
            using (var stream = new FileStream(gameDataPath, FileMode.Create, FileAccess.ReadWrite))
                UndertaleIO.Write(stream, Data);

            if (autoGameStart)
            {
                Log.Information("Game Data has been recompiled, Launching the game...");
                Process.Start(gameExecutable);
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            else
            {
                Log.Information("Game Data has been recompiled, you can now launch the modded data through the game executable.\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        catch (Exception e)
        {
            Log.Warning("An error occurred: " + e.Message);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }

    #region Helper Methods

    public static void mkDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static ulong ComputeFileHash(string filePath)
    {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
            ulong hash = xxHash64.ComputeHash(fileStream);
            return hash;
        }
    }
    private static void PrintFileTree(string path, string indent, bool isLast)
    {
        string[] files = Directory.GetFiles(path);
        string[] directories = Directory.GetDirectories(path);

        // Process files in the current directory
        for (int i = 0; i < files.Length; i++)
        {
            bool lastFile = (i == files.Length - 1 && directories.Length == 0);
            string prefix = lastFile ? "└── " : "├── ";
            Log.Debug($"{indent}{prefix}{Path.GetFileName(files[i])}");
        }

        // Process subdirectories, excluding 'lib'
        for (int i = 0; i < directories.Length; i++)
        {
            string dirName = Path.GetFileName(directories[i]);
            if (dirName.Equals("lib", StringComparison.OrdinalIgnoreCase))
                continue; // Skip 'lib' directory

            bool lastDir = i == directories.Length - 1;
            string dirPrefix = lastDir ? "└── " : "├── ";

            Log.Debug($"{indent}{dirPrefix}{dirName}");

            // Recursive call with updated indent
            string newIndent = indent + (lastDir ? "    " : "|   ");
            PrintFileTree(directories[i], newIndent, lastDir);
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
            Log.Information(exc.Message);
            throw;
        }

        lines = $"#line 1 \"{path}\"\n" + lines;
        var ScriptPath = path;
        RunCSharpCode(lines, ScriptPath);
    }

    private static void RunCSharpCode(string code, string scriptFile = null)
    {
        Log.Information($"Attempting to execute '{Path.GetFileName(scriptFile)}'");
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
            Log.Information(exc.ToString());
        }

        //if (!FinishedMessageEnabled) return;

        if (ScriptExecutionSuccess)
        {
            Log.Information($"Finished executing '{Path.GetFileName(scriptFile)}'");
        }
        else
        {
            Log.Information(ScriptErrorMessage);
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
                "UndertaleModLib.Util", "GMLoader", "GMLoader.GMLoaderProgram", "Serilog", "System", "System.Linq", 
                "System.IO", "System.Collections.Generic", "System.Drawing", "System.Drawing.Imaging", 
                "System.Collections", "System.Text.RegularExpressions", "System.Text.Json", "System.Diagnostics",
                "System.Threading", "System.Threading.Tasks", "Microsoft.Extensions.Configuration", "Newtonsoft.Json.Linq")
            // "WithEmitDebugInformation(true)" not only lets us to see a script line number which threw an exception,
            // but also provides other useful debug info when we run UMT in "Debug".
            .WithEmitDebugInformation(true);
    }

    #endregion

}
