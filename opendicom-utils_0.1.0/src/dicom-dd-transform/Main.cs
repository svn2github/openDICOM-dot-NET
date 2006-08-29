/*
    openDICOM.NET Utils 0.1.0

    openDICOM.NET Utils provides DICOM utility applications for DICOM
    related manipulation on Mono.
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
using System.Text.RegularExpressions;
using openDicom.Registry;


public sealed class DicomDataDictionaryTransformer
{
    public static readonly int normalExitCode = 0;
    public static readonly int errorExitCode = 1;

    public static DataElementDictionary dataElementDic = 
        new DataElementDictionary();

    public static UidDictionary uidDic = new UidDictionary();

    public static string defaultFormat = "src:b:";
    public static string defaultDataElementDic = "dicom-elements-2004.dic";
    public static string defaultUidDic = "dicom-uids-2004.dic";
    public static string defaultLinuxDir = 
        "/usr/share/opendicom.net/opendicom-utils/dd/";

    public static string[] dicType = new string[0];
    public static string[] srcDic = new string[0];
    public static string[] destDic = new string[0];

    public static int PrintUsage()
    {
        Console.Error.WriteLine("openDICOM.NET Utils");
        Console.Error.WriteLine(
            "Transforms data dictionary files between different formats.");
        Console.Error.WriteLine();
        Console.Error.WriteLine(
            "Usage: dicom-dd-transform dict:<type> [src:<format>:<source>] " +
            "dest:<format>:<dest>");
        Console.Error.WriteLine();
        Console.Error.WriteLine("type      specified dictionary type");
        Console.Error.WriteLine(
            "          data-element - data element dictionary");
        Console.Error.WriteLine(
            "          uid          - unique identifier dictionary");
        Console.Error.WriteLine("format    specified file format");
        Console.Error.WriteLine("          b - binary");
        Console.Error.WriteLine("          p - property");
        Console.Error.WriteLine("          x - xml");
        Console.Error.WriteLine("          c - csv");
        Console.Error.WriteLine("source    local source file");
        Console.Error.WriteLine("dest      local destination file");
        return errorExitCode;
    }

    public static int GetParameters(string[] args)
    {
        string dicTypePattern = "^dict:(data-element|uid)$";
        string dicPattern = "(b|B|p|P|x|X|c|C):[^:]+$";
        for (int i = 0; i < args.Length; i++)
        {
            if (Regex.IsMatch(args[i].ToLower(), dicTypePattern))
                dicType = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), "^src:" + dicPattern))
                srcDic = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), "^dest:" + dicPattern))
                destDic = args[i].Split(':');
            else
                return PrintUsage();
        }
        if (dicType.Length == 0) return PrintUsage();
        if (destDic.Length == 0) return PrintUsage();
        if (srcDic.Length == 0)
        {
            if (dicType[1].ToLower().Equals("data-element"))
                srcDic = GetDefaultDic(defaultDataElementDic).Split(':');
            else
                srcDic = GetDefaultDic(defaultUidDic).Split(':');
        }
        return normalExitCode;
    }

    public static string GetDefaultDic(string fileName)
    {
        if (File.Exists(defaultLinuxDir + fileName))
            return defaultFormat + defaultLinuxDir + fileName;
        else
            return defaultFormat + fileName;
    }

    public static int LoadFrom(IDicomDictionary dic, string format, 
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

    public static int SaveTo(IDicomDictionary dic, string format,
        string fileName)
    {
        Console.WriteLine("Writing {0}.", fileName);
        try
        {
            switch (format.ToLower())
            {
                case "b":
                    dic.SaveTo(fileName, DictionaryFileFormat.BinaryFile);
                    break;
                case "p":
                    dic.SaveTo(fileName, DictionaryFileFormat.PropertyFile);
                    break;
                case "x":
                    dic.SaveTo(fileName, DictionaryFileFormat.XmlFile);
                    break;
                case "c":
                    dic.SaveTo(fileName, DictionaryFileFormat.CsvFile);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Problems writing to file:\n" + e);
            return errorExitCode;
        }
        return normalExitCode;
    }

    public static int Transform(IDicomDictionary dic, 
        string[] srcFormatAndFileName, string[] destFormatAndFileName)
    {
        int exitCode = LoadFrom(dic, srcFormatAndFileName[1], 
            srcFormatAndFileName[2]);
        if (exitCode == errorExitCode) return exitCode;
        return SaveTo(dic, destFormatAndFileName[1], destFormatAndFileName[2]);
    }

    public static int Main(string[] args)
    {
        int exitCode = GetParameters(args);
        if (exitCode == errorExitCode) return exitCode;
        switch (dicType[1].ToLower())
        {
            case "data-element":
                exitCode = Transform(dataElementDic, srcDic, destDic); 
                if (exitCode == errorExitCode) return exitCode;
                Console.WriteLine("Processed {0} dictionary entries.", 
                    dataElementDic.Count);
                break;
            case "uid":
                exitCode = Transform(uidDic, srcDic, destDic); 
                if (exitCode == errorExitCode) return exitCode;
                Console.WriteLine("Processed {0} dictionary entries.", 
                    uidDic.Count);
                break;                  
        }
        return normalExitCode;
    }
}
