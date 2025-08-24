mkDir(gmlCodePath);
string[] dirFiles = Directory.GetFiles(gmlCodePath, "*.gml")
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                             .ToArray();
string[] codeConfigDirFiles = Directory.GetFiles(gmlCodePatchPath, "*.yaml")
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                             .ToArray();

if (!compileGML)
{
    Log.Debug("GML compiling is disabled, skipping the process.");
    return;
}
else if (dirFiles.Length == 0 && codeConfigDirFiles.Length == 0)
{
    Log.Debug("The GML import folder path is empty, skipping the process");
    return;
}
else if (!dirFiles.Any(x => x.EndsWith(".gml", StringComparison.OrdinalIgnoreCase)) && !codeConfigDirFiles.Any(x => x.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)))
{
    Log.Debug("The GML import folder doesn't have any GML files, skipping the process.");
    return;
}

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, null, defaultDecompSettings)
{
    ThrowOnNoOpFindReplace = true
};

foreach (string file in dirFiles)
{
    Log.Information($"Importing {Path.GetFileName(file)}");
    string code = File.ReadAllText(file);
    string codeName = Path.GetFileNameWithoutExtension(file);
    importGroup.QueueReplace(codeName, code);
}
importGroup.Import(true);
importConfigDefinedCode(importGroup);
