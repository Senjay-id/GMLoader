if (Data is null)
{
    Log.Error("Exception: No Data loaded!");
    throw new Exception();
}

string outputFolder = exportGameObjectOutputPath;

int coreCount = Environment.ProcessorCount - 1;
// If you want to use all your cores just uncomment the code below
//coreCount = Environment.ProcessorCount;

//Realistically no pc should ever only have a single core
if (coreCount == 0)
    coreCount = 1;

var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism
Log.Information($"Using {coreCount} cores to dump the objects");

Directory.CreateDirectory(exportGameObjectOutputPath);

Parallel.ForEach(Data.GameObjects, options, dumpObject);

void dumpObject(UndertaleGameObject obj)
{
    var objData = new
    {
        Sprite = obj.Sprite?.Name.Content,
        Parent = obj.ParentId?.Name.Content,
        TextureMaskID = obj.TextureMaskId?.Name.Content,
        CollisionShape = obj.CollisionShape.ToString(),
        IsVisible = obj.Visible,
        IsSolid = obj.Solid,
        IsPersistent = obj.Persistent,
        UsesPhysics = obj.UsesPhysics,
        IsSensor = obj.IsSensor
    };

    string json = JsonConvert.SerializeObject(objData, Formatting.Indented);
    string fileName = Path.Combine(outputFolder, $"{obj.Name.Content}.json");

    File.WriteAllText(fileName, json);
    Log.Information($"Dumped {obj.Name} to {fileName}");
}