/*
    openDICOM.NET Navigator 0.1.1

    Simple GTK ACR-NEMA and DICOM Viewer for Mono / .NET based on the 
    openDICOM.NET library.

    Copyright (C) 2006-2007  Albert Gnandt

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
using System.Reflection;
using System.IO;
using System.Xml;
using openDicom.Registry;


public sealed class Configuration
{
    public static Configuration Global = null;

    public static string DefaultResource = string.Empty;
    public static string DefaultFileName = null;

    public bool AreDictionariesAvailable
    {
        get
        {
            try
            {
                return (DataElementDictionary.Global != null &&
                    UidDictionary.Global != null);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    //public static readonly int ResourceSize = 2048;

    public string DataElementDictionaryFileName = null;
    public DictionaryFileFormat DataElementDictionaryFileFormat = 
        DictionaryFileFormat.BinaryFile;
    public string UidDictionaryFileName = null;
    public DictionaryFileFormat UidDictionaryFileFormat = 
        DictionaryFileFormat.BinaryFile;
    public bool UseStrictDecoding = false;
    public string LastOpenFolder = null;
    public string LastSaveFolder = null;
    public int SlideCyclingSpeed = 250;
    public string GimpRemoteExecutable = null;
    public double ImageBrightnessFactor = 1.0;

    public static DataElementDictionary DataElementDictionary =
        new DataElementDictionary();

    public static UidDictionary UidDictionary = new UidDictionary();

    public string Resource = string.Empty;
    public string FileName = null;


    public Configuration() {}

    public Configuration(string resource, string fileName)
    {
        Resource = resource;
        FileName = fileName;
        if (File.Exists(FileName))
            LoadFromFile();
        else
            LoadFromAssembly();
        LoadDictionaries();
    }

    public static void Init()
    {
        Init(DefaultResource, DefaultFileName);
    }

    public static void Init(string resource, string fileName)
    {
        Global = new Configuration(resource, fileName);
    }

    public static void Store()
    {
        Global.SaveToFile();
      /*  try
        {
            Global.SaveToAssembly();
            if (File.Exists(Global.FileName)) File.Delete(Global.FileName);
        }
        catch (Exception e)
        {
            Global.SaveToFile();
        } */
    }

    public void LoadDictionaries()
    {
        if (File.Exists(DataElementDictionaryFileName) &&
            File.Exists(UidDictionaryFileName))
        {
            DataElementDictionary.LoadFrom(DataElementDictionaryFileName,
                DataElementDictionaryFileFormat);
            DataElementDictionary.Global = DataElementDictionary;
            UidDictionary.LoadFrom(UidDictionaryFileName,
                UidDictionaryFileFormat);
            UidDictionary.Global = UidDictionary;
        }
        else
        {
            string defaultDir = 
                "/usr/share/opendicom.net/opendicom-navigator/dd";
            string defaultDataElementDic = "dicom-elements-2007.dic";
            string defaultUidDic = "dicom-uids-2007.dic";
            try
            {
                DataElementDictionary.LoadFrom(defaultDir + "/" + 
                    defaultDataElementDic,
                    DictionaryFileFormat.BinaryFile);
                DataElementDictionary.Global = DataElementDictionary;
                UidDictionary.LoadFrom(defaultDir + "/" + defaultUidDic,
                    DictionaryFileFormat.BinaryFile);
                UidDictionary.Global = UidDictionary;
                DataElementDictionaryFileName = 
                    defaultDir + "/" + defaultDataElementDic;
                DataElementDictionaryFileFormat =
                    DictionaryFileFormat.BinaryFile;
                UidDictionaryFileName = 
                    defaultDir + "/" + defaultUidDic;
                UidDictionaryFileFormat = DictionaryFileFormat.BinaryFile;
            }
            catch (Exception e1)
            {
                try
                {
                    DataElementDictionary.LoadFrom(defaultDataElementDic,
                        DictionaryFileFormat.BinaryFile);
                    DataElementDictionary.Global = DataElementDictionary;
                    UidDictionary.LoadFrom(defaultUidDic,
                        DictionaryFileFormat.BinaryFile);
                    UidDictionary.Global = UidDictionary;
                    DataElementDictionaryFileName = defaultDataElementDic;
                    DataElementDictionaryFileFormat =
                        DictionaryFileFormat.BinaryFile;
                    UidDictionaryFileName = defaultUidDic;
                    UidDictionaryFileFormat = DictionaryFileFormat.BinaryFile;
                }
                catch (Exception e2) {}
            }
        }
    }

    public void LoadFrom(Stream stream)
    {
        XmlTextReader xml = new XmlTextReader(stream);
        while (xml.Read())
        {
            if (xml.Name == "add")
            {
                string key = xml.GetAttribute("key");
                string value = xml.GetAttribute("value");
                switch (key)
                {
                    case "DataElementDictionaryFile":
                        if (File.Exists(value))
                            DataElementDictionaryFileName = value;
                        break;
                    case "DataElementDictionaryFormat":
                        DataElementDictionaryFileFormat = 
                            (DictionaryFileFormat) Enum.Parse(
                            typeof(DictionaryFileFormat), value);
                        break;
                    case "UidDictionaryFile":
                        if (File.Exists(value))
                            UidDictionaryFileName = value;
                        break;
                    case "UidDictionaryFormat":
                        UidDictionaryFileFormat = 
                            (DictionaryFileFormat) Enum.Parse(
                            typeof(DictionaryFileFormat), value);
                        break;
                    case "UseStrictDecoding":
                        UseStrictDecoding = bool.Parse(value);
                        break;
                    case "LastOpenFolder":
                        if (value == "")
                            LastOpenFolder = Environment.GetFolderPath(
                                Environment.SpecialFolder.Personal);
                        else
                            LastOpenFolder = value;
                        break;
                    case "LastSaveFolder":
                        if (value == "")
                            LastSaveFolder = Environment.GetFolderPath(
                                Environment.SpecialFolder.Personal);
                        else
                            LastSaveFolder = value;
                        break;
                    case "SlideCyclingSpeed":
                        if (value == "")
                            SlideCyclingSpeed = 250;
                        else
                            SlideCyclingSpeed = int.Parse(value);
                        break;
                    case "GimpRemoteExecutable":
                        if (File.Exists(value))
                            GimpRemoteExecutable = value;
                        break;
                    case "ImageBrightnessFactor":
                        if (value != "")
                            ImageBrightnessFactor = double.Parse(value);
                        break;
                }
            }
        }
        if (Global == null) Global = this;
    }

    public void LoadFromFile()
    {
        FileStream fileStream = new FileStream(FileName, FileMode.Open,
            FileAccess.Read);
        try
        {
            LoadFrom(fileStream);
        }
        finally
        {
            fileStream.Close();
        }
    }

    public void LoadFromAssembly()
    {
        Stream stream = 
            Assembly.GetEntryAssembly().GetManifestResourceStream(Resource);
        try
        {
            LoadFrom(stream);
        }
        finally
        {
            stream.Close();
        }
    }

    public void SaveTo(Stream stream)
    {
        XmlTextWriter xml = new XmlTextWriter(stream,
            System.Text.Encoding.UTF8);
        xml.Formatting = Formatting.Indented;
        xml.Indentation = 4;
        xml.WriteStartDocument();
        xml.WriteStartElement("configuration");
        xml.WriteStartElement("appSettings");
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "DataElementDictionaryFile");
        xml.WriteAttributeString("value", DataElementDictionaryFileName);
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "DataElementDictionaryFormat");
        xml.WriteAttributeString("value", 
            DataElementDictionaryFileFormat.ToString());
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "UidDictionaryFile");
        xml.WriteAttributeString("value", UidDictionaryFileName);
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "UidDictionaryFormat");
        xml.WriteAttributeString("value",
            UidDictionaryFileFormat.ToString());
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "UseStrictDecoding");
        xml.WriteAttributeString("value", UseStrictDecoding.ToString());
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "LastOpenFolder");
        xml.WriteAttributeString("value", LastOpenFolder);
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "LastSaveFolder");
        xml.WriteAttributeString("value", LastSaveFolder);
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "SlideCyclingSpeed");
        xml.WriteAttributeString("value", SlideCyclingSpeed.ToString());
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "GimpRemoteExecutable");
        xml.WriteAttributeString("value", GimpRemoteExecutable);
        xml.WriteEndElement();
        xml.WriteStartElement("add");
        xml.WriteAttributeString("key", "ImageBrightnessFactor");
        xml.WriteAttributeString("value", ImageBrightnessFactor.ToString());
        xml.WriteEndElement();
        xml.WriteEndElement();
        xml.WriteEndElement();
        /*xml.WriteComment(
            string.Format("This is an embedded resource file with an " +
                "exact size of {0} bytes guaranteed by trailing white " +
                "spaces!", ResourceSize));*/
        xml.WriteEndDocument();
        xml.Flush();
        /*BinaryWriter binWriter = new BinaryWriter(stream);
        for (int i = (int) stream.Length; i < ResourceSize; i++)
            binWriter.Write((byte) 0x20);*/
    }

    public void SaveToFile()
    {
        // prevents trouble on Windows platfroms
        if (File.Exists(FileName)) File.Delete(FileName);
        FileStream fileStream = new FileStream(FileName, FileMode.Create,
            FileAccess.Write);
        try
        {
            SaveTo(fileStream);
        }
        finally
        {
            fileStream.Close();
        }
        File.SetAttributes(FileName, FileAttributes.Hidden);
    }

    /*public void SaveToAssembly()
    {
        // File permissions have to be set right. Works well as root.
        // Very dirty. Be careful!
        FileStream fileStream = new FileStream(
            Assembly.GetEntryAssembly().Location, FileMode.Open,
            FileAccess.ReadWrite);
        try
        {
            // start of resource
            int[] template = new int[8] { 0xEF, 0xBB, 0xBF, 0x3C, 0x3F, 0x78,
                0x6D, 0x6C };
            bool foundMatch = false;
            bool reachedFileEnd = false;
            int b = fileStream.ReadByte();
            while ( ! foundMatch && ! reachedFileEnd)
            {
                if (b == 0xEF)
                {
                    int i;
                    for (i = 1; i < template.Length; i++)
                    {
                        b = fileStream.ReadByte();
                        if (b != template[i]) break;
                    }
                    foundMatch = (i == template.Length);
                }
                else if (b == -1)
                    reachedFileEnd = true;
                else
                    b = fileStream.ReadByte();
            }
            if (reachedFileEnd && ! foundMatch)
                throw new Exception("Embedded resource for configuration " +
                    "storage cannot be found.");
            if (foundMatch)
                fileStream.Seek(-template.Length, SeekOrigin.Current);
            Console.WriteLine(fileStream.Position.ToString());
            MemoryStream memStream = new MemoryStream();
            SaveTo(memStream);
            memStream.WriteTo(fileStream);
            memStream.Close();
        }
        finally
        {
            fileStream.Close();
        }
    }*/

    public void CopyTo(Configuration config)
    {
        config.DataElementDictionaryFileName = DataElementDictionaryFileName;
        config.DataElementDictionaryFileFormat = 
            DataElementDictionaryFileFormat;
        config.UidDictionaryFileName = UidDictionaryFileName;
        config.UidDictionaryFileFormat = UidDictionaryFileFormat;
        config.UseStrictDecoding = UseStrictDecoding;
        config.LastOpenFolder = LastOpenFolder;
        config.LastSaveFolder = LastSaveFolder;
        config.SlideCyclingSpeed = SlideCyclingSpeed;
        config.GimpRemoteExecutable = GimpRemoteExecutable;
        config.ImageBrightnessFactor = ImageBrightnessFactor;
    }
}
