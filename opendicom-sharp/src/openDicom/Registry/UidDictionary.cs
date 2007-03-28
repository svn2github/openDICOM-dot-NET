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
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.Registry
{

    /// <summary>
    ///     UID (Unique Identifier) dictionary. This class represents the
    ///     registry of DICOM UIDs.
    /// </summary>
    public class UidDictionary: IDicomDictionary
    {   
        private Hashtable hashTable = new Hashtable(198);

        private static UidDictionary global = null;
        /// <summary>
        ///     The global UID dictionary instance. Normally, the
        ///     first loaded UID dictionary is assigned as this
        ///     instance, automatically.
        /// </summary>
        public static UidDictionary Global
        {
            get
            {
                if (global == null || global.IsEmpty)
                    throw new DicomException(
                        "No global UID dictionary available." +
                        "Possibly it has not been initialized or is empty.");
                else
                    return global;
            }

            set
            {            
                if (value == null || value.IsEmpty)
                    throw new DicomException(
                        "UidDictionary.Global is null or empty.");
                else
                    global = value;
            }
        }

        /// <summary>
        ///     Access UID dictionary instance as array. DICOM
        ///     UIDs are used for indexing.
        /// </summary>
        public UidDictionaryEntry this[Uid index]
        {
            get 
            {
                if (index != null)
                    return (UidDictionaryEntry) hashTable[index.ToString()];
                else
                    throw new DicomException("index is null.",
                        "UidDictionary[index]");
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
        ///     Creates a new empty UID dictionary instance.
        /// </summary>
        public UidDictionary() {}

        /// <summary>
        ///     Creates a new UID dictionary instance and fills it
        ///     with entries from the specified UID dictionary file
        ///     of specified file format.
        /// </summary>
        public UidDictionary(string fileName, DictionaryFileFormat fileFormat)
        {
            LoadFrom(fileName, fileFormat);
        }

        private void LoadFromBinary(StreamReader streamReader)
        {
            BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream);
            while (streamReader.BaseStream.Position < streamReader.BaseStream.Length)
            {
                int length = binaryReader.ReadInt32();
                byte[] buffer = new byte[length];
                binaryReader.Read(buffer, 0, length);
                string stringUid = ByteConvert.ToString(buffer, 
                    CharacterRepertoire.Ascii);
                Uid uid = new Uid(stringUid);
                length = binaryReader.ReadInt32();
                buffer = new byte[length];
                binaryReader.Read(buffer, 0, length);
                string name = ByteConvert.ToString(buffer, 
                    CharacterRepertoire.Ascii);
                int intType = binaryReader.ReadInt32();
                UidType type = (UidType) UidType.ToObject(typeof(UidType), 
                    intType);
                try
                {
                    UidDictionaryEntry entry = 
                        new UidDictionaryEntry(uid, name, type);
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
                        "^[0-9\\.]+=[^,]+,[^,]+$"))
                    {
                        result = line.Split('=');
                        string uid = result[0];
                        result = result[1].Split(',');
                        try
                        {
                            UidDictionaryEntry entry = 
                                new UidDictionaryEntry(uid,
                                    result[0].Trim(), result[1].Trim());
                            Add(entry);
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
                        "^[0-9\\.]+;[^;]+;[^;]+$"))
                    {
                        result = line.Split(';');
                        string uid = result[0];
                        try
                        {
                            UidDictionaryEntry entry = 
                                new UidDictionaryEntry(uid, 
                                    result[1].Trim(), result[2].Trim());
                            Add(entry);
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
            string uid = null;
            string name = null;
            string type = null;
            while (xmlTextReader.Read())
            {
                xmlTextReader.MoveToContent();
                if (xmlTextReader.HasValue)
                {
                    if (uid == null) uid = xmlTextReader.Value;
                    else if (name == null) 
                        name = xmlTextReader.Value;
                    else if (type == null) 
                    {
                        type = xmlTextReader.Value;
                        try
                        {
                            UidDictionaryEntry entry = 
                                new UidDictionaryEntry(uid, name.Trim(),
                                    type.Trim());
                            Add(entry);
                        }
                        catch (Exception e)
                        {
                            throw new DicomException("Wrong entry at UID " +
                                uid + ": " + e.Message);
                        }
                        uid = name = type = null;
                    }
                }                
            }
            xmlTextReader.Close();
        }

        /// <summary>
        ///     Re-creates a UID dictionary instance and fills it
        ///     with entries from the specified UID dictionary file
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
            UidDictionaryEntry[] entryArray)
        {
            streamWriter.AutoFlush = true;
            BinaryWriter binaryWriter = new BinaryWriter(streamWriter.BaseStream);
            foreach (UidDictionaryEntry entry in entryArray)
            {
                binaryWriter.Write(entry.Uid.Value.Length);
                streamWriter.Write(entry.Uid.Value);
                binaryWriter.Write(entry.Name.Length);
                streamWriter.Write(entry.Name);
                binaryWriter.Write((int) entry.Type);
            }
        }

        private void SaveAsProperty(TextWriter textWriter, 
            UidDictionaryEntry[] entryArray)
        {
            textWriter.WriteLine("# " + fileComment);
            foreach (UidDictionaryEntry entry in entryArray)
            {
                textWriter.WriteLine(entry.Uid + " = " + 
                    entry.Name + ", " + 
                    UidType.GetName(typeof(UidType), entry.Type));
            }
        }

        private void SaveAsCsv(TextWriter textWriter, 
            UidDictionaryEntry[] entryArray)
        {
            textWriter.WriteLine("# " + fileComment);
            foreach (UidDictionaryEntry entry in entryArray)
            {
                textWriter.WriteLine(entry.Uid + "; " + 
                    entry.Name + "; " + 
                    UidType.GetName(typeof(UidType), entry.Type));
            }
        }

        protected virtual void SaveAsXml(TextWriter textWriter, 
            UidDictionaryEntry[] entryArray)
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.Indentation = 4;
            xmlTextWriter.WriteStartDocument();
            xmlTextWriter.WriteComment(" " + fileComment + " ");
            xmlTextWriter.WriteStartElement("DicomUidDictionary");
            foreach (UidDictionaryEntry entry in entryArray)
            {
                xmlTextWriter.WriteStartElement("DictionaryEntry");
                xmlTextWriter.WriteElementString("Uid", entry.Uid.ToString());
                xmlTextWriter.WriteElementString("Name", entry.Name);
                xmlTextWriter.WriteElementString("Type", 
                    UidType.GetName(typeof(UidType), entry.Type));
                xmlTextWriter.WriteEndElement();
            }
            xmlTextWriter.WriteEndElement();
            xmlTextWriter.Close();
        }

        /// <summary>
        ///     Saves the entire UID dictionary instance content to
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
        ///     Returns the entire UID dictionary as array of
        ///     <see cref="UidDictionaryEntry" />.
        /// </summary>
        public UidDictionaryEntry[] ToArray()
        {
            UidDictionaryEntry[] entryArray = 
                new UidDictionaryEntry[Count];
            hashTable.Values.CopyTo(entryArray, 0);
            Array.Sort(entryArray);
            return entryArray;
        }

        /// <summary>
        ///     Adds a new UID dictionary entry to a UID
        ///     dictionary instance.
        /// </summary>
        public void Add(UidDictionaryEntry entry)
        {
            if (entry != null)
            {
                if ( ! Contains(entry.Uid))
                    hashTable.Add(entry.Uid.ToString(), entry);
                else
                    throw new DicomException(
                        "UID already exists in UID dictionary.", 
                        "entry.Uid", entry.Uid.ToString());
            }
            else
                throw new DicomException("entry is null.", "entry");
        }

        /// <summary>
        ///     Clears all UID dictionary properties.
        /// </summary>
        public void Clear()
        {
            hashTable.Clear();
        }
      
        /// <summary>
        ///     Determines whether a DICOM UID already is in use within a UID
        ///     dictionary instance.
        /// </summary>      
        public bool Contains(Uid uid)
        {
            return hashTable.Contains(uid.ToString());
        }

        /// <summary>
        ///     Returns a dictionary entry by specified DICOM UID.
        /// </summary>      
        public UidDictionaryEntry GetDictionaryEntry(Uid uid)
        {
            return this[uid];
        }
    }
    
}
