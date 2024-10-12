string vanillaSoundBankDir = "./sound/Desktop";
string xdeltaSoundBankDir = "./mods/xdelta";

Directory.CreateDirectory(xdeltaSoundBankDir);

string[] bankFiles = Directory.GetFiles(vanillaSoundBankDir, "*.bank");
string[] xdeltaFiles = Directory.GetFiles(xdeltaSoundBankDir, "*.xdelta");

foreach (string bankFile in bankFiles)
{
    string backupFile = Path.Combine(bankFile + ".BACKUP");
    if (!File.Exists(backupFile))
    {
        File.Copy(bankFile, backupFile, false); // Don't overwrite
        Log.Information($"Created backup file for {bankFile}");
    }
        
    string bankName = Path.GetFileNameWithoutExtension(bankFile);
    string xdeltaFile = Path.Combine(xdeltaSoundBankDir, Path.GetFileName(bankFile) + ".xdelta");

    if (File.Exists(xdeltaFile))
    {
        try
        {
            applyDeltaPatch(backupFile, xdeltaFile, bankFile);
            Log.Information($"Patched {Path.GetFileName(bankFile)}");
        }
        catch (Exception ex)
        {
            invalidXdeltaNames.Add(Path.GetFileName(bankFile));
            invalidXdelta++;
            Log.Error($"An unexpected error occurred while patching {Path.GetFileName(bankFile)}: {ex.Message}");
        }
    }
    else
    {
        Log.Debug($"xdelta file doesn't exist for {Path.GetFileName(bankFile)}");
    }
}
