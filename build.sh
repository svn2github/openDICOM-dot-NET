#!/bin/sh
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de, http://www.gnandt.com/)


LIB="opendicom-sharp"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

case "$1" in
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
        echo "usage: sh build.sh {lib|utils|navi}"
        exit 1
        ;;
esac

exit 0
