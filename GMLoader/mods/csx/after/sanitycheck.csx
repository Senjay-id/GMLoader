Log.Information("Executing sanity check for corrupted sprites");

bool hasError = false;
int i = 0;

foreach (var spriteName in spriteList)
{
    UndertaleSprite corruptedSpritePotential = Data.Sprites.ByName(spriteName);

    if (corruptedSpritePotential == null)
    {
        hasError = true;
        i++;
        Log.Error($"Error, failed to import {spriteName} to the data");
    }

}

if (hasError)
{
    Log.Error($"Warning there's {i} sprite files failed to be imported, press any key to continue...");
    Console.ReadKey();
}
