#region Using Directives
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
using Config.Net;
using ImageMagick;
#endregion

namespace GMLoader;
public interface IConfig
{
    public bool AutoGameStart { get; }
    public string GameExecutable { get; }
    public string SupportedDataHash { get; }
    public string ImportPreCSX { get; }
    public string ImportBuiltinCSX { get; }
    public string ImportPostCSX { get; }
    public string ImportAfterCSX { get; }
    public bool CompilePreCSX { get; }
    public bool CompileBuiltinCSX { get; }
    public bool CompilePostCSX { get; }
    public bool CompileAfterCSX { get; }
    public bool CompileGML { get; }
    public bool CompileASM { get; }
    public string BackupData { get; }
    public string GameData { get; }
    public string ModsDirectory { get; }
    public string TexturesDirectory { get; }
    public string ShaderDirectory { get; }
    public string ConfigDirectory { get; }
    public string GMLCodeDirectory { get; }
    public string CollisionDirectory { get; }
    public string ASMDirectory { get; }
    public string AppendGMLDirectory { get; }
    public string AppendGMLCollisionDirectory { get; }
    public string NewObjectDirectory { get; }
    public string ExistingObjectDirectory { get; }
    public int DefaultSpriteX { get; }
    public int DefaultSpriteY { get; }
    public uint DefaultSpriteSpeedType { get; }
    public float DefaultSpriteFrameSpeed { get; }
    public uint DefaultSpriteBoundingBoxType { get; }
    public int DefaultSpriteBoundingBoxLeft { get; }
    public int DefaultSpriteBoundingBoxRight { get; }
    public int DefaultSpriteBoundingBoxBottom { get; }
    public int DefaultSpriteBoundingBoxTop { get; }
    public uint DefaultSpriteSepMasksType { get; }
    public bool DefaultSpriteTransparent { get; }
    public bool DefaultSpriteSmooth { get; }
    public bool DefaultSpritePreload { get; }
    public uint DefaultSpriteSpecialVer { get; }
    public bool DefaultBGTransparent { get; }
    public bool DefaultBGSmooth { get; }
    public bool DefaultBGPreload { get; }
    public uint DefaultBGTileWidth { get; }
    public uint DefaultBGTileHeight { get; }
    public uint DefaultBGBorderX { get; }
    public uint DefaultBGBorderY { get; }
    public uint DefaultBGTileColumn { get; }
    public uint DefaultBGItemOrFramePerTile { get; }
    public uint DefaultBGTileCount { get; }
    public int DefaultBGFrameTime { get; }
}

public class GMLoaderProgram
{
    #region Properties
    public static UndertaleData Data { get; set; }
    private static ScriptOptions CliScriptOptions { get; set; }
    public static string modsPath { get; set; }
    public static string texturesPath { get; set; }
    public static string shaderPath { get; set; }
    public static string configPath { get; set; }
    public static string gmlCodePath { get; set; }
    public static string collisionPath { get; set; }
    public static string asmPath { get; set; }
    public static string appendGMLPath { get; set; }
    public static string appendGMLCollisionPath { get; set; }
    public static string newObjectPath { get; set; }
    public static string existingObjectPath { get; set; }
    public static bool compileGML { get; set; }
    public static bool compileASM { get; set; }
    public static int defaultSpriteX { get; set; }
    public static int defaultSpriteY { get; set; }
    public static float defaultSpriteFrameSpeed { get; set; }
    public static int defaultSpriteBoundingBoxLeft { get; set; }
    public static int defaultSpriteBoundingBoxRight { get; set; }
    public static int defaultSpriteBoundingBoxBottom { get; set; }
    public static int defaultSpriteBoundingBoxTop { get; set; }
    public static bool defaultSpriteTransparent { get; set; }
    public static bool defaultSpriteSmooth { get; set; }
    public static bool defaultSpritePreload { get; set; }
    public static uint defaultSpriteSpecialVer { get; set; }
    public static uint defaultSpriteSpeedType { get; set; }
    public static uint defaultSpriteBoundingBoxType { get; set; }
    public static uint defaultSpriteSepMasksType { get; set; }
    public static bool defaultBGTransparent { get; set; }
    public static bool defaultBGSmooth { get; set; }
    public static bool defaultBGPreload { get; set; }
    public static uint defaultBGTileWidth { get; set; }
    public static uint defaultBGTileHeight { get; set; }
    public static uint defaultBGBorderX { get; set; }
    public static uint defaultBGBorderY { get; set; }
    public static uint defaultBGTileColumn { get; set; }
    public static uint defaultBGItemOrFramePerTile { get; set; }
    public static uint defaultBGTileCount { get; set; }
    public static int defaultBGFrameTime { get; set; }

    public static List<string> spriteList = new List<string>();

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

            IConfig config = new ConfigurationBuilder<IConfig>()
               .UseIniFile("GMLoader.ini")
               .Build();

            #region Config
            //Variables that doesn't have a type can be accessed by CSX scripts.
            ulong currentHash = 0;
            bool autoGameStart = config.AutoGameStart;
            string gameExecutable = config.GameExecutable;
            string supportedHashVersion = config.SupportedDataHash;
            string importPreCSXPath = config.ImportPreCSX;
            string importBuiltInCSXPath = config.ImportBuiltinCSX;
            string importPostCSXPath = config.ImportPostCSX;
            string importAfterCSXPath = config.ImportAfterCSX;
            bool compilePreCSX = config.CompilePreCSX;
            bool compileBuiltInCSX = config.CompileBuiltinCSX;
            bool compilePostCSX = config.CompilePostCSX;
            bool compileAfterCSX = config.CompileAfterCSX;
            compileGML = config.CompileGML;
            compileASM = config.CompileASM;
            string backupDataPath = config.BackupData;
            string gameDataPath = config.GameData;
            modsPath = config.ModsDirectory;
            texturesPath = config.TexturesDirectory;
            shaderPath = config.ShaderDirectory;
            configPath = config.ConfigDirectory;
            gmlCodePath = config.GMLCodeDirectory;
            collisionPath = config.CollisionDirectory;
            asmPath = config.ASMDirectory;
            appendGMLPath = config.AppendGMLDirectory;
            appendGMLCollisionPath = config.AppendGMLCollisionDirectory;
            newObjectPath = config.NewObjectDirectory;
            existingObjectPath = config.ExistingObjectDirectory;

            defaultSpriteX = config.DefaultSpriteX;
            defaultSpriteY = config.DefaultSpriteY;
            defaultSpriteSpeedType = config.DefaultSpriteSpeedType;
            defaultSpriteFrameSpeed = config.DefaultSpriteFrameSpeed;
            defaultSpriteBoundingBoxType = config.DefaultSpriteBoundingBoxType;
            defaultSpriteBoundingBoxLeft = config.DefaultSpriteBoundingBoxLeft;
            defaultSpriteBoundingBoxRight = config.DefaultSpriteBoundingBoxRight;
            defaultSpriteBoundingBoxBottom = config.DefaultSpriteBoundingBoxBottom;
            defaultSpriteBoundingBoxTop = config.DefaultSpriteBoundingBoxTop;
            defaultSpriteSepMasksType = config.DefaultSpriteSepMasksType;
            defaultSpriteTransparent = config.DefaultSpriteTransparent;
            defaultSpriteSmooth = config.DefaultSpriteSmooth;
            defaultSpritePreload = config.DefaultSpritePreload;
            defaultSpriteSpecialVer = config.DefaultSpriteSpecialVer;

            defaultBGTransparent = config.DefaultBGTransparent;
            defaultBGSmooth = config.DefaultBGSmooth;
            defaultBGPreload = config.DefaultBGPreload;
            defaultBGTileWidth = config.DefaultBGTileWidth;
            defaultBGTileHeight = config.DefaultBGTileHeight;
            defaultBGBorderX = config.DefaultBGBorderX;
            defaultBGBorderY = config.DefaultBGBorderY;
            defaultBGTileColumn = config.DefaultBGTileColumn;
            defaultBGItemOrFramePerTile = config.DefaultBGItemOrFramePerTile;
            defaultBGTileCount = config.DefaultBGTileCount;
            defaultBGFrameTime = config.DefaultBGFrameTime;
            #endregion

            mkDir(modsPath);
            mkDir(importPreCSXPath);
            mkDir(importBuiltInCSXPath);
            mkDir(importPostCSXPath);
            mkDir(importAfterCSXPath);

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
            string[] dirAfterCSXFiles = Directory.GetFiles(importAfterCSXPath, "*.csx");

            if (!compilePreCSX && !compileBuiltInCSX && !compilePostCSX)
            {
                Log.Information("Bruh. \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (dirBuiltInCSXFiles.Length == 0 && dirPreCSXFiles.Length == 0 && dirPostCSXFiles.Length == 0 && dirAfterCSXFiles.Length == 0)
            {
                Log.Information($"The CSX Script folder path is empty.\nAborting the process\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (!dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")) && !dirPreCSXFiles.Any(x => x.EndsWith(".csx")) && !dirPostCSXFiles.Any(x => x.EndsWith(".csx")) && !dirAfterCSXFiles.Any(x => x.EndsWith(".csx")))
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
            if (File.Exists(backupDataPath) && supportedHashVersion == currentHash.ToString()) //fewer instruction to convert them to string rather than using integer
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


            if (dirAfterCSXFiles.Length != 0)
            {
                if (compileAfterCSX)
                {
                    Log.Information("Loading CSX Scripts after compilation.");
                    foreach (string file in dirAfterCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    Log.Information("Loading after-CSX script is disabled, skipping the process.");
                }
            }
            else if (compileAfterCSX)
            {
                Log.Debug($"The post-CSX folder is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importAfterCSXPath))} , skipping the process.");
            }
            else if (compileAfterCSX && !dirAfterCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                Log.Debug($"No post-CSX script file found At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importAfterCSXPath))} , skipping the process.");
            }

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
            typeof(ImageMagick.Drawing.DrawableAlpha).Assembly,
            typeof(System.Drawing.Imaging.PixelFormat).Assembly,
            typeof(Newtonsoft.Json.Linq.JObject).Assembly,
            typeof(System.Text.Json.JsonSerializer).Assembly
        };

        CliScriptOptions = ScriptOptions.Default
            .WithReferences(references)
            .AddImports("UndertaleModLib", "UndertaleModLib.Models", "UndertaleModLib.Decompiler",
                "UndertaleModLib.Scripting", "UndertaleModLib.Compiler",
                "UndertaleModLib.Util", "GMLoader", "GMLoader.GMLoaderProgram", "ImageMagick", "Serilog", "System", "System.Linq", 
                "System.IO", "System.Collections.Generic", "System.Drawing", "System.Drawing.Imaging", 
                "System.Collections", "System.Text.RegularExpressions", "System.Text.Json", "System.Diagnostics",
                "System.Threading", "System.Threading.Tasks", "Newtonsoft.Json.Linq")
            // "WithEmitDebugInformation(true)" not only lets us to see a script line number which threw an exception,
            // but also provides other useful debug info when we run UMT in "Debug".
            .WithEmitDebugInformation(true);
    }

    #endregion

}
