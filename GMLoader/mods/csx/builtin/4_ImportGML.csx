

mkDir(gmlCodePath);
string[] dirFiles = Directory.GetFiles(gmlCodePath, "*.gml");

if (!compileGML)
{
    Log.Debug("GML compiling is disabled, skipping the process.");
    return;
}
else if (dirFiles.Length == 0)
{
    Log.Debug("The GML import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, gmlCodePath)) + " , skipping the process");
    return;
}
else if (!dirFiles.Any(x => x.EndsWith(".gml", StringComparison.OrdinalIgnoreCase)))
{
    Log.Debug("The GML import folder doesn't have any GML files, skipping the process.");
    return;
}

await Task.Run(() =>
{
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data);

    foreach (string file in dirFiles)
    {
        Log.Information($"Importing {Path.GetFileName(file)}");
        string code = File.ReadAllText(file);
        string codeName = Path.GetFileNameWithoutExtension(file);
        importGroup.QueueReplace(codeName, code);
    }
    
    importGroup.Import(true);
});

/*
mkDir(collisionPath);
string[] collisionFiles = Directory.GetFiles(collisionPath, "*.gml");

if (collisionFiles.Length == 0)
{
    Log.Debug("The collision import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, collisionPath)) + " , skipping the process");
    hasCollisionGML = false;
}
else if (!collisionFiles.Any(x => x.EndsWith(".gml")))
{
    Log.Debug("The collision import folder doesn't have any GML files, skipping the process.");
    hasCollisionGML = false;
}


await Task.Run(() =>
{
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

            //parentObj.EventHandlerFor(EventType.Collision, (uint)childObjIndex, Data.Strings, Data.Code, Data.CodeLocals).ReplaceGML(File.ReadAllText(file), Data);
            var obj = parentObj.EventHandlerFor(EventType.Collision, (uint)childObjIndex, Data);
            var code = File.ReadAllText(file);

            importGroup.QueueReplace(obj, code);
        }
        importGroup.Import(true);
    }
});
*/