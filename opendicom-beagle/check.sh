#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id: check.sh 22 2007-03-27 22:39:49Z agnandt $

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
    echo -n "Looking for library $2 ... "
    EXISTS=`pkg-config --exists $2 && echo "TRUE"`
    case "$1" in
        --exists|-e)
            if [ "$EXISTS" = "TRUE" ]; then
                print_ok
            else
                print_failed
            fi
            ;;
        --not-exists|-n)
            if [ "$EXISTS" != "TRUE" ]; then
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

print_usage()
{
    echo "usage: sh check.sh {--cmd,-c|--dir,-d|--lib,-l} {--exists,-e|--not-exists,-n} <name1> <name2> ... <nameN>"
    exit 1
}


if [ $# -lt 3 ]; then
    print_usage
else
    ARG_LIST=""
    for ARG in $@; do
        if [ "$ARG" != "$1" ] && [ "$ARG" != "$2" ]; then
            ARG_LIST="$ARG_LIST $ARG"
        fi
    done
fi


case "$1" in
    --cmd|-c)
        for ARG in $ARG_LIST; do
            check_cmd $2 $ARG
        done
        ;;
    --dir|-d)
        for ARG in $ARG_LIST; do
            check_dir $2 $ARG
        done
        ;;
    --lib|-l)
        check_cmd --exists pkg-config
        for ARG in $ARG_LIST; do
            check_lib $2 $ARG
        done
        ;;
    *)
        print_usage
        ;;
esac

exit 0
