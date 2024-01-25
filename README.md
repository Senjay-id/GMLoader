GMLoader is a program that can load CSharp Script files and recompile non-YYC GM:Studio games. It will make a backup of your data files to ensure that you'll never corrupt your game data while recompiling. The builtin scripts currently allow you to add/replace GML/ASM/Room assets. It uses UndertaleMod library from UTMT and xxHash to hash the data.win. 

You will only need to run the program once everytime you install/remove a mod that requires GMLoader. This will save your time in recompiling the game's data.

# Features
* Adding or Replacing GML assets by loading the gml files inside GameFolder/mods/code/*.gml
* Supports Gamemaker's assembly recompiling by loading the asm files inside GameFolder/mods/code/asm/*.asm
* Experimental Room Compiling by loading the exported room json files inside GameFolder/mods/rooms/*.json
* Existing GameObjects Manipulation by loading configuration files inside GameFolder/mods/config/existing_object/*.json
* Add and Manipulate new GameObjects by loading configuration files inside GameFolder/mods/config/new_object/*.json
* Load your own custom CSharp Script files inside GameFolder/csx/*.csx

# Yet to be added
* Fixing Room compiling
* Extension Injection script

# Credits
[UTMT Team](https://github.com/krzys-h/UndertaleModTool) - All of this isn't possible without the UTMT tool and library

[GMLML](https://github.com/BlurOne-GIT/GML-Mod-Loader) - For the ProofOfConcept Code

Eldoofus - Helping my skill issues

[Crusenho](https://crusenho.itch.io/) - Executable Icon
