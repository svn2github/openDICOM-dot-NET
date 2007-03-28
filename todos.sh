#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

echo
echo "*** opendicom-beagle ***"
echo
grep -n -R "TODO:" `find opendicom-beagle/src -name *.cs` | sed "s/[ \\/]*TODO://"
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

