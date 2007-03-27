#!/bin/sh
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

SAMPLES_DIR="$1"

DICOM_FILE_LIST=`find $SAMPLES_DIR -name *.dcm`

for DICOM_FILE in $DICOM_FILE_LIST; do
    sh output.sh --cyan -n "echo $DICOM_FILE:"
    dicom-file-query $DICOM_FILE tag:dummy decode:lax
    sleep 2
done
