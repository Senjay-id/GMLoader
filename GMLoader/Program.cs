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
using Newtonsoft.Json;
using xdelta3.net;
using Underanalyzer.Compiler;
using Underanalyzer.Decompiler;
using System.Text;
using VYaml.Serialization;
using VYaml.Annotations;
using UndertaleModLib.Compiler;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.IO;
#endregion

namespace GMLoader;
public interface IConfig
{
    public bool AutoGameStart { get; }
    public bool CheckHash { get; }
    public bool ConvertMode { get; }
    public bool ReuseVanillaExport { get; }
    public string ConvertOutputPath { get; }
    public string ExportDataPath { get; }
    public string ExportOutputPath { get; }
    public bool ExportCode { get; }
    public bool ExportGameObject { get; }
    public bool ExportTexture { get; }
    public bool ExportAudio { get; }
    public bool ExportRoom { get; }
    public string ConvertVanillaData { get; }
    public string ConvertModdedData { get; }
    public string ExportAudioScriptPath { get; }
    public string ExportAudioOutputPath { get; }
    public string ExportTextureScriptPath { get; }
    public string ExportTextureOutputPath { get; }
    public string ExportTextureBackgroundOutputPath { get; }
    public string ExportTextureNoStripOutputPath { get; }
    public string ExportTextureConfigOutputPath { get; }
    public string TextureExclusion { get; }
    public string ExportBackgroundTextureConfigOutputPath { get; }
    public string ExportGameObjectScriptPath { get; }
    public string ExportGameObjectOutputPath { get; }
    public string ExportCodeScriptPath { get; }
    public string ExportCodeOutputPath { get; }
    public string ExportRoomScriptPath { get; }
    public string ExportRoomOutputPath { get; }
    public string GameExecutable { get; }
    public ulong SupportedDataHash { get; }
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
    public string BackgroundTextureDirectory { get; }
    public string NoStripTexturesDirectory { get; }
    public string TexturesConfigDirectory { get; }
    public string BackgroundsConfigDirectory { get; }
    public string AudioDirectory { get; }
    public string AudioConfigDirectory { get; }
    public string ShaderDirectory { get; }
    public string ConfigDirectory { get; }
    public string GMLCodeDirectory { get; }
    public string GMLCodePatchDirectory { get; }
    public string CollisionDirectory { get; }
    public string ASMDirectory { get; }
    public string PrependGMLDirectory { get; }
    public string AppendGMLDirectory { get; }
    public string AppendGMLCollisionDirectory { get; }
    public string NewObjectDirectory { get; }
    public string ExistingObjectDirectory { get; }
    public string RoomDirectory { get; }
    public string DeltaruneBaseDirectory { get; }
    public string DeltaruneCH1Directory { get; }
    public string DeltaruneCH2Directory { get; }
    public string DeltaruneCH3Directory { get; }
    public string DeltaruneCH4Directory { get; }
    public string DeltaruneCH1DataPath { get; }
    public string DeltaruneCH2DataPath { get; }
    public string DeltaruneCH3DataPath { get; }
    public string DeltaruneCH4DataPath { get; }
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
    public string DefaultAudioType { get; }
    public bool DefaultAudioEmbedded { get; }
    public bool DefaultAudioCompressed { get; }
    public uint DefaultAudioEffects { get; }
    public float DefaultAudioVolume { get; }
    public float DefaultAudioPitch { get; }
    public int DefaultAudioGroupIndex { get; }
    public int DefaultAudioFileID { get; }
    public bool DefaultAudioPreload { get; }
}

[YamlObject]
public partial class CodeData
{

    [YamlMember("type")]
    public string? yml_type { get; set; }

    [YamlMember("find")]
    public string? yml_find { get; set; }

    [YamlMember("code")]
    public string? yml_code { get; set; }

    [YamlMember("case_sensitive")]
    public bool? yml_casesensitive { get; set; }

}

[YamlObject]
public partial class SpriteData
{

    [YamlMember("frames")]
    public int? yml_frame { get; set; }  

    [YamlMember("x")]
    public int? yml_x { get; set; }      

    [YamlMember("y")]
    public int? yml_y { get; set; }      

    [YamlMember("transparent")]
    public bool? yml_transparent { get; set; }  

    [YamlMember("smooth")]
    public bool? yml_smooth { get; set; }      

    [YamlMember("preload")]
    public bool? yml_preload { get; set; }     

    [YamlMember("speed_type")]
    public uint? yml_speedtype { get; set; }   

    [YamlMember("frame_speed")]
    public float? yml_framespeed { get; set; }

    [YamlMember("bounding_box_type")]
    public uint? yml_boundingboxtype { get; set; }  

    [YamlMember("bbox_left")]
    public int? yml_bboxleft { get; set; }     

    [YamlMember("bbox_right")]
    public int? yml_bboxright { get; set; }    

    [YamlMember("bbox_bottom")]
    public int? yml_bboxbottom { get; set; }   

    [YamlMember("bbox_top")]
    public int? yml_bboxtop { get; set; }     

    [YamlMember("sepmasks")]
    public uint? yml_sepmask { get; set; }     
}

[YamlObject]
public partial class BackgroundData
{
    [YamlMember("tile_count")]
    public uint? yml_tile_count { get; set; }  
    [YamlMember("tile_width")]
    public uint? yml_tile_width { get; set; }      
    [YamlMember("tile_height")]
    public uint? yml_tile_height { get; set; }      
    [YamlMember("border_x")]
    public uint? yml_border_x { get; set; }      
    [YamlMember("border_y")]
    public uint? yml_border_y { get; set; }  
    [YamlMember("tile_column")]
    public uint? yml_tile_column { get; set; }      
    [YamlMember("item_per_tile")]
    public uint? yml_item_per_tile { get; set; }     
    [YamlMember("transparent")]
    public bool? yml_transparent { get; set; }     
    [YamlMember("smooth")]
    public bool? yml_smooth { get; set; }     
    [YamlMember("preload")]
    public bool? yml_preload { get; set; }     
    [YamlMember("frametime")]
    public long? yml_frametime { get; set; }     
}

[YamlObject]
public partial class AudioData
{
    [YamlMember("type")]
    public string? yml_type { get; set; }
    [YamlMember("embedded")]
    public bool? yml_embedded { get; set; }
    [YamlMember("compressed")]
    public bool? yml_compressed { get; set; }
    [YamlMember("effects")]
    public uint? yml_effects { get; set; }
    [YamlMember("volume")]
    public float? yml_volume { get; set; }
    [YamlMember("pitch")]
    public float? yml_pitch { get; set; }
    [YamlMember("audiogroup_index")]
    public int? yml_audiogroup_index { get; set; }
    [YamlMember("audiofile_id")]
    public int? yml_audiofile_id { get; set; }
    [YamlMember("preload")]
    public bool? yml_preload { get; set; }
}

public class GMLoaderProgram
{
    #region Properties
    public static UndertaleData Data { get; set; }
    private static ScriptOptions CliScriptOptions { get; set; }
    public static string gameDataPath { get; set; }
    public static string gameName { get; set; }
    public static string modsPath { get; set; }
    public static bool exportTexture { get; set; }
    public static bool exportGameObject { get; set; }
    public static bool exportCode { get; set; }
    public static bool exportAudio { get; set; }
    public static bool exportRoom { get; set; }
    public static string exportDataPath { get; set; }
    public static bool reuseVanillaExport { get; set; }
    public static string convertVanillaData { get; set; }
    public static string convertModdedData { get; set; }
    public static string convertOutputPath { get; set; }
    public static string exportTextureScriptPath { get; set; }
    public static string exportAudioScriptPath { get; set; }
    public static string exportGameObjectScriptPath { get; set; }
    public static string exportCodeScriptPath { get; set; }
    public static string exportRoomScriptPath { get; set; }
    public static string exportOutputPath { get; set; }
    public static string exportTextureOutputPath { get; set; }
    public static string exportTextureBackgroundOutputPath { get; set; }
    public static string exportTextureNoStripOutputPath { get; set; }
    public static string exportTextureConfigOutputPath { get; set; }
    public static string textureExclusion { get; set; }
    public static List<string> textureExclusionList { get; set; }
    public static string exportBackgroundTextureConfigOutputPath { get; set; }
    public static string exportAudioOutputPath { get; set; }
    public static string exportGameObjectOutputPath { get; set; }
    public static string exportCodeOutputPath { get; set; }
    public static string exportRoomOutputPath { get; set; }
    public static bool compilePreCSX { get; set; }
    public static bool compileBuiltInCSX { get; set; }
    public static bool compilePostCSX { get; set; }
    public static bool compileAfterCSX { get; set; }
    public static string deltaruneBasePath { get; set; }
    public static string deltaruneCH1Path { get; set; }
    public static string deltaruneCH2Path { get; set; }
    public static string deltaruneCH3Path { get; set; }
    public static string deltaruneCH4Path { get; set; }
    public static string deltaruneCH1DataPath { get; set; }
    public static string deltaruneCH2DataPath { get; set; }
    public static string deltaruneCH3DataPath { get; set; }
    public static string deltaruneCH4DataPath { get; set; }
    public static bool processDeltaruneCH1Mods { get; set; }
    public static bool processDeltaruneCH2Mods { get; set; }
    public static bool processDeltaruneCH3Mods { get; set; }
    public static bool processDeltaruneCH4Mods { get; set; }
    public static string importPreCSXPath { get; set; }
    public static string importBuiltInCSXPath { get; set; }
    public static string importPostCSXPath { get; set; }
    public static string importAfterCSXPath { get; set; }
    public static List<string> invalidCodeNames { get; set; }
    public static int invalidCode { get; set; }
    public static List<string> invalidSpriteNames { get; set; }
    public static int invalidSprite { get; set; }
    public static List<string> invalidSpriteSizeNames { get; set; }
    public static int invalidXdelta { get; set; }
    public static List<string> invalidXdeltaNames { get; set; }
    public static int invalidSpriteSize { get; set; }
    public static string texturesPath { get; set; }
    public static string backgroundsTexturePath { get; set; }
    public static string texturesConfigPath { get; set; }
    public static string noStripTexturesPath { get; set; }
    public static string backgroundsConfigPath { get; set; }
    public static string audioPath { get; set; }
    public static string audioConfigPath { get; set; }
    public static string shaderPath { get; set; }
    public static string configPath { get; set; }
    public static DecompileSettings defaultDecompSettings { get; set; }
    public static string gmlCodePath { get; set; }
    public static string gmlCodePatchPath { get; set; }
    public static string collisionPath { get; set; }
    public static string asmPath { get; set; }
    public static string prependGMLPath { get; set; }
    public static string appendGMLPath { get; set; }
    public static string appendGMLCollisionPath { get; set; }
    public static string newObjectPath { get; set; }
    public static string existingObjectPath { get; set; }
    public static string roomPath { get; set; }
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
    public static string defaultAudioType { get; set; }
    public static bool defaultAudioEmbedded { get; set; }
    public static bool defaultAudioCompressed { get; set; }
    public static uint defaultAudioEffects { get; set; }
    public static float defaultAudioVolume { get; set; }
    public static float defaultAudioPitch { get; set; }
    public static int defaultAudioGroupIndex { get; set; }
    public static int defaultAudioFileID { get; set; }
    public static bool defaultAudioPreload { get; set; }

    public static Dictionary<string, int> moddedTextureCounts = new Dictionary<string, int>();
    public static List<string> vanillaSpriteList = new List<string>();
    public static List<string> spriteList = new List<string>();
    public static List<string> backgroundList = new List<string>();
    public static Dictionary<string, SpriteData> spriteDictionary = new Dictionary<string, SpriteData>();
    public static Dictionary<string, BackgroundData> backgroundDictionary = new Dictionary<string, BackgroundData>();
    public static string[] spritesToImport;
    public static string[] noStripStyleSpritesToImport;
    public static string[] backgroundsToImport;

    public static List<string> audioList = new List<string>();
    public static string[] audioToImport;
    public static Dictionary<string, AudioData> audioDictionary = new Dictionary<string, AudioData>();

    #endregion

    static void Main(string[] args)
    {
        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string baseDir = AppContext.BaseDirectory;
            string logFile = Path.Combine(baseDir, "GMLoader.log");
            string configFile = Path.Combine(baseDir, "GMLoader.ini");

            if (File.Exists(logFile))
                File.Delete(logFile);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(logFile)
                .CreateLogger();

            Console.Title = "GMLoader";

            if (!File.Exists(configFile))
            {
                Log.Information($"Missing GMLoader.ini while trying to find it on {logFile}\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            IConfig config = new ConfigurationBuilder<IConfig>()
               .UseIniFile(configFile)
               .Build();

            #region Config
            //Variables that doesn't have a type can be accessed by CSX scripts.
            ulong currentHash = 0;
            bool autoGameStart = config.AutoGameStart;
            bool checkHash = config.CheckHash;
            bool convertMode = config.ConvertMode;
            reuseVanillaExport = config.ReuseVanillaExport;
            convertOutputPath = config.ConvertOutputPath;
            exportCode = config.ExportCode;
            exportGameObject = config.ExportGameObject;
            exportTexture = config.ExportTexture;
            exportAudio = config.ExportAudio;
            exportRoom = config.ExportRoom;
            convertVanillaData = config.ConvertVanillaData;
            convertModdedData = config.ConvertModdedData;
            exportDataPath = config.ExportDataPath;
            exportTextureScriptPath = config.ExportTextureScriptPath;
            exportAudioScriptPath = config.ExportAudioScriptPath;
            exportGameObjectScriptPath = config.ExportGameObjectScriptPath;
            exportCodeScriptPath = config.ExportCodeScriptPath;
            exportRoomScriptPath = config.ExportRoomScriptPath;
            exportGameObjectOutputPath = config.ExportGameObjectOutputPath;
            exportOutputPath = config.ExportOutputPath;
            exportTextureOutputPath = config.ExportTextureOutputPath;
            exportTextureBackgroundOutputPath = config.ExportTextureBackgroundOutputPath;
            exportTextureNoStripOutputPath = config.ExportTextureNoStripOutputPath;
            exportTextureConfigOutputPath = config.ExportTextureConfigOutputPath;
            textureExclusion = config.TextureExclusion;
            exportBackgroundTextureConfigOutputPath = config.ExportBackgroundTextureConfigOutputPath;
            exportAudioOutputPath = config.ExportAudioOutputPath;
            exportCodeOutputPath = config.ExportCodeOutputPath;
            exportRoomOutputPath = config.ExportRoomOutputPath;
            deltaruneBasePath = config.DeltaruneBaseDirectory;
            deltaruneCH1Path = config.DeltaruneCH1Directory;
            deltaruneCH2Path = config.DeltaruneCH2Directory;
            deltaruneCH3Path = config.DeltaruneCH3Directory;
            deltaruneCH4Path = config.DeltaruneCH4Directory;
            string gameExecutable = config.GameExecutable;
            ulong supportedDataHash = config.SupportedDataHash;
            importPreCSXPath = config.ImportPreCSX;
            importBuiltInCSXPath = config.ImportBuiltinCSX;
            importPostCSXPath = config.ImportPostCSX;
            importAfterCSXPath = config.ImportAfterCSX;
            compilePreCSX = config.CompilePreCSX;
            compileBuiltInCSX = config.CompileBuiltinCSX;
            compilePostCSX = config.CompilePostCSX;
            compileAfterCSX = config.CompileAfterCSX;
            compileGML = config.CompileGML;
            compileASM = config.CompileASM;
            string backupDataPath = config.BackupData;
            gameDataPath = config.GameData;
            modsPath = config.ModsDirectory;
            texturesPath = config.TexturesDirectory;
            backgroundsTexturePath = config.BackgroundTextureDirectory;
            noStripTexturesPath = config.NoStripTexturesDirectory;
            texturesConfigPath = config.TexturesConfigDirectory;
            backgroundsConfigPath = config.BackgroundsConfigDirectory;
            audioPath = config.AudioDirectory;
            audioConfigPath = config.AudioConfigDirectory;
            shaderPath = config.ShaderDirectory;
            configPath = config.ConfigDirectory;
            gmlCodePath = config.GMLCodeDirectory;
            gmlCodePatchPath = config.GMLCodePatchDirectory;
            collisionPath = config.CollisionDirectory;
            asmPath = config.ASMDirectory;
            prependGMLPath = config.PrependGMLDirectory;
            appendGMLPath = config.AppendGMLDirectory;
            appendGMLCollisionPath = config.AppendGMLCollisionDirectory;
            newObjectPath = config.NewObjectDirectory;
            existingObjectPath = config.ExistingObjectDirectory;
            roomPath = config.RoomDirectory;

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

            defaultAudioType = config.DefaultAudioType;
            defaultAudioEmbedded = config.DefaultAudioEmbedded;
            defaultAudioCompressed = config.DefaultAudioCompressed;
            defaultAudioEffects = config.DefaultAudioEffects;
            defaultAudioVolume = config.DefaultAudioVolume;
            defaultAudioPitch = config.DefaultAudioPitch;
            defaultAudioGroupIndex = config.DefaultAudioGroupIndex;
            defaultAudioFileID = config.DefaultAudioFileID;
            defaultAudioPreload = config.DefaultAudioPreload;
            #endregion

            textureExclusionList = textureExclusion.Split(',').ToList();

            mkDir(modsPath);
            mkDir(texturesPath);
            mkDir(texturesConfigPath);
            mkDir(audioPath);
            mkDir(audioConfigPath);
            mkDir(gmlCodePath);
            mkDir(gmlCodePatchPath);
            mkDir(noStripTexturesPath);
            mkDir(backgroundsConfigPath);
            mkDir(roomPath);
            
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

            string[] dirPreCSXFiles = Directory.GetFiles(importPreCSXPath, "*.csx")
                                              .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                              .ToArray();

            string[] dirBuiltInCSXFiles = Directory.GetFiles(importBuiltInCSXPath, "*.csx")
                                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                                 .ToArray();

            string[] dirPostCSXFiles = Directory.GetFiles(importPostCSXPath, "*.csx")
                                              .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                              .ToArray();

            string[] dirAfterCSXFiles = Directory.GetFiles(importAfterCSXPath, "*.csx")
                                               .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                               .ToArray();

            if (!compilePreCSX && !compileBuiltInCSX && !compilePostCSX)
            {
                Log.Information("What are you trying to do? \n\n\nPress any key to close...");
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

            if (args.Contains("-convert") || convertMode)
            {
                ScriptOptionsInitialize();

                ConvertBothData();

                Environment.Exit(0);
            }

            if (!File.Exists(backupDataPath))
            {
                if (File.Exists(gameDataPath))
                {
                    ulong dataHash = ComputeFileHash3(gameDataPath);

                    if (dataHash != supportedDataHash && checkHash)
                    {
                        Console.WriteLine($"\nError, game data hash is not equal to the supportedDataHash, if your game is modded previously just reinstall or verify the game.\nOtherwise the modloader hash data is outdated and you need to wait for the update.\n\nIf your using MO2, check the overwrite folder and delete {gameDataPath} if it exists");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else if (checkHash)
                    {
                        File.Copy(gameDataPath, backupDataPath);
                        Log.Information($"Backup of the data has been created at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, backupDataPath))}");
                        currentHash = ComputeFileHash3(backupDataPath);
                    }
                    else
                    {
                        currentHash = ComputeFileHash3(gameDataPath);
                    }
                }
                else
                {
                    Log.Error($"Error, Missing {gameDataPath}");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            else
            {
                currentHash = ComputeFileHash3(backupDataPath);
            }

            Data = new UndertaleData();

            if (File.Exists(backupDataPath) && supportedDataHash == currentHash)
            {
                Log.Information($"Reading game data from {Path.GetFileName(backupDataPath)}");
                using (var stream = new FileStream(backupDataPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    Data = UndertaleIO.Read(stream);
                }
                    
            }
            else if (File.Exists(backupDataPath) && supportedDataHash != currentHash && checkHash)
            {
                Log.Information($"\nError, Game Data Hash Mismatch.\nThis happens because modloader is outdated or the {gameDataPath} is modified.\n\nDelete {backupDataPath} and reinstall or verify the integrity of game.\n\nIf your using MO2, check the overwrite folder and delete {backupDataPath}\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                // This should only happen if checkHash is false and backup.win doesn't exists
                Log.Warning($"Warning, checkHash is false, make sure that you know what your doing. Reading game data from {gameDataPath}");
                using (var stream = new FileStream(gameDataPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    Data = UndertaleIO.Read(stream);
                }
            }

            // exposed variable, can be used in csx script
            defaultDecompSettings = new Underanalyzer.Decompiler.DecompileSettings()
            {
                RemoveSingleLineBlockBraces = true,
                EmptyLineAroundBranchStatements = true,
                EmptyLineBeforeSwitchCases = true,
            };

            gameName = Data.GeneralInfo.Name.Content;

            Console.Title = $"GMLoader  -  {gameName}";
            Log.Information($"Loaded game {gameName}");

            //CSX Handling
            ScriptOptionsInitialize();
            invalidXdeltaNames = new List<string>();
            invalidXdelta = 0;

            // certified toby fox moment, still wip
            if (gameName == "DELTARUNE")
            {
                mkDir(deltaruneCH1Path);
                mkDir(deltaruneCH2Path);
                mkDir(deltaruneCH3Path);
                mkDir(deltaruneCH4Path);

                processDeltaruneCH1Mods = HasNonFolderFiles(deltaruneCH1Path);
                processDeltaruneCH2Mods = HasNonFolderFiles(deltaruneCH2Path);
                processDeltaruneCH3Mods = HasNonFolderFiles(deltaruneCH3Path);
                processDeltaruneCH4Mods = HasNonFolderFiles(deltaruneCH4Path);

                
            }

            processCSXScripts(dirPreCSXFiles, dirBuiltInCSXFiles, dirPostCSXFiles, dirAfterCSXFiles);

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
                var startInfo = new ProcessStartInfo
                {
                    FileName = gameExecutable,
                    WorkingDirectory = Path.GetDirectoryName(gameExecutable),
                    UseShellExecute = true
                };
                var process = Process.Start(startInfo);
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
            Log.Error("An error occurred: " + e.Message);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }

    public static void importConfigDefinedCode(UndertaleModLib.Compiler.CodeImportGroup importGroup)
    {
        string[] configFiles = Directory.GetFiles(gmlCodePatchPath, "*.yaml*", SearchOption.TopDirectoryOnly)
                                  .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                  .ToArray();

        if (configFiles.Length == 0)
        {
            Log.Debug($"The config files are empty, at {gmlCodePatchPath}, skipping...");
            return;
        }

        Log.Information("Executing built-in ImportCodePatch");
        Console.Title = $"GMLoader - Deserializing code configuration files";

        // Dictionary to track modification history
        Dictionary<string, List<string>> modificationHistory = new Dictionary<string, List<string>>();

        foreach (string file in configFiles)
        {
            try
            {
                byte[] yamlBytes = File.ReadAllBytes(file);
                string fileName = Path.GetFileName(file);
                Log.Information($"Deserializing {fileName}");

                var yamlContent = YamlSerializer.Deserialize<Dictionary<string, List<CodeData>>>(yamlBytes);

                foreach (KeyValuePair<string, List<CodeData>> scriptEntry in yamlContent)
                {
                    string scriptName = scriptEntry.Key;
                    List<CodeData> patches = scriptEntry.Value;

                    // Track that this file modified the script
                    if (!modificationHistory.ContainsKey(scriptName))
                    {
                        modificationHistory[scriptName] = new List<string>();
                    }
                    modificationHistory[scriptName].Add(fileName);

                    foreach (CodeData patch in patches)
                    {
                        ProcessCodePatch(importGroup, fileName, scriptName, patch, modificationHistory);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to process {Path.GetFileName(file)}: {e}");
            }
        }

        Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content}";
    }

    public static void ProcessCodePatch(CodeImportGroup importGroup, string fileName, string scriptName, CodeData patch, Dictionary<string, List<string>> modificationHistory)
    {
        string type = patch.yml_type ?? "";
        string find = patch.yml_find;
        string code = patch.yml_code ?? "";
        bool caseSensitive = patch.yml_casesensitive ?? true;

        switch (type.ToLowerInvariant())
        {
            case "findreplace":
                if (string.IsNullOrEmpty(find))
                {
                    Log.Error($"Find pattern is empty for {scriptName}, skipping");
                    return;
                }
                importGroup.QueueFindReplace(scriptName, find, code, caseSensitive);
                break;
            case "findreplacetrim":
                importGroup.QueueTrimmedLinesFindReplace(scriptName, find, code, caseSensitive);
                break;
            case "append":
                importGroup.QueueAppend(scriptName, code);
                break;
            case "prepend":
                importGroup.QueuePrepend(scriptName, code);
                break;
            case "findappend":
                if (string.IsNullOrEmpty(find))
                {
                    Log.Error($"Find pattern is empty for {scriptName}, skipping");
                    return;
                }
                importGroup.QueueFindReplace(scriptName, find, find + Environment.NewLine + code, caseSensitive);
                break;
            case "findprepend":
                if (string.IsNullOrEmpty(find))
                {
                    Log.Error($"Find pattern is empty for {scriptName}, skipping");
                    return;
                }
                importGroup.QueueFindReplace(scriptName, find, code + Environment.NewLine + find, caseSensitive);
                break;
            case "findappendtrim":
                importGroup.QueueTrimmedLinesFindReplace(scriptName, find, find + Environment.NewLine + code, caseSensitive);
                break;
            case "findprependtrim":
                importGroup.QueueTrimmedLinesFindReplace(scriptName, find, code + Environment.NewLine + find, caseSensitive);
                break;
            case "findreplaceregex":
                if (string.IsNullOrEmpty(find))
                {
                    Log.Error($"Regex pattern is empty for {scriptName}, skipping");
                    return;
                }
                importGroup.QueueRegexFindReplace(scriptName, find, code, caseSensitive);
                break;
            default:
                Log.Error($"Unknown patch type '{type}' for {scriptName}, skipping");
                break;
        }

        try
        {
            importGroup.Import();
        }
        catch (Exception e)
        {
            // Get the modification history for this script
            string history = modificationHistory.ContainsKey(scriptName)
                ? string.Join(", ", modificationHistory[scriptName])
                : "no modifications recorded";

            Log.Warning($"An error has occurred on {fileName} while processing {scriptName}\n\n" +
                     $"'{scriptName}' was modified by these files in order: {history}\n\n" +
                     $"Find string:\n{find}\n\n" +
                     $"Code string:\n{code}\n\n" +
                     $"Exception: \n{e}\n\n");
        }
    }

    private static async Task importGraphic()
    {
        spriteDictionary.Clear();
        spriteList.Clear();
        backgroundDictionary.Clear();
        backgroundList.Clear();
        string pngExt = ".png";

        var spriteConfigFilesTask = Task.Run(() =>
            Directory.GetFiles(texturesConfigPath, "*.yaml", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray());

        var spriteStripStyleConfigFilesTask = Task.Run(() =>
            Directory.GetFiles(noStripTexturesPath, "*.yaml", SearchOption.AllDirectories)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray());

        var backgroundConfigFilesTask = Task.Run(() =>
            Directory.GetFiles(backgroundsConfigPath, "*.yaml", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray());

        await Task.WhenAll(spriteConfigFilesTask, spriteStripStyleConfigFilesTask, backgroundConfigFilesTask);

        string[] spriteConfigFIles = await spriteConfigFilesTask;
        string[] spriteStripStyleConfigFiles = await spriteStripStyleConfigFilesTask;
        string[] backgroundConfigFIles = await backgroundConfigFilesTask;

        if (spriteConfigFIles.Length == 0 && backgroundConfigFIles.Length == 0 && spriteStripStyleConfigFiles.Length == 0)
        {
            Log.Debug($"The sprite and background configuration files are empty, at {texturesConfigPath}, skipping texture import");
            return;
        }

        Log.Information("Executing built-in ImportGraphic");

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        if (spriteConfigFIles.Length != 0)
        {
            var spriteFilenames = new ConcurrentBag<string>();
            var localSpriteList = new ConcurrentBag<string>();
            var localModdedTextureCounts = new ConcurrentDictionary<string, int>();
            var localSpriteDictionary = new ConcurrentDictionary<string, SpriteData>();

            Log.Information("Deserializing sprite configuration files...");
            Console.Title = $"GMLoader - Deserializing sprite configuration files";

            await Parallel.ForEachAsync(spriteConfigFIles, parallelOptions, async (file, cancellationToken) =>
            {
                byte[] yamlBytes = await File.ReadAllBytesAsync(file);
                Log.Information($"Deserializing {Path.GetFileName(file)}");
                var deserialized = YamlSerializer.Deserialize<Dictionary<string, SpriteData>>(yamlBytes);

                foreach (var (spritename, configs) in deserialized)
                {
                    spriteFilenames.Add(spritename + pngExt);
                    localSpriteList.Add(spritename);
                    localModdedTextureCounts[spritename] = configs.yml_frame ?? 1;

                    localSpriteDictionary[spritename] = new SpriteData
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
            });

            spriteList.AddRange(localSpriteList);
            spritesToImport = spriteFilenames.ToArray();
            foreach (var kvp in localModdedTextureCounts) moddedTextureCounts[kvp.Key] = kvp.Value;
            foreach (var kvp in localSpriteDictionary) spriteDictionary[kvp.Key] = kvp.Value;
        }
        else
        {
            Log.Debug("The sprite configuration files are empty, skipping...");
        }

        if (backgroundConfigFIles.Length != 0)
        {
            var backgroundFilenames = new ConcurrentBag<string>();
            var localBackgroundList = new ConcurrentBag<string>();
            var localBackgroundDictionary = new ConcurrentDictionary<string, BackgroundData>();

            Log.Information("Deserializing backgrounds configuration files...");
            Console.Title = $"GMLoader - Deserializing backgrounds configuration files";

            await Parallel.ForEachAsync(backgroundConfigFIles, parallelOptions, async (file, cancellationToken) =>
            {
                byte[] yamlBytes = await File.ReadAllBytesAsync(file);
                Log.Information($"Deserializing {Path.GetFileName(file)}");
                var deserialized = YamlSerializer.Deserialize<Dictionary<string, BackgroundData>>(yamlBytes);

                foreach (var (backgroundname, configs) in deserialized)
                {
                    backgroundFilenames.Add(backgroundname + pngExt);
                    localBackgroundList.Add(backgroundname);

                    localBackgroundDictionary[backgroundname] = new BackgroundData
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
            });

            backgroundList.AddRange(localBackgroundList);
            backgroundsToImport = backgroundFilenames.ToArray();
            foreach (var kvp in localBackgroundDictionary) backgroundDictionary[kvp.Key] = kvp.Value;
        }
        else
        {
            Log.Debug("The background sprite configuration files are empty, skipping...");
        }

        if (spriteStripStyleConfigFiles.Length != 0)
        {
            var localSpriteList = new ConcurrentBag<string>();
            var localSpriteDictionary = new ConcurrentDictionary<string, SpriteData>();

            Log.Information("Deserializing nostrip style sprite configuration files...");
            Console.Title = $"GMLoader - Deserializing nostrip style sprite configuration files";

            await Parallel.ForEachAsync(spriteStripStyleConfigFiles, parallelOptions, async (file, cancellationToken) =>
            {

                byte[] yamlBytes = await File.ReadAllBytesAsync(file);
                Log.Information($"Deserializing {Path.GetFileName(file)} ({Path.GetFileName(Path.GetDirectoryName(file))})");
                var deserialized = YamlSerializer.Deserialize<SpriteData>(yamlBytes);
                string spriteName = Path.GetFileName(Path.GetDirectoryName(file));

                localSpriteList.Add(spriteName);
                localSpriteDictionary[spriteName] = new SpriteData
                {
                    yml_x = deserialized.yml_x ?? defaultSpriteX,
                    yml_y = deserialized.yml_y ?? defaultSpriteY,
                    yml_transparent = deserialized.yml_transparent ?? defaultSpriteTransparent,
                    yml_smooth = deserialized.yml_smooth ?? defaultSpriteSmooth,
                    yml_preload = deserialized.yml_preload ?? defaultSpritePreload,
                    yml_speedtype = deserialized.yml_speedtype ?? defaultSpriteSpeedType,
                    yml_framespeed = deserialized.yml_framespeed ?? defaultSpriteFrameSpeed,
                    yml_boundingboxtype = deserialized.yml_boundingboxtype ?? defaultSpriteBoundingBoxType,
                    yml_bboxleft = deserialized.yml_bboxleft ?? defaultSpriteBoundingBoxLeft,
                    yml_bboxright = deserialized.yml_bboxright ?? defaultSpriteBoundingBoxRight,
                    yml_bboxbottom = deserialized.yml_bboxbottom ?? defaultSpriteBoundingBoxBottom,
                    yml_bboxtop = deserialized.yml_bboxtop ?? defaultSpriteBoundingBoxTop,
                    yml_sepmask = deserialized.yml_sepmask ?? defaultSpriteSepMasksType
                };
            });

            spriteList.AddRange(localSpriteList);
            foreach (var kvp in localSpriteDictionary) spriteDictionary[kvp.Key] = kvp.Value;
        }
        else
        {
            Log.Debug("The nostrip style sprite configuration files are empty, skipping...");
        }

        Console.Title = $"GMLoader - {Data.GeneralInfo.Name.Content}";
    }

    private static async Task importAudio()
    {
        audioDictionary.Clear();
        audioList.Clear();

        var AudioConfigFilesTask = Task.Run(() =>
            Directory.GetFiles(audioConfigPath, "*.yaml", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray());

        await Task.WhenAll(AudioConfigFilesTask);
        string[] audioConfigFiles = await AudioConfigFilesTask;

        if (audioConfigFiles.Length == 0)
        {
            Log.Debug($"The audio configuration files are empty, at {audioConfigPath}, skipping audio import");
            return;
        }

        Log.Information("Executing built-in ImportAudio");

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        if (audioConfigFiles.Length != 0)
        {
            var audioFilenames = new ConcurrentBag<string>();
            var localAudioList = new ConcurrentBag<string>();
            var localAudioDictionary = new ConcurrentDictionary<string, AudioData>();

            Log.Information("Deserializing audio configuration files...");
            Console.Title = $"GMLoader - Deserializing audio configuration files";

            await Parallel.ForEachAsync(audioConfigFiles, parallelOptions, async (file, cancellationToken) =>
            {
                byte[] yamlBytes = await File.ReadAllBytesAsync(file);
                Log.Information($"Deserializing {Path.GetFileName(file)}");
                var deserialized = YamlSerializer.Deserialize<Dictionary<string, AudioData>>(yamlBytes);

                foreach (var (audioname, configs) in deserialized)
                {
                    audioFilenames.Add(audioname); // plus extensionname
                    localAudioList.Add(audioname);

                    localAudioDictionary[audioname] = new AudioData
                    {
                        yml_type = configs.yml_type ?? defaultAudioType,
                        yml_embedded = configs.yml_embedded ?? defaultAudioEmbedded,
                        yml_compressed = configs.yml_compressed ?? defaultAudioCompressed,
                        yml_effects = configs.yml_effects ?? defaultAudioEffects,
                        yml_volume = configs.yml_volume ?? defaultAudioVolume,
                        yml_pitch = configs.yml_pitch ?? defaultAudioPitch,
                        yml_audiogroup_index = configs.yml_audiogroup_index ?? defaultAudioGroupIndex,
                        yml_audiofile_id = configs.yml_audiofile_id ?? defaultAudioFileID,
                        yml_preload = configs.yml_preload ?? defaultAudioPreload
                    };
                }
            });

            audioList.AddRange(localAudioList);
            audioToImport = audioFilenames.ToArray();
            foreach (var kvp in localAudioDictionary) audioDictionary[kvp.Key] = kvp.Value;
        }
        else
        {
            Log.Debug($"The audio configuration files are empty at {audioConfigPath}, skipping...");
        }

        Console.Title = $"GMLoader - {Data.GeneralInfo.Name.Content}";

    }

    #region Helper Methods

    public static void mkDir(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void DeleteEmptyFolders(string path)
    {
        if (!Directory.Exists(path))
        {
            Log.Error($"{path} doesn't exists");
            return;
        }

        foreach (var directory in Directory.GetDirectories(path))
        {
            DeleteEmptyFolders(directory);
        }

        try
        {
            var files = Directory.GetFiles(path);
            var subDirs = Directory.GetDirectories(path);

            if (files.Length == 0 && subDirs.Length == 0 && path != Path.GetPathRoot(path))
            {
                Directory.Delete(path);
                Log.Information($"Deleted empty folder: {path}");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error($"Access denied: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Log.Error($"Directory not found: {ex.Message}");
        }
        catch (IOException ex)
        {
            Log.Error($"IO error: {ex.Message}");
        }
    }

    public static bool HasNonFolderFiles(string directoryPath)
    {
        // Check if current directory has any files
        if (Directory.GetFiles(directoryPath).Length > 0)
        {
            return true;
        }

        // Recursively check subdirectories
        foreach (var subdirectory in Directory.GetDirectories(directoryPath))
        {
            if (HasNonFolderFiles(subdirectory))
            {
                return true;
            }
        }

        return false;
    }

    public static ulong ComputeFileHash64(string filePath)
    {
        using (var fileStream = File.OpenRead(filePath))
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
        // Get and sort files alphabetically (case-insensitive)
        string[] files = Directory.GetFiles(path)
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                 .ToArray();

        // Get and sort directories alphabetically (case-insensitive), excluding 'lib'
        string[] directories = Directory.GetDirectories(path)
                                      .Where(d => !Path.GetFileName(d).Equals("lib", StringComparison.OrdinalIgnoreCase))
                                      .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                                      .ToArray();

        // Process files in the current directory
        for (int i = 0; i < files.Length; i++)
        {
            bool lastFile = (i == files.Length - 1 && directories.Length == 0);
            string prefix = lastFile ? "└── " : "├── ";
            Log.Debug($"{indent}{prefix}{Path.GetFileName(files[i])}");
        }

        // Process subdirectories
        for (int i = 0; i < directories.Length; i++)
        {
            string dirName = Path.GetFileName(directories[i]);
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

    public static void logCode(string codeName, GlobalDecompileContext context = null)
    {
        UndertaleCode code = Data.Code.ByName(codeName);

        if (code == null)
        {
            Log.Error($"{codeName} isn't found in the data");
            return;
        }
            

        if (code.ParentEntry is not null)
        {
            Log.Error($"// This code entry is a reference to an anonymous function within \"{code.ParentEntry.Name.Content}\", decompile that instead.");
            return;
        }
             

        GlobalDecompileContext globalDecompileContext = context is null ? new(Data) : context;

        try
        {
            Log.Information( code != null
                ? $"\n{codeName}:\n" + new Underanalyzer.Decompiler.DecompileContext(globalDecompileContext, code, 
                defaultDecompSettings ?? Data.ToolInfo.DecompilerSettings).DecompileToString() : "");
        }
        catch (Exception e)
        {
            Log.Error("/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
        }
    }

    public static void ExportData(string dataPath)
    {
        Console.Title = $"GMLoader  - Export Mode";

        if (!File.Exists(exportTextureScriptPath) || !File.Exists(exportGameObjectScriptPath) ||
            !File.Exists(exportCodeScriptPath) || !File.Exists(exportAudioScriptPath) ||
            !File.Exists(exportRoomScriptPath))
        {
            Log.Error("some exporter script are missing");
            Console.ReadKey();
            Environment.Exit(0);
        }

        while (Directory.Exists(exportGameObjectOutputPath) || Directory.Exists(exportTextureOutputPath) ||
            Directory.Exists(exportCodeOutputPath) || Directory.Exists(exportAudioOutputPath) ||
            Directory.Exists(exportRoomOutputPath))
        {
            Log.Information("Please delete the Export folder before exporting.\n\nPress any key to continue..");
            Console.ReadKey();
        }

        Log.Information($"Reading game data from {Path.GetFileName(dataPath)}");

        Data = new UndertaleData();

        using (var stream = new FileStream(dataPath, FileMode.Open, FileAccess.ReadWrite))
        {
            Data = UndertaleIO.Read(stream);
        }
            
        Console.Title = $"GMLoader  -  {Data.GeneralInfo.Name.Content} - Export Mode";
        Log.Information($"Loaded game {Data.GeneralInfo.Name.Content}");

        invalidCodeNames = new List<string>();
        invalidCode = 0;

        invalidSpriteNames = new List<string>();
        invalidSprite = 0;

        invalidSpriteSizeNames = new List<string>();
        invalidSpriteSize = 0;

        if (exportTexture)
            RunCSharpFile(exportTextureScriptPath);
        if (exportGameObject)
            RunCSharpFile(exportGameObjectScriptPath);
        if (exportAudio)
            RunCSharpFile(exportAudioScriptPath);
        if (exportCode)
            RunCSharpFile(exportCodeScriptPath);
        if (exportRoom)
            RunCSharpFile(exportRoomScriptPath);

        if (!exportGameObject && !exportCode && !exportTexture && !exportAudio && !exportRoom)
        {
            Log.Information("No export option enabled?");
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

        Log.Information($"Successfully exported {dataPath}");
    }

    public static void ConvertBothData()
    {
        string vanillaExportPath = "vanilla_export";
        string moddedExportPath = "modded_export";

        while (!File.Exists(convertVanillaData) && !reuseVanillaExport)
        {
            Log.Error($"Error, missing {convertVanillaData}");
            Console.ReadKey();
        }

        while (!File.Exists(convertModdedData))
        {
            Log.Error($"Error, missing {convertModdedData}");
            Console.ReadKey();
        }

        while (Directory.Exists(vanillaExportPath) && !reuseVanillaExport)
        {
            Log.Error($"Please delete the {vanillaExportPath} folder before proceeding");
            Console.ReadKey();
        }

        while (Directory.Exists(moddedExportPath))
        {
            Log.Error($"Please delete the {moddedExportPath} folder before proceeding");
            Console.ReadKey();
        }

        while (Directory.Exists(exportOutputPath))
        {
            Log.Error($"Please delete the {exportOutputPath} folder before proceeding");
            Console.ReadKey();
        }

        while (Directory.Exists(convertOutputPath))
        {
            Log.Error($"Please delete the {convertOutputPath} folder before proceeding");
            Console.ReadKey();
        }

        if (!reuseVanillaExport || (reuseVanillaExport && !Directory.Exists(vanillaExportPath)))
        {
            ExportData(convertVanillaData);
            Directory.Move(exportOutputPath, vanillaExportPath);
        }

        ExportData(convertModdedData);
        Directory.Move(exportOutputPath, moddedExportPath);

        mkDir(convertOutputPath);
        CompareAndCopyFiles(vanillaExportPath, moddedExportPath, convertOutputPath);

        if (exportTexture)
        {
            CopyVanillaSpriteConfig(moddedExportPath, convertOutputPath);
            CopyNoStripStyleSprites(moddedExportPath, convertOutputPath);
            MergeSpriteConfigurations(convertOutputPath);
        }

        RefactorIntoGMLoaderFormat(convertOutputPath);

        Log.Information($"Done converting, files has been copied into {convertOutputPath}");

        DeleteEmptyFolders(convertOutputPath);

        Console.WriteLine("\nWould you like to delete the residual exported files? (Y/N)");

        string response = Console.ReadLine()?.ToLower();

        while (response != "y" && response != "n" && response != "yes" && response != "no")
        {
            Console.WriteLine("Acceptable response is only \"y\" or \"n\" ");
            response = Console.ReadLine()?.ToLower();
        }

        if (response == "y" || response == "yes")
        {
            Log.Information($"Deleting {vanillaExportPath} ...");
            Directory.Delete(vanillaExportPath, true);

            Log.Information($"Deleting {moddedExportPath} ...");
            Directory.Delete(moddedExportPath, true);
        }
        else
        {
            Environment.Exit(0);
        }
    }

    public static void processCSXScripts(string[] preCSXFiles, string[] builtInCSXFiles, string[] postCSXFiles, string[] afterCSXFiles)
    {
        if (preCSXFiles.Length != 0)
        {
            if (compilePreCSX)
            {
                Log.Information("Loading pre-CSX Scripts.");
                foreach (string file in preCSXFiles)
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
        else if (compilePreCSX && !preCSXFiles.Any(x => x.EndsWith(".csx")))
        {
            Log.Debug($"No pre-CSX script file found. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPreCSXPath))} , skipping the process.");
        }

        //Compile builtin scripts
        if (builtInCSXFiles.Length != 0)
        {
            if (compileBuiltInCSX)
            {
                Log.Information("Loading builtin-CSX scripts.");
                
                importGraphic();    // Had to be done on GMLoader's side because of VYaml issues
                importAudio();      // same

                foreach (string file in builtInCSXFiles)
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
        else if (compileBuiltInCSX && !builtInCSXFiles.Any(x => x.EndsWith(".csx")))
        {
            Log.Information($"No builtin-CSX script file found at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath))} , skipping the process.");
        }

        //Compile users script after builtin scripts

        if (postCSXFiles.Length != 0)
        {
            if (compilePostCSX)
            {

                Log.Information("Loading post-CSX Scripts.");
                foreach (string file in postCSXFiles)
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
        else if (compilePostCSX && !postCSXFiles.Any(x => x.EndsWith(".csx")))
        {
            Log.Debug($"No post-CSX script file found At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importPostCSXPath))} , skipping the process.");
        }
    }

    // This is by far the stupidest code that I wrote, please refactor this
    public static void CompareAndCopyFiles(string vanillaFolder, string moddedFolder, string outputFolder)
    {
        Log.Information("Executing filediff comparison");

        // why do i need to do this
        mkDir(Path.Combine(outputFolder, Path.GetFileName(exportTextureOutputPath), Path.GetFileName(backgroundsTexturePath)));
        mkDir(Path.Combine(outputFolder, Path.GetFileName(exportTextureOutputPath), Path.GetFileName(noStripTexturesPath)));
        mkDir(Path.Combine(outputFolder, Path.GetFileName(configPath)));
        mkDir(Path.Combine(outputFolder, Path.GetFileName(configPath), Path.GetFileName(newObjectPath)));
        mkDir(Path.Combine(outputFolder, Path.GetFileName(configPath), Path.GetFileName(existingObjectPath)));

        var vanillaFiles = Directory.GetFiles(vanillaFolder, "*.*", SearchOption.AllDirectories);
        var moddedFiles = Directory.GetFiles(moddedFolder, "*.*", SearchOption.AllDirectories);
        var regex = new Regex(@"^(.*?)(?=_f[0-9])", RegexOptions.Compiled);

        var vanillaFileNames = new ConcurrentDictionary<string, string>();

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

        Parallel.ForEach(moddedFiles, moddedFile =>
        {
            string moddedFilename = Path.GetFileName(moddedFile);
            var match = regex.Match(Path.GetFileName(moddedFile));
            if (match.Success)
            {
                moddedFilename = match.Groups[1].Value;
                //Log.Information($"Storing fileName: {fileName} which equals to {moddedFile}");
            }

            string relativePath = Path.GetRelativePath(moddedFolder, moddedFile);
            string outputFilePath = Path.Combine(outputFolder, relativePath);

            string outputNewObjectPath = Path.Combine(outputFolder, Path.GetFileName(configPath), Path.GetFileName(newObjectPath), moddedFilename);
            string outputExistingObjectPath = Path.Combine(outputFolder, Path.GetFileName(configPath), Path.GetFileName(existingObjectPath), moddedFilename);

            // Always copy spriteData regardless of hash
            if (moddedFilename.Equals("data.json", StringComparison.OrdinalIgnoreCase))
            {
                Log.Information($"Copying {moddedFile}");
                mkDir(Path.GetDirectoryName(outputFilePath));
                File.Copy(moddedFile, outputFilePath, true);
                return;
            }

            if (vanillaFileNames.TryGetValue(moddedFilename, out string vanillaFile))
            {
                ulong vanillaHash = ComputeFileHash3(vanillaFile);
                ulong modHash = ComputeFileHash3(moddedFile);

                // Compare the files
                if (vanillaHash != modHash)
                {
                    Log.Information($"File hash is different, Copying {Path.GetFileName(moddedFile)}");

                    if (moddedFile.Contains(Path.GetFileName(exportGameObjectOutputPath)))
                    {
                        mkDir(Path.GetDirectoryName(outputFilePath));
                        File.Copy(moddedFile, outputExistingObjectPath, true);
                        return;
                    }

                    mkDir(Path.GetDirectoryName(outputFilePath));
                    File.Copy(moddedFile, outputFilePath, true);
                }
            }
            else
            {
                Log.Information($"Vanilla file not found. Copying {Path.GetFileName(moddedFile)}");

                if (moddedFile.Contains(Path.GetFileName(exportGameObjectOutputPath)))
                {
                    mkDir(Path.GetDirectoryName(outputFilePath));
                    File.Copy(moddedFile, outputNewObjectPath, true);
                    return;
                }

                mkDir(Path.GetDirectoryName(outputFilePath));
                File.Copy(moddedFile, outputFilePath, true);
            }
        });
    }

    public static void CopyNoStripStyleSprites(string moddedFolder, string outputFolder)
    {
        Log.Information("Copying background vanilla sprite configuration files");

        string outputNoStripSpriteFolder = Path.Combine(outputFolder, Path.GetFileName(exportTextureOutputPath), Path.GetFileName(exportTextureNoStripOutputPath));
        mkDir(outputNoStripSpriteFolder);
        string moddedNoStripSpriteFolder = Path.Combine(moddedFolder, Path.GetFileName(exportTextureOutputPath), Path.GetFileName(exportTextureNoStripOutputPath));

        string[] outputSpriteDirectories = Directory.GetDirectories(outputNoStripSpriteFolder);

        foreach (string outputSpriteDir in outputSpriteDirectories)
        {
            string spriteName = Path.GetFileName(outputSpriteDir);

            string moddedSpriteDir = Path.Combine(moddedNoStripSpriteFolder, spriteName);

            if (Directory.Exists(moddedSpriteDir))
            {
                string[] pngFiles = Directory.GetFiles(moddedSpriteDir, "*.png");

                foreach (string pngFile in pngFiles)
                {
                    string destFile = Path.Combine(outputSpriteDir, Path.GetFileName(pngFile));

                    try
                    {
                        File.Copy(pngFile, destFile, overwrite: true);
                        Log.Information($"Copied {pngFile} to {destFile}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to copy {pngFile}: {ex.Message}");
                    }
                }
            }
            else
            {
                Log.Debug($"No modded sprites found for {spriteName}");
            }
        }
    }

    public static void CopyVanillaSpriteConfig(string moddedFolder, string outputFolder)
    {
        Log.Information("Copying vanilla sprite configuration files");
        string outputSpriteFolder = Path.Combine(outputFolder, Path.GetFileName(exportTextureOutputPath));
        string outputSpriteConfigFolder = Path.Combine(outputFolder, Path.GetFileName(exportTextureConfigOutputPath));
        string moddedOutputSpriteConfigFolder = Path.Combine(moddedFolder, Path.GetFileName(exportTextureConfigOutputPath));

        var spriteFiles = Directory.GetFiles(outputSpriteFolder, "*.*", SearchOption.AllDirectories)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToList();

        mkDir(outputSpriteConfigFolder);

        foreach (var yamlFile in Directory.GetFiles(moddedOutputSpriteConfigFolder, "*.yaml", SearchOption.AllDirectories))
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(yamlFile);

            if (spriteFiles.Contains(fileNameWithoutExt))
            {
                string destPath = Path.Combine(outputSpriteConfigFolder, Path.GetFileName(yamlFile));

                try
                {
                    File.Copy(yamlFile, destPath, overwrite: true);
                    Log.Information($"Copied {Path.GetFileName(yamlFile)}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to copy {yamlFile}: {ex.Message}");
                }
            }
        }

        string outputBackgroundSpriteFolder = Path.Combine(outputSpriteFolder, Path.GetFileName(exportTextureBackgroundOutputPath));
        string outputBackgroundSpriteConfigFolder = Path.Combine(outputSpriteConfigFolder, Path.GetFileName(exportBackgroundTextureConfigOutputPath));
        string moddedBackgroundSpriteConfigFolder = Path.Combine(moddedOutputSpriteConfigFolder, Path.GetFileName(exportBackgroundTextureConfigOutputPath));

        var backgroundSpriteFiles = Directory.GetFiles(outputBackgroundSpriteFolder, "*.*", SearchOption.AllDirectories)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToList();

        mkDir(outputBackgroundSpriteConfigFolder);

        foreach (var yamlFile in Directory.GetFiles(moddedBackgroundSpriteConfigFolder, "*.yaml", SearchOption.AllDirectories))
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(yamlFile);

            if (backgroundSpriteFiles.Contains(fileNameWithoutExt))
            {
                string destPath = Path.Combine(outputBackgroundSpriteConfigFolder, Path.GetFileName(yamlFile));

                try
                {
                    File.Copy(yamlFile, destPath, overwrite: true);
                    Log.Information($"Copied {Path.GetFileName(yamlFile)}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to copy {yamlFile}: {ex.Message}");
                }
            }
        }
    }

    public static void MergeSpriteConfigurations(string basePath)
    {
        Log.Information("Merging sprite configuration files");
        string spriteConfigFileName = "MyModdedSpriteConfig.yaml";
        string backgroundSpriteConfigFilename = "MyModdedBackgroundSpriteConfig.yaml";
        string spriteConfigRelativePath = Path.Combine(basePath, Path.GetFileName(exportTextureConfigOutputPath));
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
                }
            }
            foreach (string file in spriteConfigFiles)
            {
                File.Delete(file);
                Log.Debug($"Deleted: {Path.GetFileName(file)}");
            }
        }
        else
        {
            Log.Information("No sprite configuration files found, skipping the process");
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
                }
                foreach (string file in BackgroundSpriteConfigFIles)
                {
                    Log.Debug($"Deleting {Path.GetFileName(file)}");
                    File.Delete(file);
                }
            }
        }
        else
        {
            Log.Information("No background sprite configuration files found, skipping the process");
        }

        if (File.Exists(spriteConfigFileName))
            File.Move(spriteConfigFileName, Path.Combine(spriteConfigRelativePath, spriteConfigFileName));

        if (File.Exists(backgroundSpriteConfigFilename))
            File.Move(backgroundSpriteConfigFilename, Path.Combine(backgroundSpriteConfigRelativePath, backgroundSpriteConfigFilename));

    }

    public static void RefactorIntoGMLoaderFormat(string targetFolder)
    {
        Log.Information("Refactoring into GMLoader format");
        string _configPath = Path.Combine(targetFolder, Path.GetFileName(configPath));
        mkDir(_configPath);

        string texturePath = Path.Combine(targetFolder, Path.GetFileName(exportTextureOutputPath));
        string refactoredTexturePath = Path.Combine(targetFolder, Path.GetFileName(texturesPath));

        string spriteConfigPath = Path.Combine(targetFolder, Path.GetFileName(exportTextureConfigOutputPath));
        string spriteConfigRelativePath = Path.Combine(Path.GetFileName(Path.GetDirectoryName(texturesConfigPath)), Path.GetFileName(texturesConfigPath)); // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHHHHHHHHHHH
        string refactoredSpriteConfigPath = Path.Combine(targetFolder, spriteConfigRelativePath);

        string _objectPath = Path.Combine(targetFolder, Path.GetFileName(exportGameObjectOutputPath));

        string _codePath = Path.Combine(targetFolder, Path.GetFileName(exportCodeOutputPath));
        string refactoredexportCodePath = Path.Combine(targetFolder, Path.GetFileName(gmlCodePath));

        string _roomPath = Path.Combine(targetFolder, Path.GetFileName(exportRoomOutputPath));
        string refactoredroomPath = Path.Combine(targetFolder, Path.GetFileName(roomPath));

        if (Directory.Exists(texturePath))
            Directory.Move(texturePath, refactoredTexturePath);

        if (Directory.Exists(spriteConfigPath))
            Directory.Move(spriteConfigPath, refactoredSpriteConfigPath);

        //Yes this is intended
        if (Directory.Exists(_objectPath))
            Directory.Delete(_objectPath);

        if (Directory.Exists(_codePath))
            Directory.Move(_codePath, refactoredexportCodePath);

        if (Directory.Exists(_roomPath))
            Directory.Move(_roomPath, refactoredroomPath);
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
