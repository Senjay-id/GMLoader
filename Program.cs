#region Using Directives
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
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
                "System.Text.RegularExpressions", "GMLoader", "GMLoader.GMLoaderProgram", "System.Threading.Tasks", "Microsoft.Extensions.Configuration", "System.Linq")
            .AddReferences(typeof(UndertaleObject).GetTypeInfo().Assembly,
                typeof(GMLoaderProgram).GetTypeInfo().Assembly,
                typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
                typeof(TextureWorker).GetTypeInfo().Assembly)
            // "WithEmitDebugInformation(true)" not only lets us to see a script line number which threw an exception,
            // but also provides other useful debug info when we run UMT in "Debug".
            .WithEmitDebugInformation(true);
    }

    #endregion

    #region Experimental Room Import Method

    public static void ImportRoomFile(string filePath)
    {
        FileStream stream = File.OpenRead(filePath);
        byte[] jsonUtf8Bytes = new byte[stream.Length];

        stream.Read(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);
        stream.Close();

        JsonReaderOptions options = new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        Utf8JsonReader reader = new Utf8JsonReader(jsonUtf8Bytes, options);

        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartObject);

        ReadName(ref reader);
        ReadMainValues(ref reader);

        ClearRoomLists();

        ReadBackgrounds(ref reader);
        ReadViews(ref reader);
        ReadGameObjects(ref reader);
        ReadTiles(ref reader);
        ReadLayers(ref reader);

        // Adds room to data file, if it doesn't exist.
        if (Data.Rooms.ByName(newRoom.Name.Content) == null)
            Data.Rooms.Add(newRoom);

    }

    static void ReadMainValues(ref Utf8JsonReader reader)
    {
        string caption = ReadString(ref reader);

        newRoom.Width = (uint)ReadNum(ref reader);
        newRoom.Height = (uint)ReadNum(ref reader);
        newRoom.Speed = (uint)ReadNum(ref reader);
        newRoom.Persistent = ReadBool(ref reader);
        newRoom.BackgroundColor = (uint)(0xFF000000 | ReadNum(ref reader)); // make alpha 255 (BG color doesn't have alpha)
        newRoom.DrawBackgroundColor = ReadBool(ref reader);

        string ccIdName = ReadString(ref reader);

        newRoom.Flags = (UndertaleRoom.RoomEntryFlags)ReadNum(ref reader);
        newRoom.World = ReadBool(ref reader);
        newRoom.Top = (uint)ReadNum(ref reader);
        newRoom.Left = (uint)ReadNum(ref reader);
        newRoom.Right = (uint)ReadNum(ref reader);
        newRoom.Bottom = (uint)ReadNum(ref reader);
        newRoom.GravityX = ReadFloat(ref reader);
        newRoom.GravityY = ReadFloat(ref reader);
        newRoom.MetersPerPixel = ReadFloat(ref reader);

        newRoom.Caption = (caption == null) ? null : new UndertaleString(caption);

        if ((newRoom.Caption != null) && !Data.Strings.Any(s => s == newRoom.Caption))
            Data.Strings.Add(newRoom.Caption);

        newRoom.CreationCodeId = (ccIdName == null) ? null : Data.Code.ByName(ccIdName);
    }

    static void ReadName(ref Utf8JsonReader reader)
    {
        string name = ReadString(ref reader);
        if (name == null)
            throw new ScriptException("ERROR: Object name was null - object name must be defined!");

        if (Data.Rooms.ByName(name) != null)
        {
            newRoom = Data.Rooms.ByName(name);
        }
        else
        {
            newRoom = new UndertaleRoom();
            newRoom.Name = new UndertaleString(name);
            Data.Strings.Add(newRoom.Name);
        }
    }

    static void ClearRoomLists()
    {
        newRoom.Backgrounds.Clear();
        newRoom.Views.Clear();
        newRoom.GameObjects.Clear();
        newRoom.Tiles.Clear();
        newRoom.Layers.Clear();
    }

    static void ReadBackgrounds(ref Utf8JsonReader reader)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.Background newBg = new UndertaleRoom.Background();

                newBg.ParentRoom = newRoom;

                newBg.Enabled = ReadBool(ref reader);
                newBg.Foreground = ReadBool(ref reader);
                string bgDefName = ReadString(ref reader);
                newBg.X = (int)ReadNum(ref reader);
                newBg.Y = (int)ReadNum(ref reader);
                newBg.TiledHorizontally = ReadBool(ref reader);
                newBg.TiledVertically = ReadBool(ref reader);
                newBg.SpeedX = (int)ReadNum(ref reader);
                newBg.SpeedY = (int)ReadNum(ref reader);
                newBg.Stretch = ReadBool(ref reader);

                newBg.BackgroundDefinition = (bgDefName == null) ? null : Data.Backgrounds.ByName(bgDefName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newRoom.Backgrounds.Add(newBg);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
        }
    }

    static void ReadViews(ref Utf8JsonReader reader)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.View newView = new UndertaleRoom.View();

                newView.Enabled = ReadBool(ref reader);
                newView.ViewX = (int)ReadNum(ref reader);
                newView.ViewY = (int)ReadNum(ref reader);
                newView.ViewWidth = (int)ReadNum(ref reader);
                newView.ViewHeight = (int)ReadNum(ref reader);
                newView.PortX = (int)ReadNum(ref reader);
                newView.PortY = (int)ReadNum(ref reader);
                newView.PortWidth = (int)ReadNum(ref reader);
                newView.PortHeight = (int)ReadNum(ref reader);
                newView.BorderX = (uint)ReadNum(ref reader);
                newView.BorderY = (uint)ReadNum(ref reader);
                newView.SpeedX = (int)ReadNum(ref reader);
                newView.SpeedY = (int)ReadNum(ref reader);
                string objIdName = ReadString(ref reader);

                newView.ObjectId = (objIdName == null) ? null : Data.GameObjects.ByName(objIdName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newRoom.Views.Add(newView);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
        }
    }

    static void ReadGameObjects(ref Utf8JsonReader reader)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.GameObject newObj = new UndertaleRoom.GameObject();

                newObj.X = (int)ReadNum(ref reader);
                newObj.Y = (int)ReadNum(ref reader);

                string objDefName = ReadString(ref reader);

                newObj.InstanceID = (uint)ReadNum(ref reader);

                string ccIdName = ReadString(ref reader);

                newObj.ScaleX = ReadFloat(ref reader);
                newObj.ScaleY = ReadFloat(ref reader);
                newObj.Color = (uint)ReadNum(ref reader);
                newObj.Rotation = ReadFloat(ref reader);

                string preCcIdName = ReadString(ref reader);

                newObj.ImageSpeed = ReadFloat(ref reader);
                newObj.ImageIndex = (int)ReadNum(ref reader);

                newObj.ObjectDefinition = (objDefName == null) ? null : Data.GameObjects.ByName(objDefName);
                newObj.CreationCode = (ccIdName == null) ? null : Data.Code.ByName(ccIdName);
                newObj.PreCreateCode = (preCcIdName == null) ? null : Data.Code.ByName(preCcIdName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newRoom.GameObjects.Add(newObj);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
        }
    }

    static void ReadTiles(ref Utf8JsonReader reader)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.Tile newTile = new UndertaleRoom.Tile();

                newTile.spriteMode = ReadBool(ref reader);
                newTile.X = (int)ReadNum(ref reader);
                newTile.Y = (int)ReadNum(ref reader);

                string bgDefName = ReadString(ref reader);
                string sprDefName = ReadString(ref reader);

                newTile.SourceX = (uint)ReadNum(ref reader);
                newTile.SourceY = (uint)ReadNum(ref reader);
                newTile.Width = (uint)ReadNum(ref reader);
                newTile.Height = (uint)ReadNum(ref reader);
                newTile.TileDepth = (int)ReadNum(ref reader);
                newTile.InstanceID = (uint)ReadNum(ref reader);
                newTile.ScaleX = ReadFloat(ref reader);
                newTile.ScaleY = ReadFloat(ref reader);
                newTile.Color = (uint)ReadNum(ref reader);

                newTile.BackgroundDefinition = (bgDefName == null) ? null : Data.Backgrounds.ByName(bgDefName);
                newTile.SpriteDefinition = (sprDefName == null) ? null : Data.Sprites.ByName(sprDefName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newRoom.Tiles.Add(newTile);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
        }
    }

    static void ReadLayers(ref Utf8JsonReader reader)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.Layer newLayer = new UndertaleRoom.Layer();

                string layerName = ReadString(ref reader);
                newLayer.ParentRoom = newRoom;

                newLayer.LayerId = (uint)ReadNum(ref reader);
                newLayer.LayerType = (UndertaleRoom.LayerType)ReadNum(ref reader);
                newLayer.LayerDepth = (int)ReadNum(ref reader);
                newLayer.XOffset = ReadFloat(ref reader);
                newLayer.YOffset = ReadFloat(ref reader);
                newLayer.HSpeed = ReadFloat(ref reader);
                newLayer.VSpeed = ReadFloat(ref reader);
                newLayer.IsVisible = ReadBool(ref reader);


                newLayer.LayerName = (layerName == null) ? null : new UndertaleString(layerName);

                if ((layerName != null) && !Data.Strings.Any(s => s == newLayer.LayerName))
                    Data.Strings.Add(newLayer.LayerName);

                switch (newLayer.LayerType)
                {
                    case UndertaleRoom.LayerType.Background:
                        ReadBackgroundLayer(ref reader, newLayer);
                        break;
                    case UndertaleRoom.LayerType.Instances:
                        ReadInstancesLayer(ref reader, newLayer);
                        break;
                    case UndertaleRoom.LayerType.Assets:
                        ReadAssetsLayer(ref reader, newLayer);
                        break;
                    case UndertaleRoom.LayerType.Tiles:
                        ReadTilesLayer(ref reader, newLayer);
                        break;
                    default:
                        throw new ScriptException("ERROR: Invalid value for layer data type.");
                }

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newRoom.Layers.Add(newLayer);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
        }
    }

    static void ReadBackgroundLayer(ref Utf8JsonReader reader, UndertaleRoom.Layer newLayer)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartObject);

        UndertaleRoom.Layer.LayerBackgroundData newLayerData = new UndertaleRoom.Layer.LayerBackgroundData();

        newLayerData.Visible = ReadBool(ref reader);
        newLayerData.Foreground = ReadBool(ref reader);
        newLayerData.ParentLayer = newLayer;

        string spriteName = ReadString(ref reader);

        newLayerData.TiledHorizontally = ReadBool(ref reader);
        newLayerData.TiledVertically = ReadBool(ref reader);
        newLayerData.Stretch = ReadBool(ref reader);
        newLayerData.Color = (uint)ReadNum(ref reader);
        newLayerData.FirstFrame = ReadFloat(ref reader);
        newLayerData.AnimationSpeed = ReadFloat(ref reader);
        newLayerData.AnimationSpeedType = (AnimationSpeedType)ReadNum(ref reader);

        try
        {
            if (newLayerData != null)
            {
                newLayerData.Sprite = (spriteName == null) ? null : Data.Sprites.ByName(spriteName);
            }
        }
        catch (NullReferenceException ex)
        {
            throw new ScriptException(ex.Message + "\nspriteName: " + spriteName + "\nData.Sprites.ByName(spriteName).Name.Content: " + Data.Sprites.ByName(spriteName).Name.Content + "\nnewLayerData.Visible.ToString(): " + newLayerData.Visible.ToString());
        }

        ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

        newLayer.Data = newLayerData;
    }

    static void ReadInstancesLayer(ref Utf8JsonReader reader, UndertaleRoom.Layer newLayer)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartObject);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                continue;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new ScriptException("ERROR: Did not correctly stop reading instances layer");

            UndertaleRoom.Layer.LayerInstancesData newLayerData = new UndertaleRoom.Layer.LayerInstancesData();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                    continue;

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    UndertaleRoom.GameObject newObj = new UndertaleRoom.GameObject();

                    newObj.X = (int)ReadNum(ref reader);
                    newObj.Y = (int)ReadNum(ref reader);

                    string objDefName = ReadString(ref reader);

                    newObj.InstanceID = (uint)ReadNum(ref reader);

                    string ccIdName = ReadString(ref reader);

                    newObj.ScaleX = ReadFloat(ref reader);
                    newObj.ScaleY = ReadFloat(ref reader);
                    newObj.Color = (uint)ReadNum(ref reader);
                    newObj.Rotation = ReadFloat(ref reader);

                    string preCcIdName = ReadString(ref reader);

                    newObj.ImageSpeed = ReadFloat(ref reader);
                    newObj.ImageIndex = (int)ReadNum(ref reader);

                    newObj.ObjectDefinition = (objDefName == null) ? null : Data.GameObjects.ByName(objDefName);

                    newObj.CreationCode = (ccIdName == null) ? null : Data.Code.ByName(ccIdName);

                    newObj.PreCreateCode = (preCcIdName == null) ? null : Data.Code.ByName(preCcIdName);

                    ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                    newLayerData.Instances.Add(newObj);
                    continue;
                }

                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                throw new ScriptException("ERROR: Did not correctly stop reading instances in instance layer");
            }

            ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

            newLayer.Data = newLayerData;

            return;

        }
    }

    static void ReadAssetsLayer(ref Utf8JsonReader reader, UndertaleRoom.Layer newLayer)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartObject);
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        UndertaleRoom.Layer.LayerAssetsData newLayerData = new UndertaleRoom.Layer.LayerAssetsData();

        newLayerData.LegacyTiles = new UndertalePointerList<UndertaleRoom.Tile>();
        newLayerData.Sprites = new UndertalePointerList<UndertaleRoom.SpriteInstance>();
        newLayerData.Sequences = new UndertalePointerList<UndertaleRoom.SequenceInstance>();
        newLayerData.NineSlices = new UndertalePointerList<UndertaleRoom.SpriteInstance>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.Tile newTile = new UndertaleRoom.Tile();

                newTile.spriteMode = ReadBool(ref reader);
                newTile.X = (int)ReadNum(ref reader);
                newTile.Y = (int)ReadNum(ref reader);

                string bgDefName = ReadString(ref reader);
                string sprDefName = ReadString(ref reader);

                newTile.SourceX = (uint)ReadNum(ref reader);
                newTile.SourceY = (uint)ReadNum(ref reader);
                newTile.Width = (uint)ReadNum(ref reader);
                newTile.Height = (uint)ReadNum(ref reader);
                newTile.TileDepth = (int)ReadNum(ref reader);
                newTile.InstanceID = (uint)ReadNum(ref reader);
                newTile.ScaleX = ReadFloat(ref reader);
                newTile.ScaleY = ReadFloat(ref reader);
                newTile.Color = (uint)ReadNum(ref reader);

                newTile.BackgroundDefinition = (bgDefName == null) ? null : Data.Backgrounds.ByName(bgDefName);

                newTile.SpriteDefinition = (sprDefName == null) ? null : Data.Sprites.ByName(sprDefName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newLayerData.LegacyTiles.Add(newTile);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
        }

        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                continue;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.SpriteInstance newSpr = new UndertaleRoom.SpriteInstance();

                string name = ReadString(ref reader);
                string spriteName = ReadString(ref reader);

                newSpr.X = (int)ReadNum(ref reader);
                newSpr.Y = (int)ReadNum(ref reader);
                newSpr.ScaleX = ReadFloat(ref reader);
                newSpr.ScaleY = ReadFloat(ref reader);
                newSpr.Color = (uint)ReadNum(ref reader);
                newSpr.AnimationSpeed = ReadFloat(ref reader);
                newSpr.AnimationSpeedType = (AnimationSpeedType)ReadNum(ref reader);
                newSpr.FrameIndex = ReadFloat(ref reader);
                newSpr.Rotation = ReadFloat(ref reader);

                newSpr.Name = (name == null) ? null : new UndertaleString(name);

                if ((name != null) && !Data.Strings.Any(s => s == newSpr.Name))
                    Data.Strings.Add(newSpr.Name);

                newSpr.Sprite = (spriteName == null) ? null : Data.Sprites.ByName(spriteName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newLayerData.Sprites.Add(newSpr);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException("ERROR: Did not correctly stop reading instances in instance layer");
        }

        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                continue;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.SequenceInstance newSeq = new UndertaleRoom.SequenceInstance();

                string name = ReadString(ref reader);
                string sequenceName = ReadString(ref reader);

                newSeq.X = (int)ReadNum(ref reader);
                newSeq.Y = (int)ReadNum(ref reader);
                newSeq.ScaleX = ReadFloat(ref reader);
                newSeq.ScaleY = ReadFloat(ref reader);
                newSeq.Color = (uint)ReadNum(ref reader);
                newSeq.AnimationSpeed = ReadFloat(ref reader);
                newSeq.AnimationSpeedType = (AnimationSpeedType)ReadNum(ref reader);
                newSeq.FrameIndex = ReadFloat(ref reader);
                newSeq.Rotation = ReadFloat(ref reader);


                newSeq.Name = (name == null) ? null : new UndertaleString(name);

                if ((name != null) && !Data.Strings.Any(s => s == newSeq.Name))
                    Data.Strings.Add(newSeq.Name);

                newSeq.Sequence = (sequenceName == null) ? null : Data.Sequences.ByName(sequenceName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newLayerData.Sequences.Add(newSeq);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException("ERROR: Did not correctly stop reading instances in instance layer");
        }

        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                continue;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                UndertaleRoom.SpriteInstance newSpr = new UndertaleRoom.SpriteInstance();

                string name = ReadString(ref reader);
                string spriteName = ReadString(ref reader);

                newSpr.X = (int)ReadNum(ref reader);
                newSpr.Y = (int)ReadNum(ref reader);
                newSpr.ScaleX = ReadFloat(ref reader);
                newSpr.ScaleY = ReadFloat(ref reader);
                newSpr.Color = (uint)ReadNum(ref reader);
                newSpr.AnimationSpeed = ReadFloat(ref reader);
                newSpr.AnimationSpeedType = (AnimationSpeedType)ReadNum(ref reader);
                newSpr.FrameIndex = ReadFloat(ref reader);
                newSpr.Rotation = ReadFloat(ref reader);

                newSpr.Name = (name == null) ? null : new UndertaleString(name);

                if ((name != null) && !Data.Strings.Any(s => s == newSpr.Name))
                    Data.Strings.Add(newSpr.Name);

                newSpr.Sprite = spriteName == null ? null : Data.Sprites.ByName(spriteName);

                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

                newLayerData.NineSlices.Add(newSpr);
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            throw new ScriptException("ERROR: Did not correctly stop reading instances in instance layer");
        }

        newLayer.Data = newLayerData;
        ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);
    }

    static void ReadTilesLayer(ref Utf8JsonReader reader, UndertaleRoom.Layer newLayer)
    {
        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartObject);
        UndertaleRoom.Layer.LayerTilesData newLayerData = new UndertaleRoom.Layer.LayerTilesData();

        string backgroundName = ReadString(ref reader);

        newLayerData.TilesX = (uint)ReadNum(ref reader);
        newLayerData.TilesY = (uint)ReadNum(ref reader);

        newLayerData.Background = (backgroundName == null) ? null : Data.Backgrounds.ByName(backgroundName);

        uint[][] tileIds = new uint[newLayerData.TilesY][];
        for (int i = 0; i < newLayerData.TilesY; i++)
        {
            tileIds[i] = new uint[newLayerData.TilesX];
        }

        ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
        for (int y = 0; y < newLayerData.TilesY; y++)
        {
            ReadAnticipateJSONObject(ref reader, JsonTokenType.StartArray);
            for (int x = 0; x < newLayerData.TilesX; x++)
            {
                ReadAnticipateJSONObject(ref reader, JsonTokenType.StartObject);
                (tileIds[y])[x] = (uint)ReadNum(ref reader);
                ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);
            }

            ReadAnticipateJSONObject(ref reader, JsonTokenType.EndArray);
        }

        newLayerData.TileData = tileIds;
        ReadAnticipateJSONObject(ref reader, JsonTokenType.EndArray);
        ReadAnticipateJSONObject(ref reader, JsonTokenType.EndObject);

        newLayer.Data = newLayerData;
    }

    // Read tokens of specified type

    static bool ReadBool(ref Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName: continue;
                case JsonTokenType.True: return true;
                case JsonTokenType.False: return false;
                default: throw new ScriptException($"ERROR: Unexpected token type. Expected Boolean - found {reader.TokenType}");
            }
        }

        throw new ScriptException("ERROR: Did not find value of expected type. Expected Boolean.");
    }

    static long ReadNum(ref Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName: continue;
                case JsonTokenType.Number: return reader.GetInt64();
                default: throw new ScriptException($"ERROR: Unexpected token type. Expected Integer - found {reader.TokenType}");
            }
        }

        throw new ScriptException("ERROR: Did not find value of expected type. Expected Integer.");
    }

    static float ReadFloat(ref Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName: continue;
                case JsonTokenType.Number: return reader.GetSingle();
                default: throw new ScriptException($"ERROR: Unexpected token type. Expected Decimal - found {reader.TokenType}");
            }
        }

        throw new ScriptException("ERROR: Did not find value of expected type. Expected Decimal.");
    }

    static string ReadString(ref Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName: continue;
                case JsonTokenType.String: return reader.GetString();
                case JsonTokenType.Null: return null;
                default: throw new ScriptException($"ERROR: Unexpected token type. Expected String - found {reader.TokenType}");
            }
        }

        throw new ScriptException("ERROR: Did not find value of expected type. Expected String.");
    }

    // Watch for certain meta-tokens

    static void ReadAnticipateJSONObject(ref Utf8JsonReader reader, JsonTokenType allowedTokenType)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
                continue;
            if (reader.TokenType == allowedTokenType)
                return;
            throw new ScriptException($"ERROR: Unexpected token type. Expected {allowedTokenType} - found {reader.TokenType}");
        }

        throw new ScriptException("ERROR: Did not find value of expected type. Expected String.");
    }

    #endregion

}
