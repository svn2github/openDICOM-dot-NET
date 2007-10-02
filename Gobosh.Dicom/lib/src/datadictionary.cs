/* Gobosh.DICOM
 * Data Dictionary handling
 * 
 * (C) 2006,2007 Timo Kaluza
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
 */

using System;
using System.Xml;
using System.Collections;

namespace Gobosh
{
    namespace DICOM
    {
        #region VR Consts
        /// <summary>
        /// Contains constants for faster comparison to VR codes
        /// </summary>
        sealed class ValueRepresentationConsts
        {
            public const int AE = 0x4145;
            public const int AS = 0x4153;
            public const int AT = 0x4154;
            public const int CS = 0x4353;
            public const int DA = 0x4441;
            public const int DS = 0x4453;
            public const int DT = 0x4454;
            public const int FL = 0x464C;
            public const int FD = 0x4644;
            public const int IS = 0x4953;
            public const int LO = 0x4C4F;
            public const int LT = 0x4C54;
            public const int OB = 0x4F42;
            public const int OF = 0x4F46;
            public const int OW = 0x4F57;
            public const int PN = 0x504E;
            public const int SH = 0x5348;
            public const int SL = 0x534C;
            public const int SQ = 0x5351;
            public const int SS = 0x5353;
            public const int ST = 0x5354;
            public const int TM = 0x544D;
            public const int UI = 0x5549;
            public const int UL = 0x554C;
            public const int UN = 0x554E;
            public const int US = 0x5553;
            public const int UT = 0x5554;
        }
        #endregion

        #region DataDictionaryEntry

        /// <summary>
        /// Describes an DataDictionaryEntry
        /// </summary>
        public sealed class DataDictionaryEntry
        {
            public int Group;
            public int Element;
            public string Name;
            public string ValueRepresentation;
            public int Min, Max, Tupel;
            public bool Retired;

            public DataDictionaryEntry(int group, int element, string name, string valuerepresentation, int min, int max, int tupel, bool retired)
            {
                Name = name;
                Group = group;
                Element = element;
                ValueRepresentation = valuerepresentation;
                Min = min;
                Max = max;
                Tupel = tupel;
                Retired = retired;
            }
        }

        #endregion

        #region DataDictionary class
        /// <summary>
        /// DataDictionary holds a complete DataDictionary from an XML file
        /// </summary>
        public sealed class DataDictionary
        {
            #region private data members

            /// <summary>
            /// ElementsByGroup is a Hashtable, which key is "groupnumber" and value
            /// another Hashtable, which key is "elementnumber" and value is the
            /// DataElement description object
            /// </summary>
            Hashtable ElementsByGroup;

            /// <summary>
            /// The filename of the loaded datadictionary
            /// </summary>
            string FileName;

            #endregion

            public DataDictionary(string filename)
			{
				ElementsByGroup = null;
				// create a document
				XmlDocument myDoc = new XmlDocument();

                // keep the "friendly name"
				FileName = System.IO.Path.GetFileNameWithoutExtension(filename);
                bool isLoaded = false;

                // and load it from file if existent
                if ( System.IO.File.Exists(filename) )
                {
				    try
				    {
					    myDoc.Load(filename);
                        isLoaded = true;
				    }
                    catch
                    {
                        // don't throw, there are alternatives..
                    }
                }
                if (!isLoaded)
                {
                    try
                    {
                        // get the DLL or, if compiled in, the executable
                        System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                        // get the simple name
                        string mySimpleAssemblyName = myAssembly.GetName().Name;
                        // construct the resource name (add the datadictionaries and set them to "Embedded Resource")
                        string myResourceName = mySimpleAssemblyName + ".Resources." + System.IO.Path.GetFileName(filename);// .Replace('-', '_');
                        // create a memory stream
                        System.IO.Stream myStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(myResourceName);
                        // load data dictionary from memory stream
                        myDoc.Load(myStream);
                        isLoaded = true;
                    }
                    catch (Exception er)
                    {
                        throw new Exception("Error loading Dictionary: " + er.Message);
                    }
                }

                if (isLoaded)
                {
                    CreateStructures(myDoc);
                }
                else
                {
                    throw new Exception("The data dictionary could not be loaded");
                }
			}

            /// <summary>
            /// The Name of the loaded Dictionary
            /// </summary>
            public string Name
            {
                get
                {
                    return FileName;
                }
            }

            /// <summary>
            /// Reads the XML DOM and builds lookup structures for the Data Dictionary
            /// </summary>
            /// <param name="document">A reference to an XmlDocument object.</param>
            private void CreateStructures(XmlDocument document)
            {

                int group, element;
                string valuerep;
                string name;
                int min, max, tupel;
                if (ElementsByGroup == null)
                {
                    ElementsByGroup = new Hashtable(document.FirstChild.ChildNodes.Count);
                }
                else
                {
                    ElementsByGroup.Clear();
                }

                XmlNode rootnode = document.FirstChild;
                foreach (XmlElement node in rootnode.ChildNodes)
                {
                    group = int.Parse(node.GetAttribute("group"), System.Globalization.NumberStyles.HexNumber);
                    element = int.Parse(node.GetAttribute("tag"), System.Globalization.NumberStyles.HexNumber);

                    valuerep = node.GetAttribute("vr");
                    min = int.Parse(node.GetAttribute("min"));
                    if (node.HasAttribute("max"))
                    {
                        string myMax = node.GetAttribute("max");
                        if (myMax == "n")
                        {
                            max = -1;
                        }
                        else
                        {
                            max = int.Parse(myMax);
                        }
                    }
                    else
                    {
                        max = min;
                    }
                    if (node.HasAttribute("tupel"))
                    {
                        tupel = int.Parse(node.GetAttribute("tupel"));
                    }
                    else
                    {
                        tupel = 1;
                    }

                    name = node.InnerText;
                    if (ElementsByGroup.ContainsKey(group))
                    {
                        Hashtable val = (Hashtable)ElementsByGroup[group];
                        if (val.ContainsKey(element))
                        {
                            ArrayList valobject = (ArrayList)val[element];
                            valobject.Add(
                                new DataDictionaryEntry(group, element, name, valuerep, min, max, tupel, node.HasAttribute("retired"))
                                );
                        }
                        else
                        {
                            ArrayList valobject = new ArrayList();
                            valobject.Add(
                                new DataDictionaryEntry(group, element, name, valuerep, min, max, tupel, node.HasAttribute("retired"))
                                );
                            val.Add(element, valobject);
                        }
                    }
                    else
                    {
                        Hashtable val = new Hashtable();
                        ArrayList valobject = new ArrayList();
                        valobject.Add(
                            new DataDictionaryEntry(group, element, name, valuerep, min, max, tupel, node.HasAttribute("retired"))
                            );
                        val.Add(element, valobject);
                        ElementsByGroup.Add(group, val);
                    }
                }
            }

            public byte[] getValueRepresentation(int group, int element)
            {
                byte[] result = new byte[2];
                result[0] = (byte)'U';
                result[1] = (byte)'N';
                if (this.ElementsByGroup.ContainsKey(group))
                {
                    Hashtable myElements = (Hashtable)this.ElementsByGroup[group];
                    if (myElements.ContainsKey(element))
                    {
                        ArrayList myList = (ArrayList)myElements[element];

                        string myVR = ((DataDictionaryEntry)myList[0]).ValueRepresentation;
                        result[0] = (byte)myVR[0];
                        result[1] = (byte)myVR[1];
                    }
                }
                return result;
            }

            public string getValueDescription(int group, int element)
            {
                // element number 0 is always "Group Length"
                if (element == 0)
                {
                    return "Group Length";
                }
                // check for private element
                // PS.3.5-2006, 7.8.1: odd groups are private
                if ((group & 1) != 0)
                {
                    // but odd groups < 8 are illegal
                    if (group < 8)
                    {
                        return "(ILLEGAL)";
                    }
                    return "(private)";
                }
                if ((0xFFFE == group) && (0xE000 == element))
                {
                    return "Item";
                }
                string result = "(unknown)";
                // a check at the data dictionary
                if (this.ElementsByGroup.ContainsKey(group))
                {
                    // get the hash table of known elements
                    Hashtable myElements = (Hashtable)this.ElementsByGroup[group];
                    if (myElements.ContainsKey(element))
                    {
                        // get possible elements
                        ArrayList myList = (ArrayList)myElements[element];

                        // get the first one
                        result = ((DataDictionaryEntry)myList[0]).Name;
                    }
                }
                return result;
            }
        }

        #endregion
    }
}
