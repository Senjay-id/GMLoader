void ReassignGUIDs(string guid, uint objectIndex)
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

uint ReduceCollisionValue(List<uint> possibleValues)
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

string GetGUIDFromCodeName(string codeName)
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

List<uint> GetCollisionValueFromGUID(string guid)
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

List<uint> GetCollisionValueFromCodeNameGUID(string codeName)
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

string SimpleTextInput(string title, string label, string defaultValue, bool allowMultiline, bool showDialog = true)
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

void SafeImport(string codeName, string gmlCode, bool isGML, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
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
            string errorText = $"Code import error at {(isGML ? "GML" : "ASM")} code \"{codeName}\": {ex.Message}";
            Log.Error(errorText);    //Console.Error.WriteLine(errorText); //Modified to suit GMLoader

            if (throwOnError)
                throw new ScriptException("*codeImportError*");
        }
        else
        {
            code.ReplaceGML("", Data);
        }
    }
}

void ImportCode(string codeName, string gmlCode, bool isGML = true, bool doParse = true, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
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
                    Log.Information("Object of ID " + methodNumber.ToString() + " was not found, Adding the object.");
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
                            Log.Information($"Object {objName} was not found. Adding the object...");
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
                Log.Information($"Object {objName} was not found. Adding the object...");
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

void ImportCodeFromFile(string file, bool isGML = true, bool doParse = true, bool destroyASM = true, bool checkDecompiler = false, bool throwOnError = false)
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
            Log.Error("Import" + (isGML ? "GML" : "ASM") + "File error! Send the following error to Grossley#2869 (Discord) and make an issue on Github: " + exc); //Console.Error.WriteLine("Import" + (isGML ? "GML" : "ASM") + "File error! Send the following error to Grossley#2869 (Discord) and make an issue on Github:\n\n" + exc); \\Modified to suit GMLoader

            if (throwOnError)
                throw new ScriptException("Code files importation stopped because of error(s).");
        }
        else
            throw new Exception("Error!");
    }
}

void ImportGMLFile(string fileName, bool doParse = true, bool checkDecompiler = false, bool throwOnError = false)
{
    ImportCodeFromFile(fileName, true, doParse, true, checkDecompiler, throwOnError);
}

void EnsureDataLoaded()
{
    if (Data is null)
        throw new ScriptException("No data file is currently loaded!");
}

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
    .Build();

string compileGMLString = configuration["universal:compilegml"];
bool compileGML = bool.Parse(compileGMLString);
bool hasGML = true;
bool hasCollisionGML = true;

string modDir = "./mods/code";
mkDir(modDir);
string[] dirFiles = Directory.GetFiles(modDir, "*.gml");

if (!compileGML)
{
    Log.Debug("GML compiling is disabled, skipping the process.");
    return;
}
else if (dirFiles.Length == 0)
{
    Log.Debug("The GML import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, modDir)) + " , skipping the process");
    hasGML = false;
}
else if (!dirFiles.Any(x => x.EndsWith(".gml")))
{
    Log.Debug("The GML import folder doesn't have any GML files, skipping the process.");
    hasGML = false;
}

EnsureDataLoaded();

if (hasGML)
{
    foreach (string file in dirFiles)
    {
        Log.Information($"Importing {Path.GetFileName(file)}");
        ImportGMLFile(file, true, false, true);
    }
}

string collisionDir = "./mods/code/collision";
mkDir(collisionDir);
string[] collisionFiles = Directory.GetFiles(collisionDir, "*.gml");

if (collisionFiles.Length == 0)
{
    Log.Debug("The collision import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, collisionDir)) + " , skipping the process");
    hasCollisionGML = false;
}
else if (!collisionFiles.Any(x => x.EndsWith(".gml")))
{
    Log.Debug("The collision import folder doesn't have any GML files, skipping the process.");
    hasCollisionGML = false;
}

if (hasCollisionGML)
{
    foreach (string file in collisionFiles)
    {
        Log.Information($"Importing {Path.GetFileName(file)}");
        var filename = Path.GetFileName(file);

        int startIdx = filename.IndexOf("Object_") + "Object_".Length;
        int endIdx = filename.IndexOf("_Collision_");
        var parentObjName = filename.Substring(startIdx, endIdx - startIdx);

        startIdx = filename.IndexOf("_Collision_") + "_Collision_".Length;
        var childObjName = filename.Substring(startIdx);
        childObjName = childObjName.Substring(0, childObjName.Length - ".gml".Length);

        var parentObj = Data.GameObjects.ByName($"{parentObjName}");
        var childObjIndex = Data.GameObjects.IndexOf(Data.GameObjects.ByName($"{childObjName}"));

        parentObj.EventHandlerFor(EventType.Collision, (uint)childObjIndex, Data.Strings, Data.Code, Data.CodeLocals).ReplaceGML(File.ReadAllText(file), Data);
    }
}