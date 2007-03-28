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


    $Id$
*/
using System;
using System.IO;
using System.Text.RegularExpressions;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding.Type;
using openDicom.File;


public sealed class DicomFileQuery
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
    public static string[] keyAndPattern = new string[0];
    public static string[] decodingMode = new string[0];


    public static int PrintUsage()
    {
        Console.Error.WriteLine("openDICOM.NET Utils");
        Console.Error.WriteLine(
            "Queries a DICOM file for user specified informations.");
        Console.Error.WriteLine();
        Console.Error.WriteLine(
            "Usage: dicom-file-query [element-dic:<format>:<src>] " +
            "[uid-dic:<format>:<src>] <dcm-file> <key>:<pattern> " +
            "[decode:<mode>]");
        Console.Error.WriteLine();
        Console.Error.WriteLine("format    specified dictionary file format");
        Console.Error.WriteLine("          b - binary");
        Console.Error.WriteLine("          p - property");
        Console.Error.WriteLine("          x - xml");
        Console.Error.WriteLine("          c - csv");
        Console.Error.WriteLine("src       local dictionary file");
        Console.Error.WriteLine("dcm-file  DICOM file");
        Console.Error.WriteLine("key       specified query key");
        Console.Error.WriteLine("          tag         - tag");
        Console.Error.WriteLine("          description - description");
        Console.Error.WriteLine("          vr          - value representation");
        Console.Error.WriteLine("          vm          - value multiplicity");
        Console.Error.WriteLine("          length      - length");
        Console.Error.WriteLine("          value       - value as string");
        Console.Error.WriteLine(
            "          any         - look for matches overall keys");
        Console.Error.WriteLine("pattern   query pattern corresponding to key");
        Console.Error.WriteLine(
            "          - wildcards (*) represent any substring");
        Console.Error.WriteLine(
            "          - question marks (?) represent any character");
        Console.Error.WriteLine("          - matching is not case sensitive");
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
                "^(tag|description|vr|vm|length|value|any):[^:]+$"))
                keyAndPattern = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), "^decode:(strict|lax)$"))
                decodingMode = args[i].Split(':');
            else if (dicomFileName == null)
                dicomFileName = args[i];
            else
                return PrintUsage();
        }
        if (uidDic.Length == 0) 
            uidDic = GetDefaultDic(defaultUidDic, "uid-dic:" + defaultFormat)
                .Split(':');
        if (elementDic.Length == 0) 
            elementDic = GetDefaultDic(defaultDataElementDic, 
                "element-dic:" + defaultFormat)
                    .Split(':');
        if (dicomFileName == null) return PrintUsage();
        if (keyAndPattern.Length == 0) return PrintUsage();
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

    public static bool SearchFor(Sequence sequence, string key, string pattern)
    {
        Console.WriteLine(ToString("(pos)", "(tag)", "(vr)", "(vm)", 
            "(description)", "(length)", new string[1] { "(values)" }));
        Console.Write("---------+-");
        Console.Write("------------+-");
        Console.Write("---------------------------+-");
        Console.Write("-----+-");
        Console.Write("---------------------------------------------+-");
        Console.Write("---------+-");
        Console.WriteLine("----------------------");
        bool foundMatches = false;
        foreach (DataElement d in sequence)
        {
            bool isMatch = false;
            switch (key)
            {
                case "tag": 
                    isMatch = Regex.IsMatch(d.Tag.ToString().ToLower(),
                        pattern.Replace(" ", null));
                    break;
                case "description": 
                    isMatch = Regex.IsMatch(d.VR.Tag.GetDictionaryEntry()
                        .Description.ToLower(), pattern);
                    break;
                case "vr": 
                    isMatch = Regex.IsMatch(d.VR.ToLongString().ToLower(), 
                        pattern);
                    break;
                case "vm": 
                    isMatch = Regex.IsMatch(
                        d.VR.Tag.GetDictionaryEntry().VM.Value, 
                        pattern);
                    break;
                case "length": 
                    isMatch = Regex.IsMatch(d.ValueLength.ToString(), 
                        pattern);
                    break;
                case "value": 
                    int i = 0;
                    while ( ! isMatch && i < d.Value.Count)
                    {
                        isMatch = Regex.IsMatch(d.Value[i].ToString().ToLower(),
                            pattern);
                        i++;
                    }
                    break;
                case "any": 
                    isMatch = Regex.IsMatch(d.Tag.ToString().ToLower(),
                        pattern.Replace(" ", null)) ||
                        Regex.IsMatch(
                            d.VR.Tag.GetDictionaryEntry().Description.ToLower(), 
                            pattern) ||
                        Regex.IsMatch(d.VR.ToLongString().ToLower(), pattern) ||
                        Regex.IsMatch(d.VR.Tag.GetDictionaryEntry().VM.Value, 
                            pattern) ||
                        Regex.IsMatch(d.ValueLength.ToString(), pattern);
                    i = 0;
                    while ( ! isMatch && i < d.Value.Count)
                    {
                        isMatch = Regex.IsMatch(d.Value[i].ToString().ToLower(),
                            pattern);
                        i++;
                    }                    
                    break;
            }
            if (isMatch)
            {
                Console.WriteLine(ToString(d));
                foundMatches = true;
            }
        }
        return foundMatches;
    }

    public static string ToString(string pos, string tag, string vr, string vm,
        string description, string length, object[] value)
    {
        string result = string.Format(
            "{0,-8} | {1,-11} | {2,-26} | {3,-4} | {4,-44} | {5,-8} | {6}",
            pos, tag, vr, vm, description, length, value[0]);
        for (int i = 1; i < value.Length; i++)
            result += "\n" + string.Format(
                "{0,-8} | {1,-11} | {2,-26} | {3,-4} | {4,-44} | {5,-8} | {6}",
                "", "", "", "", "", "", value[i]);
        return result;
    }

    public static string ToString(DataElement element)
    {
        string pos = string.Format("{0:X8}", element.StreamPosition);
        string tag = element.Tag.ToString();
        string vr = element.VR.ToLongString();
        string vm = element.Tag.GetDictionaryEntry().VM.ToString();
        string description = element.Tag.GetDictionaryEntry().Description;
        string length = element.ValueLength.ToString();
        string output = "";
        if (element.Value.IsDate && ! element.Value.IsEmpty)
        {
            // hide zero valued time
            string[] date = new string[element.Value.Count];
            for (int i = 0; i < date.Length; i++)
                date[i] = ((DateTime) element.Value[i]).ToShortDateString();
            output = ToString(pos, tag, vr, vm, description, length, date);
        }
        else if ( ! element.Value.IsEmpty)
            output = ToString(pos, tag, vr, vm, description, length, 
                element.Value.ToArray());
        else
            output = ToString(pos, tag, vr, vm, description, length, 
                new string[1] { "" });
        return output;
    }

    public static int QueryMatches(string key, string pattern)
    {
        Console.WriteLine("Query results:");
        try
        {
            bool foundMatches = SearchFor(
                dicomFile.GetJointDataSets().GetJointSubsequences(), key, 
                pattern);
            if ( ! foundMatches) Console.WriteLine("No matches found.");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Problems processing query: " + e.Message);
            return errorExitCode;
        }
        return normalExitCode;
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
        string pattern = "^" + keyAndPattern[1].ToLower()
            .Replace(".", "\\.")
            .Replace("?", ".")
            .Replace("*", ".*")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace(",", "\\,") + "$";
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
            exitCode = QueryMatches(keyAndPattern[0].ToLower(), pattern);
            if (exitCode == errorExitCode) return exitCode;
        }
        catch (Exception e)
        {
           Console.Error.WriteLine("Problems processing file:\n" + e);
           return errorExitCode;
        }
        return normalExitCode;
    }
}
