#!/bin/sh
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE="opendicom-beagle"
LIB="opendicom-sharp"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

case "$1" in
    beagle)
        gedit --new-window `find $BEAGLE/src -name *.cs` &
	;;
    lib)
        gedit --new-window `find $LIB/src -name *.cs | grep -v Encoding | grep -v AssemblyInfo.cs` &
        gedit --new-window `find $LIB/src -name *.cs | grep Encoding | grep -v AssemblyInfo.cs` &
        gedit --new-window `find $LIB/src -name AssemblyInfo.cs` &
        ;;
    utils)
        gedit --new-window `find $UTILS/src -name *.cs | grep -v AssemblyInfo.cs` &
        gedit --new-window `find $UTILS/src -name AssemblyInfo.cs` &
        ;;
    navi)
        gedit --new-window `find $NAVI/src -name *.cs | grep -v AssemblyInfo.cs` &
        gedit --new-window `find $NAVI/src -name AssemblyInfo.cs` &
        ;;
    *)
        echo "usage: sh edit.sh {beagle|lib|utils|navi}"
        exit 1
        ;; 
esac

exit 0
