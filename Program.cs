#region Using Directives
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
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
    public static UndertaleRoom newRoom { get; set; }
    private static ScriptOptions CliScriptOptions { get; set; }
    #endregion

    static void Main()
    {
        if (File.Exists("GMLoader.log"))
        {
            File.Delete("GMLoader.log");
        }

        Data = new UndertaleData();
        newRoom = new UndertaleRoom();

        Console.Title = "GMLoader";

        if (!File.Exists("GMLoader.ini"))
        {
            PrintMessage("Missing GMLoader.ini file \n\n\nPress any key to close...");
            Console.ReadKey();
            Environment.Exit(0);
        }
        else
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
                .Build();

            #region Field
            string backupDataPath = configuration["universal:databackup"];
            string gameDataPath = configuration["universal:originaldata"];

            string importBuiltInCSXPath = configuration["universal:importbuiltincsx"];
            string compileBuiltInCSXString = configuration["universal:compilebuiltincsx"];

            string importCSXPath = configuration["universal:importcsx"];
            string compileCSXString = configuration["universal:compilecsx"];

            string supportedHashVersion = configuration["gamespecific:supportedhashversion"];
            string autoGameStartString = configuration["universal:autogamestart"];
            string gameExecutable = configuration["gamespecific:gameexecutable"];

            bool compileBuiltInCSX = bool.Parse(compileCSXString);
            bool compileCSX = bool.Parse(compileCSXString);

            bool autoGameStart = bool.Parse(autoGameStartString);
            ulong currentHash = 0;
            #endregion

            CreateDirectoryIfNotExists(importCSXPath);
            CreateDirectoryIfNotExists(importBuiltInCSXPath);
            string[] dirBuiltInCSXFiles = Directory.GetFiles(importBuiltInCSXPath, "*.csx");
            string[] dirCSXFiles = Directory.GetFiles(importCSXPath, "*.csx");

            if (!compileBuiltInCSX && !compileCSX)
            {
                PrintMessage("What's the point of using GMLoader if you disable CSX COMPILING REEEEEEEEEEE \n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (dirBuiltInCSXFiles.Length == 0 && dirCSXFiles.Length == 0)
            {
                PrintMessage($"The CSX Script folder path is empty. At {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importCSXPath))}. \nAborting the process\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else if (!dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")) && !dirCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"No CSX Script file found at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importCSXPath))}. \nAborting the process\n\n\nPress any key to close...");
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

            if (File.Exists(backupDataPath) && supportedHashVersion == currentHash.ToString())
            {
                using (var stream = new FileStream(backupDataPath, FileMode.Open, FileAccess.ReadWrite))
                    Data = UndertaleIO.Read(stream);
            }
            else if (File.Exists(backupDataPath) && supportedHashVersion != currentHash.ToString())
            {
                PrintMessage("Game Data Hash Mismatch Error\nThis happens because loader is outdated or the data.win is modified.\nTry deleting backup.win and verify the integrity of game.\n\n\nPress any key to close...");
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

            if (dirBuiltInCSXFiles.Length != 0)
            {
                if (compileBuiltInCSX)
                {
                    PrintMessage("\nExecuting BuiltIn Scripts.");
                    foreach (string file in dirBuiltInCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    PrintMessage("Loading BuiltIn CSX Script is disabled, skipping the process.");
                }
            }
            else if (compileBuiltInCSX)
            {
                PrintMessage("The BuiltIn CSX folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath)) + "\nSkipping the process.");
            }
            else if (compileBuiltInCSX && !dirBuiltInCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"No CSX Script file found at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importBuiltInCSXPath))}. \nSkipping the process.");
            }

            //Compile User's Script Files

            if (dirCSXFiles.Length != 0)
            {
                if (compileCSX)
                {
                    PrintMessage("\nExecuting User's CSX Scripts.");
                    foreach (string file in dirCSXFiles)
                    {
                        RunCSharpFile(file);
                    }
                }
                else
                {
                    PrintMessage("Loading CSX script is disabled, skipping the process.");
                }
            }
            else if (compileCSX)
            {
                PrintMessage("\nThe User's CSX folder is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importCSXPath)) + "\nSkipping the process.");
            }
            else if (compileCSX && !dirCSXFiles.Any(x => x.EndsWith(".csx")))
            {
                PrintMessage($"No CSX Script file found at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importCSXPath))}. \nSkipping the process.");
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
                Thread.Sleep(10000);
                Environment.Exit(0);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                PrintMessage("\nGame Data has been recompiled, you can now launch the modded data through the game executable.\n\n\n\n\nPress any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

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

    public static void DummyNETJsonParsing() //To Enforce using NET Json
    {
        JObject jsonObject = JObject.Parse("bruh");
        jsonObject.RemoveAll();
    }

    public static void DummyJsonParsing()
    {
        Utf8JsonReader jsonObject = new Utf8JsonReader();
        jsonObject.GetBoolean();
    }
    #endregion

    #region UTMTMethods

    public static void EnsureDataLoaded()
    {
        if (Data is null)
            throw new ScriptException("No data file is currently loaded!");
    }

    public static void ImportCode(string codeName, string gmlCode, bool isGML = true, bool doParse = true, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
    {
        bool skipPortions = false;
        UndertaleCode code = Data.Code.ByName(codeName);
        if (code is null)
        {
            code = new UndertaleCode();
            code.Name = Data.Strings.MakeString(codeName);
            Data.Code.Add(code);
        }
        else if (code.ParentEntry is not null)
            return;

        if (Data?.GeneralInfo.BytecodeVersion > 14 && Data.CodeLocals.ByName(codeName) == null)
        {
            UndertaleCodeLocals locals = new UndertaleCodeLocals();
            locals.Name = code.Name;

            UndertaleCodeLocals.LocalVar argsLocal = new UndertaleCodeLocals.LocalVar();
            argsLocal.Name = Data.Strings.MakeString("arguments");
            argsLocal.Index = 0;

            locals.Locals.Add(argsLocal);

            code.LocalsCount = 1;
            Data.CodeLocals.Add(locals);
        }
        if (doParse)
        {
            // This portion links code.
            if (codeName.StartsWith("gml_Script"))
            {
                // Add code to scripts section.
                if (Data.Scripts.ByName(codeName.Substring(11)) == null)
                {
                    UndertaleScript scr = new UndertaleScript();
                    scr.Name = Data.Strings.MakeString(codeName.Substring(11));
                    scr.Code = code;
                    Data.Scripts.Add(scr);
                }
                else
                {
                    UndertaleScript scr = Data.Scripts.ByName(codeName.Substring(11));
                    scr.Code = code;
                }
            }
            else if (codeName.StartsWith("gml_GlobalScript"))
            {
                // Add code to global init section.
                UndertaleGlobalInit initEntry = null;
                // This doesn't work, have to do it the hard way: UndertaleGlobalInit init_entry = Data.GlobalInitScripts.ByName(scr_dup_code_name_con);
                foreach (UndertaleGlobalInit globalInit in Data.GlobalInitScripts)
                {
                    if (globalInit.Code.Name.Content == codeName)
                    {
                        initEntry = globalInit;
                        break;
                    }
                }
                if (initEntry == null)
                {
                    UndertaleGlobalInit newInit = new UndertaleGlobalInit();
                    newInit.Code = code;
                    Data.GlobalInitScripts.Add(newInit);
                }
                else
                {
                    UndertaleGlobalInit NewInit = initEntry;
                    NewInit.Code = code;
                }
            }
            else if (codeName.StartsWith("gml_Object"))
            {
                string afterPrefix = codeName.Substring(11);
                int underCount = 0;
                string methodNumberStr = "", methodName = "", objName = "";
                for (int i = afterPrefix.Length - 1; i >= 0; i--)
                {
                    if (afterPrefix[i] == '_')
                    {
                        underCount++;
                        if (underCount == 1)
                        {
                            methodNumberStr = afterPrefix.Substring(i + 1);
                        }
                        else if (underCount == 2)
                        {
                            objName = afterPrefix.Substring(0, i);
                            methodName = afterPrefix.Substring(i + 1, afterPrefix.Length - objName.Length - methodNumberStr.Length - 2);
                            break;
                        }
                    }
                }
                int methodNumber = 0;
                try
                {
                    methodNumber = int.Parse(methodNumberStr);
                    if (methodName == "Collision" && (methodNumber >= Data.GameObjects.Count || methodNumber < 0))
                    {
                        bool doNewObj = true; //ScriptQuestion("Object of ID " + methodNumber.ToString() + " was not found.\nAdd new object?"); \\Modified to suit GMLoader
                        PrintMessage("Object of ID " + methodNumber.ToString() + " was not found.\nAdding the object.");
                        if (doNewObj)
                        {
                            UndertaleGameObject gameObj = new UndertaleGameObject();
                            gameObj.Name = Data.Strings.MakeString(SimpleTextInput("Enter object name", "Enter object name", "This is a single text line input box test.", false));
                            Data.GameObjects.Add(gameObj);
                        }
                        else
                        {
                            // It *needs* to have a valid value, make the user specify one.
                            List<uint> possibleValues = new List<uint>();
                            possibleValues.Add(uint.MaxValue);
                            methodNumber = (int)ReduceCollisionValue(possibleValues);
                        }
                    }
                }
                catch
                {
                    if (afterPrefix.LastIndexOf("_Collision_") != -1)
                    {
                        string s2 = "_Collision_";
                        objName = afterPrefix.Substring(0, (afterPrefix.LastIndexOf("_Collision_")));
                        methodNumberStr = afterPrefix.Substring(afterPrefix.LastIndexOf("_Collision_") + s2.Length, afterPrefix.Length - (afterPrefix.LastIndexOf("_Collision_") + s2.Length));
                        methodName = "Collision";
                        // GMS 2.3+ use the object name for the one colliding, which is rather useful.
                        if (Data.IsVersionAtLeast(2, 3))
                        {
                            if (Data.GameObjects.ByName(methodNumberStr) != null)
                            {
                                for (var i = 0; i < Data.GameObjects.Count; i++)
                                {
                                    if (Data.GameObjects[i].Name.Content == methodNumberStr)
                                    {
                                        methodNumber = i;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                bool doNewObj = true; //ScriptQuestion("Object " + objName + " was not found.\nAdd new object called " + objName + "?"); \\Modified to suit GMLoader
                                PrintMessage($"\nObject {objName} was not found. Adding the object...\n");
                                if (doNewObj)
                                {
                                    UndertaleGameObject gameObj = new UndertaleGameObject();
                                    gameObj.Name = Data.Strings.MakeString(objName);
                                    Data.GameObjects.Add(gameObj);
                                }
                            }
                            if (Data.GameObjects.ByName(methodNumberStr) != null)
                            {
                                // It *needs* to have a valid value, make the user specify one, silly.
                                List<uint> possibleValues = new List<uint>();
                                possibleValues.Add(uint.MaxValue);
                                ReassignGUIDs(methodNumberStr, ReduceCollisionValue(possibleValues));
                            }
                        }
                        else
                        {
                            // Let's try to get this going
                            methodNumber = (int)ReduceCollisionValue(GetCollisionValueFromCodeNameGUID(codeName));
                            ReassignGUIDs(methodNumberStr, ReduceCollisionValue(GetCollisionValueFromCodeNameGUID(codeName)));
                        }
                    }
                }
                UndertaleGameObject obj = Data.GameObjects.ByName(objName);
                if (obj == null)
                {
                    bool doNewObj = true; //ScriptQuestion("Object " + objName + " was not found.\nAdd new object called " + objName + "?"); //Modified to suit GMLoader
                    PrintMessage($"\nObject {objName} was not found. Adding the object...\n");
                    if (doNewObj)
                    {
                        UndertaleGameObject gameObj = new UndertaleGameObject();
                        gameObj.Name = Data.Strings.MakeString(objName);
                        Data.GameObjects.Add(gameObj);
                    }
                    else
                    {
                        skipPortions = true;
                    }
                }

                if (!(skipPortions))
                {
                    obj = Data.GameObjects.ByName(objName);
                    int eventIdx = Convert.ToInt32(Enum.Parse(typeof(EventType), methodName));
                    bool duplicate = false;
                    try
                    {
                        foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                        {
                            foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                            {
                                if (action.CodeId?.Name?.Content == codeName)
                                    duplicate = true;
                            }
                        }
                    }
                    catch
                    {
                        // Something went wrong, but probably because it's trying to check something non-existent
                        // Just keep going
                    }
                    if (duplicate == false)
                    {
                        UndertalePointerList<UndertaleGameObject.Event> eventList = obj.Events[eventIdx];
                        UndertaleGameObject.EventAction action = new UndertaleGameObject.EventAction();
                        UndertaleGameObject.Event evnt = new UndertaleGameObject.Event();

                        action.ActionName = code.Name;
                        action.CodeId = code;
                        evnt.EventSubtype = (uint)methodNumber;
                        evnt.Actions.Add(action);
                        eventList.Add(evnt);
                    }
                }
            }
        }
        SafeImport(codeName, gmlCode, isGML, destroyASM, checkDecompiler, throwOnError);
    }

    public static void ImportCodeFromFile(string file, bool isGML = true, bool doParse = true, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
    {
        try
        {
            if (!Path.GetFileName(file).ToLower().EndsWith(isGML ? ".gml" : ".asm"))
                return;
            if (Path.GetFileName(file).ToLower().EndsWith("cleanup_0" + (isGML ? ".gml" : ".asm")) && (Data.GeneralInfo.Major < 2))
                return;
            if (Path.GetFileName(file).ToLower().EndsWith("precreate_0" + (isGML ? ".gml" : ".asm")) && (Data.GeneralInfo.Major < 2))
                return;
            string codeName = Path.GetFileNameWithoutExtension(file);
            string gmlCode = File.ReadAllText(file);
            ImportCode(codeName, gmlCode, isGML, doParse, destroyASM, checkDecompiler, throwOnError);
        }
        catch (ScriptException exc) when (throwOnError && exc.Message == "*codeImportError*")
        {
            throw new ScriptException("Code files importation stopped because of error(s).");
        }
        catch (Exception exc)
        {
            if (!checkDecompiler)
            {
                PrintMessage("Import" + (isGML ? "GML" : "ASM") + "File error! Send the following error to Grossley#2869 (Discord) and make an issue on Github:\n\n" + exc); //Console.Error.WriteLine("Import" + (isGML ? "GML" : "ASM") + "File error! Send the following error to Grossley#2869 (Discord) and make an issue on Github:\n\n" + exc); \\Modified to suit GMLoader

                if (throwOnError)
                    throw new ScriptException("Code files importation stopped because of error(s).");
            }
            else
                throw new Exception("Error!");
        }
    }

    public static void ImportASMFile(string fileName, bool doParse = true, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
    {
        ImportCodeFromFile(fileName, false, doParse, destroyASM, checkDecompiler, throwOnError);
    }

    public static void ImportGMLFile(string fileName, bool doParse = true, bool checkDecompiler = false, bool throwOnError = false)
    {
        ImportCodeFromFile(fileName, true, doParse, true, checkDecompiler, throwOnError);
    }

    public static void SafeImport(string codeName, string gmlCode, bool isGML, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
    {
        UndertaleCode code = Data.Code.ByName(codeName);
        if (code?.ParentEntry is not null)
            return;

        try
        {
            if (isGML)
            {
                code.ReplaceGML(gmlCode, Data);
            }
            else
            {
                var instructions = Assembler.Assemble(gmlCode, Data);
                code.Replace(instructions);
                /*
                if (destroyASM)                 //Modified to suit gmloader
                    NukeProfileGML(codeName);   //Currently not used by utmt cli
                */
            }
        }
        catch (Exception ex)
        {
            if (!checkDecompiler)
            {
                string errorText = $"Code import error at {(isGML ? "GML" : "ASM")} code \"{codeName}\":\n\n{ex.Message}";
                PrintMessage(errorText);    //Console.Error.WriteLine(errorText); //Modified to suit GMLoader

                if (throwOnError)
                    throw new ScriptException("*codeImportError*");
            }
            else
            {
                code.ReplaceGML("", Data);
            }
        }
    }

    public static void ReassignGUIDs(string guid, uint objectIndex)
    {
        int eventIdx = Convert.ToInt32(EventType.Collision);
        for (var i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject obj = Data.GameObjects[i];
            try
            {
                foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                {
                    foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                    {
                        if (action.CodeId.Name.Content.Contains(guid))
                        {
                            evnt.EventSubtype = objectIndex;
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore, some values can be null along the way
            }
        }
    }

    public static uint ReduceCollisionValue(List<uint> possibleValues)
    {
        if (possibleValues.Count == 1)
        {
            if (possibleValues[0] != uint.MaxValue)
                return possibleValues[0];

            // Nothing found, pick new one
            bool objFound = false;
            uint objIndex = 0;
            while (!objFound)
            {
                string objectIndex = SimpleTextInput("Object could not be found. Please enter it below:",
                    "Object enter box.", "", false).ToLower();
                for (var i = 0; i < Data.GameObjects.Count; i++)
                {
                    if (Data.GameObjects[i].Name.Content.ToLower() == objectIndex)
                    {
                        objFound = true;
                        objIndex = (uint)i;
                    }
                }
            }
            return objIndex;
        }

        if (possibleValues.Count != 0)
        {
            // 2 or more possible values, make a list to choose from

            string gameObjectNames = "";
            foreach (uint objID in possibleValues)
                gameObjectNames += Data.GameObjects[(int)objID].Name.Content + "\n";

            bool objFound = false;
            uint objIndex = 0;
            while (!objFound)
            {
                string objectIndex = SimpleTextInput("Multiple objects were found. Select only one object below from the set, or, if none below match, some other object name:",
                    "Object enter box.", gameObjectNames, true).ToLower();
                for (var i = 0; i < Data.GameObjects.Count; i++)
                {
                    if (Data.GameObjects[i].Name.Content.ToLower() == objectIndex)
                    {
                        objFound = true;
                        objIndex = (uint)i;
                    }
                }
            }
            return objIndex;
        }

        return 0;
    }

    public static string GetGUIDFromCodeName(string codeName)
    {
        string afterPrefix = codeName.Substring(11);
        if (afterPrefix.LastIndexOf("_Collision_") != -1)
        {
            string s2 = "_Collision_";
            return afterPrefix.Substring(afterPrefix.LastIndexOf("_Collision_") + s2.Length, afterPrefix.Length - (afterPrefix.LastIndexOf("_Collision_") + s2.Length));
        }
        else
            return "Invalid";
    }

    public static List<uint> GetCollisionValueFromGUID(string guid)
    {
        int eventIdx = Convert.ToInt32(EventType.Collision);
        List<uint> possibleValues = new List<uint>();
        for (var i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject obj = Data.GameObjects[i];
            try
            {
                foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                {
                    foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                    {
                        if (action.CodeId.Name.Content.Contains(guid))
                        {
                            if (!possibleValues.Contains(evnt.EventSubtype))
                            {
                                possibleValues.Add(evnt.EventSubtype);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore, some values can be null along the way
            }
        }

        if (possibleValues.Count == 0)
        {
            possibleValues.Add(uint.MaxValue);
            return possibleValues;
        }
        else
        {
            return possibleValues;
        }
    }

    public static List<uint> GetCollisionValueFromCodeNameGUID(string codeName)
    {
        int eventIdx = Convert.ToInt32(EventType.Collision);
        List<uint> possibleValues = new List<uint>();
        for (var i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject obj = Data.GameObjects[i];
            try
            {
                foreach (UndertaleGameObject.Event evnt in obj.Events[eventIdx])
                {
                    foreach (UndertaleGameObject.EventAction action in evnt.Actions)
                    {
                        if (action.CodeId.Name.Content == codeName)
                        {
                            if (Data.GameObjects[(int)evnt.EventSubtype] != null)
                            {
                                possibleValues.Add(evnt.EventSubtype);
                                return possibleValues;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently ignore, some values can be null along the way
            }
        }
        possibleValues = GetCollisionValueFromGUID(GetGUIDFromCodeName(codeName));
        return possibleValues;
    }

    public static string SimpleTextInput(string title, string label, string defaultValue, bool allowMultiline, bool showDialog = true)
    {
        // default value gets ignored, as it doesn't really have a use in CLI.

        string result = "";

        Console.WriteLine("-----------------------INPUT----------------------");
        Console.WriteLine(title);
        Console.WriteLine(label + (allowMultiline ? " (Multiline, hit SHIFT+ENTER to insert newline)" : ""));
        Console.WriteLine("--------------------------------------------------");

        if (!allowMultiline)
        {
            result = Console.ReadLine();
        }
        else
        {
            bool isEnterWithoutShiftPressed = false;
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
                //result += keyInfo.KeyChar;

                // If Enter is pressed without shift
                if (((keyInfo.Modifiers & ConsoleModifiers.Shift) == 0) && (keyInfo.Key == ConsoleKey.Enter))
                    isEnterWithoutShiftPressed = true;

                else
                {
                    // If we have Enter + any other modifier pressed, append newline. Otherwise, just the content.
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        result += "\n";
                        Console.WriteLine();
                    }
                    // If backspace, display new empty char and move one back
                    // TODO: There's some weird bug with ctrl+backspace, i'll ignore it for now.
                    // Also make some of the multiline-backspace better.
                    else if ((keyInfo.Key == ConsoleKey.Backspace) && (result.Length > 0))
                    {
                        Console.Write(' ');
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        result = result.Remove(result.Length - 1);
                    }
                    else
                        result += keyInfo.KeyChar;
                }

            } while (!isEnterWithoutShiftPressed);
        }

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("-----------------------INPUT----------------------");
        Console.WriteLine("--------------------------------------------------");

        return result;
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
        CliScriptOptions = ScriptOptions.Default
            .AddImports("UndertaleModLib", "UndertaleModLib.Models", "UndertaleModLib.Decompiler",
                "UndertaleModLib.Scripting", "UndertaleModLib.Compiler",
                "UndertaleModLib.Util", "System", "System.IO", "System.Collections.Generic",
                "System.Text.RegularExpressions", "GMLoader", "GMLoader.GMLoaderProgram", "System.Threading.Tasks", "Microsoft.Extensions.Configuration", "System.Linq", "Newtonsoft.Json.Linq", "System.Text.Json")
            .AddReferences(typeof(UndertaleObject).GetTypeInfo().Assembly,
                typeof(GMLoaderProgram).GetTypeInfo().Assembly,
                typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
                typeof(TextureWorker).GetTypeInfo().Assembly)
            // "WithEmitDebugInformation(true)" not only lets us to see a script line number which threw an exception,
            // but also provides other useful debug info when we run UMT in "Debug".
            .WithEmitDebugInformation(true);
    }

    #endregion

}
