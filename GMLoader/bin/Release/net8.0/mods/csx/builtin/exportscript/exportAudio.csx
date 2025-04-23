// Original script by Kneesnap, updated by Grossley, modified by Senjay for GMLoader

if (Data is null)
{
    Log.Error("Exception: No Data loaded!");
    throw new Exception();
}

string dataPath = gameDataPath;
string winFolder = Path.GetDirectoryName(dataPath) + Path.DirectorySeparatorChar;
bool usesAGRP = (Data.AudioGroups.Count > 0);
string exportedSoundsDir = exportAudioOutputPath;

// EXTERNAL OGG CHECK
bool externalOGG_Copy = true;

// Group by audio group check
bool groupedExport;
if (usesAGRP)
{
    //groupedExport = ScriptQuestion("Group sounds by audio group?");
    groupedExport = true;
}

byte[] EMPTY_WAV_FILE_BYTES = System.Convert.FromBase64String("UklGRiQAAABXQVZFZm10IBAAAAABAAIAQB8AAAB9AAAEABAAZGF0YQAAAAA=");
string DEFAULT_AUDIOGROUP_NAME = "audiogroup_default";

Directory.CreateDirectory(exportedSoundsDir);

int exportedAudio = 0;
await Task.Run(DumpAudios); // This runs sync, because it has to load audio groups.

if (exportedAudio == 0)
    Log.Information("No audio file found.");
else
    Log.Information("All audio has been exported to " + Path.GetFullPath(exportedSoundsDir));

void DumpAudios()
{
    //MakeFolder("Exported_Sounds");
    foreach (UndertaleSound sound in Data.Sounds)
        DumpAudio(sound);
}

void DumpAudio(UndertaleSound sound)
{
    string soundName = sound.Name.Content;
    bool flagCompressed = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsCompressed);
    bool flagEmbedded = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsEmbedded);
    // Compression, Streamed, Unpack on Load.
    // 1 = 000 = IsEmbedded, Regular.               '.wav' type saved in win.
    // 2 = 100 = IsCompressed, Regular.             '.ogg' type saved in win
    // 3 = 101 = IsEmbedded, IsCompressed, Regular. '.ogg' type saved in win.
    // 4 = 110 = Regular.                           '.ogg' type saved outside win.
    string audioExt = ".ogg";
    string soundFilePath;
    if (groupedExport)
        soundFilePath = Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content, soundName);
    else
        soundFilePath = Path.Combine(exportedSoundsDir, soundName);
    Directory.CreateDirectory(exportedSoundsDir);
    if (groupedExport)
        Directory.CreateDirectory(Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content));

    bool process = true;
    if (flagEmbedded && !flagCompressed) // 1.
        audioExt = ".wav";
    else if (flagCompressed && !flagEmbedded) // 2.
        audioExt = ".ogg";
    else if (flagCompressed && flagEmbedded) // 3.
        audioExt = ".ogg";
    else if (!flagCompressed && !flagEmbedded)
    {
        process = false;
        audioExt = ".ogg";
        string source = Path.Combine(winFolder, soundName + audioExt);
        string dest = Path.Combine(winFolder, "External_Sounds", soundName + audioExt);
        if (externalOGG_Copy)
        {
            if (groupedExport)
            {
                dest = Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content, soundName + audioExt);
                Directory.CreateDirectory(Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content));
            }
            Directory.CreateDirectory(exportedSoundsDir);
            File.Copy(source, dest, false);
        }
    }
    if (process && !File.Exists(soundFilePath + audioExt))
    {
        File.WriteAllBytes(soundFilePath + audioExt, GetSoundData(sound));
        Log.Information("Exported " + soundName);
        ++exportedAudio;
    }
}

Dictionary<string, IList<UndertaleEmbeddedAudio>> loadedAudioGroups;
IList<UndertaleEmbeddedAudio> GetAudioGroupData(UndertaleSound sound)
{
    if (loadedAudioGroups is null)
        loadedAudioGroups = new Dictionary<string, IList<UndertaleEmbeddedAudio>>();

    string audioGroupName = sound.AudioGroup is not null ? sound.AudioGroup.Name.Content : DEFAULT_AUDIOGROUP_NAME;
    if (loadedAudioGroups.ContainsKey(audioGroupName))
        return loadedAudioGroups[audioGroupName];

    string groupFilePath = Path.Combine(winFolder, "audiogroup" + sound.GroupID + ".dat");
    if (!File.Exists(groupFilePath))
        return null; // Doesn't exist.

    try
    {
        UndertaleData data = null;
        using (var stream = new FileStream(groupFilePath, FileMode.Open, FileAccess.Read))
            data = UndertaleIO.Read(stream, warning => Log.Warning("A warning occured while trying to load " + audioGroupName + ":\n" + warning));

        loadedAudioGroups[audioGroupName] = data.EmbeddedAudio;
        return data.EmbeddedAudio;
    } 
    catch (Exception e)
    {
        Log.Error("An error occured while trying to load " + audioGroupName + ":\n" + e.Message);
        return null;
    }
}

byte[] GetSoundData(UndertaleSound sound)
{
    if (sound.AudioFile is not null)
        return sound.AudioFile.Data;

    if (sound.GroupID > Data.GetBuiltinSoundGroupID())
    {
        IList<UndertaleEmbeddedAudio> audioGroup = GetAudioGroupData(sound);
        if (audioGroup is not null)
            return audioGroup[sound.AudioID].Data;
    }
    return EMPTY_WAV_FILE_BYTES;
}
