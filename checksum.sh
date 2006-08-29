#!/bin/sh
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de, http://www.gnandt.com/)

LIB="opendicom-sharp"
DOC="opendicom-sharp-doc"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

case "$1" in
    lib)
        DIR="${LIB}_${2}_deb"
        ;;
    doc)
        DIR="${DOC}_${2}_deb"
        ;;
    utils)
        DIR="${UTILS}_${2}_deb"
        ;;
    navi)
        DIR="${NAVI}_${2}_deb"
        ;;
    *)
        echo "usage: sh checksum.sh {lib|doc|utils|navi} <version>"
        exit 1
        ;;
esac

pushd $PWD > /dev/null
cd $DIR
find usr -type f -print0 | xargs -0 md5sum > DEBIAN/md5sum
popd > /dev/null

exit 0
