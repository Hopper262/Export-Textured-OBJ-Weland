#!/bin/sh
if [ -d 5D_Intersections ]; then rm -r 5D_Intersections; fi
if [ -f 5D_Intersections.zip ]; then rm 5D_Intersections.zip; fi
mkdir 5D_Intersections
cp 5D_Intersections.dll 5D_Intersections/
cp pkg-readme.txt 5D_Intersections/"Read Me.txt"
zip -r -X 5D_Intersections.zip 5D_Intersections
rm -r 5D_Intersections
