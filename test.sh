#!/bin/sh
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de)

DICOM_FILE_LIST=`find samples -name *.dcm | grep -v heidelberg | grep -v kater | grep -v valid | grep -v wrongentry | grep -v pixeldata`

for DICOM_FILE in $DICOM_FILE_LIST; do
    sh output.sh --cyan -n "echo $DICOM_FILE:"
    dicom-file-query $DICOM_FILE tag:dummy decode:lax
    sleep 2
done
