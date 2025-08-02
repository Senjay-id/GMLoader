// Modified with the help of Agentalex9
// Modified by Senjay for GMLoader

const int MAX_WIDTH = 65536;

bool padded = true;
bool useSubDirectories = false;
const string ext = ".png";

string texFolder = exportTextureOutputPath + Path.DirectorySeparatorChar;
string texConfigFolder = exportTextureConfigOutputPath;
string bgFolder = exportTextureBackgroundOutputPath;
string bgConfigFolder = exportBackgroundTextureConfigOutputPath;
string fontFolder = texFolder + "fonts";
string noStripFolder = exportTextureNoStripOutputPath;

while (File.Exists(texFolder) || File.Exists(bgFolder))
{
    Log.Information("Please delete the Export folder before exporting.\n\nPress any key to continue..");
    Console.ReadKey();
}

//List<string> invalidSpriteNames = new List<string>();
//int invalidSprite = 0;

//List<string> invalidSpriteSizeNames = new List<string>();
//int invalidSpriteSize = 0;



Directory.CreateDirectory(texFolder);
Directory.CreateDirectory(bgFolder);
Directory.CreateDirectory(noStripFolder);
Directory.CreateDirectory(texConfigFolder);
Directory.CreateDirectory(bgConfigFolder);
//Directory.CreateDirectory(Path.Combine(exportTextureOutputPath + "fonts"));

int coreCount = Environment.ProcessorCount - 1;
// If you want to use all your cores just uncomment the code below
//coreCount = Environment.ProcessorCount;

//Realistically no pc should ever only have a single core
if (coreCount == 0)
    coreCount = 1;

var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism
Log.Information($"Using {coreCount} cores to dump the sprites");

Stopwatch stopwatch = new Stopwatch();
TimeSpan elapsed;

/*
TimeThis();
Parallel.ForEach(Data.Fonts, options, DumpFont);
TimeStop();
*/

TextureWorker worker = null;

using (worker = new())
{
    Parallel.ForEach(Data.Backgrounds, options, DumpBackground);

    Parallel.ForEach(Data.Sprites, options, DumpSprite);

    Parallel.ForEach(invalidSpriteNames, name =>
    {
        UndertaleSprite spr = Data.Sprites.ByName(name);

        if (spr != null)
        {
            string spriteName = spr.Name.Content;

            // Export textures concurrently
            Parallel.For(0, spr.Textures.Count, i =>
            {
                if (spr.Textures[i]?.Texture != null)
                {
                    UndertaleTexturePageItem tex = spr.Textures[i].Texture;
                    Directory.CreateDirectory(Path.Combine(noStripFolder, spriteName));
                    string texturePath = Path.Combine(noStripFolder, spriteName, spriteName + "_" + i + ".png");
                    using (var image = worker.GetTextureFor(tex, spriteName, padded))
                    {
                        image.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "all"); // Strip all metadata
                        image.Settings.Compression = CompressionMethod.Zip; // Standard compression
                        image.Write(texturePath);
                    }
                }
            });

            // can be optimized maybe
            var config = new SpriteData
            {
                yml_x = spr.OriginX,
                yml_y = spr.OriginY,
                yml_transparent = spr.Transparent,
                yml_smooth = spr.Smooth,
                yml_preload = spr.Preload,
                yml_boundingboxtype = spr.BBoxMode,
                yml_bboxleft = spr.MarginLeft,
                yml_bboxright = spr.MarginRight,
                yml_bboxtop = spr.MarginTop,
                yml_bboxbottom = spr.MarginBottom,
                yml_sepmask = (uint)spr.SepMasks,
                yml_speedtype = (uint)spr.GMS2PlaybackSpeedType,
                yml_framespeed = spr.GMS2PlaybackSpeed
            };

            var yamlBytes = YamlSerializer.Serialize(config);
            string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
            string fileName = Path.Combine(noStripFolder, spriteName, "data.yaml");

            File.WriteAllText(fileName, yaml);
            Log.Information($"Exported {name} as No-Strip type sprite");
        }
    });

}

/*
if (invalidSpriteSize > 0)
{
    Log.Error("The sprite below has invalid height or width:");
    foreach (var name in invalidSpriteSizeNames)
    {
        Log.Error(name);
    }
}
*/

Log.Information($"All sprite files has been exported to {Path.GetFullPath(texFolder)}");

void DumpSprite(UndertaleSprite sprite)
{
    // Cannot be cached outside of the function because of race condition, these variables needs to be reinitialized
    string spriteName = sprite.Name.Content;
    if (spriteName == "") {
        Log.Error("Skipped sprite that has an empty name to prevent an exception");
        return;
    }
    foreach (string i in textureExclusionList)
    {
        if (spriteName.Contains(i))
        {
            Log.Information($"Skipped {spriteName} because it's included on the exclusion list");
            return;
        }
    }

    Log.Information($"Exporting {sprite.Name.Content}");
    int spriteFrame = 0;
    int originX = 0;
    int originY = 0;
    bool transparent = false;
    bool smooth = false;
    bool preload = false;
    uint speedType = 0;
    float frameSpeed = 0;
    uint boundingBoxType = 0;
    int boundingBoxLeft = 0;
    int boundingBoxRight = 0;
    int boundingBoxBottom = 0;
    int boundingBoxTop = 0;
    uint sepmask = 0;

    string fileName = spriteName + ext;

    // Calculate total width and maximum height for the strip
    uint totalWidth = 0;
    uint maxHeight = 0;

    List<IMagickImage<byte>> images = new List<IMagickImage<byte>>();

    // Gather all textures as bitmaps
    foreach (var texture in sprite.Textures)
    {
        if (texture?.Texture != null)
        {
            var image = worker.GetTextureFor(texture.Texture, sprite.Name.Content, padded);
            images.Add(image);
            totalWidth += image.Width;
            maxHeight = Math.Max(maxHeight, image.Height);

            spriteFrame++;
        }
    }

    if (totalWidth == 0 || maxHeight == 0)
    {
        Log.Error($"Error, {spriteName} has invalid width or height");
        invalidSpriteSizeNames.Add(spriteName);
        invalidSpriteSize++;
        return;
    }

    if (totalWidth < MAX_WIDTH)
    {
        // can be optimized maybe
        var config = new SpriteData
        {
            yml_frame = spriteFrame,
            yml_x = sprite.OriginX,
            yml_y = sprite.OriginY,
            yml_transparent = sprite.Transparent,
            yml_smooth = sprite.Smooth,
            yml_preload = sprite.Preload,
            yml_boundingboxtype = sprite.BBoxMode,
            yml_bboxleft = sprite.MarginLeft,
            yml_bboxright = sprite.MarginRight,
            yml_bboxtop = sprite.MarginTop,
            yml_bboxbottom = sprite.MarginBottom,
            yml_sepmask = (uint)sprite.SepMasks,
            yml_speedtype = (uint)sprite.GMS2PlaybackSpeedType,
            yml_framespeed = sprite.GMS2PlaybackSpeed
        };
        var data = new Dictionary<string, SpriteData>
        {
            [spriteName] = config
        };
        var yamlBytes = YamlSerializer.Serialize(data);
        string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
        string configFileName = Path.Combine(texConfigFolder, spriteName + ".yaml");

        File.WriteAllText(configFileName, yaml);

        // Create the final strip image
        using (var stripImage = new MagickImage(MagickColors.Transparent, totalWidth, maxHeight))
        {
            int offsetX = 0; // Prefer `int` over `uint` for offsets (avoids casting issues)
            foreach (var image in images)
            {
                stripImage.Composite(image, offsetX, 0, CompositeOperator.Over); // Correct order
                offsetX += (int)image.Width; // Ensure offsetX is `int` (cast if needed)
                image.Dispose();
            }
            
            string stripPath = Path.Combine(texFolder, fileName);
            stripImage.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "all");
            stripImage.Settings.Compression = CompressionMethod.Zip;
            stripImage.Write(stripPath);
        }
    }
    else
    {
        Log.Warning($"Skipped {sprite.Name.Content} because the width is over 65536");
        invalidSpriteNames.Add(sprite.Name.Content);
        invalidSprite++;
    }
}

void DumpBackground(UndertaleBackground background)
{
    if (background.Texture != null)
    {
        var backgroundConfig = new BackgroundData
        {
            yml_tile_count = background.GMS2TileCount,
            yml_tile_width = background.GMS2TileWidth,
            yml_tile_height = background.GMS2TileHeight,
            yml_border_x = background.GMS2OutputBorderX,
            yml_border_y = background.GMS2OutputBorderY,
            yml_tile_column = background.GMS2TileColumns,
            yml_item_per_tile = background.GMS2ItemsPerTileCount,
            yml_transparent = background.Transparent,
            yml_smooth = background.Smooth,
            yml_preload = background.Preload,
            yml_frametime = background.GMS2FrameLength,
        };
        var data = new Dictionary<string, BackgroundData>
        {
            [background.Name.Content] = backgroundConfig
        };

        var yamlBytes = YamlSerializer.Serialize(data);
        string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
        string configFileName = Path.Combine(bgConfigFolder, background.Name.Content + ".yaml");

        File.WriteAllText(configFileName, yaml);

        string fileName = background.Name.Content + ext;
        string outputPath = Path.Combine(bgFolder, fileName);
        UndertaleTexturePageItem tex = background.Texture;

        using (var image = worker.GetTextureFor(tex, background.Name.Content, padded))
        {
            image.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "all"); // Strip all metadata
            image.Settings.Compression = CompressionMethod.Zip; // Standard compression
            image.Write(outputPath);
        }

        Log.Information($"Exported {background.Name.Content}");
    }
}

void DumpFont(UndertaleFont font)
{
    if (font.Texture != null)
    {
        UndertaleTexturePageItem tex = font.Texture;
        worker.ExportAsPNG(tex, Path.Combine(fontFolder, font.Name.Content + "_0.png"));
        Log.Information($"Exported {font.Name.Content}");
    }
}

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

void TimeThis()
{
    stopwatch.Start();
}

void TimeStop()
{
    stopwatch.Stop();
    elapsed = stopwatch.Elapsed;
    Log.Information($"Elapsed Time (Milliseconds): {elapsed.TotalMilliseconds} ms");
    Log.Information($"Elapsed Time (Seconds): {elapsed.TotalSeconds} second");
    stopwatch.Reset();
}
