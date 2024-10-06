void EnsureDataLoaded()
{
    if (Data is null)
        throw new ScriptException("No data file is currently loaded!");
}

bool hasGML = true;
bool hasCollisionGML = true;

mkDir(appendGMLPath);
string[] directories = Directory.GetDirectories(appendGMLPath);

if (!compileGML)
{
    Log.Information("GML compiling is disabled, skipping the process.");
    return;
}
else if (directories.Length == 0)
{
    Log.Debug("The appendGML import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, appendGMLPath)) + " , skipping the process");
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
            Log.Information($"Appending {Path.GetFileName(file)} from {Path.GetRelativePath(appendGMLPath, directory)} Folder");
            Data.Code.ByName(Path.GetFileNameWithoutExtension(file)).AppendGML(File.ReadAllText(file), Data);
        }
    }
}

//collision handling
mkDir(appendGMLCollisionPath);
string[] collisionDirectories = Directory.GetDirectories(appendGMLCollisionPath);

if (collisionDirectories.Length == 0)
{
    Log.Debug("The collision import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, appendGMLCollisionPath)) + " , skipping the process");
    hasCollisionGML = false;
}

if (hasCollisionGML)
{
    foreach (string directory in collisionDirectories)
    {
        string[] collisionFiles = Directory.GetFiles(directory, "*.gml");

        foreach (string file in collisionFiles)
        {
            Log.Information($"Appending {Path.GetFileName(file)} from {Path.GetRelativePath(appendGMLCollisionPath, directory)} Folder");
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