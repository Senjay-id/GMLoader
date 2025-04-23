Log.Information("Executing sanity check for corrupted sprites");

bool hasSpriteError = false;
bool hasBGError = false;
int i = 0;
int v = 0;

foreach (var spriteName in spriteList)
{
    UndertaleSprite corruptedSpritePotential = Data.Sprites.ByName(spriteName);

    if (corruptedSpritePotential == null)
    {
        hasSpriteError = true;
        i++;
        Log.Error($"Error, failed to import {spriteName} to the data");
    }
}

foreach (var bgName in backgroundList)
{
    UndertaleBackground corruptedBGPotential = Data.Backgrounds.ByName(bgName);

    if (corruptedBGPotential == null)
    {
        hasBGError = true;
        v++;
        Log.Error($"Error, failed to import {bgName} to the data");
    }
}

if (hasSpriteError || hasBGError)
{
    if (hasSpriteError)
        Log.Error($"Warning there's {i} sprite files failed to be imported.");

    if (hasBGError)
        Log.Error($"Warning there's {v} background sprite files failed to be imported.");

    Log.Error("Press any key to continue...");

    Console.ReadKey();
}
else
{
    Log.Information("All good.");
}


