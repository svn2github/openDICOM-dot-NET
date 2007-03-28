#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

echo
echo "*** opendicom-beagle ***"
echo
echo "(lines) (words) (file)"
wc -lw `find opendicom-beagle/src -name *.cs`
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

