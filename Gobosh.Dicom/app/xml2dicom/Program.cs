/*
 * xml2dicom commandline utility
 * 
 * (C) 2006,2007 Timo Kaluza
 * tk@gobosh.net
 * 
 * This program is free software; you can redistribute it and/or modify   
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 *     You should have received a copy of the GNU General Public License
 *     along with this program; if not, write to the Free Software
 * 
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
 * 
 * this command line utility converts an XML representation into a DICOM file
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Gobosh.DICOM;

namespace xml2dicom
{
    class Program
    {
        static void Main(string[] args)
        {
            if ((args.Length < 2) || (args.Length > 3))
            {
                Console.WriteLine("xml2dicom - (c) 2007 T.Kaluza");
                Console.WriteLine("converts a DICOM XML file into native DICOM format");
                Console.WriteLine("usage:");
                Console.WriteLine("\txml2dicom (xmlfile|directory) (dicomfile|directory) [datadictionary]");
                Console.WriteLine("");
            }
            else
            {
                Gobosh.DICOM.Document m;
                string datadict = "dd-ps3-2006.xml";
                if (args.Length == 3)
                {
                    datadict = args[2];
                }
                // determine if directory or file mode
                if (System.IO.Directory.Exists(args[0]))
                {
                    // directory mode
                    Console.WriteLine("converting all files in {0}:", args[0]);
                    string[] myFileList = System.IO.Directory.GetFiles(args[0], "*", System.IO.SearchOption.AllDirectories);
                    foreach (string inputFile in myFileList)
                    {
                        Console.Write("reading {0}...", inputFile);
                        m = Gobosh.DICOM.XMLReader.ReadFromFile(inputFile, datadict);
                        string outputFile = args[1]+System.IO.Path.DirectorySeparatorChar+System.IO.Path.GetFileName(inputFile);
                        // remove trailing .xml
                        if (outputFile.EndsWith(".xml", true, System.Globalization.CultureInfo.CurrentCulture))
                        {
                            outputFile = outputFile.Remove(outputFile.Length - 4, 4);
                        }

                        Console.WriteLine("writing {0}...", outputFile);
                        m.SaveToFile(outputFile);
                    }

                }
                else
                {
                    Console.Write("reading {0}...", args[0]);
                    m = Gobosh.DICOM.XMLReader.ReadFromFile(args[0], datadict);
                    if (System.IO.Directory.Exists(args[1]))
                    {
                        string outputFile = args[1] + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(args[0]);
                        // remove trailing .xml
                        if (outputFile.EndsWith(".xml", true, System.Globalization.CultureInfo.CurrentCulture))
                        {
                            outputFile = outputFile.Remove(outputFile.Length - 4, 4);
                        }
                        Console.WriteLine("writing {0}...", outputFile);
                        m.SaveToFile(outputFile);
                    }
                    else
                    {
                        m.SaveToFile(args[1]);
                    }
                }
                
            }
        }
    }

}
