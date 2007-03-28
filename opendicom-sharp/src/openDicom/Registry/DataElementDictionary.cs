/*
   
    openDICOM.NET openDICOM# 0.1.0

    openDICOM# provides a library for DICOM related development on Mono.
    Copyright (C) 2006  Albert Gnandt

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


    $Id$
*/
using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.Registry
{

    /// <summary>
    ///     Data element dictionary. This class represents the registry of
    ///     DICOM data elements.
    /// </summary>
    /// <remarks>
    ///     This implementation does not support DICOM VR overloading like
    ///     "US/SS" which is known from the DICOM standard. VR from a
    ///     dictionary is an a-priori information which is used for decoding,
    ///     especially in the case of VR implicitness (DICOM output stream
    ///     provides no DICOM VR content).
    /// </remarks>
    public class DataElementDictionary: IDicomDictionary
    {   
        private Hashtable hashTable = new Hashtable(2590);

        private static DataElementDictionary global = null;
        /// <summary>
        ///     The global data element dictionary instance. Normally, the
        ///     first loaded data element dictionary is assigned as this
        ///     instance, automatically.
        /// </summary>
        public static DataElementDictionary Global
        {
            get
            {
                if (global == null || global.IsEmpty)
                    throw new DicomException(
                        "No global data element dictionary available." +
                        "Possibly it has not been initialized or is empty.");
                else
                    return global;
            }

            set
            {            
                if (value == null || value.IsEmpty)
                    throw new DicomException(
                        "DataElementDictionary.Global is null or empty.");
                else
                    global = value;
            }
        }

        /// <summary>
        ///     Access data element dictionary instance as array. DICOM
        ///     tags are used for indexing.
        /// </summary>
        public DataElementDictionaryEntry this[Tag index]
        {
            get 
            {
                if (index != null)
                    return (DataElementDictionaryEntry) 
                        hashTable[index.ToString()];
                else
                    throw new DicomException("index is null.",
                        "DataElementDictionary[index]");
            }
        }

        /// <summary>
        ///     Returns the count of dictionary entries.
        /// </summary>
        public int Count
        {
            get { return hashTable.Count; }
        }
        
        /// <summary>
        ///     Returns whether a dictionary does not contain entries.
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        private static readonly string fileComment = 
            "This file was automatically generated by openDICOM#.";


        /// <summary>
        ///     Creates a new empty data element dictionary instance.
        /// </summary>
        public DataElementDictionary() {}

        /// <summary>
        ///     Creates a new data element dictionary instance and fills it
        ///     with entries from the specified data element dictionary file
        ///     of specified file format.
        /// </summary>
        public DataElementDictionary(string fileName, 
            DictionaryFileFormat fileFormat)
        {
            LoadFrom(fileName, fileFormat);
        }

        private void LoadFromBinary(StreamReader streamReader)
        {
            BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
            while (streamReader.BaseStream.Position < streamReader.BaseStream.Length)
            {
                try
                {
                    int group = binaryReader.ReadInt32();
                    int element = binaryReader.ReadInt32();
                    Tag tag = new Tag(group, element);
                    int length = binaryReader.ReadInt32();
                    byte[] buffer = new byte[length];
                    binaryReader.Read(buffer, 0, length);
                    string description = ByteConvert.ToString(buffer, 
                        CharacterRepertoire.Ascii);
                    buffer = new byte[2];
                    binaryReader.Read(buffer, 0, 2);
                    string vr = ByteConvert.ToString(buffer, 
                        CharacterRepertoire.Ascii);
                    vr = vr.Trim();
                    length = binaryReader.ReadInt32();
                    buffer = new byte[length];
                    binaryReader.Read(buffer, 0, length);
                    string vm = ByteConvert.ToString(buffer, 
                        CharacterRepertoire.Ascii);
                    DataElementDictionaryEntry entry = 
                        new DataElementDictionaryEntry(tag.ToString(), 
                            description, vr, vm);
                    Add(entry);
                }
                catch (Exception e)
                {
                    throw new DicomException("Wrong entry before index " +
                        streamReader.BaseStream.Position + ": " + e.Message);
                }
            }
        }

        private void LoadFromProperty(TextReader textReader)
        {
            string line = textReader.ReadLine();
            int lineNumber = 1;
            string[] result = null;
            while (line != null)
            {
                string lineWithoutSpaces = line.Replace(" ", null);
                if ( ! lineWithoutSpaces.StartsWith("#") && 
                    ! lineWithoutSpaces.Equals(""))
                {
                    if (Regex.IsMatch(lineWithoutSpaces, 
                        "^[^=]+=[^,]+,([A-Za-z]{2})?,[0-9\\-nN]+$"))
                    {
                        result = line.Split('=');
                        string tag = result[0];
                        result = result[1].Split(',');
                        try
                        {
                            if (Regex.IsMatch(tag.ToLower(), "(50xx|60xx)"))
                            {
                                // Dicom repeating groups
                                for (int i = 0; i <= 0x1E; i += 2)
                                {
                                    string uniqueTag = tag.ToLower()
                                        .Replace("xx", 
                                            string.Format("{0:X2}", i));
                                    DataElementDictionaryEntry entry = 
                                        new DataElementDictionaryEntry(
                                            uniqueTag, 
                                            result[0].Trim(), result[1].Trim(),
                                            result[2].Trim());
                                    Add(entry);
                                }
                            }
                            else
                            {
                                DataElementDictionaryEntry entry = 
                                    new DataElementDictionaryEntry(tag, 
                                        result[0].Trim(), result[1].Trim(),
                                        result[2].Trim());
                                Add(entry);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new DicomException("Wrong entry in line " +
                                lineNumber.ToString() + ": " + e.Message);
                        }
                    }
                    else
                        throw new DicomException("Wrong entry in line " +
                            lineNumber.ToString() + ".");
                }
                line = textReader.ReadLine();
                lineNumber++;
            }            
        }

        private void LoadFromCsv(TextReader textReader)
        {
            string line = textReader.ReadLine();
            int lineNumber = 1;
            string[] result = null;
            while (line != null)
            {
                string lineWithoutSpaces = line.Replace(" ", null);
                if ( ! lineWithoutSpaces.StartsWith("#") && 
                    ! lineWithoutSpaces.Equals(""))
                {
                    if (Regex.IsMatch(lineWithoutSpaces, 
                        "^[^;]+;[^;]+;([A-Za-z]{2})?;[0-9\\-nN]+$"))
                    {
                        result = line.Split(';');
                        string tag = result[0];
                        try
                        {
                            if (Regex.IsMatch(tag.ToLower(), "(50xx|60xx)"))
                            {
                                // Dicom repeating groups
                                for (int i = 0; i <= 0x1E; i += 2)
                                {
                                    string uniqueTag = tag.ToLower()
                                        .Replace("xx", 
                                            string.Format("{0:X2}", i));
                                    DataElementDictionaryEntry entry = 
                                        new DataElementDictionaryEntry(
                                            uniqueTag, 
                                            result[1].Trim(), result[2].Trim(),
                                            result[3].Trim());
                                    Add(entry);
                                }
                            }
                            else
                            {
                                DataElementDictionaryEntry entry = 
                                    new DataElementDictionaryEntry(tag, 
                                        result[1].Trim(), result[2].Trim(), 
                                        result[3].Trim());
                                Add(entry);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new DicomException("Wrong entry in line " +
                                lineNumber.ToString() + ": " + e.Message);
                        }
                    }
                    else
                        throw new DicomException("Wrong entry in line " +
                            lineNumber.ToString() + ".");
                }
                line = textReader.ReadLine();
                lineNumber++;
            }            
        }

        protected virtual void LoadFromXml(TextReader textReader)
        {
            XmlTextReader xmlTextReader = new XmlTextReader(textReader);
            string tag = null;
            string description = null;
            string vr = null;
            string vm = null;
            while (xmlTextReader.Read())
            {
                switch(xmlTextReader.Name)
                {
                    case "Tag":
                        xmlTextReader.MoveToContent();
                        tag = xmlTextReader.ReadString();
                        break;
                    case "Description":
                        xmlTextReader.MoveToContent();
                        description = xmlTextReader.ReadString();
                        break;
                    case "VR":
                        xmlTextReader.MoveToContent();
                        vr = xmlTextReader.ReadString();
                        break;
                    case "VM":
                        xmlTextReader.MoveToContent();
                        vm = xmlTextReader.ReadString();
                        break;
                }
                if (tag != null && description != null && vr != null && 
                    vm != null)
                {
                    try
                    {
                        if (Regex.IsMatch(tag.ToLower(), "(50xx|60xx)"))
                        {
                            // Dicom repeating groups
                            for (int i = 0; i <= 0x1E; i += 2)
                            {
                                string uniqueTag = tag.ToLower()
                                   .Replace("xx", string.Format("{0:X2}", i));
                                DataElementDictionaryEntry entry = 
                                    new DataElementDictionaryEntry(
                                        uniqueTag, 
                                        description.Trim(),
                                        vr.Trim(), vm.Trim());
                                Add(entry);
                            }
                        }
                        else
                        {
                            DataElementDictionaryEntry entry = 
                                new DataElementDictionaryEntry(tag, 
                                    description.Trim(),
                                    vr.Trim(), vm.Trim());
                            Add(entry);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DicomException("Wrong entry at tag " +
                            tag + ": " + e.Message);
                    }
                    tag = description = vr = vm = null;
                }                
            }
            xmlTextReader.Close();
        }

        /// <summary>
        ///     Re-creates a data element dictionary instance and fills it
        ///     with entries from the specified data element dictionary file
        ///     of given file format.
        /// </summary>
        public void LoadFrom(string fileName, DictionaryFileFormat fileFormat)
        {
            if (! IsEmpty) Clear();
            StreamReader streamReader = new StreamReader(fileName);
            switch (fileFormat)
            {
                case DictionaryFileFormat.BinaryFile:
                    LoadFromBinary(streamReader); break;
                case DictionaryFileFormat.PropertyFile:
                    LoadFromProperty(streamReader); break;
                case DictionaryFileFormat.CsvFile:
                    LoadFromCsv(streamReader); break;
                case DictionaryFileFormat.XmlFile:
                    LoadFromXml(streamReader); break;
            }
            streamReader.Close();
            if (global == null) Global = this;
        }

        private void SaveAsBinary(StreamWriter streamWriter, 
            DataElementDictionaryEntry[] entryArray)
        {
            streamWriter.AutoFlush = true;
            BinaryWriter binaryWriter = new BinaryWriter(streamWriter.BaseStream);
            foreach (DataElementDictionaryEntry entry in entryArray)
            {
                binaryWriter.Write(int.Parse(entry.Tag.Group,
                    NumberStyles.HexNumber));
                binaryWriter.Write(int.Parse(entry.Tag.Element,
                    NumberStyles.HexNumber));
                binaryWriter.Write(entry.Description.Length);
                streamWriter.Write(entry.Description);
                if (entry.VR.IsUndefined)
                    streamWriter.Write("  ");
                else
                    streamWriter.Write(entry.VR.Name);
                binaryWriter.Write(entry.VM.Value.Length);
                streamWriter.Write(entry.VM.Value);
            }
        }

        private void SaveAsProperty(TextWriter textWriter, 
            DataElementDictionaryEntry[] entryArray)
        {
            textWriter.WriteLine("# " + fileComment);
            foreach (DataElementDictionaryEntry entry in entryArray)
            {
                textWriter.WriteLine(entry.Tag.ToString() + " = " + 
                    entry.Description + ", " + entry.VR.Name + 
                    ", " + entry.VM);
            }
        }

        private void SaveAsCsv(TextWriter textWriter, 
            DataElementDictionaryEntry[] entryArray)
        {
            textWriter.WriteLine("# " + fileComment);
            foreach (DataElementDictionaryEntry entry in entryArray)
            {
                textWriter.WriteLine(entry.Tag.ToString() + "; " + 
                    entry.Description + "; " + entry.VR.Name + 
                    "; " + entry.VM);
            }
        }

        protected virtual void SaveAsXml(TextWriter textWriter, 
            DataElementDictionaryEntry[] entryArray)
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.Indentation = 4;
            xmlTextWriter.WriteStartDocument();
            xmlTextWriter.WriteComment(" " + fileComment + " ");
            xmlTextWriter.WriteStartElement("DicomDataDictionary");
            foreach (DataElementDictionaryEntry entry in entryArray)
            {
                xmlTextWriter.WriteStartElement("DictionaryEntry");
                xmlTextWriter.WriteElementString("Tag", 
                    entry.Tag.ToString());
                xmlTextWriter.WriteElementString("Description",
                    entry.Description);
                xmlTextWriter.WriteElementString("VR", entry.VR.Name);
                xmlTextWriter.WriteElementString("VM", entry.VM.Value);
                xmlTextWriter.WriteEndElement();
            }
            xmlTextWriter.WriteEndElement();
            xmlTextWriter.Close();
        }

        /// <summary>
        ///     Saves the entire data element dictionary instance content to
        ///     file using specified file format.
        /// </summary>
        public void SaveTo(string fileName, DictionaryFileFormat fileFormat)
        {
            StreamWriter streamWriter = new StreamWriter(fileName);
            switch (fileFormat)
            {
                case DictionaryFileFormat.BinaryFile:
                    SaveAsBinary(streamWriter, ToArray()); break;
                case DictionaryFileFormat.PropertyFile:
                    SaveAsProperty(streamWriter, ToArray()); break;
                case DictionaryFileFormat.CsvFile:
                    SaveAsCsv(streamWriter, ToArray()); break;
                case DictionaryFileFormat.XmlFile:
                    SaveAsXml(streamWriter, ToArray()); break;
            }
            streamWriter.Close();
        }

        /// <summary>
        ///     Returns the entire data element dictionary as array of
        ///     <see cref="DataElementDictionaryEntry" />.
        /// </summary>
        public DataElementDictionaryEntry[] ToArray()
        {
            DataElementDictionaryEntry[] entryArray = 
                new DataElementDictionaryEntry[hashTable.Count];
            hashTable.Values.CopyTo(entryArray, 0);
            Array.Sort(entryArray);
            return entryArray;
        }

        /// <summary>
        ///     Adds a new data element dictionary entry to a data element
        ///     dictionary instance.
        /// </summary>
        public void Add(DataElementDictionaryEntry entry)
        {
            if (entry != null)
            {
                if ( ! Contains(entry.Tag))
                    hashTable.Add(entry.Tag.ToString(), entry);
                else
                    throw new DicomException(
                        "Tag already exists in data element dictionary.", 
                        "entry.Tag", entry.Tag.ToString());
            }
            else
                throw new DicomException("entry is null.", "entry");
        }

        /// <summary>
        ///     Clears all data element dictionary properties.
        /// </summary>
        public void Clear()
        {
            hashTable.Clear();
        }

        /// <summary>
        ///     Determines whether a DICOM tag already is in use within a data
        ///     element dictionary instance.
        /// </summary>      
        public bool Contains(Tag tag)
        {
            return hashTable.Contains(tag.ToString());
        }

        /// <summary>
        ///     Returns a dictionary entry by specified DICOM tag.
        /// </summary>      
        public DataElementDictionaryEntry GetDictionaryEntry(Tag tag)
        {
            return this[tag];
        }
    }
    
}
