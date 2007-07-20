#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE="opendicom-beagle"
LIB="opendicom-sharp"
DOC="opendicom-sharp-doc"
GDK="opendicom-sharp-gdk"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"


case "$1" in
    beagle)
        NAME=$BEAGLE 
	;;
    lib)
        NAME=$LIB
        ;;
    doc)
        NAME=$DOC
        ;;
    gdk)
        NAME=$GDK
        ;;
    utils)
        NAME=$UTILS
        ;;
    navi)
        NAME=$NAVI
        ;;
    *)
        echo "usage: bash build-deb.sh {beagle|lib|doc|gdk|utils|navi}"
        exit 1
        ;;
esac


RELEASE=`cat $NAME/release`
PACKAGE="${NAME}_${RELEASE}_all.deb"
DIR="${NAME}_deb"

# use temp work dir
pushd $PWD > /dev/null
cp -R $DIR /tmp/

# remove subversion context
cd /tmp/$DIR
rm -Rf `find -name .svn`

# md5sum of all files
find usr -type f -print0 | xargs -0 md5sum > DEBIAN/md5sum

# set installed size
cd ..
SIZE=`du -s $DIR | awk '{ print $1 }'`
cat $DIR/DEBIAN/control | sed "s/^Version:.*$/Version: $RELEASE/" > $DIR/DEBIAN/control
cat $DIR/DEBIAN/control | sed "s/^Installed-Size:.*$/Installed-Size: $SIZE/" > $DIR/DEBIAN/control

# build package
dpkg -b $DIR $PACKAGE > /dev/null

# copy and clear
rm -Rf $DIR
popd > /dev/null
mv /tmp/$PACKAGE .


exit 0
