mkDir(newObjectPath);
string[] objFiles = Directory.GetFiles(newObjectPath, "*.json");

if (objFiles.Length == 0)
{
    Log.Debug("The config import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, newObjectPath)) + " , skipping the process");
    return;
}
else if (!objFiles.Any(x => x.EndsWith(".json")))
{
    Log.Debug("The config folder doesn't have any json files, skipping the process.");
    return;
}

string jsonContent;
//General Properties
string _objName;
string _objSprite;
string _objParentID;
string _objTextureMaskID;
string _objCollisionShape;

bool _objIsVisible;
bool _objIsSolid;
bool _objIsPersistent;
bool _objUsesPhysics;
bool _objIsSensor;

//Physics Properties (currently not used)
int _objDensity;
int _objRestitution;
int _objGroup;
int _objLinearDamping;
int _objAngularDamping;
int _objFriction;

bool _objIsAwake;
bool _objIsKinematic;

foreach (string file in objFiles)
{
    Log.Information($"Adding and Manipulating {Path.GetFileNameWithoutExtension(file)} properties");
    _objName = Path.GetFileNameWithoutExtension(file);

    jsonContent = File.ReadAllText(file);
    JObject jsonObject = JObject.Parse(jsonContent);

    _objSprite = (string)jsonObject["Sprite"];
    _objParentID = (string)jsonObject["Parent"];
    _objTextureMaskID = (string)jsonObject["TextureMaskID"];
    _objCollisionShape = (string)jsonObject["CollisionShape"];
    Enum.TryParse<CollisionShapeFlags>(_objCollisionShape, true, out CollisionShapeFlags result);
    _objIsVisible = (bool)jsonObject["IsVisible"];
    _objIsSolid = (bool)jsonObject["IsSolid"];
    _objIsPersistent = (bool)jsonObject["IsPersistent"];
    _objUsesPhysics = (bool)jsonObject["UsesPhysics"];
    _objIsSensor = (bool)jsonObject["IsSensor"];

    var Obj = new UndertaleGameObject();
    Data.GameObjects.Add(Obj);

    //General Properties
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

    //Physics Properties (unimplemented)

}
