/*
    openDICOM.NET Utils 0.1.1

    openDICOM.NET Utils provides DICOM utility applications for DICOM related
    manipulation on Mono.
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
using System.Text.RegularExpressions;
using System.Collections;
using openDicom.Registry;
using openDicom.DataStructure;


public sealed class DicomDataDictionaryQuery
{
    public static readonly int normalExitCode = 0;
    public static readonly int errorExitCode = 1;

    public static DataElementDictionary dataElementDic = 
        new DataElementDictionary();

    public static UidDictionary uidDic = new UidDictionary();

    public static string defaultFormat = "b:";
    public static string defaultDataElementDic = "dicom-elements-2007.dic";
    public static string defaultUidDic = "dicom-uids-2007.dic";
    public static string defaultLinuxDir = 
        "/usr/share/opendicom.net/opendicom-utils/dd/";

    public static string[] dicType = new string[0];
    public static string[] dic = new string[0];
    public static string[] keyAndPattern = new string[0];

    public static string[] resultBuffer = new string[5];
    public static ArrayList resultList = new ArrayList();

    public static int PrintUsage()
    {
        Console.Error.WriteLine("openDICOM.NET Utils");
        Console.Error.WriteLine(
            "Queries a DICOM data dictionary for user specified informations.");
        Console.Error.WriteLine();
        Console.Error.WriteLine(
            "Usage: dicom-dd-query dict:<type> [<format>:<src>] " +
            "<key>:<pattern>");
        Console.Error.WriteLine();
        Console.Error.WriteLine("type      specified dictionary type");
        Console.Error.WriteLine(
            "          tag - DICOM data element dictionary");
        Console.Error.WriteLine(
            "          uid - DICOM unique identifier dictionary");
        Console.Error.WriteLine("format    specified file format");
        Console.Error.WriteLine("          b - binary");
        Console.Error.WriteLine("          p - property");
        Console.Error.WriteLine("          x - xml");
        Console.Error.WriteLine("          c - csv");
        Console.Error.WriteLine("src       local dictionary file");
        Console.Error.WriteLine("key       specified query key");
        Console.Error.WriteLine("          - data element dictionary:");
        Console.Error.WriteLine("             tag         - tag");
        Console.Error.WriteLine("             description - description");
        Console.Error.WriteLine(
            "             vr          - value representation");
        Console.Error.WriteLine(
            "             vm          - value multiplicity");
        Console.Error.WriteLine("          - UID dictionary:");
        Console.Error.WriteLine("             uid  - unique identifier");
        Console.Error.WriteLine("             name - description");
        Console.Error.WriteLine("             type - type");
        Console.Error.WriteLine("          - all dictionaries:");
        Console.Error.WriteLine("             retired     - retired entries");
        Console.Error.WriteLine(
            "             non-retired - non-retired entries");
        Console.Error.WriteLine(
            "             any         - look for matches overall keys");
        Console.Error.WriteLine("pattern   query pattern corresponding to key");
        Console.Error.WriteLine(
            "          - wildcards (*) represent any substring");
        Console.Error.WriteLine(
            "          - question marks (?) represent any character");
        Console.Error.WriteLine("          - matching is not case sensitive");
        return errorExitCode;
    }

    public static int GetParameters(string[] args)
    {
        string dicTypePattern = "^dict:(tag|uid)$";
        string dicPattern = "^(b|B|p|P|x|X|c|C):[^:]+$";
        for (int i = 0; i < args.Length; i++)
        {
            if (Regex.IsMatch(args[i].ToLower(), dicTypePattern))
                dicType = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), dicPattern))
                dic = args[i].Split(':');
            else if (Regex.IsMatch(args[i].ToLower(), 
                "^(tag|description|vr|vm|uid|name|type|retired|non-retired" +
                    "|any):[^:]+$"))
                keyAndPattern = args[i].Split(':');
            else
                return PrintUsage();
        }
        if (dicType.Length == 0) return PrintUsage();
        if (dic.Length == 0) 
        {
            if (dicType[1].ToLower().Equals("tag"))
                dic = GetDefaultDic(defaultDataElementDic).Split(':');
            else
                dic = GetDefaultDic(defaultUidDic).Split(':');
        }
        if (keyAndPattern.Length == 0) return PrintUsage();
        if ((dicType[1].ToLower().Equals("tag") && 
            ! Regex.IsMatch(keyAndPattern[0].ToLower(), 
            "^(tag|description|vr|vm|retired|non-retired|any)$")) ||
            (dicType[1].ToLower().Equals("uid") && 
            ! Regex.IsMatch(keyAndPattern[0].ToLower(), 
            "^(uid|name|type|retired|non-retired|any)$")))
            return PrintUsage();                
        return normalExitCode;
    }

    public static string GetDefaultDic(string fileName)
    {
        if (File.Exists(defaultLinuxDir + fileName))
            return defaultFormat + defaultLinuxDir + fileName;
        else
            return defaultFormat + fileName;
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

    public static int QueryDataElementDicMatches(string key, string pattern)
    {
        resultBuffer = new string[5]
            { "(tag)", "(description)", "(vr)", "(vm)", "(retired)" };
        resultList.Add(resultBuffer);
        try
        {
            bool foundMatches = false;
            switch (key)
            {
                case "tag":
                    foreach (DataElementDictionaryEntry d in 
                        dataElementDic.ToArray())
                    {
                        if (Regex.IsMatch(d.Tag.ToString().ToLower(),
                            pattern.Replace(" ", null)))
                        {
                            AddDataElementDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "description":
                    foreach (DataElementDictionaryEntry d in 
                        dataElementDic.ToArray())
                    {
                        if (Regex.IsMatch(d.Description.ToLower(), pattern))
                        {
                            AddDataElementDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "vr":
                    foreach (DataElementDictionaryEntry d in
                        dataElementDic.ToArray())
                    {
                        if (Regex.IsMatch(d.VR.ToLongString().ToLower(), 
                            pattern))
                        {
                            AddDataElementDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "vm":
                    foreach (DataElementDictionaryEntry d in 
                        dataElementDic.ToArray())
                    {
                        if (Regex.IsMatch(d.VM.Value, pattern))
                        {
                            AddDataElementDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "any":
                    foreach (DataElementDictionaryEntry d in 
                        dataElementDic.ToArray())
                    {
                        if (Regex.IsMatch(d.Tag.ToString().ToLower(),
                            pattern.Replace(" ", null)) ||
                            Regex.IsMatch(d.Description.ToLower(), pattern) ||
                            Regex.IsMatch(d.VR.ToLongString().ToLower(), 
                                pattern) ||
                            Regex.IsMatch(d.VM.Value, pattern))
                        {
                            AddDataElementDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "retired":
                    foreach (DataElementDictionaryEntry d in 
                        dataElementDic.ToArray())
                    {
                        if (d.IsRetired)
                        {
                            if (Regex.IsMatch(d.Tag.ToString().ToLower(),
                                pattern.Replace(" ", null)) ||
                                Regex.IsMatch(d.Description.ToLower(), pattern) ||
                                Regex.IsMatch(d.VR.ToLongString().ToLower(), 
                                    pattern) ||
                                Regex.IsMatch(d.VM.Value, pattern))
                            {
                                AddDataElementDicResult(d);
                                foundMatches = true;
                            }
                        }
                    }
                    break;
                case "non-retired":
                    foreach (DataElementDictionaryEntry d in 
                        dataElementDic.ToArray())
                    {
                        if ( ! d.IsRetired)
                        {
                            if (Regex.IsMatch(d.Tag.ToString().ToLower(),
                                pattern.Replace(" ", null)) ||
                                Regex.IsMatch(d.Description.ToLower(), pattern) ||
                                Regex.IsMatch(d.VR.ToLongString().ToLower(), 
                                    pattern) ||
                                Regex.IsMatch(d.VM.Value, pattern))
                            {
                                AddDataElementDicResult(d);
                                foundMatches = true;
                            }
                        }
                    }
                    break;
            }
            if ( ! foundMatches) Console.WriteLine("No matches found.");
            else
            {
                Console.WriteLine("Found {0} matches.", resultList.Count - 1);
                Console.WriteLine("Query results:");
                int[] maxLength = { 0, 0, 0, 0, 0 };
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
                        "{4,-" + maxLength[4].ToString() + "}", 
                        result[0], result[1], result[2], result[3], result[4]);
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

    public static void AddDataElementDicResult(
        DataElementDictionaryEntry entry)
    {
        resultBuffer = new string[5] { entry.Tag.ToString(), entry.Description, 
            entry.VR.ToLongString(), entry.VM.ToString(),
            entry.IsRetired ? "RET" : "" };
        resultList.Add(resultBuffer);
    }

    public static int QueryUidDicMatches(string key, string pattern)
    {
        resultBuffer = new string[5] { "(uid)", "(name)", "(type)", 
            "(retired)", "" };
        resultList.Add(resultBuffer);
        try
        {
            bool foundMatches = false;
            switch (key)
            {
                case "uid":
                    foreach (UidDictionaryEntry d in uidDic.ToArray())
                    {
                        if (Regex.IsMatch(d.Uid.ToString().ToLower(), pattern))
                        {
                            AddUidDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "name":
                    foreach (UidDictionaryEntry d in uidDic.ToArray())
                    {
                        if (Regex.IsMatch(d.Name.ToLower(), pattern))
                        {
                            AddUidDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "type":
                    foreach (UidDictionaryEntry d in uidDic.ToArray())
                    {
                        if (Regex.IsMatch(UidType.GetName(typeof(UidType),
                            d.Type).ToLower(), pattern))
                        {
                            AddUidDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "any":
                    foreach (UidDictionaryEntry d in uidDic.ToArray())
                    {
                        if (Regex.IsMatch(d.Name.ToLower(), pattern) ||
                            Regex.IsMatch(d.Uid.ToString().ToLower(), pattern) ||
                            Regex.IsMatch(UidType.GetName(typeof(UidType),
                                d.Type).ToLower(), pattern))
                        {
                            AddUidDicResult(d);
                            foundMatches = true;
                        }
                    }
                    break;
                case "retired":
                    foreach (UidDictionaryEntry d in uidDic.ToArray())
                    {
                        if (d.IsRetired)
                        {
                            if (Regex.IsMatch(d.Name.ToLower(), pattern) ||
                                Regex.IsMatch(d.Uid.ToString().ToLower(), pattern) ||
                                Regex.IsMatch(UidType.GetName(typeof(UidType),
                                    d.Type).ToLower(), pattern))
                            {
                                AddUidDicResult(d);
                                foundMatches = true;
                            }
                        }
                    }
                    break;
                case "non-retired":
                    foreach (UidDictionaryEntry d in uidDic.ToArray())
                    {
                        if ( ! d.IsRetired)
                        {
                            if (Regex.IsMatch(d.Name.ToLower(), pattern) ||
                                Regex.IsMatch(d.Uid.ToString().ToLower(), pattern) ||
                                Regex.IsMatch(UidType.GetName(typeof(UidType),
                                    d.Type).ToLower(), pattern))
                            {
                                AddUidDicResult(d);
                                foundMatches = true;
                            }
                        }
                    }
                    break;
            }
            if ( ! foundMatches) Console.WriteLine("No matches found.");
            else
            {
                Console.WriteLine("Found {0} matches.", resultList.Count - 1);
                Console.WriteLine("Query results:");
                int[] maxLength = { 0, 0, 0, 0, 0 };
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
                        "{3,-" + maxLength[3].ToString() + "}", 
                        result[0], result[1], result[2], result[3]);
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

    public static void AddUidDicResult(UidDictionaryEntry entry)
    {
        resultBuffer = new string[5] { entry.Uid.ToString(), entry.Name, 
            UidType.GetName(typeof(UidType), entry.Type), 
            entry.IsRetired ? "RET" : "", "" };
        resultList.Add(resultBuffer);
    }

    public static int Main(string[] args)
    {
        int exitCode = GetParameters(args);
        if (exitCode == errorExitCode) return exitCode;
        string pattern = "^" + keyAndPattern[1].ToLower()
            .Replace(".", "\\.")
            .Replace("?", ".")
            .Replace("*", ".*")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace(",", "\\,") + "$";
        switch (dicType[1].ToLower())
        {
            case "tag":
                exitCode = LoadDicFrom(dataElementDic, dic[0], dic[1]); 
                if (exitCode == errorExitCode) return exitCode;
                Console.WriteLine("Processed {0} dictionary entries.", 
                    dataElementDic.Count);
                exitCode = QueryDataElementDicMatches(
                    keyAndPattern[0].ToLower(), pattern);
                if (exitCode == errorExitCode) return exitCode;
                break;
            case "uid":
                exitCode = LoadDicFrom(uidDic, dic[0], dic[1]);
                if (exitCode == errorExitCode) return exitCode;
                Console.WriteLine("Processed {0} dictionary entries.", 
                    uidDic.Count);
                exitCode = QueryUidDicMatches(keyAndPattern[0].ToLower(),
                    pattern);
                if (exitCode == errorExitCode) return exitCode;
                break;
        }
        return normalExitCode;
    }
}
