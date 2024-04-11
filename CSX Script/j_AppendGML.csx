IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
    .Build();

string compileGMLString = configuration["universal:compilegml"];
bool compileGML = bool.Parse(compileGMLString);

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
    return;
}

EnsureDataLoaded();

foreach (string directory in directories)
{
    string[] dirFiles = Directory.GetFiles(directory, "*.gml");

    foreach (string file in dirFiles)
    {
        PrintMessage($"Appending {Path.GetFileName(file)} from {Path.GetRelativePath(modDir, directory)} Folder");
        Data.Code.ByName(Path.GetFileNameWithoutExtension(file)).AppendGML(File.ReadAllText(file), Data);
    }
}





