#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id $

BEAGLE="opendicom-beagle"
LIB="opendicom-sharp"
DOC="opendicom-sharp-doc"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"
GOBOSH="Gobosh.Dicom"


bundle_sources()
{
    # creates <subproject>_<release>.zip or .tar.gz with
    # <subproject>_<release> as root directory

    if [ "$1" != "--tgz" ] && [ "$1" != "--zip" ]; then
        print_usage
        exit 1
    fi

    RELEASE=`cat $2/release`
    NAME="${2}_${RELEASE}"

    CURRENT_DIR=$PWD

    cp -R $2 /tmp/
    cd /tmp
    mv $2 $NAME

    case "$1" in
        --tgz)
            BUNDLE="$NAME.tar.gz"
            tar cvzf $BUNDLE $NAME --exclude=.svn --exclude=*.pidb > /dev/null
            ;;
        --zip)
            BUNDLE="$NAME.zip"
            zip -q -r $BUNDLE $NAME -x *.svn* -x *.pidb
            ;;
    esac

    rm -Rf $NAME

    cd $CURRENT_DIR

    mv /tmp/$BUNDLE .
}


print_usage()
{
    echo "usage: bash bundle-project.sh {--tgz|--zip} {beagle|lib|doc|utils|navi|gobosh}"
}


case "$2" in
    beagle)
        bundle_sources $1 $BEAGLE
        ;;
    lib)
        bundle_sources $1 $LIB
        ;;
    doc)
        bundle_sources $1 $DOC
        ;;
    utils)
        bundle_sources $1 $UTILS
        ;;
    navi)
        bundle_sources $1 $NAVI
        ;;
    gobosh)
        bundle_sources $1 $GOBOSH
        ;;
    *)
        print_usage
        exit 1
        ;;
esac

exit 0
