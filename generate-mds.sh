#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id: $

BEAGLE="opendicom-beagle"
LIB="opendicom-sharp"
GDK="opendicom-sharp-gdk"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

BEAGLE_HOME="/usr/lib/beagle"

MDP_TEMPLATE="opendicom.mdp.template"
MDS_TEMPLATE="opendicom.mds.template"


generate_mdp()
{
    FILES=`find $1/src -name *.cs | \
        sed 's#^'"$1"'/##' | \
        awk '{ print "<File name=\"" $0 "\" subtype=\"Code\" buildaction=\"Compile\" />" }'`
    # replace file list seperators
    FILES=`echo $FILES | sed 's/ / /'`

    REFS=""

    for NAME in $3; do

        if [ -e "$NAME/$NAME.mdp" ]; then
            REF_TYPE="Project"
            REF_TO="$NAME"
        elif [ -e "$NAME" ]; then
            REF_TYPE="Assembly"
            REF_TO="$NAME"
        else
            REF_TYPE="Gac"
            REF_TO=`gacutil -l $NAME | grep $NAME`
        fi

        REFS="$REFS <ProjectReference type=\"$REF_TYPE\" localcopy=\"True\" refto=\"$REF_TO\" />"    

    done

    cat $MDP_TEMPLATE | \
        sed 's/{{PROJECT}}/'"$1"'/g' | \
        sed 's/{{TARGET}}/'"$2"'/g' | \
        sed 's#{{FILES}}#'"$FILES"'#g' | \
        sed 's#{{REFS}}#'"$REFS"'#g' > $1/$1.mdp
}


generate_mds()
{
    STARTUP_ENTRY="$1"

    DEBUG=""
    RELEASE=""
    STARTUP=""
    ENTRIES=""

    for PROJECT in $@; do
       DEBUG="$DEBUG <Entry build=\"True\" name=\"$PROJECT\" configuration=\"Debug\" />"
       RELEASE="$RELEASE <Entry build=\"True\" name=\"$PROJECT\" configuration=\"Release\" />"
       STARTUP="$STARTUP <Execute type=\"None\" entry=\"$PROJECT\" />"
       ENTRIES="$ENTRIES <Entry filename=\"./$PROJECT/$PROJECT.mdp\" />"
    done

    MDS_FILE=`echo $MDS_TEMPLATE | sed 's/\.template//'`

    cat $MDS_TEMPLATE | \
        sed 's#{{DEBUG}}#'"$DEBUG"'#g' | \
        sed 's#{{RELEASE}}#'"$RELEASE"'#g' | \
        sed 's/{{STARTUP_ENTRY}}/'"$STARTUP_ENTRY"'/g' | \
        sed 's#{{STARTUP}}#'"$STARTUP"'#g' | \
        sed 's#{{ENTRIES}}#'"$ENTRIES"'#g' > $MDS_FILE
}


generate_mdp $LIB Library
generate_mdp $GDK Library "gdk-sharp $LIB"
generate_mdp $UTILS Exe "$LIB"
generate_mdp $NAVI Exe "gtk-sharp glade-sharp $LIB $GDK"
generate_mdp $BEAGLE Exe "$BEAGLE_HOME/Util.dll $BEAGLE_HOME/Beagle.dll $BEAGLE_HOME/BeagleDaemonPlugins.dll $LIB"

generate_mds $LIB $GDK $UTILS $NAVI $BEAGLE

exit 0
