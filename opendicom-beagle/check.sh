#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id: check.sh 46 2007-03-28 12:48:37Z agnandt $

print_ok()
{
    sh output.sh --green -n "echo ok."
}


print_failed()
{
    sh output.sh --red -n "echo FAILED."
    exit 1
}

check_cmd()
{
    echo -n "Looking for command $2 ... "
    CMD=`which $2`
    if [ "$CMD" = "" ]; then
        print_failed
    fi
    case "$1" in
        --exists|-e)
            if [ -f $CMD ]; then
                print_ok
            else
                print_failed
            fi
            ;;
        --not-exists|-n)
            if [ ! -f $CMD ]; then
                print_ok
            else
                print_failed
            fi
            ;;
        *)
            print_usage
            ;;
    esac
}

check_dir()
{
    echo -n "Looking for directory $2 ... "
    case "$1" in
        --exists|-e)
            if [ -d $2 ]; then
                print_ok
            else
                print_failed
            fi
            ;;
        --not-exists|-n)
            if [ ! -d $2 ]; then
                print_ok
            else
                print_failed
            fi
            ;;
        *)
            print_usage
            ;;
    esac
}

check_lib()
{
    RETURN=""
    case "$1" in
        --exists|-e)
            echo -n "Looking for library $2 ... "
            RETURN=`pkg-config --exists $2 && echo "TRUE"`
            ;;
        --not-exists|-n)
            echo -n "Looking for library $2 ... "
            RETURN=`pkg-config --exists $2 || echo "TRUE"`
            ;;
    esac
    if [ "$RETURN" = "TRUE" ]; then
        print_ok
    else
        print_failed
    fi
}

check_lib_version()
{
    RETURN=""
    case "$1" in
        --greater-equals|-ge)
            echo -n "Looking for at least version $2 of library $3 ... "
            RETURN=`pkg-config --atleast-version=$2 $3 && echo "TRUE"`
            ;;
        --equals|-eq)
            echo -n "Looking for version $2 of library $3 ... "
            RETURN=`pkg-config --exact-version=$2 $3 && echo "TRUE"`
            ;;
        --less-equals|-le)
            echo -n "Looking for maximal version $2 of library $3 ... "
            RETURN=`pkg-config --max-version=$2 $3 && echo "TRUE"`
            ;;
        *)
            print_usage
            ;;
    esac
    if [ "$RETURN" = "TRUE" ]; then
        print_ok
    else
        print_failed
    fi
}

print_usage()
{
    echo "usage: sh check.sh {{--cmd,-c|--dir,-d|--lib,-l} {--exists,-e|--not-exists,-n} <name1> <name2> ... <nameN> | {--lib-version,-lv} {--greater-equals,-ge|--equals,-eq|--less-equals,-le} <version> <name>}"
    exit 1
}


ARG_LIST=""
for ARG in $@; do
    if [ "$ARG" != "$1" ] && [ "$ARG" != "$2" ]; then
        ARG_LIST="$ARG_LIST $ARG"
    fi
done


case "$1" in
    --cmd|-c)
        if [ $# -lt 3 ]; then
            print_usage
        fi
        for ARG in $ARG_LIST; do
            check_cmd $2 $ARG
        done
        ;;
    --dir|-d)
        if [ $# -lt 3 ]; then
            print_usage
        fi
        for ARG in $ARG_LIST; do
            check_dir $2 $ARG
        done
        ;;
    --lib|-l)
        if [ $# -lt 3 ]; then
            print_usage
        fi
        check_cmd --exists pkg-config
        for ARG in $ARG_LIST; do 
            check_lib $2 $ARG
        done
        ;;
    --lib-version|-lv)
        if [ $# -lt 4 ]; then
            print_usage
        fi
        check_cmd --exists pkg-config
        check_lib_version $2 $3 $4
        ;;
    *)
        print_usage
        ;;
esac

exit 0
