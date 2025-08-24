//From umt import script, modified for GMLoader

//Cache
string _type = defaultAudioType;
bool _embedded = defaultAudioEmbedded;
bool _compressed = defaultAudioCompressed;
uint _effects = defaultAudioEffects;
float _volume = defaultAudioVolume;
float _pitch = defaultAudioPitch;
int _audiogroup_index = defaultAudioGroupIndex;
int _audiofile_id = defaultAudioFileID;
bool _preload = defaultAudioPreload;

UndertaleEmbeddedAudio audioFile = null;
int audioID = -1;
int audioGroupID = -1;
int embAudioID = -1;
bool usesAGRP = (Data.AudioGroups.Count > 0);

if (!usesAGRP)
{
    Log.Debug("This game doesn't use audiogroups.\nImporting to external audiogroups is disabled.");
}

string importFolder = audioPath;
List<string> dirFilesList = new List<string>(Directory.GetFiles(importFolder));

foreach (var audioEntry in audioDictionary)
{
    
    bool audioFileExists = false;
    string audioFilePath = "";

    for (int i = 0; i < dirFilesList.Count; i++)
    {
        string filename = Path.GetFileNameWithoutExtension(dirFilesList[i]);

        if (filename == audioEntry.Key)
        {
            audioFileExists = true;
            audioFilePath = dirFilesList[i];
            
            // Remove the file immediately
            dirFilesList.RemoveAt(i);
            i--; // Adjust index after removal
            
            break;
        }
    }

    string audioName = audioEntry.Key;
    AudioData audioData = audioEntry.Value;

    Log.Information($"Importing {audioName}");

    _type = audioData.yml_type;
    _embedded = (bool)audioData.yml_embedded;
    _compressed = (bool)audioData.yml_compressed;
    _effects = (uint)audioData.yml_effects;
    _volume = (float)audioData.yml_volume;
    _pitch = (float)audioData.yml_pitch;
    _audiogroup_index = (int)audioData.yml_audiogroup_index;
    _audiofile_id = (int)audioData.yml_audiofile_id;
    _preload = (bool)audioData.yml_preload;

    bool isOGG = _type == ".ogg";
    if (!isOGG)
    {
        // WAV cannot be external
        _embedded = true;
        _compressed = false;
    }

    bool needAGRP = true;

    if (_embedded)
        audioGroupID = _audiogroup_index;

    // Search for an existing sound with the given name.
    UndertaleSound existingSound = Data.Sounds.ByName(audioName);

    // If this is an existing sound, use its audio group ID.
    if (existingSound is not null)
    {
        audioGroupID = existingSound.GroupID;
    }

    // If the audiogroup ID is for the builtin audiogroup ID, it's embedded in the main data file and doesn't need to be loaded.
    if (audioGroupID == Data.GetBuiltinSoundGroupID())
    {
        needAGRP = false;
    }

    // Create embedded audio entry if required.
    UndertaleEmbeddedAudio soundData = null;
    if (_embedded && audioFileExists)
    {
        soundData = new UndertaleEmbeddedAudio() { Data = File.ReadAllBytes(audioFilePath) };
        Data.EmbeddedAudio.Add(soundData);
        if (existingSound is not null)
        {
            Data.EmbeddedAudio.Remove(existingSound.AudioFile);
        }
        embAudioID = Data.EmbeddedAudio.Count - 1;
    }

    // Update external audio group file if required.
    if (_embedded && _audiogroup_index != 0)
    {
        // Load audiogroup into memory.
        UndertaleData audioGroupDat;
        string relativeAudioGroupPath;
        if (audioGroupID < Data.AudioGroups.Count && Data.AudioGroups[audioGroupID] is UndertaleAudioGroup { Path.Content: string customRelativePath })
        {
            relativeAudioGroupPath = customRelativePath;
        }
        else
        {
            relativeAudioGroupPath = $"audiogroup{audioGroupID}.dat";
        }
        string audioGroupPath = Path.Combine(relativeAudioGroupPath);
        using (FileStream audioGroupReadStream = new(audioGroupPath, FileMode.Open, FileAccess.Read))
        {
            audioGroupDat = UndertaleIO.Read(audioGroupReadStream);
        }

        // Add the EmbeddedAudio entry to the audiogroup data.
        audioGroupDat.EmbeddedAudio.Add(soundData);
        if (existingSound is not null)
        {
            audioGroupDat.EmbeddedAudio.Remove(existingSound.AudioFile);
        }
        audioID = audioGroupDat.EmbeddedAudio.Count - 1;

        // Write audio group back to disk.
        using FileStream audioGroupWriteStream = new(audioGroupPath, FileMode.Create);
        UndertaleIO.Write(audioGroupWriteStream, audioGroupDat);
    }

    // Determine sound flags.
    UndertaleSound.AudioEntryFlags flags = UndertaleSound.AudioEntryFlags.Regular;
    if (_compressed)
        flags |= UndertaleSound.AudioEntryFlags.IsCompressed;
    if (_embedded)
        flags |= UndertaleSound.AudioEntryFlags.IsEmbedded;
    else
        audioID = -1;

    // Determine final embedded audio reference (or null).
    UndertaleEmbeddedAudio finalAudioReference = null;
    if (!_embedded)
    {
        finalAudioReference = null;
    }
    if (_embedded && !needAGRP)
    {
        finalAudioReference = Data.EmbeddedAudio[embAudioID];
    }
    if (_embedded && needAGRP)
    {
        finalAudioReference = null;
    }

    // Determine final audio group reference (or null).
    UndertaleAudioGroup finalGroupReference = null;
    if (!usesAGRP)
    {
        finalGroupReference = null;
    }
    else
    {
        finalGroupReference = needAGRP ? Data.AudioGroups[audioGroupID] : Data.AudioGroups[Data.GetBuiltinSoundGroupID()];
    }

    // Update/create actual sound asset.
    if (existingSound is null)
    {
        UndertaleSound newSound = new()
        {
            Name = Data.Strings.MakeString(audioName),
            Flags = flags,
            Type = Data.Strings.MakeString(_type),
            File = audioFileExists ? Data.Strings.MakeString(audioFilePath) : Data.Strings.MakeString(audioName), // This only tells it where to look for the file is the audio is not embedded
            Effects = _effects,
            Volume = _volume,
            Pitch = _pitch,
            AudioID = _audiofile_id,
            AudioFile = finalAudioReference,
            AudioGroup = Data.AudioGroups[_audiogroup_index],
            GroupID = needAGRP ? audioGroupID : Data.GetBuiltinSoundGroupID(),
            Preload = _preload
        };
        Data.Sounds.Add(newSound);
    }
    else
    {
        existingSound.Flags = flags;
        existingSound.Type = Data.Strings.MakeString(_type);
        existingSound.File = audioFileExists ? Data.Strings.MakeString(audioFilePath) : Data.Strings.MakeString(audioName); // This only tells it where to look for the file is the audio is not embedded
        existingSound.Effects = _effects;
        existingSound.Volume = _volume;
        existingSound.Pitch = _pitch;
        existingSound.AudioID = _audiofile_id;
        existingSound.AudioFile = finalAudioReference;
        existingSound.AudioGroup = Data.AudioGroups[_audiogroup_index];
        existingSound.GroupID = needAGRP ? audioGroupID : Data.GetBuiltinSoundGroupID();
        existingSound.Preload = _preload;
    }
}
