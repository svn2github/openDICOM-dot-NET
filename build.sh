#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE="opendicom-beagle"
LIB="opendicom-sharp"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

case "$1" in
    beagle)
        make -C $BEAGLE clean build &&\
        sudo make -C $BEAGLE uninstall install
        ;;
    lib)
        make -C $LIB clean build &&\
        sudo make -C $LIB uninstall install
        ;;
    utils)
        make -C $UTILS clean build &&\
        sudo make -C $UTILS uninstall install
        ;;
    navi)
        make -C $NAVI clean build &&\
        sudo make -C $NAVI uninstall install
        ;;
    *)
        echo "usage: sh build.sh {beagle|lib|utils|navi}"
        exit 1
        ;;
esac

exit 0
