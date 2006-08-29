#!/bin/sh
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de, http://www.gnandt.com/)

echo
echo "*** opendicom-sharp ***"
echo
echo "(lines) (words) (file)"
wc -lw `find opendicom-sharp/src -name *.cs`
echo
echo "*** opendicom-utils ***"
echo
echo "(lines) (words) (file)"
wc -lw `find opendicom-utils/src -name *.cs`
echo
echo "*** opendicom-navigator ***"
echo
echo "(lines) (words) (file)"
wc -lw `find opendicom-navigator/src -name *.cs`
echo

