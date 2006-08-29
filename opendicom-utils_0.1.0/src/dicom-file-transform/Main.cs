/*
    openDICOM.NET Utils 0.1.0

    openDICOM.NET Utils provides DICOM utility applications for DICOM related
    manipulation on Mono
    Copyright (C) 2006  Albert Gnandt

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

*/
using System;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using openDicom;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;
using openDicom.Encoding.Type;
using openDicom.File;
using openDicom.Image;

using DateTime = System.DateTime;


public sealed class DicomFileTransform
{
    public static readonly int normalExitCode = 0;
    public static readonly int errorExitCode = 1;

    public static DataElementDictionary dataElementDictionary = 
        new DataElementDictionary();

    public static UidDictionary uidDictionary = new UidDictionary();

    public static string defaultFormat = "b:";
    public static string defaultDataElementDic = "dicom-elements-2004.dic";
    public static string defaultUidDic = "dicom-uids-2004.dic";
    public static string defaultLinuxDir = 
        "/usr/share/opendicom.net/opendicom-utils/dd/";

    public static string defaultDecodingMode = "decode:strict";

    public static AcrNemaFile dicomFile = null;

    public static string[] elementDic = new string[0];
    public static string[] uidDic = new string[0];
    public static string dicomFileName = null;
    public static string[] target = new string[0];
    public static string[] decodingMode = new string[0];


    public static int PrintUsage()
    {
        Console.Error.WriteLine("openDICOM.NET Utils");
        Console.Error.WriteLine(
            "Transforms a DICOM file content to XML, raw and image data " +
            "file(s).");
        Console.Error.WriteLine();
        Console.Error.WriteLine(
            "Usage: dicom-file-transform [element-dic:<format>:<src>] " +
            "[uid-dic:<format>:<src>] <dcm-file> " +
            "target:<target>[:<dest-file>] [decode:<mode>]");
        Console.Error.WriteLine();
        Console.Error.WriteLine("format    specified dictionary file format");
        Console.Error.WriteLine("          b - binary");
        Console.Error.WriteLine("          p - property");
        Console.Error.WriteLine("          x - xml");
        Console.Error.WriteLine("          c - csv");
        Console.Error.WriteLine("src       local dictionary file");
        Console.Error.WriteLine("dcm-file  DICOM file");
        Console.Error.WriteLine("target    specified targets of transcoding");
        Console.Error.WriteLine("          xml    - XML file");
        Console.Error.WriteLine(
            "          xml-pd - XML file without pixel data");
        Console.Error.WriteLine("          raw    - Raw pixel data file(s)");
        Console.Error.WriteLine(
            "          img    - determines image file format for pixel data " +
            "if possible");
        Console.Error.WriteLine("dest-file specified destination file name");
        Console.Error.WriteLine("mode      decoding mode of DICOM content");
        Console.Error.WriteLine("          strict - proper decoding (default)");
        Console.Error.WriteLine("          lax    - improper decoding");
        return errorExitCode;
    }

    public static int GetParameters(string[] args)
    {
        string dicPattern = "(b|B|p|P|x|X|c|C):[^:]+$";
        for (int i = 0; i < args.Length; i++)
        {
            if (Regex.IsMatch(args[i].ToLower(), "^element-dic:" + dicPattern))
                elementDic = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), "^uid-dic:" + dicPattern))
                uidDic = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), 
                "^target:(xml|xml-pd|raw|img)(:[^:]+)?$"))
                target = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), "^decode:(strict|lax)$"))
                decodingMode = args[i].Split(':');
            else if (dicomFileName == null)
                dicomFileName = args[i];
            else
                return PrintUsage();
        }
        if (uidDic.Length == 0) 
            uidDic = GetDefaultDic(defaultUidDic, 
                "element-dic:" + defaultFormat)
                    .Split(':');
        if (elementDic.Length == 0) 
            elementDic = GetDefaultDic(defaultDataElementDic, 
                "uid-dic:" + defaultFormat)
                    .Split(':');
        if (dicomFileName == null) return PrintUsage();
        if (target.Length == 0) return PrintUsage();
        if (decodingMode.Length == 0) 
            decodingMode = defaultDecodingMode.Split(':');
        return normalExitCode;
    }

    public static string GetDefaultDic(string fileName, string format)
    {
        if (File.Exists(defaultLinuxDir + fileName))
            return format + defaultLinuxDir + fileName;
        else
            return format + fileName;
    }

    public static int LoadDicFrom(IDicomDictionary dic, string format, 
        string fileName)
    {
        Console.WriteLine("Reading {0}.", fileName);
        try
        {
            switch (format.ToLower())
            {
                case "b":
                    dic.LoadFrom(fileName, DictionaryFileFormat.BinaryFile);
                    break;
                case "p":
                    dic.LoadFrom(fileName, DictionaryFileFormat.PropertyFile);
                    break;
                case "x":
                    dic.LoadFrom(fileName, DictionaryFileFormat.XmlFile);
                    break;
                case "c":
                    dic.LoadFrom(fileName, DictionaryFileFormat.CsvFile);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Problems reading from file:\n" + e);
            return errorExitCode;
        }
        return normalExitCode;
    }

    private static void SaveBytesToFile(string fileName, byte[] bytes)
    {
        FileStream fileStream = new FileStream(fileName, FileMode.Create,
            FileAccess.Write);
        try
        {
            fileStream.Write(bytes, 0, bytes.Length);
        }
        finally
        {
            fileStream.Close();
        }
    }

    public static int SavePixelDataToRawFiles(string fileNamePattern)
    {
        if (dicomFile.HasPixelData)
        {
            long startTicks = DateTime.Now.Ticks;
            PixelData pixelData = dicomFile.PixelData;
            byte[][] rawData = pixelData.ToBytesArray();
            if (rawData.Length > 1)
            {
                for (int i = 0; i < rawData.Length; i++)
                {
                    string fileName = string.Format(fileNamePattern, 
                        "-" + i.ToString());
                    Console.WriteLine("Writing {0}.", fileName);
                    SaveBytesToFile(fileName, rawData[i]);
                }
            }
            else
            {
                string fileName = string.Format(fileNamePattern, "");
                Console.WriteLine("Writing {0}.", fileName);
                SaveBytesToFile(fileName, rawData[0]);
            }
            Console.WriteLine("Writing took {0} ms.",
                (DateTime.Now.Ticks - startTicks) / 10000);
            return normalExitCode;
        }
        else
        {
            Console.Error.WriteLine("Found no pixel data.");
            return errorExitCode;
        }
    }

    public static string GetRawFileNamePattern(string targetFileName)
    {
        if (targetFileName.Equals("")) 
            targetFileName = dicomFileName;
        if (Regex.IsMatch(targetFileName.ToLower(), "\\.raw$"))
        {
            int i = targetFileName.ToLower().IndexOf(".raw");
            targetFileName = targetFileName.Insert(i, "{0}");
        }
        else
            targetFileName += "{0}.raw";
        return targetFileName;
    }

    public static string GetJpegFileNamePattern(string targetFileName)
    {
        if (targetFileName.Equals("")) 
            targetFileName = dicomFileName;
        if (Regex.IsMatch(targetFileName.ToLower(), "\\.(jpg|jpeg)$"))
        {
            int i = targetFileName.ToLower().IndexOf(".jp");
            targetFileName = targetFileName.Insert(i, "{0}");
        }
        else
            targetFileName += "{0}.jpg";
        return targetFileName;
    }

    public static int SavePixelDataToJpegFiles(string fileNamePattern)
    {
        return SavePixelDataToRawFiles(fileNamePattern);
    }

    public static int Main(string[] args)
    {
        int exitCode = GetParameters(args);
        if (exitCode == errorExitCode) return exitCode;
        exitCode = LoadDicFrom(dataElementDictionary, 
            elementDic[1].ToLower(), elementDic[2]);
        if (exitCode == errorExitCode) return exitCode;
        Console.WriteLine("Data element dictionary contains {0} entries.",
            dataElementDictionary.Count);
        exitCode = LoadDicFrom(uidDictionary, uidDic[1].ToLower(), 
            uidDic[2]);
        if (exitCode == errorExitCode) return exitCode;
        Console.WriteLine("Uid dictionary contains {0} entries.",
            uidDictionary.Count);
        bool useStrictDecoding = decodingMode[1].ToLower().Equals("strict");
        Console.WriteLine("Decoding mode is {0}.", decodingMode[1].ToLower());
        Console.WriteLine("Reading {0}.", dicomFileName);
        try
        {
            if (DicomFile.IsDicomFile(dicomFileName))
            {
                long startTicks = DateTime.Now.Ticks;
                dicomFile = new DicomFile(dicomFileName, useStrictDecoding);
                Console.WriteLine("Reading took {0} ms.",
                    (DateTime.Now.Ticks - startTicks) / 10000);
            }
            else if (AcrNemaFile.IsAcrNemaFile(dicomFileName))
            {
                Console.WriteLine("Found ACR-NEMA file instead of DICOM file.");
                long startTicks = DateTime.Now.Ticks;
                dicomFile = new AcrNemaFile(dicomFileName, useStrictDecoding);
                Console.WriteLine("Reading took {0} ms.",
                    (DateTime.Now.Ticks - startTicks) / 10000);
            }
            else if (XmlFile.IsXmlFile(dicomFileName))
            {
                Console.WriteLine(
                    "Found DICOM-/ACR-NEMA-XML file instead of DICOM file.");
                Console.Error.WriteLine("This function is not implemented.");
                return errorExitCode;
            }            
            else
            {
                Console.Error.WriteLine("User specified file is whether " +
                    "a DICOM, ACR-NEMA nor a compliant XML file.");
                return errorExitCode;
            }
            string targetType = target[1].ToLower();
            string targetFileName = "";
            if (target.Length == 3) targetFileName = target[2];
            switch (targetType)
            {
                case "xml":
                case "xml-pd":
                    bool includePixelData = target[1].ToLower().Equals("xml");
                    if (includePixelData)
                        Console.WriteLine(
                            "Transcoding entire content to XML.");
                    else
                        Console.WriteLine(
                            "Transcoding pixel data excluded content to XML.");
                    if (targetFileName.Equals(""))
                        targetFileName = dicomFileName + ".xml";
                    Console.WriteLine("Writing {0}.", targetFileName); 
                    long startTicks = DateTime.Now.Ticks;
                    XmlFile xml = new XmlFile(dicomFile, ! includePixelData);
                    xml.SaveTo(targetFileName);
                    Console.WriteLine("Writing took {0} ms.",
                        (DateTime.Now.Ticks - startTicks) / 10000);                        
                    break;
                case "raw":
                    Console.WriteLine("Transcoding pixel data to raw data.");
                    targetFileName = GetRawFileNamePattern(targetFileName);
                    exitCode = SavePixelDataToRawFiles(targetFileName);
                    if (exitCode == errorExitCode) return exitCode;
                    break;
                case "img":
                    if (dicomFile.HasPixelData)
                    {
                        if (dicomFile.PixelData.IsJpeg)
                        {
                            Console.WriteLine(
                                "Transcoding pixel data to JPEG data.");
                            targetFileName =
                                GetJpegFileNamePattern(targetFileName);
                            exitCode = SavePixelDataToJpegFiles(targetFileName);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Transcoding pixel data to raw data.");
                            targetFileName = 
                                GetRawFileNamePattern(targetFileName);
                            exitCode = SavePixelDataToRawFiles(targetFileName);
                        }
                        if (exitCode == errorExitCode) return exitCode;
                    }
                    else
                    {
                        Console.Error.WriteLine("Found no pixel data.");
                        return errorExitCode;
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Problems processing file:\n" + e);
            return errorExitCode;
        }
        if (exitCode == errorExitCode) return exitCode;
        return normalExitCode;
    }
}
