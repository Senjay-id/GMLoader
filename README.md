GMLoader is a program that can load CSharp Script files and recompile non-YYC GM:Studio games using [UndertalModLib](https://github.com/krzys-h/UndertaleModTool). It will make a backup of your data files to ensure that you'll never corrupt your game data while recompiling. 


You will only need to run the program once everytime you install/remove a mod that requires GMLoader. This will save your time in recompiling the game's data.

# Features
* Adding or Replacing GML assets by loading the gml files inside GameFolder/mods/code/*.gml
* Supports Gamemaker's assembly recompiling by loading the asm files inside GameFolder/mods/code/asm/*.asm
* Append GML code by importing the gml files inside GameFolder/mods/code/appendgml/Any Folder Name/*.gml
* Existing GameObjects Manipulation by loading configuration files inside GameFolder/mods/config/existing_object/*.json
* Add and Manipulate new GameObjects by loading configuration files inside GameFolder/mods/config/new_object/*.json
* Import shaders by loading exported shader files inside GameFolder/mods/shader/*any_shader_folder
* Load your own custom CSharp Script files inside GameFolder/csx/pre/*.csx or GameFolder/csx/post/*.csx
* Add or replace collision event code by loading the gml files inside GameFolder/mods/code/collision/*.gml
* Append collision event code by loading the gml files inside GameFolder/mods/code/appendgml/collision/Any Folder Name/*.gml

# Yet to be added
* Importing Room

# Wiki
TBA

# Building
* Clone the project
* Compile [UndertalModLib](https://github.com/krzys-h/UndertaleModTool) and [xxHash](https://github.com/uranium62/xxHash) then put their dll inside the lib folder
* Open the solution and build

# Credits
[UTMT Team](https://github.com/krzys-h/UndertaleModTool) - All of this isn't possible without the UTMT tool and library

[GMLML](https://github.com/BlurOne-GIT/GML-Mod-Loader) - For the ProofOfConcept Code

Eldoofus - Helping my skill issues

[Crusenho](https://crusenho.itch.io/) - Executable Icon
