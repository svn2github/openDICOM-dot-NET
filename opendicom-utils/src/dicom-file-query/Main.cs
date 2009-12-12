/*
    openDICOM.NET Utils 0.2

    openDICOM.NET Utils provides DICOM utility applications for DICOM related
    manipulation on Mono
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
using System.IO;
using System.Collections;
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
    public static string defaultDataElementDic = "dicom-elements-2007.dic";
    public static string defaultUidDic = "dicom-uids-2007.dic";
    public static string defaultLinuxDir = 
        "/usr/share/opendicom.net/opendicom-utils/dd/";

    public static string defaultDecodingMode = "decode:strict";

    public static AcrNemaFile dicomFile = null;

    public static string[] elementDic = new string[0];
    public static string[] uidDic = new string[0];
    public static string dicomFileName = null;
    public static string[] keyAndPattern = new string[0];
    public static string[] decodingMode = new string[0];

    public static string[] resultBuffer = new string[8];
    public static ArrayList resultList = new ArrayList();

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
        Console.Error.WriteLine("          retired     - retired entries");
        Console.Error.WriteLine(
            "          non-retired - non-retired entries");
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
                "^(tag|description|vr|vm|length|value|retired|non-retired|" +
                    "any):[^:]+$"))
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
        if (System.IO.File.Exists(defaultLinuxDir + fileName))
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
        resultBuffer = new string[8] { "(offset)", "(tag)", "(description)", 
            "(value)", "(vr)", "(length)", "(vm)", "(retired)" };
        resultList.Add(resultBuffer);
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
                case "retired": 
                    if (d.Tag.GetDictionaryEntry().IsRetired)
                    {
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
                    }               
                    break;
                case "non-retired": 
                    if ( ! d.Tag.GetDictionaryEntry().IsRetired)
                    {
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
                    }               
                    break;
            }
            if (isMatch)
            {
                AddResult(d);
                foundMatches = true;
            }
        }
        return foundMatches;
    }

    public static void AddResult(DataElement element)
    {
        string value = "";
        if (element.Value.IsDate)
        {
            value = ((DateTime) element.Value[0]).ToShortDateString();
        }
        else if (element.Value.IsMultiValue)
        {
            value = element.Value[0].ToString();
            for (int i = 1; i < element.Value.Count; i++)
                value += "\\" + element.Value[i].ToString();
        }
        else if ( ! element.Value.IsArray && ! element.Value.IsSequence &&
            ! element.Value.IsNestedDataSet)
        {
            value = element.Value[0].ToString();
        }
        resultBuffer = new string[8] {
            string.Format("{0:X8}", element.StreamPosition),
            element.Tag.ToString(),
            element.Tag.GetDictionaryEntry().Description,
            value,
            element.VR.ToLongString(),
            element.ValueLength.ToString(),
            element.Tag.GetDictionaryEntry().VM.ToString(),
            element.Tag.GetDictionaryEntry().IsRetired ? "RET" : "" };
        resultList.Add(resultBuffer);
    }

    public static int QueryMatches(string key, string pattern)
    {
        try
        {
            bool foundMatches = SearchFor(
                dicomFile.GetJointDataSets().GetJointSubsequences(), key, 
                pattern);
            if ( ! foundMatches) Console.WriteLine("No matches found.");
            else
            {
                Console.WriteLine("Found {0} matches.", resultList.Count - 1);
                Console.WriteLine("Query results:");
                int[] maxLength = { 0, 0, 0, 0, 0, 0, 0, 0 };
                string[] result;
                int i = 0;
                for (i = 0; i < resultList.Count; i++)
                {
                    result = (resultList[i] as string[]);
                    for (int k = 0; k < result.Length; k++)
                    {
                        if (maxLength[k] < result[k].Length)
                            maxLength[k] = result[k].Length;
                    }
                }
                string s;
                for (i = 0; i < resultList.Count; i++)
                {
                    result = (resultList[i] as string[]);
                    s = string.Format(
                        "{0,-" + maxLength[0].ToString() + "} | " +
                        "{1,-" + maxLength[1].ToString() + "} | " +
                        "{2,-" + maxLength[2].ToString() + "} | " +
                        "{3,-" + maxLength[3].ToString() + "} | " +
                        "{4,-" + maxLength[4].ToString() + "} | " +
                        "{5,-" + maxLength[5].ToString() + "} | " +
                        "{6,-" + maxLength[6].ToString() + "} | " +
                        "{7,-" + maxLength[7].ToString() + "}", 
                        result[0], result[1], result[2], result[3], result[4],
                        result[5], result[6], result[7]);
                    Console.WriteLine(s);
                    if (i == 0)
                    {
                        for (int k = 0; k < s.Length; k++)
                            Console.Write("-");
                        Console.WriteLine();
                    }
                }
            }
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
