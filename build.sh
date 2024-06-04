#!/bin/sh
# WELAND="$HOME/Source/weland-svn/Weland.exe"
WELAND="/Applications/Games/Aleph One/Utilities/Weland.app/Contents/MacOS/Weland.exe"
PATH="/Library/Frameworks/Mono.framework/Commands:$PATH" \
PKG_CONFIG_PATH="/Library/Frameworks/Mono.framework/Libraries/pkgconfig" \
  mcs -debug -r:"$WELAND" \
  -pkg:gtk-sharp-2.0 \
  -target:library \
  -out:Export_Textured_OBJ.dll \
  HopOBJExporter.cs Plugin.cs \
  && \
  cp Export_Textured_OBJ.dll "$HOME/.config/Weland/Plugins/Export_Textured_OBJ.dll"
