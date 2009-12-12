#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id: $

# Beagle is not supported yet
LIB="opendicom-sharp"
GDK="opendicom-sharp-gdk"
UTILS="opendicom-utils"
NAVI="opendicom-navigator"

CSPROJ_TEMPLATE="opendicom.csproj.template"
SLN_TEMPLATE="opendicom.sln.template"


generate_csproj()
{
    ASSEMBLIES=`find $1/src -name AssemblyInfo.cs`

    if [ `echo $ASSEMBLIES | sed 's# #\n#g' | wc -l` -gt 1 ]; then
        ASSEMBLIES=`echo $ASSEMBLIES | sed 's#'"$1"'/src/##g' | sed 's#/AssemblyInfo.cs##g'`
        for PROJECT in $ASSEMBLIES; do
            generate_single_csproj $PROJECT $1 $2 $3
        done
    else
        generate_single_csproj $1 $1 $2 $3
    fi
}

generate_single_csproj()
{
    if [ "$1" != "$2" ]; then
        FILES=`find $2/src/$1 -name *.cs`
    else
        FILES=`find $2/src -name *.cs`
    fi

    COMPILE=""
    for F in $FILES; do
        F=`echo $F | \
            sed 's#'"$2"'/##g' | \
            sed 's#/#\\\\\\\\#g' | \
            awk '{ print "<Compile Include=\""$0"\" />" }'`
        COMPILE="$COMPILE $F"
    done

    # replace file list seperators
    FILES=`echo $COMPILE | sed 's/ / /'`

    REFS=""

    for NAME in `echo $4 | sed 's#,# #g'`; do
        if [ -e "$NAME/$NAME.csproj" ]; then
            # project
            REF_TO="$NAME"
            REFS="$REFS <Reference Include=\"$REF_TO\"><HintPath>..\\\\$NAME\\\\bin\\\\$NAME.dll</HintPath></Reference>"
        elif [ -e "$NAME" ]; then
            # assembly
            REF_TO="$NAME"
            REFS="$REFS <Reference Include=\"$REF_TO\" />"
        else
            # GAC
            REF_TO="$NAME"
            REFS="$REFS <Reference Include=\"$REF_TO\" />"
        fi

    done

    cat $CSPROJ_TEMPLATE | \
        sed 's/{{PROJECT}}/'"$1"'/g' | \
        sed 's/{{TARGET}}/'"$3"'/g' | \
        sed 's#{{FILES}}#'"$FILES"'#g' | \
        sed 's#{{REFS}}#'"$REFS"'#g' > $2/$1.csproj
}

generate_sln()
{
    ENTRIES=""

    for PROJECT in $@; do
        FILES=`ls -A1 $PROJECT/*.csproj`
        for F in $FILES; do
            F=`echo $F | sed -e 's#^'"$PROJECT"'/##' -e 's#\.csproj$##'`
            ENTRIES="$ENTRIES\nProject(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"$F\", \"$PROJECT\\\\$F.csproj\", \"{9DEFBD13-B7C7-48D9-8AC8-FAE3B547AC8B}\"\nEndProject"
        done
    done

    SLN_FILE=`echo $SLN_TEMPLATE | sed 's/\.template//'`

    cat $SLN_TEMPLATE | \
        sed 's#{{ENTRIES}}#'"$ENTRIES"'#g' > $SLN_FILE
}


generate_csproj $LIB Library "System,System.XML"
generate_csproj $GDK Library "glib-sharp,gdk-sharp,$LIB"
generate_csproj $UTILS Exe "System,System.XML,$LIB"
generate_csproj $NAVI WinExe "System,System.XML,atk-sharp,glib-sharp,gdk-sharp,gtk-sharp,glade-sharp,pango-sharp,$LIB,$GDK"

generate_sln $LIB $GDK $UTILS $NAVI

exit 0
