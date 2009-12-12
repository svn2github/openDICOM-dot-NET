/*
 * Anonymizer.exe
 * 
 * A simple DICOM anonymizer tool
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
 *  
 * 
 * Anonymizes all DA, TM and PN Data Elements
 * For demonstration and educational purposes only
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Gobosh.DICOM;

namespace Anonymizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Anonymizer - a simple DICOM anonymizer tool");
                Console.WriteLine("(C) 2006,2007 T. Kaluza");
                Console.WriteLine("    tk@gobosh.net");
                Console.WriteLine("Usage:");
                Console.WriteLine("    Anonymizer Filename [Filename] [...]");
                Console.WriteLine("");
                Console.WriteLine("    Anonymizes all DA,TM,DT and PN elements");
                Console.WriteLine("    For demonstration and educational purpose only!");
                Console.WriteLine("");
            }
            else
            {
                foreach (string filename in args)
                {
                    // create the document object
                    Document myDocument = new Document();
                    // use the current datadictionary
                    myDocument.SetDataDictionary("dd-ps3-2006.xml");

                    // load the document
                    Console.Write("loading "+filename+"...");
                    myDocument.LoadFromFile(filename);

                    // anonymize it
                    Console.Write("anonymizing...");
                    Anonymize(myDocument.GetRootNode());

                    // save the document back to disk
                    Console.Write("saving...");
                    myDocument.SaveToFile("anon_" + filename);
                    Console.WriteLine("done");

                }
            }
        }

        static void Anonymize(DataElement node)
        {
            // iterate through all nodes
            foreach (DataElement n in node)
            {
                if (n is Gobosh.DICOM.DataElements.DA)
                {
                    n.SetValue("19000101");
                }
                if (n is Gobosh.DICOM.DataElements.TM)
                {
                    n.SetValue("00:00");
                }
                if (n is Gobosh.DICOM.DataElements.PN)
                {
                    n.SetValue("Onymous^A^N^^");
                }
                if (n is Gobosh.DICOM.DataElements.DT)
                {
                    n.SetValue("19000101");
                }
                // recurse down (sequences and items)
                if (n.Count > 0)
                {
                    Anonymize(n);
                }
            }
        }
    }
}
