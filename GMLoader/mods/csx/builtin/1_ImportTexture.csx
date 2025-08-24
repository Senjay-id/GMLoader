// ImportGraphics but it can also set sprite properties and import more types of files.

// Based off of ImportGraphics.csx by the UTMT team
// and ImportGraphicsWithParameters.csx by @DonavinDraws
// ImportGraphicsAdvanced-specific edits (extra formats and animation speed) made by CST1229

// Texture packer by Samuel Roy
// Uses code from https://github.com/mfascia/TexturePacker

// revision 2: fixed gif import not working unless the folder was named Sprites,
// fixed the default origin being Top Center instead of Top Left and
// reworded Is special type?'s boolean and the background import error message
// revision 3: added optional support for single-frame sprites if a frame number is not specified
// revision 4: added support for the texture handling refactor
// revision 5: handle breaking Magick.NET changes, disabled animation speed options in GMS1 games, hi-DPI support,
// renamed from ImportGraphicsWithParametersPlus to ImportGraphicsAdvanced
// revision 6: sprite texture items are now cropped, to save on texture page space and to fix sprite fonts

// Modified for GMLoader

static bool importAsSprite = true;
static bool importFrameless = false;
public static int frame = 1;
public static string spriteName;

int xCoordinate;
int yCoordinate;
uint speedType;
float frameSpeed;
uint boundingBoxType;
uint sepMaskType;
int leftCollision;
int rightCollision;
int bottomCollision;
int topCollision;
bool transparent= false;
bool smooth= false;
bool preload= false;
bool isSpecial = Data.IsGameMaker2();
uint specialVer = defaultSpriteSpecialVer;

bool bgTransparent;
bool bgSmooth;
bool bgPreload;
uint tileWidth;
uint tileHeight;
uint borderX;
uint borderY;
uint tileColumn;
uint itemOrFramePerTile;
uint tileCount;
int frameTime;

static List<MagickImage> imagesToCleanup = new();

HashSet<string> spritesStartAt1 = new HashSet<string>();

int coreCount = Environment.ProcessorCount - 1;
// If you want to use all your cores just uncomment the code below
//coreCount = Environment.ProcessorCount;

//Realistically no pc should ever only have a single core
if (coreCount == 0)
	coreCount = 1;

var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism

string importFolder = texturesPath;

CheckDuplicates();

string[] dirFiles = Directory.GetFiles(importFolder, "*.*", SearchOption.AllDirectories);
if (dirFiles.Length == 0)
{
    Log.Debug("The texture import folder path is empty. At " + Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importFolder)) + " , skipping the process");
    return;
}
else if (!dirFiles.Any(x => x.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)))
{
    Log.Debug("The texture import folder doesn't have any texture files, skipping the process.");
    return;
}

string packDir = Path.GetFullPath(Path.Combine("mods", "logs", "TexturePackagerLogs"));
Directory.CreateDirectory(packDir);

bool noMasksForBasicRectangles = Data.IsVersionAtLeast(2022, 9); // TODO: figure out the exact version, but this is pretty close

try
{
    string sourcePath = importFolder;
    string outName = Path.Combine(packDir, "atlas.txt");
    int textureSize = 2048;
    int PaddingValue = 2;
    bool debug = false;
    Packer packer = new Packer();
    packer.Process(sourcePath, textureSize, PaddingValue, debug);
    packer.SaveAtlasses(outName);

    int lastTextPage = Data.EmbeddedTextures.Count - 1;
    int lastTextPageItem = Data.TexturePageItems.Count - 1;

    bool bboxMasks = Data.IsVersionAtLeast(2024, 6);
    Dictionary<UndertaleSprite, Node> maskNodes = new();

    // Import everything into UTMT
    string prefix = outName.Replace(Path.GetExtension(outName), "");
    int atlasCount = 0;

    // Record the vanilla sprites
    Parallel.ForEach(spriteList, options, spriteName =>
    {
        UndertaleSprite spr = Data.Sprites.ByName(spriteName);
        if (spr != null)
        {
            vanillaSpriteList.Add(spriteName);
        }
    });

    //OffsetResult();
    foreach (Atlas atlas in packer.Atlasses)
    {
        string atlasName = Path.Combine(packDir, $"{prefix}{atlasCount:000}.png");
        using MagickImage atlasImage = TextureWorker.ReadBGRAImageFromFile(atlasName);
        IPixelCollection<byte> atlasPixels = atlasImage.GetPixels();

        UndertaleEmbeddedTexture texture = new();
        texture.Name = new UndertaleString($"Texture {++lastTextPage}");
        texture.TextureData.Image = GMImage.FromMagickImage(atlasImage).ConvertToPng(); // TODO: other formats?
        Data.EmbeddedTextures.Add(texture);
        foreach (Node n in atlas.Nodes)
        {
            if (n.Texture != null)
            {
                // Initalize values of this texture
                UndertaleTexturePageItem texturePageItem = new UndertaleTexturePageItem();
                texturePageItem.Name = new UndertaleString("PageItem " + ++lastTextPageItem);
                texturePageItem.SourceX = (ushort)n.Bounds.X;
                texturePageItem.SourceY = (ushort)n.Bounds.Y;
                texturePageItem.SourceWidth = (ushort)n.Bounds.Width;
                texturePageItem.SourceHeight = (ushort)n.Bounds.Height;
                texturePageItem.TargetX = (ushort)n.Texture.TargetX;
                texturePageItem.TargetY = (ushort)n.Texture.TargetY;
                texturePageItem.TargetWidth = (ushort)n.Bounds.Width;
                texturePageItem.TargetHeight = (ushort)n.Bounds.Height;
                texturePageItem.BoundingWidth = (ushort)n.Texture.BoundingWidth;
                texturePageItem.BoundingHeight = (ushort)n.Texture.BoundingHeight;
                texturePageItem.TexturePage = texture;

                // Add this texture to UMT
                Data.TexturePageItems.Add(texturePageItem);

                // String processing
                string stripped = Path.GetFileNameWithoutExtension(n.Texture.Source);
                bool isSubimages = n.Texture.Source.IndexOf("subimages", StringComparison.OrdinalIgnoreCase) >= 0;

                SpriteType spriteType = GetSpriteType(n.Texture.Source);

                if (importAsSprite)
                {
                    if ((spriteType == SpriteType.Unknown) || (spriteType == SpriteType.Font))
                    {
                        spriteType = SpriteType.Sprite;
                    }
                }

                if (spriteType == SpriteType.Background)
                {
                    string bgSpriteName = stripped;
                    
                    if (backgroundDictionary.TryGetValue(bgSpriteName, out BackgroundData bgProps))
                        {
                            bgTransparent = bgProps.yml_transparent.Value;
                            bgSmooth = bgProps.yml_smooth.Value;
                            bgPreload = bgProps.yml_preload.Value;
                            tileWidth = bgProps.yml_tile_width.Value;
                            tileHeight = bgProps.yml_tile_height.Value;
                            borderX = bgProps.yml_border_x.Value;
                            borderY = bgProps.yml_border_y.Value;
                            tileColumn = bgProps.yml_tile_column.Value;
                            itemOrFramePerTile = bgProps.yml_item_per_tile.Value;
                            tileCount = bgProps.yml_tile_count.Value;
                            //frameTime = bgProps.yml_frametime.Value;
                            //Log.Information($"{bgSpriteName} tile_count: {bgProps.yml_tile_count.Value} tile_width: {bgProps.yml_tile_width.Value}");
                        }
                    else
                    {
                        Log.Information($"No sprite properties found for {bgSpriteName}, using default .ini settings");
                    }

                    UndertaleBackground background = Data.Backgrounds.ByName(stripped);

                    if (background != null)
                    {
                        background.Transparent = bgTransparent;
                        background.Smooth = bgSmooth;
                        background.Preload = bgPreload;
                        background.GMS2TileWidth = tileWidth;
                        background.GMS2TileHeight = tileHeight;
                        background.GMS2OutputBorderX = borderX;
                        background.GMS2OutputBorderY = borderY;
                        background.GMS2TileColumns = tileColumn;
                        background.GMS2ItemsPerTileCount = itemOrFramePerTile;
                        background.GMS2TileCount = tileCount;
                        background.GMS2FrameLength = frameTime;
                        background.Texture = texturePageItem;
                    }
                    else
                    {
                        // No background found, let's make one
                        UndertaleString backgroundUTString = Data.Strings.MakeString(bgSpriteName);
                        UndertaleBackground newBackground = new UndertaleBackground();
                        newBackground.Name = backgroundUTString;
                        newBackground.Transparent = bgTransparent;
                        newBackground.Smooth = bgSmooth;
                        newBackground.Preload = bgPreload;
                        newBackground.GMS2TileWidth = tileWidth;
                        newBackground.GMS2TileHeight = tileHeight;
                        newBackground.GMS2OutputBorderX = borderX;
                        newBackground.GMS2OutputBorderY = borderY;
                        newBackground.GMS2TileColumns = tileColumn;
                        newBackground.GMS2ItemsPerTileCount = itemOrFramePerTile;
                        newBackground.GMS2TileCount = tileCount;
                        newBackground.GMS2FrameLength = frameTime;
                        newBackground.Texture = texturePageItem;
                        for (uint i = 0; i < tileCount; i++)
                        {
                            newBackground.GMS2TileIds.Add(new UndertaleBackground.TileID
                            {
                                ID = (uint)i
                            });
                        }
                        Data.Backgrounds.Add(newBackground);
                    }
                }
                else if (spriteType == SpriteType.Sprite)
                {
                    // Get sprite to add this texture to
                    string spriteName;
                    int lastUnderscore = stripped.LastIndexOf('_');
				    int frame = 0;

                    spriteName = stripped.Substring(0, lastUnderscore);
                    if (Int32.TryParse(stripped.Substring(lastUnderscore + 1), out int parsedFrame))
                    {
                        frame = parsedFrame;
                    }
                    else
                    {
                        spriteName = stripped;
                        frame = 0;
                        Log.Debug("Image " + stripped + " has an invalid frame number, assigning the frame to 0");
                    }

                    if (isSubimages)
                    {
                        // the sprite is intended to be imported as subimages, only show debug.
                        Log.Debug($"{spriteName} will be imported using default .ini settings");
                        spriteName = Path.GetFileName(Path.GetDirectoryName(n.Texture.Source));
                    }
                    else if (spriteDictionary.TryGetValue(spriteName, out SpriteData spriteProps))
                    {
                        xCoordinate = spriteProps.yml_x.Value;
                        yCoordinate = spriteProps.yml_y.Value;
                        speedType = spriteProps.yml_speedtype.Value;
                        frameSpeed = spriteProps.yml_framespeed.Value;
                        boundingBoxType = spriteProps.yml_boundingboxtype.Value;
                        leftCollision = spriteProps.yml_bboxleft.Value;
                        rightCollision = spriteProps.yml_bboxright.Value;
                        bottomCollision = spriteProps.yml_bboxbottom.Value;
                        topCollision = spriteProps.yml_bboxtop.Value;
                        transparent = spriteProps.yml_transparent.Value;
                        smooth = spriteProps.yml_smooth.Value;
                        preload = spriteProps.yml_preload.Value;
                        //Log.Information($"{spriteName} x: {spriteProps.yml_x.Value} y: {spriteProps.yml_y.Value}");
                    }
                    else
                    {
                        Log.Information($"No sprite properties found for {spriteName}, using default .ini settings");
                    }

                    if (spritesStartAt1.Contains(spriteName))
                    {
                        frame--;
                    }

                    // Create TextureEntry object
                    UndertaleSprite.TextureEntry texentry = new UndertaleSprite.TextureEntry();
                    texentry.Texture = texturePageItem;

                    // Set values for new sprites
                    UndertaleSprite sprite = Data.Sprites.ByName(spriteName);
                    if (sprite is null)
                    {
                        UndertaleString spriteUTString = Data.Strings.MakeString(spriteName);
                        UndertaleSprite newSprite = new UndertaleSprite();
                        newSprite.Name = spriteUTString;
                        newSprite.Width = (uint)n.Texture.BoundingWidth;
                        newSprite.Height = (uint)n.Texture.BoundingHeight;
                        newSprite.MarginLeft = n.Texture.TargetX;
                        newSprite.MarginRight = n.Texture.TargetX + n.Bounds.Width - 1;
                        newSprite.MarginTop = n.Texture.TargetY;
                        newSprite.MarginBottom = n.Texture.TargetY + n.Bounds.Height - 1;                        
                        newSprite.OriginX = xCoordinate;
					    newSprite.OriginY = yCoordinate;
                        newSprite.BBoxMode = boundingBoxType;
                        if (boundingBoxType == 2)
                        {
                            newSprite.MarginLeft = leftCollision;
                            newSprite.MarginRight = rightCollision;
                            newSprite.MarginTop = topCollision;
                            newSprite.MarginBottom = bottomCollision;
                        }                        
                        newSprite.GMS2PlaybackSpeedType = (AnimSpeedType)speedType;
                        newSprite.GMS2PlaybackSpeed = frameSpeed;
                        newSprite.IsSpecialType = isSpecial;
                        newSprite.SVersion = specialVer;
                        newSprite.SepMasks = (UndertaleSprite.SepMaskType)sepMaskType;
                        newSprite.Transparent = transparent;
                        newSprite.Smooth = smooth;
                        newSprite.Preload = preload;
                        if (frame > 0)
                        {
                            for (int i = 0; i < frame; i++)
                                newSprite.Textures.Add(null);
                        }

                        // Only generate collision masks for sprites that need them (in newer GameMaker versions)
                        if (!noMasksForBasicRectangles ||
                            newSprite.SepMasks is not (UndertaleSprite.SepMaskType.AxisAlignedRect or UndertaleSprite.SepMaskType.RotatedRect))
                        {
                            // Generate mask later (when the current atlas is about to be unloaded)
                            maskNodes.Add(newSprite, n);
                        }

                        newSprite.Textures.Add(texentry);
                        Data.Sprites.Add(newSprite);
                        continue;
                    }

                // Replacing existing sprite part

                    if (isSubimages)
                    {
                        int textureCount = sprite.Textures.Count;
                        frame = ExtractSecondToLastNumber(stripped) ?? 0;
                    }
                    else if (frame > sprite.Textures.Count - 1)
                    {
                        while (frame > sprite.Textures.Count - 1)
                        {
                            sprite.Textures.Add(texentry);
                        }
                        continue;
                    }

                    if (frame > sprite.Textures.Count - 1)
                    {
                        sprite.Textures.Add(texentry);
                    }

                    sprite.Textures[frame] = texentry;

                    sprite.OriginX = xCoordinate;
                    sprite.OriginY = yCoordinate;
                    sprite.BBoxMode = boundingBoxType;

                    if (boundingBoxType == 2)
                    {
                        sprite.MarginLeft = leftCollision;
                        sprite.MarginRight = rightCollision;
                        sprite.MarginTop = topCollision;
                        sprite.MarginBottom = bottomCollision;
                    }

                    sprite.GMS2PlaybackSpeedType = (AnimSpeedType)speedType;
                    sprite.GMS2PlaybackSpeed = frameSpeed;
                    sprite.IsSpecialType = isSpecial;
                    sprite.SVersion = specialVer;
                    sprite.SepMasks = (UndertaleSprite.SepMaskType)sepMaskType;
                    sprite.Transparent = transparent;
                    sprite.Smooth = smooth;
                    sprite.Preload = preload;

                    // Update sprite dimensions
                    uint oldWidth = sprite.Width, oldHeight = sprite.Height;
                    sprite.Width = (uint)n.Texture.BoundingWidth;
                    sprite.Height = (uint)n.Texture.BoundingHeight;
                    bool changedSpriteDimensions = (oldWidth != sprite.Width || oldHeight != sprite.Height);

                    // Grow bounding box depending on how much is trimmed
                    bool grewBoundingBox = false;
                    bool fullImageBbox = sprite.BBoxMode == 1;
                    bool manualBbox = sprite.BBoxMode == 2;

                    if (!manualBbox)
                    {
                        int marginLeft = fullImageBbox ? 0 : n.Texture.TargetX;
                        int marginRight = fullImageBbox ? ((int)sprite.Width - 1) : (n.Texture.TargetX + n.Bounds.Width - 1);
                        int marginTop = fullImageBbox ? 0 : n.Texture.TargetY;
                        int marginBottom = fullImageBbox ? ((int)sprite.Height - 1) : (n.Texture.TargetY + n.Bounds.Height - 1);

                        if (marginLeft < sprite.MarginLeft)
                        {
                            sprite.MarginLeft = marginLeft;
                            grewBoundingBox = true;
                        }
                        if (marginTop < sprite.MarginTop)
                        {
                            sprite.MarginTop = marginTop;
                            grewBoundingBox = true;
                        }
                        if (marginRight > sprite.MarginRight)
                        {
                            sprite.MarginRight = marginRight;
                            grewBoundingBox = true;
                        }
                        if (marginBottom > sprite.MarginBottom)
                        {
                            sprite.MarginBottom = marginBottom;
                            grewBoundingBox = true;
                        }                  
                    }
                
                    // Only generate collision masks for sprites that need them (in newer GameMaker versions)
                    if (!noMasksForBasicRectangles || 
                        sprite.SepMasks is not (UndertaleSprite.SepMaskType.AxisAlignedRect or UndertaleSprite.SepMaskType.RotatedRect) || 
                        sprite.CollisionMasks.Count > 0)
                    {
                        if ((bboxMasks && grewBoundingBox) || (sprite.SepMasks is UndertaleSprite.SepMaskType.Precise && sprite.CollisionMasks.Count == 0) || (!bboxMasks && changedSpriteDimensions))
                        {
                            // Use this node for the sprite's collision mask if the bounding box grew (or if no collision mask exists for a precise sprite)
                            maskNodes[sprite] = n;
                        }
                    }
                }
            }
        }

        // Update masks for when bounding box masks are enabled
        foreach ((UndertaleSprite maskSpr, Node maskNode) in maskNodes)
        {
            // Generate collision mask using either bounding box or sprite dimensions
            maskSpr.CollisionMasks.Clear();
            maskSpr.CollisionMasks.Add(maskSpr.NewMaskEntry(Data));
            (int maskWidth, int maskHeight) = maskSpr.CalculateMaskDimensions(Data);
            int maskStride = ((maskWidth + 7) / 8) * 8;

            BitArray maskingBitArray = new BitArray(maskStride * maskHeight);
            for (int y = 0; y < maskHeight && y < maskNode.Bounds.Height; y++)
            {
                for (int x = 0; x < maskWidth && x < maskNode.Bounds.Width; x++)
                {
                    IMagickColor<byte> pixelColor = atlasPixels.GetPixel(x + maskNode.Bounds.X, y + maskNode.Bounds.Y).ToColor();
                    if (bboxMasks)
                    {
                        maskingBitArray[(y * maskStride) + x] = (pixelColor.A > 0);
                    }
                    else
                    {
                        maskingBitArray[((y + maskNode.Texture.TargetY) * maskStride) + x + maskNode.Texture.TargetX] = (pixelColor.A > 0);
                    }
                }
            }
            BitArray tempBitArray = new BitArray(maskingBitArray.Length);
            for (int i = 0; i < maskingBitArray.Length; i += 8)
            {
                for (int j = 0; j < 8; j++)
                {
                    tempBitArray[j + i] = maskingBitArray[-(j - 7) + i];
                }
            }

            int numBytes = maskingBitArray.Length / 8;
            byte[] bytes = new byte[numBytes];
            tempBitArray.CopyTo(bytes, 0);
            for (int i = 0; i < bytes.Length; i++)
                maskSpr.CollisionMasks[0].Data[i] = bytes[i];
        }
        maskNodes.Clear();
        
        // Increment atlas
        atlasCount++;
    }

    // Remove the excess vanilla sprites
    Log.Information("Removing excess vanilla sprite...");
    Parallel.ForEach(vanillaSpriteList, options, spriteName =>
    {
        UndertaleSprite spr = Data.Sprites.ByName(spriteName);
        if (spr != null)
        {
            int moddedCount = moddedTextureCounts.TryGetValue(spriteName, out var _count) ? _count : 0;
            while (spr.Textures.Count > moddedCount)
            {
                Log.Debug($"Removing frame {spr.Textures.Count} for {spriteName}");
                spr.Textures.RemoveAt(spr.Textures.Count - 1); // Remove the last frame
            }
        }
        else
            Log.Warning($"{spriteName} isn't found in the data");
    });

    Log.Information("Done!");
}
finally
{
    foreach (MagickImage img in imagesToCleanup)
    {
        img.Dispose();
    }
}

public class TextureInfo
{
    public string Source;
    public int Width;
    public int Height;
    public int TargetX;
    public int TargetY;
    public int BoundingWidth;
    public int BoundingHeight;
    public MagickImage Image;
}

public enum SpriteType
{
    Sprite,
    Background,
    Font,
    Unknown
}

public enum SplitType
{
    Horizontal,
    Vertical,
}

public enum BestFitHeuristic
{
    Area,
    MaxOneAxis,
}

public struct Rect
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Node
{
    public Rect Bounds;
    public TextureInfo Texture;
    public SplitType SplitType;
}

public class Atlas
{
    public int Width;
    public int Height;
    public List<Node> Nodes;
}

public class Packer
{
    public List<TextureInfo> SourceTextures;
    public StringWriter LogWriter;
    public StringWriter Error;
    public int Padding;
    public int AtlasSize;
    public bool DebugMode;
    public BestFitHeuristic FitHeuristic;
    public List<Atlas> Atlasses;
    public HashSet<string> Sources;

    public Packer()
    {
        SourceTextures = new List<TextureInfo>();
        LogWriter = new StringWriter();
        Error = new StringWriter();
    }

    public void Process(string _SourceDir, int _AtlasSize, int _Padding, bool _DebugMode)
    {
        Padding = _Padding;
        AtlasSize = _AtlasSize;
        DebugMode = _DebugMode;
        //1: scan for all the textures we need to pack
        Sources = new HashSet<string>();
        ScanForTextures(_SourceDir);
        List<TextureInfo> textures = new List<TextureInfo>();
        textures = SourceTextures.ToList();
        //2: generate as many atlasses as needed (with the latest one as small as possible)
        Atlasses = new List<Atlas>();
        while (textures.Count > 0)
        {
            Atlas atlas = new Atlas();
            atlas.Width = _AtlasSize;
            atlas.Height = _AtlasSize;
            List<TextureInfo> leftovers = LayoutAtlas(textures, atlas);
            if (leftovers.Count == 0)
            {
                // we reached the last atlas. Check if this last atlas could have been twice smaller
                while (leftovers.Count == 0)
                {
                    atlas.Width /= 2;
                    atlas.Height /= 2;
                    leftovers = LayoutAtlas(textures, atlas);
                }
                // we need to go 1 step larger as we found the first size that is too small
                // if the atlas is 0x0 then it should be 1x1 instead
                if (atlas.Width == 0)
                {
                    atlas.Width = 1;
                } else
                {
                    atlas.Width *= 2;
                }
                if (atlas.Height == 0)
                {
                    atlas.Height = 1;
                }
                else
                {
                    atlas.Height *= 2;
                }
                leftovers = LayoutAtlas(textures, atlas);
            }
            Atlasses.Add(atlas);
            textures = leftovers;
        }
    }

    public void SaveAtlasses(string _Destination)
    {
        int atlasCount = 0;
        string prefix = _Destination.Replace(Path.GetExtension(_Destination), "");
        string descFile = _Destination;
        StreamWriter tw = new StreamWriter(_Destination);
        tw.WriteLine("source_tex, atlas_tex, x, y, width, height");
        foreach (Atlas atlas in Atlasses)
        {
            string atlasName = $"{prefix}{atlasCount:000}.png";

            // 1: Save images
            using (MagickImage img = CreateAtlasImage(atlas))
                TextureWorker.SaveImageToFile(img, atlasName);

            // 2: save description in file
            foreach (Node n in atlas.Nodes)
            {
                if (n.Texture != null)
                {
                    tw.Write(n.Texture.Source + ", ");
                    tw.Write(atlasName + ", ");
                    tw.Write((n.Bounds.X).ToString() + ", ");
                    tw.Write((n.Bounds.Y).ToString() + ", ");
                    tw.Write((n.Bounds.Width).ToString() + ", ");
                    tw.WriteLine((n.Bounds.Height).ToString());
                }
            }
            ++atlasCount;
        }
        tw.Close();
        tw = new StreamWriter(prefix + ".log");
        tw.WriteLine("--- LOG -------------------------------------------");
        tw.WriteLine(LogWriter.ToString());
        tw.WriteLine("--- ERROR -----------------------------------------");
        tw.WriteLine(Error.ToString());
        tw.Close();
    }

    private void ScanForTextures(string _Path)
    {
        DirectoryInfo di = new DirectoryInfo(_Path);
        //FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);

        // need to be sorted because of nostrip style
        FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories)
            .OrderBy(f => Regex.Replace(f.Name, @"\d+", m => m.Value.PadLeft(10, '0')))
            .ToArray();
        // Filtered files should be used but I'm not enforcing sprite to have config files
        //FileInfo[] filteredFiles = files.Where(file => 
            //spritesToImport.Contains(file.Name, StringComparer.OrdinalIgnoreCase)).ToArray();
        string fullName;
        string nostripStr = "nostrip";

        foreach (FileInfo fi in files)
        {
            fullName = fi.FullName;
            if (Path.GetExtension(fullName) == ".yaml") continue;
            Log.Information($"Importing {Path.GetFileName(fullName)}");
            SpriteType spriteType = GetSpriteType(fullName);
            string ext = Path.GetExtension(fullName);

            bool isSprite = spriteType == SpriteType.Sprite || (spriteType == SpriteType.Unknown && importAsSprite);

            // fix me later maybe
            if (ext == ".gif")
            {
                // animated .gif
                string dirName = Path.GetDirectoryName(fullName);
                string spriteName = Path.GetFileNameWithoutExtension(fullName);

                MagickReadSettings settings = new()
                {
                    ColorSpace = ColorSpace.sRGB,
                };
                using MagickImageCollection gif = new(fullName, settings);
                int frames = gif.Count;
                if (!isSprite && frames > 1)
                {
                    throw new ScriptException(fullName + " is a " + spriteType + ", but has more than 1 frame. Script has been stopped.");
                }

                for (int i = frames - 1; i >= 0; i--)
                {
                    AddSource(
                        (MagickImage)gif[i],
                        Path.Join(
                            dirName,
                            isSprite ?
                                (spriteName + "_" + i + ".png") : (spriteName + ".png")
                        )
                    );
                    // don't auto-dispose
                    gif.RemoveAt(i);
                }
            }
            else if (ext == ".png")
            {
                int frames = 1;
                string spriteName = Path.GetFileNameWithoutExtension(fi.Name);
                DirectoryInfo grandparentDir = fi.Directory?.Parent;
                if (Path.GetFileName(grandparentDir.FullName) != nostripStr)
                {
                    if (spriteDictionary.TryGetValue(spriteName, out SpriteData spriteProps))
                    {
                        //Log.Error($"Properties found for {spriteName}");
                        frames = spriteProps.yml_frame.Value;
                    }
                    else
                    {
                        Log.Debug($"No sprite properties found for {spriteName}, default values will be used.");
                    }

                    if (frames <= 0)
                    {
                        Log.Error(fullName + " has 0 frames.");
                        throw new Exception();
                    }

                    if (!isSprite && frames > 1)
                    {
                        Log.Error(fullName + " is not a sprite, but has more than 1 frame.");
                        throw new Exception();
                    }

                    MagickReadSettings settings = new()
                    {
                        ColorSpace = ColorSpace.sRGB,
                    };
                    using MagickImage img = new(fullName, settings);
                    if ((img.Width % frames) > 0)
                    {
                        throw new ScriptException(fullName + " has a width not divisible by the number of frames. Script has been stopped.");
                    }

                    string dirName = Path.GetDirectoryName(fullName);

                    uint frameWidth = (uint)img.Width / (uint)frames;
                    uint frameHeight = (uint)img.Height;
                    for (uint i = 0; i < frames; i++)
                    {
                        AddSource(
                            (MagickImage)img.Clone(
                                (int)(frameWidth * i), 0, frameWidth, frameHeight
                            ),
                            Path.Join(dirName,
                                isSprite ?
                                    (spriteName + "_" + i + ".png") : (spriteName + ".png")
                            )
                        );
                    }
                }
                else
                {
                    // Handle nostrip style sprites
                    MagickReadSettings settings = new()
                    {
                        ColorSpace = ColorSpace.sRGB,
                    };
                    MagickImage img = new(fullName);
                    AddSource(img, fullName);
                }
            }
        }
    }

    private void AddSource(MagickImage img, string fullName)
    {
        imagesToCleanup.Add(img);
        if (img.Width <= AtlasSize && img.Height <= AtlasSize)
        {
            TextureInfo ti = new TextureInfo();

            if (!Sources.Add(fullName))
            {
                Log.Error(
                    Path.GetFileNameWithoutExtension(fullName) +
                    " as a frame already exists (possibly due to having multiple types of sprite images named the same). Script has been stopped."
                );
            }

            ti.Source = fullName;
            ti.BoundingWidth = (int)img.Width;
            ti.BoundingHeight = (int)img.Height;

            // GameMaker doesn't trim tilesets. I assume it didn't trim backgrounds too
            ti.TargetX = 0;
            ti.TargetY = 0;
            
            if (GetSpriteType(ti.Source) != SpriteType.Background)
            {
                img.BorderColor = MagickColors.Transparent;
                img.BackgroundColor = MagickColors.Transparent;
                img.Border(1);
                IMagickGeometry? bbox = img.BoundingBox;
                if (bbox is not null)
                {
                    ti.TargetX = bbox.X - 1;
                    ti.TargetY = bbox.Y - 1;
                    // yes, .Trim() mutates the image...
                    // it doesn't really matter though since it isn't written back or anything
                    img.Trim();
                }
                else
                {
                    // Empty sprites should be 1x1
                    ti.TargetX = 0;
                    ti.TargetY = 0;
                    img.Crop(1, 1);
                }
                img.ResetPage();
            }
            
            ti.Width = (int)img.Width;
            ti.Height = (int)img.Height;
            ti.Image = img;

            SourceTextures.Add(ti);

            LogWriter.WriteLine("Added " + fullName);
        }
        else
        {
            Error.WriteLine(fullName + " is too large to fix in the atlas. Skipping!");
        }
    }

    private void HorizontalSplit(Node _ToSplit, int _Width, int _Height, List<Node> _List)
    {
        Node n1 = new Node();
        n1.Bounds.X = _ToSplit.Bounds.X + _Width + Padding;
        n1.Bounds.Y = _ToSplit.Bounds.Y;
        n1.Bounds.Width = _ToSplit.Bounds.Width - _Width - Padding;
        n1.Bounds.Height = _Height;
        n1.SplitType = SplitType.Vertical;
        Node n2 = new Node();
        n2.Bounds.X = _ToSplit.Bounds.X;
        n2.Bounds.Y = _ToSplit.Bounds.Y + _Height + Padding;
        n2.Bounds.Width = _ToSplit.Bounds.Width;
        n2.Bounds.Height = _ToSplit.Bounds.Height - _Height - Padding;
        n2.SplitType = SplitType.Horizontal;
        if (n1.Bounds.Width > 0 && n1.Bounds.Height > 0)
            _List.Add(n1);
        if (n2.Bounds.Width > 0 && n2.Bounds.Height > 0)
            _List.Add(n2);
    }

    private void VerticalSplit(Node _ToSplit, int _Width, int _Height, List<Node> _List)
    {
        Node n1 = new Node();
        n1.Bounds.X = _ToSplit.Bounds.X + _Width + Padding;
        n1.Bounds.Y = _ToSplit.Bounds.Y;
        n1.Bounds.Width = _ToSplit.Bounds.Width - _Width - Padding;
        n1.Bounds.Height = _ToSplit.Bounds.Height;
        n1.SplitType = SplitType.Vertical;
        Node n2 = new Node();
        n2.Bounds.X = _ToSplit.Bounds.X;
        n2.Bounds.Y = _ToSplit.Bounds.Y + _Height + Padding;
        n2.Bounds.Width = _Width;
        n2.Bounds.Height = _ToSplit.Bounds.Height - _Height - Padding;
        n2.SplitType = SplitType.Horizontal;
        if (n1.Bounds.Width > 0 && n1.Bounds.Height > 0)
            _List.Add(n1);
        if (n2.Bounds.Width > 0 && n2.Bounds.Height > 0)
            _List.Add(n2);
    }

    private TextureInfo FindBestFitForNode(Node _Node, List<TextureInfo> _Textures)
    {
        TextureInfo bestFit = null;
        float nodeArea = _Node.Bounds.Width * _Node.Bounds.Height;
        float maxCriteria = 0.0f;
        foreach (TextureInfo ti in _Textures)
        {
            switch (FitHeuristic)
            {
                // Max of Width and Height ratios
                case BestFitHeuristic.MaxOneAxis:
                    if (ti.Width <= _Node.Bounds.Width && ti.Height <= _Node.Bounds.Height)
                    {
                        float wRatio = (float)ti.Width / (float)_Node.Bounds.Width;
                        float hRatio = (float)ti.Height / (float)_Node.Bounds.Height;
                        float ratio = wRatio > hRatio ? wRatio : hRatio;
                        if (ratio > maxCriteria)
                        {
                            maxCriteria = ratio;
                            bestFit = ti;
                        }
                    }
                    break;
                // Maximize Area coverage
                case BestFitHeuristic.Area:
                    if (ti.Width <= _Node.Bounds.Width && ti.Height <= _Node.Bounds.Height)
                    {
                        float textureArea = ti.Width * ti.Height;
                        float coverage = textureArea / nodeArea;
                        if (coverage > maxCriteria)
                        {
                            maxCriteria = coverage;
                            bestFit = ti;
                        }
                    }
                    break;
            }
        }
        return bestFit;
    }

    private List<TextureInfo> LayoutAtlas(List<TextureInfo> _Textures, Atlas _Atlas)
    {
        List<Node> freeList = new List<Node>();
        List<TextureInfo> textures = new List<TextureInfo>();
        _Atlas.Nodes = new List<Node>();
        textures = _Textures.ToList();
        Node root = new Node();
        root.Bounds.Width = _Atlas.Width;
        root.Bounds.Height = _Atlas.Height;
        root.SplitType = SplitType.Horizontal;
        freeList.Add(root);
        while (freeList.Count > 0 && textures.Count > 0)
        {
            Node node = freeList[0];
            freeList.RemoveAt(0);
            TextureInfo bestFit = FindBestFitForNode(node, textures);
            if (bestFit != null)
            {
                if (node.SplitType == SplitType.Horizontal)
                {
                    HorizontalSplit(node, bestFit.Width, bestFit.Height, freeList);
                }
                else
                {
                    VerticalSplit(node, bestFit.Width, bestFit.Height, freeList);
                }
                node.Texture = bestFit;
                node.Bounds.Width = bestFit.Width;
                node.Bounds.Height = bestFit.Height;
                textures.Remove(bestFit);
            }
            _Atlas.Nodes.Add(node);
        }
        return textures;
    }

    private MagickImage CreateAtlasImage(Atlas _Atlas)
    {
        MagickImage img = new(MagickColors.Transparent, (uint)_Atlas.Width, (uint)_Atlas.Height);
        foreach (Node n in _Atlas.Nodes)
        {
            if (n.Texture is not null)
            {
                using IMagickImage<byte> resizedSourceImg = TextureWorker.ResizeImage(n.Texture.Image, n.Bounds.Width, n.Bounds.Height);
                img.Composite(resizedSourceImg, n.Bounds.X, n.Bounds.Y, CompositeOperator.Copy);
            }
        }
        return img;
    }
}

public static SpriteType GetSpriteType(string path)
{
    string folderPath = Path.GetDirectoryName(path);
    string folderName = new DirectoryInfo(folderPath).Name;
    string lowerName = folderName.ToLower();

    if (lowerName == "backgrounds" || lowerName == "background")
    {
        return SpriteType.Background;
    }
    else if (lowerName == "fonts" || lowerName == "font")
    {
        return SpriteType.Font;
    }
    else if (lowerName == "sprites" || lowerName == "sprite")
    {
        return SpriteType.Sprite;
    }
    return SpriteType.Unknown;
}

public static int? ExtractSecondToLastNumber(string input)
{
    if (string.IsNullOrEmpty(input))
        return null;
    
    // Find the last underscore
    int lastUnderscoreIndex = input.LastIndexOf('_');
    if (lastUnderscoreIndex == -1 || lastUnderscoreIndex == 0)
        return null;
    
    // Find the underscore before the last one
    int secondLastUnderscoreIndex = input.LastIndexOf('_', lastUnderscoreIndex - 1);
    if (secondLastUnderscoreIndex == -1)
        return null;
    
    // Extract the number between the two underscores
    int startIndex = secondLastUnderscoreIndex + 1;
    int length = lastUnderscoreIndex - startIndex;
    
    if (length <= 0)
        return null;
    
    ReadOnlySpan<char> numberSpan = input.AsSpan(startIndex, length);
    
    return int.TryParse(numberSpan, out int result) ? result : null;
}

void CheckDuplicates()
{
    // Get import folder
    string importFolder = texturesPath;
    bool hasSpriteStripFiles = true;

    bool hadMessage = false;
    bool hadFramelessMessage = false;
    string[] dirFiles = Directory.GetFiles(importFolder, "*.png", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(importFolder, "*.gif", SearchOption.AllDirectories))
        .ToArray();
    dirFiles = dirFiles.Where(file => !file.Contains(Path.Combine(importFolder, "nostrip"))).ToArray();
    
    if (dirFiles.Length == 0)
    {
        Log.Debug($"No sprite strip textures to import at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importFolder))}");
		hasSpriteStripFiles = false;
    }

    string[] allFiles = Directory.GetFiles(importFolder, "*.*", SearchOption.AllDirectories);
    var fileSet = new HashSet<string>(allFiles.Select(Path.GetFileName));  // Create a set of filenames for quick lookup

    foreach (string file in dirFiles)
    {
        string FileNameWithExtension = Path.GetFileName(file);
        string stripped = Path.GetFileNameWithoutExtension(file);

        // Check for duplicate filenames in the cached file set
        if (fileSet.Count(f => f == FileNameWithExtension) > 1)
        {
            Log.Error($"\nDuplicate file detected. There are multiple files named: {FileNameWithExtension}\n");
        }
    }
}