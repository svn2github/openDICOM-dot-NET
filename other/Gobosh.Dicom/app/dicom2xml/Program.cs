/*
 * dicom2xml command line utility
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
 * this command line utility converts a DICOM file into an XML representation
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Gobosh.DICOM;

namespace dicom2xml
{
    class Program
    {
        static void Main(string[] args)
        {
            if ( (args.Length < 2) || (args.Length > 3) )
            {
                Console.WriteLine("dicom2xml - (c) 2007 T.Kaluza");
                Console.WriteLine("converts a DICOM file into XML representation");
                Console.WriteLine("usage:");
                Console.WriteLine("\tdicom2xml (dicomfile|directory) (xmlfile|directory) [datadictionary]");
                Console.WriteLine("");
            }
            else
            {
                if (System.IO.Directory.Exists(args[0]))
                {
                    // batch mode#
                    Console.WriteLine("converting all files in {0}:", args[0]);
                    string[] myFileList = System.IO.Directory.GetFiles(args[0], "*", System.IO.SearchOption.AllDirectories);
                    foreach (string inputFile in myFileList)
                    {
                        Console.Write("reading {0}...", inputFile);
                        try
                        {
                            Gobosh.DICOM.Document m = new Document();
                            // if data dictionary is given as third argument, then use this
                            if (args.Length == 3)
                            {
                                m.SetDataDictionary(args[2]);
                            }
                            else
                            {
                                m.SetDataDictionary("dd-ps3-2006.xml");
                            }
                            m.LoadFromFile(inputFile);
                            string outputFile = args[1] + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(inputFile);
                            // add trailing .xml
                            outputFile = outputFile + ".xml";
                            Console.WriteLine("writing {0}...", outputFile);
                            Gobosh.DICOM.XMLWriter.WriteToFile(outputFile, m.GetRootNode());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("failed: {0}", e);
                        }
                    }
                }
                else
                {
                    Gobosh.DICOM.Document m = new Document();
                    // if data dictionary is given as third argument, then use this
                    if (args.Length == 3)
                    {
                        m.SetDataDictionary(args[2]);
                    }
                    else
                    {
                        m.SetDataDictionary("dd-ps3-2006.xml");
                    }
                    m.LoadFromFile(args[0]);
                    if (System.IO.Directory.Exists(args[1]))
                    {
                        string outputFile = args[1] + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(args[0]);
                        // add trailing .xml
                        outputFile = outputFile+".xml";
                        Console.WriteLine("writing {0}...", outputFile);
                        Gobosh.DICOM.XMLWriter.WriteToFile(outputFile, m.GetRootNode());
                    }
                    else
                    {
                        Gobosh.DICOM.XMLWriter.WriteToFile(args[1], m.GetRootNode());
                    }
                }
            }
        }
    }
}
