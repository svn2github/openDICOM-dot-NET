#!/bin/sh
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE="opendicom-beagle"
LIB="opendicom-sharp"
DOC="opendicom-sharp-doc"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

case "$1" in
    beagle)
        DIR="${BEAGLE}_deb"
	;;
    lib)
        DIR="${LIB}_deb"
        ;;
    doc)
        DIR="${DOC}_deb"
        ;;
    utils)
        DIR="${UTILS}_deb"
        ;;
    navi)
        DIR="${NAVI}_deb"
        ;;
    *)
        echo "usage: sh checksum.sh {beagle|lib|doc|utils|navi}"
        exit 1
        ;;
esac

pushd $PWD > /dev/null
cd $DIR
find usr -type f -print0 | xargs -0 md5sum > DEBIAN/md5sum
popd > /dev/null

exit 0
