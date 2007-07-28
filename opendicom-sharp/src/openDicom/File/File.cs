/*
   
    openDICOM.NET openDICOM# 0.2

    openDICOM# provides a library for DICOM related development on Mono.
    Copyright (C) 2006-2007  Albert Gnandt

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA


    $Id: $
*/
using System;
using System.IO;
using openDicom;
using openDicom.Registry;


namespace openDicom.File
{

    /// <summary>
    ///     This class provides simple access to ACR-NEMA, DICOM and compliant
    ///     XML files.
    /// </summary>
    public class File
    {
        /// <summary>
        ///     Loads the global dictionaries <see cref="DataElementDictionary" />
        ///     and <see cref="UidDictionary" />.
        /// </summary>
        public static void LoadDictionariesFrom(
            string dataElementDictionaryFileName, string uidDictionaryFileName,
            DictionaryFileFormat fileFormat)
        {
            new DataElementDictionary (dataElementDictionaryFileName, 
                fileFormat);
            new UidDictionary (uidDictionaryFileName, fileFormat);
        }

        /// <summary>
        ///     Determines, whether a file is a DICOM, an ACR-NEMA or compliant
        ///     XML file and loads it. Returns 'null' if specified file is
        ///     not any of the mentioned file types.
        /// </summary>
        public static AcrNemaFile LoadFrom(string fileName)
        {
            if (DicomFile.IsDicomFile(fileName))
            {
                return new DicomFile(fileName, useStrictDecoding);
            } 
            else if (AcrNemaFile.IsAcrNemaFile(fileName))
            {
                return new AcrNemaFile(fileName, useStrictDecoding);                
            }
            else if (XmlFile.IsXmlFile(fileName))
            {
                return new XmlFile(fileName);
            }
            else
            {
                return null;
            }
        }
    }

}
