GMLoader is a program that can recompile non-YYC GM:Studio games data using [UndertalModLib](https://github.com/krzys-h/UndertaleModTool).

### Features
* Adding or replacing GML assets by loading the gml files inside GameFolder/mods/code/*.gml
* Adding or replacing textures by loading the texture files inside GameFolder/mods/textures/*.png
* Manipulate sprite properties by making sprite configuration files inside GameFolder/mods/config/textures_properties/*.yaml
* Append GML code by importing the gml files inside GameFolder/mods/code/appendgml/Any Folder Name/*.gml
* Prepend GML code by importing the gml files inside GameFolder/mods/code/prependgml/Any Folder Name/*.gml
* Existing GameObjects Manipulation by loading configuration files inside GameFolder/mods/config/existing_object/*.json
* Add and Manipulate new GameObjects by loading configuration files inside GameFolder/mods/config/new_object/*.json
* Import shaders by loading exported shader files inside GameFolder/mods/shader/*any_shader_folder
* Load your own custom CSharp Script files inside GameFolder/csx/pre/*.csx or GameFolder/csx/post/*.csx

### Yet to be added
* Importing Room

### [Wiki](https://github.com/Senjay-id/GMLoader/wiki)
Contains User guide and Modders guide

### Building
* Clone the project
* Compile [UndertalModLib and Underanalyzer](https://github.com/krzys-h/UndertaleModTool) then put their dll inside the lib folder
* Open the solution and build

### Credits
[UTMT Team](https://github.com/krzys-h/UndertaleModTool) - All of this isn't possible without the UTMT tool and library

[GMLML](https://github.com/BlurOne-GIT/GML-Mod-Loader) - For the ProofOfConcept Code

Eldoofus - Helping my skill issues

[Crusenho](https://crusenho.itch.io/) - Executable Icon
