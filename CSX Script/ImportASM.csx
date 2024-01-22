IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddIniFile("GMLoader.ini", optional: false, reloadOnChange: false)
    .Build();

string compileASMString = configuration["universal:compileasm"];
bool compileASM = bool.Parse(compileASMString);

string modDir = "./mods/code/asm";
CreateDirectoryIfNotExists(modDir);
string[] dirFiles = Directory.GetFiles(modDir, "*.asm");

if (!compileASM)
{
    PrintMessage("ASM compiling is disabled, skipping the process.");
    return;
}
else if (dirFiles.Length == 0)
{
    PrintMessage("The ASM import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, modDir)) + " , skipping the process");
    return;
}
else if (!dirFiles.Any(x => x.EndsWith(".asm")))
{
    PrintMessage("The ASM import folder path doesn't have any ASM files, skipping the process.");
    return;
}

EnsureDataLoaded();

foreach (string file in dirFiles)
{
    PrintMessage($"Importing {Path.GetFileName(file)}");
    ImportASMFile(file, true, false, false, true);
}

