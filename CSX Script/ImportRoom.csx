IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
    .Build();

string compileRoomString = configuration["universal:compileroom"];
bool compileRoom = bool.Parse(compileRoomString);

string modDir = "./mods/rooms";
CreateDirectoryIfNotExists(modDir);
string[] dirFiles = Directory.GetFiles(modDir, "*.json");

if (!compileRoom)
{
    PrintMessage("Room compiling is disabled, skipping the process.");
    return;
}
else if (dirFiles.Length == 0)
{
    PrintMessage("The Room import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, modDir)) + " , skipping the process");
    return;
}
else if (!dirFiles.Any(x => x.EndsWith(".json")))
{
    PrintMessage("The Room import folder doesn't have any ROOM files, skipping the process.");
    return;
}

EnsureDataLoaded();

foreach (string file in dirFiles)
{
    PrintMessage($"Importing {Path.GetFileName(file)}");
    ImportRoomFile(file);
}

