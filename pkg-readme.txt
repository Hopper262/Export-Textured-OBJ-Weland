Export Textured OBJ 1.0
-----------------------
by Hopper

----------------------------------------------------------------
DESCRIPTION:

This Weland plugin exports level geometry as a Wavefront OBJ file, similar to the built-in feature, but it also produces an .mtl file and texture information so that fully-textured levels can be loaded and viewed in other programs.

Source code for this plugin is available at:

https://github.com/Hopper262/Export-Textured-OBJ-Weland

----------------------------------------------------------------
INSTALLATION:

Move "Export_Textured_OBJ.dll" to a "Plugins" directory in the same directory as Weland. You can create that directory if it doesn't already exist.

Once it's installed, launch Weland (if Weland was already running, quit and relaunch it), open the level you wish to export, and select "Export Textured OBJ..." from the "Plugins" menu. It will create both an .obj and a .mtl file at the destination.

----------------------------------------------------------------
LOADING TEXTURED MODELS:

The textures themselves are not read by Weland, and the exported models use file references with a standard naming scheme. Bitmap 6 in texture collection collection 18 is expected to be found at:

    18/bitmap006.bmp

To use textures from a Shapes file, create a folder alongside your .obj file with the collection number (for example, "18"). Then in ShapeFusion, go to the bitmaps for that collection and choose "Export all bitmaps..." Choose your newly-created folder as the export destination. After that, you should be set to open the .obj in a model viewer and see the textures.

To use replacement texture files, you can either convert them to .bmp format and store them in the appropriate spot, or you can edit the .mtl file in a text editor to change the paths. Make sure your model viewer can read your chosen texture format; the .bmp and .png formats are widely supported.

----------------------------------------------------------------
CHANGELOG:

v1.0:
* First release

----------------------------------------------------------------
CONTACT:

If you have any questions, comments, or bugs to report, open an issue on GitHub:
https://github.com/Hopper262/Export-Textured-OBJ-Weland
