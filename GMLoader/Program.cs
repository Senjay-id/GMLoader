﻿#region Using Directives
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
using Newtonsoft.Json;
using xdelta3.net;
using Underanalyzer.Compiler;
using Underanalyzer.Decompiler;
using System.Text;
using VYaml.Serialization;
using VYaml.Annotations;
#endregion

namespace GMLoader;
public interface IConfig
{
    public bool AutoGameStart { get; }
    public bool CheckHash { get; }
    public bool ExportMode { get; }
    public string ExportDataPath { get; }
    public bool ExportCode { get; }
    public bool ExportGameObject { get; }
    public bool ExportTexture { get; }
    public bool ExportAudio { get; }
    public bool ExportRoom { get; }
    public string ExportAudioScriptPath { get; }
    public string ExportAudioOutputPath { get; }
    public string ExportTextureScriptPath { get; }
    public string ExportTextureOutputPath { get; }
    public string ExportGameObjectScriptPath { get; }
    public string ExportGameObjectOutputPath { get; }
    public string ExportCodeScriptPath { get; }
    public string ExportCodeOutputPath { get; }
    public string ExportRoomScriptPath { get; }
    public string ExportRoomOutputPath { get; }
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
    public string TexturesConfigDirectory { get; }
    public string BackgroundsConfigDirectory { get; }
    public string ShaderDirectory { get; }
    public string ConfigDirectory { get; }
    public string GMLCodeDirectory { get; }
    public string CollisionDirectory { get; }
    public string ASMDirectory { get; }
    public string PrependGMLDirectory { get; }
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

[YamlObject]
public partial class SpriteData
{
    [YamlMember("frames")]
    public int? yml_frame { get; set; }  // Nullable int

    [YamlMember("x")]
    public int? yml_x { get; set; }      // Nullable int

    [YamlMember("y")]
    public int? yml_y { get; set; }      // Nullable int

    [YamlMember("transparent")]
    public bool? yml_transparent { get; set; }  // Nullable bool

    [YamlMember("smooth")]
    public bool? yml_smooth { get; set; }      // Nullable bool

    [YamlMember("preload")]
    public bool? yml_preload { get; set; }     // Nullable bool

    [YamlMember("speed_type")]
    public uint? yml_speedtype { get; set; }   // Nullable uint

    [YamlMember("frame_speed")]
    public float? yml_framespeed { get; set; } // Nullable float

    [YamlMember("bounding_box_type")]
    public uint? yml_boundingboxtype { get; set; }  // Nullable uint

    [YamlMember("bbox_left")]
    public int? yml_bboxleft { get; set; }     // Nullable int

    [YamlMember("bbox_right")]
    public int? yml_bboxright { get; set; }    // Nullable int

    [YamlMember("bbox_bottom")]
    public int? yml_bboxbottom { get; set; }   // Nullable int

    [YamlMember("bbox_top")]
    public int? yml_bboxtop { get; set; }     // Nullable int

    [YamlMember("sepmasks")]
    public uint? yml_sepmask { get; set; }     // Nullable uint
}

[YamlObject]
public partial class BackgroundData
{
    [YamlMember("tile_count")]
    public uint? yml_tile_count { get; set; }  // Nullable uint
    [YamlMember("tile_width")]
    public uint? yml_tile_width { get; set; }      // Nullable uint
    [YamlMember("tile_height")]
    public uint? yml_tile_height { get; set; }      // Nullable uint
    [YamlMember("border_x")]
    public uint? yml_border_x { get; set; }      // Nullable uint
    [YamlMember("border_y")]
    public uint? yml_border_y { get; set; }  // Nullable uint
    [YamlMember("tile_column")]
    public uint? yml_tile_column { get; set; }      // Nullable uint
    [YamlMember("item_per_tile")]
    public uint? yml_item_per_tile { get; set; }     // Nullable uint
    [YamlMember("transparent")]
    public bool? yml_transparent { get; set; }     // Nullable bool
    [YamlMember("smooth")]
    public bool? yml_smooth { get; set; }     // Nullable bool
    [YamlMember("preload")]
    public bool? yml_preload { get; set; }     // Nullable bool
    [YamlMember("frametime")]
    public int? yml_frametime { get; set; }     // Nullable int
}

public class GMLoaderProgram
{
    #region Properties
    public static UndertaleData Data { get; set; }
    private static ScriptOptions CliScriptOptions { get; set; }
    public static string gameDataPath { get; set; }
    public static string modsPath { get; set; }
    //public static bool exportCode { get; set; }
    //public static bool exportGameObject { get; set; }
    //public static bool exportTexture { get; set; }
    public static string exportTextureOutputPath { get; set; }
    public static string exportAudioOutputPath { get; set; }
    public static string exportGameObjectOutputPath { get; set; }
    public static string exportCodeOutputPath { get; set; }
    public static string exportRoomOutputPath { get; set; }
    public static List<string> invalidCodeNames { get; set; }
    public static int invalidCode { get; set; }
    public static List<string> invalidSpriteNames { get; set; }
    public static int invalidSprite { get; set; }
    public static List<string> invalidSpriteSizeNames { get; set; }
    public static int invalidXdelta { get; set; }
    public static List<string> invalidXdeltaNames { get; set; }
    public static int invalidSpriteSize { get; set; }
    public static string texturesPath { get; set; }
    public static string texturesConfigPath { get; set; }
    public static string backgroundsConfigPath { get; set; }
    public static string shaderPath { get; set; }
    public static string configPath { get; set; }
    public static string gmlCodePath { get; set; }
    public static string collisionPath { get; set; }
    public static string asmPath { get; set; }
    public static string prependGMLPath { get; set; }
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
    public static List<string> backgroundList = new List<string>();
    public static Dictionary<string, SpriteData> spriteDictionary = new Dictionary<string, SpriteData>();
    public static Dictionary<string, BackgroundData> backgroundDictionary = new Dictionary<string, BackgroundData>();
    public static string[] spritesToImport;
    public static string[] backgroundsToImport;

    #endregion

    static async Task Main()
    {
        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

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
            bool checkHash = config.CheckHash;
            bool exportMode = config.ExportMode;
            bool exportCode = config.ExportCode;
            bool exportGameObject = config.ExportGameObject;
            bool exportTexture = config.ExportTexture;
            bool exportAudio = config.ExportAudio;
            bool exportRoom = config.ExportRoom;
            string exportDataPath = config.ExportDataPath;
            string exportTextureScriptPath = config.ExportTextureScriptPath;
            string exportAudioScriptPath = config.ExportAudioScriptPath;
            string exportGameObjectScriptPath = config.ExportGameObjectScriptPath;
            string exportCodeScriptPath = config.ExportCodeScriptPath;
            string exportRoomScriptPath = config.ExportRoomScriptPath;
            exportGameObjectOutputPath = config.ExportGameObjectOutputPath;
            exportTextureOutputPath = config.ExportTextureOutputPath;
            exportAudioOutputPath = config.ExportAudioOutputPath;
            exportCodeOutputPath = config.ExportCodeOutputPath;
            exportRoomOutputPath = config.ExportRoomOutputPath;
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
            gameDataPath = config.GameData;
            modsPath = config.ModsDirectory;
            texturesPath = config.TexturesDirectory;
            texturesConfigPath = config.TexturesConfigDirectory;
            backgroundsConfigPath = config.BackgroundsConfigDirectory;
            shaderPath = config.ShaderDirectory;
            configPath = config.ConfigDirectory;
            gmlCodePath = config.GMLCodeDirectory;
            collisionPath = config.CollisionDirectory;
            asmPath = config.ASMDirectory;
            prependGMLPath = config.PrependGMLDirectory;
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
            mkDir(texturesPath);
            mkDir(texturesConfigPath);
            mkDir(backgroundsConfigPath);
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

            if (exportMode)
            {
                Console.Title = $"GMLoader  - Export Mode";

                if (!File.Exists(exportTextureScriptPath) || !File.Exists(exportGameObjectScriptPath) || 
                    !File.Exists(exportCodeScriptPath) || !File.Exists(exportAudioScriptPath) || 
                    !File.Exists(exportRoomScriptPath))
                {
                    Log.Error("Missing exporter script");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                while (Directory.Exists(exportGameObjectOutputPath) || Directory.Exists(exportTextureOutputPath) || 
                    Directory.Exists(exportCodeOutputPath) || Directory.Exists(exportAudioOutputPath) || 
                    Directory.Exists(exportRoomOutputPath))
                {
                    Log.Information("Please delete the Export folder before exporting.\n\nPress any key to continue..");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                
                Log.Information($"Reading game data from {Path.GetFileName(exportDataPath)}");
                Data = new UndertaleData();
                using (var stream = new FileStream(exportDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);

                Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content} - Export Mode";
                Log.Information($"Loaded game {Data.GeneralInfo.Name.Content}");

                ScriptOptionsInitialize();

                invalidCodeNames = new List<string>();
                invalidCode = 0;

                invalidSpriteNames = new List<string>();
                invalidSprite = 0;

                invalidSpriteSizeNames = new List<string>();
                invalidSpriteSize = 0;

                if (exportGameObject)
                    await RunCSharpFile(exportGameObjectScriptPath);
                if (exportAudio)
                    await RunCSharpFile(exportAudioScriptPath);
                if (exportCode)
                    await RunCSharpFile(exportCodeScriptPath);
                if (exportTexture)
                    await RunCSharpFile(exportTextureScriptPath);
                if (exportRoom)
                    await RunCSharpFile(exportRoomScriptPath);

                if (!exportGameObject && !exportCode && !exportTexture && !exportAudio && !exportRoom)
                {
                    Log.Information("No export option enabled??");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                if (invalidCode > 0)
                {
                    Log.Information("");
                    Log.Error("Error, Failed to decompile the code below:");
                    foreach (string name in invalidCodeNames)
                    {
                        Log.Error(name);
                    }
                    Log.Information("");
                }
                if (invalidSpriteSize > 0)
                {
                    Log.Information("");
                    Log.Error("Error, the sprite below has invalid height or width:");
                    foreach (var name in invalidSpriteSizeNames)
                    {
                        Log.Error(name);
                    }
                    Log.Information("");
                }

                Log.Information("Export done. Press any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            if (File.Exists(backupDataPath))
            {
                currentHash = ComputeFileHash3(backupDataPath);
            }

            Data = new UndertaleData();
            if (File.Exists(backupDataPath) && supportedHashVersion == currentHash.ToString()) //fewer instruction to convert them to string rather than using integer
            {
                Log.Information($"Reading game data from {Path.GetFileName(backupDataPath)}");
                using (var stream = new FileStream(backupDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);
            }
            else if (File.Exists(backupDataPath) && supportedHashVersion != currentHash.ToString() && checkHash)
            {
                Log.Information("\nGame Data Hash Mismatch Error.\nThis happens because modloader is outdated or the data.win is modified.\n\nDelete backup.win and verify the integrity of game.\n\nIf your using MO2, check the overwrite folder and delete backup.win\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                Log.Information($"Reading game data from {Path.GetFileName(gameDataPath)}");
                using (var stream = new FileStream(gameDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);
                File.Copy(gameDataPath, backupDataPath);
                Log.Information($"Backup of the data has been created at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, backupDataPath))}");
            }

            Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content}";
            Log.Information($"Loaded game {Data.GeneralInfo.Name.Content}");

            //CSX Handling
            ScriptOptionsInitialize();
            invalidXdeltaNames = new List<string>();
            invalidXdelta = 0;
            //Compile users script before builtin scripts
            if (dirPreCSXFiles.Length != 0)
            {
                if (compilePreCSX)
                {
                    Log.Information("Loading pre-CSX Scripts.");
                    foreach (string file in dirPreCSXFiles)
                    {
                        await RunCSharpFile(file);
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
                    // Had to be done on GMLoader's side because of VYaml issues
                    await importGraphic();
                    foreach (string file in dirBuiltInCSXFiles)
                    {
                        await RunCSharpFile(file);
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
                        await RunCSharpFile(file);
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
                        await RunCSharpFile(file);
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

            if (invalidXdelta > 0)
            {
                Log.Information("");
                Log.Error("Error, Failed to xdelta patch the files below:");
                foreach (string name in invalidXdeltaNames)
                {
                    Log.Error(name);
                }
                Log.Information("");
            }

            stopwatch.Stop();

            if (autoGameStart)
            {
                Log.Information($"Game Data has been recompiled, Launching the game...\n\nElapsed time: {stopwatch.Elapsed.TotalSeconds:F2} seconds ({stopwatch.ElapsedMilliseconds} ms)");
                Process.Start(gameExecutable);
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            else
            {
                Log.Information($"Game Data has been recompiled, you can now launch the modded data through the game executable.\n\nElapsed time: {stopwatch.Elapsed.TotalSeconds:F2} seconds ({stopwatch.ElapsedMilliseconds} ms)\n\n\nPress any key to close...");
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

    private static async Task importGraphic()
    {
        spriteDictionary.Clear();
        spriteList.Clear();
        backgroundDictionary.Clear();
        backgroundList.Clear();
        string pngExt = ".png";

        string[] spriteConfigFIles = Directory.GetFiles(texturesConfigPath, "*.yaml*", SearchOption.TopDirectoryOnly);
        string[] backgroundConfigFIles = Directory.GetFiles(backgroundsConfigPath, "*.yaml*", SearchOption.TopDirectoryOnly);
        if (spriteConfigFIles.Length == 0 && backgroundConfigFIles.Length == 0)
        {
            Log.Debug($"The sprite and background configuration files are empty, at {texturesConfigPath}, skipping texture import");
            return;
        }

        Log.Information("Executing built-in ImportGraphic");

        if (spriteConfigFIles.Length != 0)
        {
            Dictionary<string, SpriteData> spriteParameters = new();
            List<string> spriteFilenames = new List<string>();

            Log.Information("Deserializing sprite configuration files, please close GMLoader if it takes more than 5 second for a config file.");
            Console.Title = $"GMLoader - Deserializing sprite configuration files, please close GMLoader if it takes more than 5 second for a config file";
            foreach (string file in spriteConfigFIles)
            {
                byte[] yamlBytes = File.ReadAllBytes(file);
                Log.Information($"Deserializing {Path.GetFileName(file)}");
                var deserialized = YamlSerializer.Deserialize<Dictionary<string, SpriteData>>(yamlBytes);
                foreach (var (spritename, configs) in deserialized)
                {
                    spriteFilenames.Add(spritename + pngExt);
                    spriteList.Add(spritename);

                    spriteDictionary[spritename] = new SpriteData
                    {
                        yml_frame = configs.yml_frame ?? 1,
                        yml_x = configs.yml_x ?? defaultSpriteX,
                        yml_y = configs.yml_y ?? defaultSpriteY,
                        yml_transparent = configs.yml_transparent ?? defaultSpriteTransparent,
                        yml_smooth = configs.yml_smooth ?? defaultSpriteSmooth,
                        yml_preload = configs.yml_preload ?? defaultSpritePreload,
                        yml_speedtype = configs.yml_speedtype ?? defaultSpriteSpeedType,
                        yml_framespeed = configs.yml_framespeed ?? defaultSpriteFrameSpeed,
                        yml_boundingboxtype = configs.yml_boundingboxtype ?? defaultSpriteBoundingBoxType,
                        yml_bboxleft = configs.yml_bboxleft ?? defaultSpriteBoundingBoxLeft,
                        yml_bboxright = configs.yml_bboxright ?? defaultSpriteBoundingBoxRight,
                        yml_bboxbottom = configs.yml_bboxbottom ?? defaultSpriteBoundingBoxBottom,
                        yml_bboxtop = configs.yml_bboxtop ?? defaultSpriteBoundingBoxTop,
                        yml_sepmask = configs.yml_sepmask ?? defaultSpriteSepMasksType
                    };
                }
            }
            spritesToImport = spriteFilenames.ToArray();
        }
        else
        {
            Log.Debug("The sprite configuration files are empty, skipping...");
        }

        if (backgroundConfigFIles.Length != 0)
        {
            Dictionary<string, BackgroundData> backgroundParameters = new();

            List<string> backgroundFilenames = new List<string>();

            Log.Information("Deserializing backgrounds configuration files, please close GMLoader if it takes more than 5 second for a config file.");
            Console.Title = $"GMLoader - Deserializing backgrounds configuration files, please close GMLoader if it takes more than 5 second for a config file";
            foreach (string file in backgroundConfigFIles)
            {
                byte[] yamlBytes = File.ReadAllBytes(file);
                Log.Information($"Deserializing {Path.GetFileName(file)}");
                var deserialized = YamlSerializer.Deserialize<Dictionary<string, BackgroundData>>(yamlBytes);
                foreach (var (backgroundname, configs) in deserialized)
                {
                    backgroundFilenames.Add(backgroundname + pngExt);
                    backgroundList.Add(backgroundname);

                    backgroundDictionary[backgroundname] = new BackgroundData
                    {
                        yml_tile_count = configs.yml_tile_count ?? defaultBGTileCount,
                        yml_tile_width = configs.yml_tile_width ?? defaultBGTileWidth,
                        yml_tile_height = configs.yml_tile_height ?? defaultBGTileHeight,
                        yml_border_x = configs.yml_border_x ?? defaultBGBorderX,
                        yml_border_y = configs.yml_border_y ?? defaultBGBorderY,
                        yml_tile_column = configs.yml_tile_column ?? defaultBGTileColumn,
                        yml_item_per_tile = configs.yml_item_per_tile ?? defaultBGItemOrFramePerTile,
                        yml_transparent = configs.yml_transparent ?? defaultBGTransparent,
                        yml_smooth = configs.yml_smooth ?? defaultBGSmooth,
                        yml_preload = configs.yml_preload ?? defaultBGPreload,
                    };
                }
            }
            backgroundsToImport = backgroundFilenames.ToArray();
        }
        else
        {
            Log.Debug("The background sprite configuration files are empty, skipping...");
        }
        Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content}";
    }

    #region Helper Methods

    public static void mkDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static ulong ComputeFileHash64(string filePath)
    {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
            ulong hash = xxHash64.ComputeHash(fileStream);
            return hash;
        }
    }
    public static ulong ComputeFileHash3(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, (int)stream.Length);
            return xxHash3.ComputeHash(fileBytes, (int)stream.Length, 0);
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
    public static void makeDeltaPatch(string sourceFilePath, string modifiedFilePath, string patchFilePath)
    {
        byte[] originalData = File.ReadAllBytes(sourceFilePath);
        byte[] modifiedData = File.ReadAllBytes(modifiedFilePath);

        var delta = Xdelta3Lib.Encode(source: originalData, target: modifiedData).ToArray();

        File.WriteAllBytes(patchFilePath, delta);
    }

    public static void applyDeltaPatch(string sourceFilePath, string patchFilePath, string outputFilePath)
    {
        byte[] originalData = File.ReadAllBytes(sourceFilePath);
        byte[] delta = File.ReadAllBytes(patchFilePath);

        var recreatedData = Xdelta3Lib.Decode(source: originalData, delta: delta).ToArray();
        File.WriteAllBytes(outputFilePath, recreatedData);
    }
    #endregion

    #region Script Handling

    private static async Task RunCSharpFile(string path)
    {
        string lines;
        try
        {
            lines = File.ReadAllText(path);
        }
        catch (Exception exc)
        {
            // rethrow as otherwise this will get interpreted as success
            Log.Error(exc.Message);
            throw;
        }

        lines = $"#line 1 \"{path}\"\n" + lines;
        var ScriptPath = path;
        await RunCSharpCode(lines, ScriptPath);
    }

    private static async Task RunCSharpCode(string code, string scriptFile = null)
    {
        Log.Information($"Attempting to execute '{Path.GetFileName(scriptFile)}'");
        var ScriptErrorMessage = "";
        var ScriptExecutionSuccess = false;
        try
        {
            await CSharpScript.EvaluateAsync(code, CliScriptOptions);
            ScriptExecutionSuccess = true;
            ScriptErrorMessage = "";
        }
        catch (Exception exc)
        {
            ScriptExecutionSuccess = false;
            ScriptErrorMessage = exc.ToString();
            //ScriptErrorType = "Exception";
            Log.Error(exc.ToString());
        }

        //if (!FinishedMessageEnabled) return;

        if (ScriptExecutionSuccess)
        {
            Log.Information($"Finished executing '{Path.GetFileName(scriptFile)}'");
        }
        else
        {
            Log.Error(ScriptErrorMessage);
        }
    }

    private static void ScriptOptionsInitialize()
    {
        var references = new[]
        {
            typeof(UndertaleObject).GetTypeInfo().Assembly,
            typeof(GMLoader.GMLoaderProgram).GetTypeInfo().Assembly,
            typeof(ImageMagick.Drawing.DrawableAlpha).GetTypeInfo().Assembly,
            typeof(System.Drawing.Imaging.PixelFormat).GetTypeInfo().Assembly,
            typeof(Newtonsoft.Json.Linq.JObject).GetTypeInfo().Assembly,
            typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly,
            typeof(System.Text.Json.JsonSerializer).GetTypeInfo().Assembly,
            typeof(VYaml.Serialization.YamlSerializer).GetTypeInfo().Assembly
        };

        CliScriptOptions = ScriptOptions.Default
            .WithReferences(references)
            .AddImports(
                "UndertaleModLib", "UndertaleModLib.Models", "UndertaleModLib.Decompiler",
                "UndertaleModLib.Scripting", "UndertaleModLib.Compiler",
                "UndertaleModLib.Util", "GMLoader", "GMLoader.GMLoaderProgram", 
                "ImageMagick", "Serilog", "xdelta3.net", "System", "System.Text", "System.Linq", 
                "System.IO", "System.Collections.Generic", "System.Drawing", 
                "System.Drawing.Imaging", "System.Collections", 
                "System.Text.RegularExpressions", "System.Text.Json", "System.Diagnostics",
                "System.Threading", "System.Threading.Tasks", "Newtonsoft.Json", 
                "Newtonsoft.Json.Linq", "VYaml", "VYaml.Serialization", "VYaml.Annotations")
            // "WithEmitDebugInformation(true)" not only lets us to see a script line number which threw an exception,
            // but also provides other useful debug info when we run UMT in "Debug".
            .WithEmitDebugInformation(true);
    }

    #endregion

}
