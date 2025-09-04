// // Original script by Kneesnap, updated by Grossley, modified by Senjay for GMLoader
string exportedSoundsDir = exportAudioOutputPath;

mkDir(exportedSoundsDir);
mkDir(exportAudioConfigOutputPath);

// Prompt for export settings.
//bool copyExternalAudio = ScriptQuestion("Export external audio files as well? (Will copy to a separate folder.)");
bool copyExternalAudio = true;
bool groupedExport = false;
if ((Data.AudioGroups?.Count ?? 0) > 0)
{
    // groupedExport = ScriptQuestion("Group sounds by audio group?");
    groupedExport = true;
}

byte[] EMPTY_WAV_FILE_BYTES = System.Convert.FromBase64String("UklGRiQAAABXQVZFZm10IBAAAAABAAIAQB8AAAB9AAAEABAAZGF0YQAAAAA=");
string DEFAULT_AUDIOGROUP_NAME = "audiogroup_default";

DumpSounds(); // This runs synchronously, because it has to load audio groups.

Dictionary<string, IList<UndertaleEmbeddedAudio>> loadedAudioGroups = null;
IList<UndertaleEmbeddedAudio> GetAudioGroupData(UndertaleSound sound)
{
    loadedAudioGroups ??= new();

    // Try getting cached audio group by name.
    string audioGroupName = sound.AudioGroup is not null ? sound.AudioGroup.Name.Content : DEFAULT_AUDIOGROUP_NAME;
    if (loadedAudioGroups.ContainsKey(audioGroupName))
    {
        return loadedAudioGroups[audioGroupName];
    }

    // Not cached, so try locating audiogroup file.
    string relativeAudioGroupPath;
    if (sound.AudioGroup is UndertaleAudioGroup { Path.Content: string customRelativePath })
    {
        relativeAudioGroupPath = customRelativePath;
    }
    else
    {
        relativeAudioGroupPath = $"audiogroup{sound.GroupID}.dat";
    }
    string groupFilePath = Path.Combine(relativeAudioGroupPath);
    if (!File.Exists(groupFilePath))
    {
        // Doesn't exist... don't try loading.
        return null;
    }

    // Load data file.
    try
    {
        UndertaleData data = null;
        using (var stream = new FileStream(groupFilePath, FileMode.Open, FileAccess.Read))
        {
            data = UndertaleIO.Read(stream, (warning, _) => Log.Warning($"A warning occured while trying to load {audioGroupName}:\n{warning}"));
        }

        loadedAudioGroups[audioGroupName] = data.EmbeddedAudio;
        return data.EmbeddedAudio;
    } 
    catch (Exception e)
    {
        Log.Error($"An error occured while trying to load {audioGroupName}:\n{e.Message}");
        return null;
    }
}

byte[] GetSoundData(UndertaleSound sound)
{
    // Try to get audio directly, if embedded in main file.
    if (sound.AudioFile is not null)
    {
        return sound.AudioFile.Data;
    }

    // Try to get audio from its audiogroup.
    if (sound.GroupID > Data.GetBuiltinSoundGroupID())
    {
        IList<UndertaleEmbeddedAudio> audioGroup = GetAudioGroupData(sound);
        if (audioGroup is not null)
        {
            return audioGroup[sound.AudioID].Data;
        }
    }

    // All attempts to get data failed; just use empty WAV data.
    return EMPTY_WAV_FILE_BYTES;
}

void DumpSounds()
{
    foreach (UndertaleSound sound in Data.Sounds)
    {
        if (sound is not null)
        {
            DumpSound(sound);
        }
        // else
        // {
        //     IncProgressLocal();
        // }
    }
}

void DumpSound(UndertaleSound sound)
{
    // Determine output audio file path.
    string soundName = sound.Name.Content;
    Log.Information($"Exporting {soundName}");
    string soundFilePath;
    if (groupedExport)
    {
        //soundFilePath = Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content, soundName);
        //mkDir(Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content));
        soundFilePath = Path.Combine(exportedSoundsDir, soundName); //GMLoader has no way to import external audiogroup
        //Directory.CreateDirectory(Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content));
    }
    else
    {
        soundFilePath = Path.Combine(exportedSoundsDir, soundName);
    }

    // Determine output file type.
    bool flagCompressed = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsCompressed);
    bool flagEmbedded = sound.Flags.HasFlag(UndertaleSound.AudioEntryFlags.IsEmbedded);
    string audioExt = ".ogg";
    bool isEmbedded = true;
    if (flagEmbedded && !flagCompressed)
    {
        // IsEmbedded, Regular: WAV, embedded.
        audioExt = ".wav";
    }
    else if (flagCompressed && !flagEmbedded)
    {
        // IsCompressed, Regular: OGG, embedded.
        audioExt = ".ogg";
    }
    else if (flagCompressed && flagEmbedded)
    {
        // IsEmbedded, IsCompressed, Regular: OGG, embedded.
        audioExt = ".ogg";
    }
    else if (!flagCompressed && !flagEmbedded)
    {
        // Regular: OGG, external.
        isEmbedded = false;
        audioExt = ".ogg";

        // Only copy external audio if enabled.
        if (copyExternalAudio)
        {
            string externalFilename = sound.File.Content;
            if (!externalFilename.Contains('.'))
            {
                // Add file extension if none already exists (assume OGG).
                externalFilename += ".ogg";
            }
            string sourcePath = Path.Combine(externalFilename);
            string destPath;
            if (groupedExport)
            {
                destPath = Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content, "external", soundName + audioExt);
                Directory.CreateDirectory(Path.Combine(exportedSoundsDir, sound.AudioGroup.Name.Content, "external"));
            }
            else
            {
                destPath = Path.Combine(exportedSoundsDir, "external", soundName + audioExt);
                Directory.CreateDirectory(Path.Combine(exportedSoundsDir, "external"));
            }
            File.Copy(sourcePath, destPath, true);
        }
    }
    if (isEmbedded)
    {
        File.WriteAllBytes(soundFilePath + audioExt, GetSoundData(sound));
    }

    var config = new AudioData
    {
        yml_type = audioExt,
        yml_embedded = isEmbedded,
        yml_compressed = flagCompressed,
        yml_effects = sound.Effects,
        yml_volume = sound.Volume,
        yml_pitch = sound.Pitch,
        yml_audiogroup_index = sound.GroupID,
        yml_audiofile_id = sound.AudioID,
        yml_preload = sound.Preload
    };
    var data = new Dictionary<string, AudioData>
    {
        [soundName] = config
    };
    var yamlBytes = YamlSerializer.Serialize(data);
    string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
    string configFileName = Path.Combine(exportAudioConfigOutputPath, soundName + ".yaml");

    File.WriteAllText(configFileName, yaml);
}
