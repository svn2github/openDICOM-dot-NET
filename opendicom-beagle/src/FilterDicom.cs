/*
    openDICOM.NET Beagle Filter Plugin 0.1.1

    openDICOM.NET Beagle Filter Plugin makes DICOM file content available
    for the Beagle Desktop Search
    Copyright (C) 2007  Albert Gnandt

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA


    $Id$
*/
using System;
using System.IO;
using Beagle;
using Beagle.Daemon;
using Beagle.Filters;
using Beagle.Util;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding.Type;
using openDicom.File;


namespace Beagle.Filters
{
	
	public class FilterDicom : Beagle.Daemon.Filter
    {
        private static readonly string defaultDicDir =
            "/usr/share/opendicom.net/opendicom-beagle/dd/";

        // binaries!
        private static readonly string defaultDataElementDicFileName =
            "dicom-elements-2007.dic";
        private static readonly string defaultUidDicFileName =
            "dicom-uids-2007.dic";

        private bool isDicomFile = true;
        private AcrNemaFile dicomFile = null;
        private Sequence sequence = null;


		public FilterDicom()
		{
			AddSupportedFlavor(
                FilterFlavor.NewFromMimeType("application/dicom"));
            try
            {
                string dataElementDicFileName =
                    defaultDicDir + defaultDataElementDicFileName;
                string uidDicFileName =
                    defaultDicDir + defaultUidDicFileName;
                if ( ! File.Exists(dataElementDicFileName))
                    dataElementDicFileName = defaultDataElementDicFileName;
                if ( ! File.Exists(uidDicFileName))
                    uidDicFileName = defaultUidDicFileName;
                DataElementDictionary dataElementDic = 
                    new DataElementDictionary();
                UidDictionary uidDic = new UidDictionary();
                dataElementDic.LoadFrom(dataElementDicFileName, 
                    DictionaryFileFormat.BinaryFile);
                uidDic.LoadFrom(uidDicFileName, 
                    DictionaryFileFormat.BinaryFile);
            }
            catch (Exception e)
            {
                Log.Error("Problems processing Dictionaries:\n" + e);
                Error();
            }
		}

		override protected void DoOpen(FileInfo info)
		{
            try
            {
                if (DicomFile.IsDicomFile(info.FullName))
                {
                    dicomFile = new DicomFile(info.FullName, false);
                    sequence = 
                        dicomFile.GetJointDataSets().GetJointSubsequences();
                    isDicomFile = true;
                }
                else if (AcrNemaFile.IsAcrNemaFile(info.FullName))
                {
                    dicomFile = new AcrNemaFile(info.FullName, false);
                    sequence = 
                        dicomFile.GetJointDataSets().GetJointSubsequences();
                    isDicomFile = false;
                }
                else
                {
                    Log.Error("MIME type mismatch. Selected file is wether a " +
                        "DICOM nor an ACR-NEMA file.");
                    Error();
                }
            }
            catch (Exception e)
            {
                Log.Error("Problems processing DICOM file:\n" + e);
                Error();
            }
		}

		override protected void DoPullProperties()
		{
            try
            {
                string fileType = 
                    isDicomFile ? "DICOM 3.0" : "ACR-NEMA 1.0 or 2.0";
                AddProperty(Property.New("dicom:FileType", fileType));
                if (isDicomFile)
                    AddProperty(Property.New("dicom:FilePreamble",
                        ((DicomFile) dicomFile).MetaInformation.FilePreamble));
                Tag modalityTag = new Tag("0008", "0060");
                AddProperty(Property.New("dicom:Modality",
                    dicomFile.DataSet.Contains(modalityTag) ? 
                        dicomFile.DataSet[modalityTag].Value[0].ToString() : 
                        ""));
                AddProperty(Property.New("dicom:Width",
                        dicomFile.PixelData.Columns.ToString()));
                AddProperty(Property.New("dicom:Height",
                        dicomFile.PixelData.Rows.ToString()));
                AddProperty(Property.New("dicom:ColorBits",
                        (dicomFile.PixelData.SamplesPerPixel * 
                            dicomFile.PixelData.BitsStored).ToString()));
                Tag numberOfFramesTag = new Tag("0028", "0008");
                AddProperty(Property.New("dicom:NumberOfFrames",
                    dicomFile.DataSet.Contains(numberOfFramesTag) ?
                        dicomFile.DataSet[numberOfFramesTag].Value[0]
                            .ToString() : ""));
                AddProperty(Property.New("dicom:CharacterEncoding",
                    dicomFile.DataSet.TransferSyntax.CharacterRepertoire
                        .Encoding.WebName.ToUpper()));
                AddProperty(Property.New("dicom:TransferSyntax",
                    dicomFile.DataSet.TransferSyntax.Uid.GetDictionaryEntry()
                        .Name));
                AddProperty(Property.New("dicom:ValueRepresentation",
                    dicomFile.DataSet.TransferSyntax.IsImplicitVR ? 
                        "Implicit" : "Explicit"));
                AddProperty(Property.New("dicom:ByteOrdering",
                    dicomFile.DataSet.TransferSyntax.IsLittleEndian ? 
                        "Little Endian" : "Big Endian"));
                // to be continued ...
            }
            catch (Exception e)
            {
                Log.Error("Problems processing query of DICOM file: " +
                    e.Message);
                Error();
            }
        }

		override protected void DoPull()
		{
            try
            {
                string tag = "";
                string description = "";
                string vr = "";
                Value value = null;
                foreach (DataElement d in sequence)
                {
                    tag = d.Tag.ToString();
                    description = d.VR.Tag.GetDictionaryEntry().Description;
                    vr = d.VR.ToLongString();
                    value = d.Value;
        			AppendText(tag);
                    AppendStructuralBreak();
        			AppendText(description);
                    AppendStructuralBreak();
        			AppendText(vr);
                    AppendStructuralBreak();
                    if (value.IsDate)
                    {
                        AppendText(((DateTime) value[0]).ToShortDateString());
                        AppendStructuralBreak();
                    }
                    else if (value.IsMultiValue)
                    {
                        foreach (object o in value)
                        {
                            AppendText(o.ToString());
                            AppendStructuralBreak();
                        }
                    }
                    else
                    {
                        if ( ! (value.IsArray || value.IsSequence ||
                            value.IsNestedDataSet))
                        {
                            AppendText( 
                                value.IsEmpty ? "" : value[0].ToString());
                            AppendStructuralBreak();
                        }
                    }
                }
                Finished();
            }
            catch (Exception e)
            {
                Log.Error("Problems processing query of DICOM file: " +
                    e.Message);
                Error();
            }
		}
	}
}
