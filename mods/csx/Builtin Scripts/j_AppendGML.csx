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

string modDir = "./mods/code/appendgml";
CreateDirectoryIfNotExists(modDir);
string[] directories = Directory.GetDirectories(modDir);

if (!compileGML)
{
    PrintMessage("GML compiling is disabled, skipping the process.");
    return;
}
else if (directories.Length == 0)
{
    PrintMessage("The appendGML import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, modDir)) + " , skipping the process");
    hasGML = false;
}

EnsureDataLoaded();

if (hasGML)
{
    foreach (string directory in directories)
    {
        string[] dirFiles = Directory.GetFiles(directory, "*.gml");

        foreach (string file in dirFiles)
        {
            PrintMessage($"Appending {Path.GetFileName(file)} from {Path.GetRelativePath(modDir, directory)} Folder");
            Data.Code.ByName(Path.GetFileNameWithoutExtension(file)).AppendGML(File.ReadAllText(file), Data);
        }
    }
}

//collision handling
string collisionDir = "./mods/code/appendgml/collision";
CreateDirectoryIfNotExists(collisionDir);
string[] collisionDirectories = Directory.GetDirectories(collisionDir);

if (collisionDirectories.Length == 0)
{
    PrintMessage("The collision import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, collisionDir)) + " , skipping the process");
    hasCollisionGML = false;
}

if (hasCollisionGML)
{
    foreach (string directory in collisionDirectories)
    {
        string[] collisionFiles = Directory.GetFiles(directory, "*.gml");

        foreach (string file in collisionFiles)
        {
            PrintMessage($"Appending {Path.GetFileName(file)} from {Path.GetRelativePath(collisionDir, directory)} Folder");
            var filename = Path.GetFileName(file);

            int startIdx = filename.IndexOf("Object_") + "Object_".Length;
            int endIdx = filename.IndexOf("_Collision_");
            var parentObjName = filename.Substring(startIdx, endIdx - startIdx);

            startIdx = filename.IndexOf("_Collision_") + "_Collision_".Length;
            var childObjName = filename.Substring(startIdx);
            childObjName = childObjName.Substring(0, childObjName.Length - ".gml".Length);

            var parentObj = Data.GameObjects.ByName($"{parentObjName}");
            var childObjIndex = Data.GameObjects.IndexOf(Data.GameObjects.ByName($"{childObjName}"));

            parentObj.EventHandlerFor(EventType.Collision, (uint)childObjIndex, Data.Strings, Data.Code, Data.CodeLocals).AppendGML(File.ReadAllText(file), Data);
        }
    }
}