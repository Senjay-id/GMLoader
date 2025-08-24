string codeFolder = exportCodeOutputPath;
Directory.CreateDirectory(codeFolder);

var globalDecompileContext = new GlobalDecompileContext(Data);
var decompilerSettings = Data.ToolInfo.DecompilerSettings;
List<UndertaleCode> toDump = Data.Code.Where(c => c.ParentEntry is null).ToList();

int coreCount = CalculateOptimalCoreCount();

Log.Information($"Using {coreCount} cores to dump code to: {Path.GetFullPath(codeFolder)}");

DumpCode();

Log.Information($"All code has been exported to {Path.GetFullPath(codeFolder)}");

int CalculateOptimalCoreCount()
{
    int availableCores = Environment.ProcessorCount;
    
    // Reserve one core for system operations, but ensure at least 1 core is used
    return Math.Max(1, availableCores - 1);
    
    // this can be used alternatively
    // return availableCores;
}

void DumpCode()
{
    var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = coreCount };
    Parallel.ForEach(toDump, parallelOptions, DumpSingleCode);
}

void DumpSingleCode(UndertaleCode code)
{
    if (code is null)
    {
        Log.Warning("Encountered null code entry");
        return;
    }

    string fileName = $"{code.Name.Content}.gml";
    string filePath = Path.Combine(codeFolder, fileName);
    
    Log.Information($"Exporting: {code.Name.Content}");
    
    try
    {
        string decompiledCode = DecompileCode(code);
        File.WriteAllText(filePath, decompiledCode);
    }
    catch (Exception ex)
    {
        HandleDecompilationError(filePath, code.Name.Content, ex);
    }
}

string DecompileCode(UndertaleCode code)
{
    if (code is null)
        return string.Empty;
    
    var context = new Underanalyzer.Decompiler.DecompileContext(
        globalDecompileContext, code, decompilerSettings);
    
    return context.DecompileToString();
}

void HandleDecompilationError(string filePath, string codeName, Exception exception)
{
    Log.Error($"Decompilation failed for {codeName}: {exception.Message}");
    
    string errorContent = $"/*\nDECOMPILATION FAILED: {codeName}\n\n" +
                         $"Error: {exception.Message}\n\n" +
                         $"Stack Trace:\n{exception.StackTrace}\n*/";
    
    try
    {
        File.WriteAllText(filePath, errorContent);
    }
    catch (Exception fileEx)
    {
        Log.Error($"Failed to write error file for {codeName}: {fileEx.Message}");
    }
}