bool hasGML = true;

mkDir(appendGMLPath);
string[] directories = Directory.GetDirectories(appendGMLPath)
                               .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                               .ToArray();

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
            Log.Information($"Appending {Path.GetFileName(file)} from {Path.GetRelativePath(appendGMLPath, directory)} Folder");
            importGroup.QueueAppend(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
        }
    }
    importGroup.Import(true);
}