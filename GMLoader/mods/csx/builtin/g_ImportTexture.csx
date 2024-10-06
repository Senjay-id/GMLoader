// ImportGraphicsWithParameters but it can also set animation speeds and import more types of files
// Edits made by CST1229
// Edits made by Senjay to suit GMLoader

// Based off of ImportGraphics.csx by the UTMT team
// and ImportGraphicsWithParameters.csx by someone, I don't remmeber (AwfulNasty???)

// revision 2: fixed gif import not working unless the folder was named Sprites,
// fixed the default origin being Top Center instead of Top Left and
// reworded Is special type?'s boolean and the background import error message

// Texture packer by Samuel Roy
// Uses code from https://github.com/mfascia/TexturePacker

if (Data is null)
{
	Log.Error("Exception: No Data loaded!");
	throw new Exception();
}
Stopwatch stopwatch = new Stopwatch();
TimeSpan elapsed;
public static int frame; //Cannot be uint because Rectangle constructor parameter expects int
public static string spriteName;
static bool importAsSprite = true;

//Cache
int DefaultSpriteX = defaultSpriteX;
int DefaultSpriteY = defaultSpriteY;
uint DefaultSpriteSpeedType = defaultSpriteSpeedType;
float DefaultSpriteFrameSpeed = defaultSpriteFrameSpeed;
uint DefaultSpriteBoundingBoxType = defaultSpriteBoundingBoxType;
uint DefaultSpriteSepMasksType = defaultSpriteSepMasksType;
int DefaultSpriteBoundingBoxLeft = defaultSpriteBoundingBoxLeft;
int DefaultSpriteBoundingBoxRight = defaultSpriteBoundingBoxRight;
int DefaultSpriteBoundingBoxBottom = defaultSpriteBoundingBoxBottom;
int DefaultSpriteBoundingBoxTop = defaultSpriteBoundingBoxTop;
bool DefaultSpriteTransparent = defaultSpriteTransparent;
bool DefaultSpriteSmooth = defaultSpriteSmooth;
bool DefaultSpritePreload = defaultSpritePreload;
uint DefaultSpriteSpecialVer = defaultSpriteSpecialVer;
bool DefaultBGTransparent = defaultBGTransparent;
bool DefaultBGSmooth = defaultBGSmooth;
bool DefaultBGPreload = defaultBGPreload;
uint DefaultBGTileWidth = defaultBGTileWidth;
uint DefaultBGTileHeight = defaultBGTileHeight;
uint DefaultBGBorderX = defaultBGBorderX;
uint DefaultBGBorderY = defaultBGBorderY;
uint DefaultBGTileColumn = defaultBGTileColumn;
uint DefaultBGItemOrFramePerTile = defaultBGItemOrFramePerTile;
uint DefaultBGTileCount = defaultBGTileCount;
int DefaultBGFrameTime = defaultBGFrameTime;

//Default Sprite values
spriteName = "invalid_sprite";
frame = 1;
int xCoordinate = DefaultSpriteX;
int yCoordinate = DefaultSpriteY;
uint speedType = DefaultSpriteSpeedType;
float frameSpeed = DefaultSpriteFrameSpeed;
uint boundingBoxType = DefaultSpriteBoundingBoxType;
uint sepMaskType = DefaultSpriteSepMasksType;
int leftCollision = DefaultSpriteBoundingBoxLeft;
int rightCollision = DefaultSpriteBoundingBoxRight;
int bottomCollision = DefaultSpriteBoundingBoxBottom;
int topCollision = DefaultSpriteBoundingBoxTop;
bool transparent = DefaultSpriteTransparent;
bool smooth = DefaultSpriteSmooth;
bool preload = DefaultSpritePreload;
bool isSpecial = Data.IsGameMaker2();
uint specialVer = DefaultSpriteSpecialVer;
Regex regex = new Regex(@"(?<=_f)(\d+)|(?<=_x)(-?\d+)|(?<=_y)(-?\d+)|(?<=_st)(\d+)|(?<=_s)(\d+)|(?<=_b)(\d+)|(?<=_left)(-?\d+)|(?<=_right)(-?\d+)|(?<=_bot)(-?\d+)|(?<=_top)(-?\d+)", RegexOptions.Compiled);
Regex stripStyleFrameRegex = new(@"^(.+?)(?:_(\d+))$", RegexOptions.Compiled);


//Default Background/Tiles values
bool bgTransparent = DefaultBGTransparent;
bool bgSmooth = DefaultBGSmooth;
bool bgPreload = DefaultBGPreload;
uint tileWidth = DefaultBGTileWidth;
uint tileHeight = DefaultBGTileHeight;
uint borderX = DefaultBGBorderX;
uint borderY = DefaultBGBorderY;
uint tileColumn = DefaultBGTileColumn;
uint itemOrFramePerTile = DefaultBGItemOrFramePerTile;
uint tileCount = DefaultBGTileCount;
int frameTime = DefaultBGFrameTime;
Regex backgroundRegex = new Regex(@"_tc(\d+)|_tw(\d+)|_th(\d+)|_x(\d+)|_y(\d+)|_col(\d+)|_ipt(\d+)", RegexOptions.Compiled);

HashSet<string> spritesStartAt1 = new HashSet<string>();

public class SpriteProperties
{
	public int XCoordinate { get; set; }
	public int YCoordinate { get; set; }
	public uint SpeedType { get; set; }
	public float FrameSpeed { get; set; }
	public uint BoundingBoxType { get; set; }
	public int LeftCollision { get; set; }
	public int RightCollision { get; set; }
	public int BottomCollision { get; set; }
	public int TopCollision { get; set; }
}

Dictionary<string, SpriteProperties> spriteDictionary = new Dictionary<string, SpriteProperties>();

int coreCount = Environment.ProcessorCount - 1;
// If you want to use all your cores just uncomment the code below
//coreCount = Environment.ProcessorCount;

//Realistically no pc should ever only have a single core
if (coreCount == 0)
	coreCount = 1;

var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism

string importFolder = CheckValidity();

string packDir = Path.GetFullPath("./mods/logs/packager");
Directory.CreateDirectory(packDir);

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

// Import everything into UMT
string prefix = outName.Replace(Path.GetExtension(outName), "");
int atlasCount = 0;

Log.Information("Removing existing sprite frame before assigning the frame, please wait a moment");
//Remove all modded sprite frame before looping
Parallel.ForEach(spriteList, options, spriteName =>
{
	UndertaleSprite spr = Data.Sprites.ByName(spriteName);
	if (spr != null)
	{
		while (spr.Textures.Count > 0)
		{
			spr.Textures.RemoveAt(spr.Textures.Count - 1); // Remove the last frame
		}
	}
	else
		Log.Debug($"{spriteName} is null, probably a new sprite.");
});

foreach (Atlas atlas in packer.Atlasses)
{
	string atlasName = Path.Combine(packDir, String.Format(prefix + "{0:000}" + ".png", atlasCount));
	Bitmap atlasBitmap = new Bitmap(atlasName);
	UndertaleEmbeddedTexture texture = new UndertaleEmbeddedTexture();
	texture.Name = new UndertaleString("Texture " + ++lastTextPage);
	texture.TextureData.TextureBlob = File.ReadAllBytes(atlasName);
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
			texturePageItem.TargetX = 0;
			texturePageItem.TargetY = 0;
			texturePageItem.TargetWidth = (ushort)n.Bounds.Width;
			texturePageItem.TargetHeight = (ushort)n.Bounds.Height;
			texturePageItem.BoundingWidth = (ushort)n.Bounds.Width;
			texturePageItem.BoundingHeight = (ushort)n.Bounds.Height;
			texturePageItem.TexturePage = texture;

			// Add this texture to UMT
			Data.TexturePageItems.Add(texturePageItem);

			// String processing
			string stripped = Path.GetFileNameWithoutExtension(n.Texture.Source);
			//Log.Information($"n.Texture.Source: {n.Texture.Source} stripped: {stripped}");

			SpriteType spriteType = GetSpriteType(n.Texture.Source);

			if (importAsSprite)
			{
				if ((spriteType == SpriteType.Unknown) || (spriteType == SpriteType.Font))
				{
					spriteType = SpriteType.Sprite;
				}
			}

			setTextureTargetBounds(texturePageItem, stripped, n);

			if (spriteType == SpriteType.Background)
			{

				int tileCountIndex = stripped.IndexOf("_tc");
				string bgSpriteName = tileCountIndex != -1 ? stripped.Substring(0, tileCountIndex) : stripped;
				tileCount = DefaultBGTileCount;
				tileWidth = DefaultBGTileWidth;
				tileHeight = DefaultBGTileHeight;
				borderX = DefaultBGBorderX;
				borderY = DefaultBGBorderY;
				tileColumn = DefaultBGTileColumn;
				itemOrFramePerTile = DefaultBGItemOrFramePerTile;
				MatchCollection matches = backgroundRegex.Matches(stripped);
				foreach (Match match in matches)
				{
					if (match.Groups[1].Success)
						tileCount = uint.Parse(match.Groups[1].Value);
					else if (match.Groups[2].Success)
						tileWidth = uint.Parse(match.Groups[2].Value);
					else if (match.Groups[3].Success)
						tileHeight = uint.Parse(match.Groups[3].Value);
					else if (match.Groups[4].Success)
						borderX = uint.Parse(match.Groups[4].Value);
					else if (match.Groups[5].Success)
						borderY = uint.Parse(match.Groups[5].Value);
					else if (match.Groups[6].Success)
						tileColumn = uint.Parse(match.Groups[6].Value);
					else if (match.Groups[7].Success)
						itemOrFramePerTile = uint.Parse(match.Groups[7].Value);
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
				string spriteName;
				int lastUnderscore = stripped.LastIndexOf('_');
				int frame = 0;
				/*
				if (lastUnderscore == -1)
				{
					// No underscore found, use the whole filename as spriteName set frame from the index of 0
					spriteName = stripped;
					frame = 0;
				}
				*/

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
				//Log.Information($"stripped: {stripped} || spriteName: {spriteName} || frame: {frame}");

				// This is not optimized for sprite strip, needs to be optimized later
				if (spriteDictionary.TryGetValue(spriteName, out SpriteProperties spriteProps))
				{
					xCoordinate = spriteProps.XCoordinate;
					yCoordinate = spriteProps.YCoordinate;
					speedType = spriteProps.SpeedType;
					frameSpeed = spriteProps.FrameSpeed;
					boundingBoxType = spriteProps.BoundingBoxType;
					leftCollision = spriteProps.LeftCollision;
					rightCollision = spriteProps.RightCollision;
					bottomCollision = spriteProps.BottomCollision;
					topCollision = spriteProps.TopCollision;
				}
				else
				{
					Log.Error($"Sprite properties not found for {spriteName} this is most likely caused by incorrect naming convention.");
				}

				UndertaleSprite sprite = null;
				sprite = Data.Sprites.ByName(spriteName);

				if (spritesStartAt1.Contains(spriteName))
				{
					frame--;
				}

				// Create TextureEntry object
				UndertaleSprite.TextureEntry texentry = new UndertaleSprite.TextureEntry();
				texentry.Texture = texturePageItem;

				// Set values for new sprites
				if (sprite == null)
				{
					UndertaleString spriteUTString = Data.Strings.MakeString(spriteName);
					UndertaleSprite newSprite = new UndertaleSprite();
					newSprite.Name = spriteUTString;
					newSprite.Width = (uint)n.Bounds.Width;
					newSprite.Height = (uint)n.Bounds.Height;
					newSprite.OriginX = xCoordinate;
					newSprite.OriginY = yCoordinate;
					newSprite.BBoxMode = boundingBoxType;
					//Log.Information($"THE NAME OF THE SPRITE IS {spriteName} newSprite.Name IS {newSprite.Name}");
					if (boundingBoxType == 2)
					{
						newSprite.MarginLeft = leftCollision;
						newSprite.MarginRight = rightCollision;
						newSprite.MarginTop = topCollision;
						newSprite.MarginBottom = bottomCollision;
					}
					else
					{
						newSprite.MarginLeft = 0;
						newSprite.MarginRight = n.Bounds.Width - 1;
						newSprite.MarginTop = 0;
						newSprite.MarginBottom = n.Bounds.Height - 1;
					}
					newSprite.IsSpecialType = isSpecial;
					newSprite.SVersion = specialVer;
					newSprite.GMS2PlaybackSpeedType = (AnimSpeedType)speedType;
					newSprite.GMS2PlaybackSpeed = frameSpeed;
					newSprite.SepMasks = (UndertaleSprite.SepMaskType)sepMaskType;
					newSprite.Transparent = transparent;
					newSprite.Smooth = smooth;
					newSprite.Preload = preload;

					if (frame > 0)
					{
						for (int i = 0; i < frame; i++)
							newSprite.Textures.Add(null);
					}
					newSprite.CollisionMasks.Add(newSprite.NewMaskEntry());
					Rectangle bmpRect = new Rectangle(n.Bounds.X, n.Bounds.Y, n.Bounds.Width, n.Bounds.Height);
					System.Drawing.Imaging.PixelFormat format = atlasBitmap.PixelFormat;
					Bitmap cloneBitmap = atlasBitmap.Clone(bmpRect, format);
					int width = ((n.Bounds.Width + 7) / 8) * 8;
					BitArray maskingBitArray = new BitArray(width * n.Bounds.Height);
					for (int y = 0; y < n.Bounds.Height; y++)
					{
						for (int x = 0; x < n.Bounds.Width; x++)
						{
							Color pixelColor = cloneBitmap.GetPixel(x, y);
							maskingBitArray[y * width + x] = (pixelColor.A > 0);
						}
					}
					BitArray tempBitArray = new BitArray(width * n.Bounds.Height);
					for (int i = 0; i < maskingBitArray.Length; i += 8)
					{
						for (int j = 0; j < 8; j++)
						{
							tempBitArray[j + i] = maskingBitArray[-(j - 7) + i];
						}
					}
					int numBytes;
					numBytes = maskingBitArray.Length / 8;
					byte[] bytes = new byte[numBytes];
					tempBitArray.CopyTo(bytes, 0);
					for (int i = 0; i < bytes.Length; i++)
						newSprite.CollisionMasks[0].Data[i] = bytes[i];
					newSprite.Textures.Add(texentry);
					Data.Sprites.Add(newSprite);
					//Log.Information($"Assigning a new sprite with their name as {newSprite.Name} frame: {frame} xorg: {newSprite.OriginX} yorg: {newSprite.OriginY}");
					continue;
				}
				if (frame > sprite.Textures.Count - 1)
				{
					while (frame > sprite.Textures.Count - 1)
					{
						sprite.Textures.Add(texentry);
					}
					continue;
				}

				sprite.Textures[frame] = texentry;
				sprite.Width = (uint)n.Bounds.Width;
				sprite.Height = (uint)n.Bounds.Height;
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
				else
				{
					sprite.MarginLeft = 0;
					sprite.MarginRight = n.Bounds.Width - 1;
					sprite.MarginTop = 0;
					sprite.MarginBottom = n.Bounds.Height - 1;
				}
				//sprite.IsSpecialType = isSpecial; //Redundant
				//sprite.SVersion = specialVer;		//Redundant
				sprite.GMS2PlaybackSpeedType = (AnimSpeedType)speedType;
				sprite.GMS2PlaybackSpeed = frameSpeed;
				sprite.SepMasks = (UndertaleSprite.SepMaskType)sepMaskType;
				sprite.Transparent = transparent;
				sprite.Smooth = smooth;
				sprite.Preload = preload;
				//Log.Information($"Changing old sprite properties for {sprite.Name} frame: {frame} xorg: {sprite.OriginX} yorg: {sprite.OriginY} speedType: {speedType}");
			}
		}
	}
	// Increment atlas
	atlasCount++;
}

//HideProgressBar();
Log.Information("Done!");

void setTextureTargetBounds(UndertaleTexturePageItem tex, string textureName, Node n)
{
	tex.TargetX = 0;
	tex.TargetY = 0;
	tex.TargetWidth = (ushort)n.Bounds.Width;
	tex.TargetHeight = (ushort)n.Bounds.Height;
}

public class TextureInfo
{
	public string Source;
	public int Width;
	public int Height;
	public Image Image;
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

public class Node
{
	public Rectangle Bounds;
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
	public StringWriter LogStream;
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
		LogStream = new StringWriter();
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
				// we need to go 1 step larger as we found the first size that is to small
				atlas.Width *= 2;
				atlas.Height *= 2;
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
			string atlasName = String.Format(prefix + "{0:000}" + ".png", atlasCount);
			//1: Save images
			Image img = CreateAtlasImage(atlas);
			img.Save(atlasName, System.Drawing.Imaging.ImageFormat.Png);
			//2: save description in file
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
		tw.WriteLine(LogStream.ToString());
		tw.WriteLine("--- ERROR -----------------------------------------");
		tw.WriteLine(Error.ToString());
		tw.Close();
	}

	private void ScanForTextures(string _Path)
	{
		DirectoryInfo di = new DirectoryInfo(_Path);
		FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);
		string fullName;

		foreach (FileInfo fi in files)
		{
			fullName = fi.FullName;
			Log.Information($"Importing {Path.GetFileName(fullName)}");
			SpriteType spriteType = GetSpriteType(fullName);
			string ext = Path.GetExtension(fullName);

			bool isSprite = spriteType == SpriteType.Sprite || (spriteType == SpriteType.Unknown && importAsSprite);

			if (ext == ".gif")
			{
				// animated .gif
				string dirName = Path.GetDirectoryName(fullName);
				string spriteName = Path.GetFileNameWithoutExtension(fullName);
				Match stripMatch = null;
				stripMatch = Regex.Match(Path.GetFileNameWithoutExtension(fi.Name), @"(.*)_f(\d+)");
				if (stripMatch is not null && stripMatch.Success)
					spriteName = stripMatch.Groups[1].Value;

				Image gif = Image.FromFile(fullName);
				FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
				int frames = gif.GetFrameCount(dimension);
				if (!isSprite && frames > 1)
				{
					Log.Error(fullName + " is a " + spriteType + ", but has more than 1 frame. Script has been stopped.");
					throw new Exception();
				}

				for (int i = 0; i < frames; i++)
				{
					if (gif.SelectActiveFrame(dimension, i) == 0)
					{
						AddSource(
							(Image)gif.Clone(),
							Path.Join(
								dirName,
								isSprite ?
									(spriteName + "_" + i + ".png") : (spriteName + ".png")
							)
						);
					}
					else
					{
						Log.Error("Could not select frame " + i + " of " + fullName + ".");
						throw new Exception();
					}
				}

			}
			else if (ext == ".png")
			{
				Match stripMatch = null;
				if (isSprite)
				{
					stripMatch = Regex.Match(Path.GetFileNameWithoutExtension(fi.Name), @"(.*)_f(\d+)");
				}
				if (stripMatch is not null && stripMatch.Success)
				{
					string spriteName = stripMatch.Groups[1].Value;
					string frameCountStr = stripMatch.Groups[2].Value;

					int frames = 1;
					try
					{
						frames = Int32.Parse(frameCountStr);
					}
					catch
					{
						Log.Error(fullName + " has an invalid strip numbering scheme.");
						throw new Exception();
					}

					if (frames <= 0)
					{
						Log.Error(fullName + " has 0 frames.");
						throw new Exception();
					}

					if (!isSprite && frames > 0)
					{
						Log.Error(fullName + " is not a sprite, but has more than 1 frame.");
						throw new Exception();
					}

					Image img = Image.FromFile(fullName);
					if (img is null)
					{
						continue;
					}

					if ((img.Width % frames) > 0)
					{
						Log.Error($"{fullName} has a width not divisible by the number of frames. Width:{img.Width} Frames: {frames} result: {img.Width % frames}");
						continue;
					}

					Bitmap sheetBitmap = new Bitmap(img);

					string dirName = Path.GetDirectoryName(fullName);

					int frameWidth = img.Width / frames;
					int frameHeight = img.Height;
					for (int i = 0; i < frames; i++)
					{
						AddSource(
							sheetBitmap.Clone(
								new Rectangle(i * frameWidth, 0, frameWidth, frameHeight),
								sheetBitmap.PixelFormat
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
					//Log.Information("DOES NOT MATCH!");
					Image img = Image.FromFile(fullName);
					if (img != null)
					{
						AddSource(img, fullName);
					}
				}
			}
		}
	}

	private void AddSource(Image img, string fullName)
	{
		if (img.Width <= AtlasSize && img.Height <= AtlasSize)
		{
			TextureInfo ti = new TextureInfo();

			if (!Sources.Add(fullName))
			{
				Log.Error(
					Path.GetFileNameWithoutExtension(fullName) +
					" as a frame already exists (possibly due to having multiple types of sprite images named the same)."
				);
			}

			ti.Source = fullName;
			ti.Width = img.Width;
			ti.Height = img.Height;
			ti.Image = img;

			SourceTextures.Add(ti);

			LogStream.WriteLine("Added " + fullName);
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
		root.Bounds.Size = new Size(_Atlas.Width, _Atlas.Height);
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

	private Image CreateAtlasImage(Atlas _Atlas)
	{
		Image img = new Bitmap(_Atlas.Width, _Atlas.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		Graphics g = Graphics.FromImage(img);
		foreach (Node n in _Atlas.Nodes)
		{
			if (n.Texture != null)
			{
				g.DrawImage(n.Texture.Image, n.Bounds);
			}
		}
		// DPI FIX START
		Bitmap ResolutionFix = new Bitmap(img);
		ResolutionFix.SetResolution(96.0F, 96.0F);
		Image img2 = ResolutionFix;
		return img2;
		// DPI FIX END
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

string CheckValidity()
{
    string importFolder = texturesPath;

    // Stop the script if there's missing sprite entries or w/e.
    bool hadMessage = false;
    string[] dirFiles = Directory.GetFiles(importFolder, "*.png", SearchOption.AllDirectories)
                        .Concat(Directory.GetFiles(importFolder, "*.gif", SearchOption.AllDirectories))
                        .ToArray();

    // Filter out files that are in a directory named "nostrip"
    dirFiles = dirFiles.Where(file => !file.Contains(Path.Combine(importFolder, "nostrip"))).ToArray();

    if (dirFiles.Length == 0)
    {
        Log.Debug($"No textures to import at {Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, importFolder))}");
    }

    // Cache files from the import folder once
    string[] allFiles = Directory.GetFiles(importFolder, "*.*", SearchOption.AllDirectories);
    string[] spriteDataFiles = Directory.GetFiles(importFolder, "*.json*", SearchOption.AllDirectories);
    var fileSet = new HashSet<string>(allFiles.Select(Path.GetFileName));  // Create a set of filenames for quick lookup

    foreach (string file in spriteDataFiles)
    {
        string spritesPath = Path.GetDirectoryName(file);
        string spriteName = Path.GetFileName(spritesPath);
        string spriteData = Path.GetFullPath(Path.Combine(spritesPath, "data.json"));
        string jsonContent = File.ReadAllText(spriteData);
        JObject jsonObject = JObject.Parse(jsonContent);
        
        xCoordinate = (int)jsonObject["OriginX"];
        yCoordinate = (int)jsonObject["OriginY"];
        speedType = (uint)jsonObject["SpeedType"];
        frameSpeed = (float)jsonObject["FrameSpeed"];
        boundingBoxType = (uint)jsonObject["BBoxMode"];
        leftCollision = (int)jsonObject["BBoxLeft"];
        rightCollision = (int)jsonObject["BBoxRight"];
        topCollision = (int)jsonObject["BBoxTop"];
        bottomCollision = (int)jsonObject["BBoxBottom"];

        spriteDictionary[spriteName] = new SpriteProperties
        {
            XCoordinate = xCoordinate,
            YCoordinate = yCoordinate,
            SpeedType = speedType,
            FrameSpeed = frameSpeed,
            BoundingBoxType = boundingBoxType,
            LeftCollision = leftCollision,
            RightCollision = rightCollision,
            BottomCollision = bottomCollision,
            TopCollision = topCollision
        };  
        
        spriteList.Add(spriteName);
    }

    foreach (string file in dirFiles)
    {
        string FileNameWithExtension = Path.GetFileName(file);
        string stripped = Path.GetFileNameWithoutExtension(file);

        // Check for duplicate filenames in the cached file set
        if (fileSet.Count(f => f == FileNameWithExtension) > 1)
        {
            Log.Error($"\nDuplicate file detected. There are multiple files named: {FileNameWithExtension}\n");
        }

        SpriteType spriteType = GetSpriteType(file);

        if ((spriteType != SpriteType.Sprite) && (spriteType != SpriteType.Background))
        {
            if (!hadMessage)
            {
                hadMessage = true;
                importAsSprite = true;
            }

            if (!importAsSprite)
            {
                continue;
            }
            else
            {
                spriteType = SpriteType.Sprite;
            }
        }

        // Sprites can have multiple frames! Do some sprite-specific checking.
        if (spriteType == SpriteType.Sprite)
        {
            try
            {
                xCoordinate = DefaultSpriteX;
                yCoordinate = DefaultSpriteY;
                speedType = DefaultSpriteSpeedType;
                frameSpeed = DefaultSpriteFrameSpeed;
                boundingBoxType = DefaultSpriteBoundingBoxType;
                leftCollision = DefaultSpriteBoundingBoxLeft;
                rightCollision = DefaultSpriteBoundingBoxRight;
                bottomCollision = DefaultSpriteBoundingBoxBottom;
                topCollision = DefaultSpriteBoundingBoxTop;

                // Use regex to match strings like '_f' followed by digits
                var regexForSpriteName = new Regex(@"^(.*?)(?=_f[0-9])", RegexOptions.Compiled);

                // Try to match the pattern in the stripped string
                Match spriteMatch = regexForSpriteName.Match(stripped);
                bool matchFound = spriteMatch.Success;

                if (matchFound)
                {
                    // Capture the part before '_f' as the sprite name
                    spriteName = spriteMatch.Groups[1].Value;

                    // Regex matching
                    MatchCollection matches = regex.Matches(stripped);
                    foreach (Match match in matches)
                    {
                        if (match.Groups[1].Success)
                            frame = int.Parse(match.Groups[1].Value);
                        else if (match.Groups[2].Success)
                            xCoordinate = int.Parse(match.Groups[2].Value);
                        else if (match.Groups[3].Success)
                            yCoordinate = int.Parse(match.Groups[3].Value);
                        else if (match.Groups[4].Success)
                            speedType = uint.Parse(match.Groups[4].Value);
                        else if (match.Groups[5].Success)
                            frameSpeed = float.Parse(match.Groups[5].Value);
                        else if (match.Groups[6].Success)
                            boundingBoxType = uint.Parse(match.Groups[6].Value);
                        else if (match.Groups[7].Success)
                            leftCollision = int.Parse(match.Groups[7].Value);
                        else if (match.Groups[8].Success)
                            rightCollision = int.Parse(match.Groups[8].Value);
                        else if (match.Groups[9].Success)
                            bottomCollision = int.Parse(match.Groups[9].Value);
                        else if (match.Groups[10].Success)
                            topCollision = int.Parse(match.Groups[10].Value);
                    }
                }
                else
                {
                    // No matches, use the full filename
                    spriteName = stripped;
                }

                spriteList.Add(spriteName);
                spriteDictionary[spriteName] = new SpriteProperties
                {
                    XCoordinate = xCoordinate,
                    YCoordinate = yCoordinate,
                    SpeedType = speedType,
                    FrameSpeed = frameSpeed,
                    BoundingBoxType = boundingBoxType,
                    LeftCollision = leftCollision,
                    RightCollision = rightCollision,
                    BottomCollision = bottomCollision,
                    TopCollision = topCollision
                };
            }
            catch (Exception e)
            {
                Log.Error($"An error has occurred: {e}");
                throw;
            }
        }
    }
    return importFolder;
}

public void TimeThis()
{
	stopwatch.Start();
}

public void TimeStop()
{
	stopwatch.Stop();
	elapsed = stopwatch.Elapsed;
	Log.Information($"Elapsed Time (Milliseconds): {elapsed.TotalMilliseconds}ms\nElapsed Time (Seconds): {elapsed.TotalSeconds}second");
	stopwatch.Reset();
}