#!/bin/sh
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de, http://www.gnandt.com/)


print_usage()
{
    echo "usage: sh output.sh {--red,-r|--green,-g|--brown,-w|--blue,-b|--magenta,-m|--cyan,-c} {--echo,-e|--no-echo|-n} <command>"
    exit 1
}



if [ $# -lt 3 ]; then
    print_usage
fi


case "$1" in
    --red|-r)
        COLOR="31"
        ;;
    --green|-g)
        COLOR="32"
        ;;
    --brown|-w)
        COLOR="33"
        ;;
    --blue|-b)
        COLOR="34"
        ;;
    --magenta|-m)
        COLOR="35"
        ;;
    --cyan|-c)
        COLOR="36"
        ;;
    *)
        COLOR="0"
        ;;
esac


echo -en "\E[${COLOR}m"
case "$2" in
    --echo|-e)
        echo $3
        ;;
    --no-echo|-n)
        ;;
    *)
        print_usage
        ;;
esac
$3
tput sgr0

exit 0
