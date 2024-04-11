IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
    .Build();

string compileGMLString = configuration["universal:compilegml"];
bool compileGML = bool.Parse(compileGMLString);

string modDir = "./mods/code";
CreateDirectoryIfNotExists(modDir);
string[] dirFiles = Directory.GetFiles(modDir, "*.gml");

if (!compileGML)
{
    PrintMessage("GML compiling is disabled, skipping the process.");
    return;
}
else if (dirFiles.Length == 0)
{
    PrintMessage("The GML import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, modDir)) + " , skipping the process");
    return;
}
else if (!dirFiles.Any(x => x.EndsWith(".gml")))
{
    PrintMessage("The GML import folder doesn't have any GML files, skipping the process.");
    return;
}

EnsureDataLoaded();

foreach (string file in dirFiles)
{
    PrintMessage($"Importing {Path.GetFileName(file)}");
    ImportGMLFile(file, true, false, true);
}





