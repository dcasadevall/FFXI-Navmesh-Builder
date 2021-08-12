# FFXI Navmesh Builder
Using source from https://github.com/xenonsmurf/Ffxi_Navmesh_Builder, this application should allow users to generate nav meshes from OBJ files

This project is intended to be a simplification of user interface for generating OBJ files / NavMeshes with default settings that are recommended for most navmeshes.

## Functionality

* Generate all OBJ files for all FFXI Zones from a predefined FFXI Install path
* Generate NavMesh files for all obj files generated previously
* Only generate obj / NavMesh files when the file is not present. Making it easier to batch

## Architectural changes

* All file generation is separate from View management, and encapsulated under interfaces
* Use DI to construct all objects, pushing dependencies up to the composition root
* Greatly simplified UI Layout so one can generate OBJ files / Meshes in batch

## TODO

* Ability to modify settings
* Ability to set custom FFXI Path
* Ability to choose input / output folders
