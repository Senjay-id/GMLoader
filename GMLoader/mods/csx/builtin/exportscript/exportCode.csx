if (Data is null)
{
    Log.Error("Exception: No Data loaded!");
    throw new Exception();
}

string codeFolder = exportCodeOutputPath;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

int coreCount = Environment.ProcessorCount - 1;
// If you want to use all your cores just uncomment the code below
//coreCount = Environment.ProcessorCount;

//Realistically no pc should ever only have a single core
if (coreCount == 0)
    coreCount = 1;

var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism
Log.Information($"Using {coreCount} cores to dump the objects");

Directory.CreateDirectory(codeFolder);

bool exportFromCache = false;

List<UndertaleCode> toDump;
if (!exportFromCache)
{
    toDump = new();
    foreach (UndertaleCode code in Data.Code)
    {
        if (code.ParentEntry != null)
            continue;
        toDump.Add(code);
    }
}

List<string> invalidCodeNames = new List<string>();
int invalidCode = 0;

bool cacheGenerated = false;

await DumpCode();

if (invalidCode > 0)
{
    Log.Error("[Error] Failed to decompile the code below:");
    foreach (string name in invalidCodeNames)
    {
        Log.Error(name);
    }
}

Log.Information("All code has been exported to " + Path.GetFullPath(codeFolder));

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpCode()
{
    if (cacheGenerated)
    {
        await Task.Run(() => Parallel.ForEach(Data.GMLCache, DumpCachedCode));

        if (Data.GMLCacheFailed.Count > 0)
        {
            if (Data.KnownSubFunctions is null) //if we run script before opening any code
            {
                await Task.Run(() => Decompiler.BuildSubFunctionCache(Data));
            }

            await Task.Run(() => Parallel.ForEach(Data.GMLCacheFailed, options, (codeName) => DumpCode(Data.Code.ByName(codeName))));
        }
    }
    else
    {
        if (Data.KnownSubFunctions is null) //if we run script before opening any code
        {
            await Task.Run(() => Decompiler.BuildSubFunctionCache(Data));
        }

        await Task.Run(() => Parallel.ForEach(toDump, options, DumpCode));
    }
}

void DumpCode(UndertaleCode code)
{
    if (code is not null)
    {
        string path = Path.Combine(codeFolder, code.Name.Content + ".gml");
        try
        {
            File.WriteAllText(path, (code != null ? Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value) : ""));
            Log.Information($"Exported {code.Name.Content}");
        }
        catch (Exception e)
        {
            Log.Error($"[Error] Failed to decompile {code.Name.Content}");
            invalidCodeNames.Add(code.Name.Content);
            invalidCode++;
        }

    }
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(codeFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);
}