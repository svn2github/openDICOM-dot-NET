#!/bin/sh
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de, http://www.gnandt.com/)

echo
echo "*** opendicom-sharp ***"
echo
grep -n -R "TODO:" `find opendicom-sharp/src -name *.cs` | sed "s/[ \\/]*TODO://"
echo
echo "*** opendicom-utils ***"
grep -n -R "TODO:" `find opendicom-utils/src -name *.cs` | sed "s/[ \\/]*TODO://"
echo
echo "*** opendicom-navigator ***"
grep -n -R "TODO:" `find opendicom-navigator/src -name *.cs` | sed "s/[ \\/]*TODO://"
echo

