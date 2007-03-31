#!/bin/bash
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

print_usage()
{
    echo "usage: bash output.sh {--red,-r|--light-red,-lr|--green,-g|--light-green,-lg|--brown,-w|--yellow,-y|--blue,-b|--light-blue,-lb|--magenta,-m|--light-magenta,-lm|--cyan,-c|--light-cyan,-lc} {--echo,-e|--no-echo|-n} <command>"
    exit 1
}



if [ $# -lt 3 ]; then
    print_usage
fi


case "$1" in
    --red|-r)
        COLOR="0;31"
        ;;
    --light-red|-lr)
        COLOR="1;31"
        ;;
    --green|-g)
        COLOR="0;32"
        ;;
    --light-green|-lg)
        COLOR="1;32"
	;;
    --brown|-w)
        COLOR="0;33"
        ;;
    --yellow|-y)
        COLOR="1;33"
        ;;
    --blue|-b)
        COLOR="0;34"
        ;;
    --light-blue|-lb)
        COLOR="1;34"
        ;;
    --magenta|-m)
        COLOR="0;35"
        ;;
    --light-magenta|-lm)
        COLOR="1;35"
        ;;
    --cyan|-c)
        COLOR="0;36"
        ;;
    --light-cyan|-lc)
        COLOR="1;36"
        ;;
    *)
        COLOR="0"
        ;;
esac


case "$2" in
    --echo|-e)
        echo "\33[${COLOR}m${3}\n`${3}`\33[0m"
        ;;
    --no-echo|-n)
        echo "\33[${COLOR}m`${3}`\33[0m"
        ;;
    *)
        print_usage
        ;;
esac

exit 0
