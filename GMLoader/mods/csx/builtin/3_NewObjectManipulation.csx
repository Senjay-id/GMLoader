mkDir(newObjectPath);
string[] objFiles = Directory.GetFiles(newObjectPath, "*.json");

if (objFiles.Length == 0) {
    Log.Debug("The config import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, newObjectPath)) + " , skipping the process");
    return;
}
else if (!objFiles.Any(x => x.EndsWith(".json"))) {
    Log.Debug("The config folder doesn't have any json files, skipping the process.");
    return;
}

HashSet<string> importedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
HashSet<string> loadingStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

void ImportGameObject(string file)
{
    if (importedFiles.Contains(file))
        return;

    string _objName = Path.GetFileNameWithoutExtension(file);

    if (loadingStack.Contains(file))
        throw new ScriptException($"ERROR: Circular parent dependency detected involving '{_objName}'.");

    loadingStack.Add(file);

    string jsonContent = File.ReadAllText(file);
    JObject jsonObject = JObject.Parse(jsonContent);

    string _objParentID = (string)jsonObject["Parent"];

    if (!string.IsNullOrEmpty(_objParentID) && Data.GameObjects.ByName(_objParentID) == null)
    {
        string parentFilePath = Path.Combine(newObjectPath, $"{_objParentID}.json");
        if (File.Exists(parentFilePath))
        {
            Log.Debug($"Parent '{_objParentID}' not found yet. Importing parent first...");
            ImportGameObject(parentFilePath);
        }
    }

    Log.Information($"Adding {_objName}");

    string _objSprite = (string)jsonObject["Sprite"];
    string _objTextureMaskID = (string)jsonObject["TextureMaskID"];
    string _objCollisionShape = (string)jsonObject["CollisionShape"];
    Enum.TryParse<CollisionShapeFlags>(_objCollisionShape, true, out CollisionShapeFlags result);
    
    bool _objIsVisible = (bool)jsonObject["IsVisible"];
    bool _objIsSolid = (bool)jsonObject["IsSolid"];
    bool _objIsPersistent = (bool)jsonObject["IsPersistent"];
    bool _objUsesPhysics = (bool)jsonObject["UsesPhysics"];
    bool _objIsSensor = (bool)jsonObject["IsSensor"];

    // Physics Properties (currently not used)
    // int _objDensity = (int)jsonObject["Density"];
    // int _objRestitution = (int)jsonObject["Restitution"];
    // int _objGroup = (int)jsonObject["Group"];
    // int _objLinearDamping = (int)jsonObject["LinearDamping"];
    // int _objAngularDamping = (int)jsonObject["AngularDamping"];
    // int _objFriction = (int)jsonObject["Friction"];
    // bool _objIsAwake = (bool)jsonObject["IsAwake"];
    // bool _objIsKinematic = (bool)jsonObject["IsKinematic"];

    var Obj = new UndertaleGameObject();
    Data.GameObjects.Add(Obj);

    Obj.Name = Data.Strings.MakeString(_objName);
    Obj.Sprite = Data.Sprites.ByName(_objSprite);
    Obj.ParentId = Data.GameObjects.ByName(_objParentID);
    Obj.TextureMaskId = Data.Sprites.ByName(_objTextureMaskID);
    Obj.CollisionShape = result;
    Obj.Visible = _objIsVisible;
    Obj.Solid = _objIsSolid;
    Obj.Persistent = _objIsPersistent;
    Obj.UsesPhysics = _objUsesPhysics;
    Obj.IsSensor = _objIsSensor;

    if (Obj == Obj.ParentId)
        throw new ScriptException($"ERROR: {Obj.Name.Content} has its parent set to itself.");

    loadingStack.Remove(file);
    importedFiles.Add(file);
}

foreach (string file in objFiles)
{
    ImportGameObject(file);
}