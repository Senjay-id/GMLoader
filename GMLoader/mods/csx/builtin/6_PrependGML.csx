bool hasGML = true;

mkDir(prependGMLPath);
string[] directories = Directory.GetDirectories(prependGMLPath);

if (!compileGML)
{
    Log.Information("GML compiling is disabled, skipping the process.");
    return;
}
else if (directories.Length == 0)
{
    Log.Debug("The prependGML import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, prependGMLPath)) + " , skipping the process");
    hasGML = false;
}

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, null, defaultDecompSettings);

if (hasGML)
{
    foreach (string directory in directories)
    {
        string[] dirFiles = Directory.GetFiles(directory, "*.gml")
                                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                    .ToArray();
        
        foreach (string file in dirFiles)
        {
            Log.Information($"Prepending {Path.GetFileName(file)} from {Path.GetRelativePath(prependGMLPath, directory)} Folder");
            importGroup.QueuePrepend(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
        } 
    }
    importGroup.Import(true);
}
