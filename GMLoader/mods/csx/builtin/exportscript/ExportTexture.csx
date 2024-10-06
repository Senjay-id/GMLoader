// Modified with the help of Agentalex9
// Modified by Senjay for GMLoader
if (Data is null)
{
    Log.Error("Exception: No Data loaded!");
    throw new Exception();
}

const int MAX_WIDTH = 65536;

bool padded = true;
bool useSubDirectories = false;
const string ext = ".png";
const string UF = "_f";
const string UX = "_x";
const string UY = "_y";
const string UST = "_st";
const string US = "_s";
const string UB = "_b";
const string ULEFT = "_left";
const string URIGHT = "_right";
const string UBOT = "_bot";
const string UTOP = "_top";

const string UTC = "_tc";
const string UTW = "_tw";
const string UTH = "_th";
const string UCOL = "_col";
const string UIPT = "_ipt";

string texFolder = exportTextureOutputPath + Path.DirectorySeparatorChar;
string bgFolder = texFolder + "backgrounds";
string fontFolder = texFolder + "fonts";

while (File.Exists(texFolder) || File.Exists(bgFolder))
{
    Log.Information("Please delete the Export folder before exporting.\n\nPress any key to continue..");
    Console.ReadKey();
}

List<string> invalidSpriteNames = new List<string>();
int invalidSprite = 0;

TextureWorker worker = new TextureWorker();

Directory.CreateDirectory(texFolder);

int coreCount = Environment.ProcessorCount - 1;
// If you want to use all your cores just uncomment the code below
//coreCount = Environment.ProcessorCount;

//Realistically no pc should ever only have a single core
if (coreCount == 0)
    coreCount = 1;

var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism
Log.Information($"Using {coreCount} cores to process the sprites");

Stopwatch stopwatch = new Stopwatch();
TimeSpan elapsed;

/*
TimeThis();
Parallel.ForEach(Data.Fonts, options, DumpFont);
TimeStop();
*/

TimeThis();
Parallel.ForEach(Data.Backgrounds, options, DumpBackground);
TimeStop();

TimeThis();
Parallel.ForEach(Data.Sprites, options, DumpSprite);
TimeStop();

string noStripPath = texFolder + "nostrip";
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
                string texturePath = Path.Combine(noStripPath, spriteName, spriteName + "_" + i + ".png");
                worker.ExportAsPNG(tex, texturePath);
            }
        });

        var sprData = new
        {
            OriginX = spr.OriginX,
            OriginY = spr.OriginY,
            SpeedType = spr.GMS2PlaybackSpeedType,
            FrameSpeed = spr.GMS2PlaybackSpeed,
            BBoxMode = spr.BBoxMode,
            BBoxLeft = spr.MarginLeft,
            BBoxRight = spr.MarginRight,
            BBoxTop = spr.MarginTop,
            BBoxBottom = spr.MarginBottom,
        };

        string json = JsonConvert.SerializeObject(sprData, Formatting.Indented);
        string fileName = Path.Combine(noStripPath, spriteName, "data.json");

        File.WriteAllText(fileName, json);
        Log.Information($"Exported {name} as No-Strip type sprite");
    }
});

worker.Cleanup();
Log.Information($"All sprite files has been exported to {Path.GetFullPath(texFolder)}");

void DumpSprite(UndertaleSprite sprite)
{
    //Log.Information($"Exporting {sprite.Name.Content}");

    // Cannot be cached outside of the function because of race condition, these variables needs to be reinitialized
    uint spriteFrame = 0;
    int originX = 0;
    int originY = 0;
    uint speedType = 0;
    float frameSpeed = 0;
    uint boundingBoxType = 0;
    int boundingBoxLeft = 0;
    int boundingBoxRight = 0;
    int boundingBoxBottom = 0;
    int boundingBoxTop = 0;

    string fileName = "";
    string spriteName = sprite.Name.Content;
    string spriteFrameStr = "";
    string originXStr = "";
    string originYStr = "";
    string speedTypeStr = "";
    string frameSpeedStr = "";
    string boundingBoxTypeStr = "";
    string boundingBoxLeftStr = "";
    string boundingBoxRightStr = "";
    string boundingBoxBottomStr = "";
    string boundingBoxTopStr = "";

    // Calculate total width and maximum height for the strip
    int totalWidth = 0;
    int maxHeight = 0;

    List<Bitmap> bitmaps = new List<Bitmap>();

    // Gather all textures as bitmaps
    foreach (var texture in sprite.Textures)
    {
        if (texture?.Texture != null)
        {
            var bitmap = worker.GetTextureFor(texture.Texture, sprite.Name.Content, padded);
            bitmaps.Add(bitmap);
            totalWidth += bitmap.Width; // Accumulate total width
            maxHeight = Math.Max(maxHeight, bitmap.Height); // Track the maximum height

            originXStr = sprite.OriginX.ToString();
            originYStr = sprite.OriginY.ToString();
            speedTypeStr = sprite.GMS2PlaybackSpeedType.ToString();
            frameSpeedStr = sprite.GMS2PlaybackSpeed.ToString();
            boundingBoxType = sprite.BBoxMode;
            boundingBoxLeftStr = sprite.MarginLeft.ToString();
            boundingBoxRightStr = sprite.MarginRight.ToString();
            boundingBoxBottomStr = sprite.MarginBottom.ToString();
            boundingBoxTopStr = sprite.MarginTop.ToString();

            spriteFrame++;
        }
    }

    if (totalWidth < MAX_WIDTH)
    {
        // Create the final strip image
        using (Bitmap stripImage = new Bitmap(totalWidth, maxHeight))
        {
            using (Graphics g = Graphics.FromImage(stripImage))
            {
                int offsetX = 0;

                // Draw each bitmap onto the strip
                foreach (var bitmap in bitmaps)
                {
                    using (bitmap) // Ensure proper disposal with using
                    {
                        g.DrawImage(bitmap, new Rectangle(offsetX, 0, bitmap.Width, bitmap.Height));
                        offsetX += bitmap.Width;
                    }
                }
            }

            spriteFrameStr = spriteFrame.ToString();
            if (speedTypeStr == "FramesPerGameFrame")
                speedTypeStr = "1";
            else
                speedTypeStr = "0";

            if (boundingBoxType != 2)
            {
                boundingBoxTypeStr = boundingBoxType.ToString();
                fileName = spriteName + UF + spriteFrameStr + UX + originXStr + UY + originYStr + UST + speedTypeStr + US + frameSpeedStr + UB + boundingBoxTypeStr + ext;
            }
            else
            {
                boundingBoxTypeStr = boundingBoxType.ToString();
                fileName = spriteName + UF + spriteFrameStr + UX + originXStr + UY + originYStr + UST + speedTypeStr + US + frameSpeedStr + UB + boundingBoxTypeStr + ULEFT + boundingBoxLeftStr + URIGHT + boundingBoxRightStr + UBOT + boundingBoxBottomStr + UTOP + boundingBoxTopStr + ext;
            }
            //Log.Information($"{spriteName} FILENAME: {fileName}");
            // Save the final strip image
            string stripPath = Path.Combine(texFolder, fileName);
            TextureWorker.SaveImageToFile(stripPath, stripImage);
            Log.Information($"Exported {sprite.Name.Content}");
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
        string tileCount = background.GMS2TileCount.ToString();
        string tileWidth = background.GMS2TileWidth.ToString();
        string tileHeight = background.GMS2TileHeight.ToString();
        string borderX = background.GMS2OutputBorderX.ToString();
        string borderY = background.GMS2OutputBorderY.ToString();
        string tileColumn = background.GMS2TileColumns.ToString();
        string itemOrFramePerTile = background.GMS2ItemsPerTileCount.ToString();

        string fileName = background.Name.Content + UTC + tileCount + UTW + tileWidth + UTH + tileHeight + UX + borderX + UY + borderY + UCOL + tileColumn + UIPT + itemOrFramePerTile + ext;

        UndertaleTexturePageItem tex = background.Texture;
        worker.ExportAsPNG(tex, Path.Combine(bgFolder, fileName));
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
