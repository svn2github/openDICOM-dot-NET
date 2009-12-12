/* Gobosh.DICOM
 * Gobosh.Dicom.XML reader and writer
 * 
 *  (C) 2006,2007 Timo Kaluza
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
using System.IO;
using System.Xml;
using System.Collections;
using Gobosh.DICOM;

namespace Gobosh
{    
    namespace DICOM
    {
        #region sealed XMLReader

        /// <summary>
        /// XMLReader is a class providing static functions to read
        /// the Gobosh.Dicom.XML format. All Read functions are returning
        /// a new Document object.
        /// </summary>
        public sealed class XMLReader
        {
            const string xmlns = "http://gobosh.net/dicom/stream/xml";

            public static Document ReadFromFile(string filename, string dataDictionary)
            {
                FileStream f = File.OpenRead(filename);
                Document result = ReadFromStream(f, dataDictionary);
                f.Close();
                return result;
            }

            public static Document ReadFromStream(Stream inStream,string dataDictionary)
            {
                //DataDictionary myDD = new DataDictionary(dataDictionary);
                StringEncoding currentEncoding = new ISO_8859_1Encoding();

                Document result = new Document();
                result.SetDataDictionary(dataDictionary);
                // DataDictionary myDD = result.GetDataDictionary();

                XmlDocument k = new XmlDocument();
                k.Load(inStream);

                XmlNode n = k.ChildNodes.Item(0);

                // parentMap creates a map Id->DataElement so we can add nodes to their parents
                Hashtable parentMap = new Hashtable(50);    // 50 is a rough estimate.

                //// connectMap creates a map parentID->DataElement, so we can add to their parent
                //// after reading all nodes to memory. It is a volatile structure to ensure that
                //// loaded elements may not neccessarily available during load of a child node.
                //Hashtable connectMap = new Hashtable(100);  // 100 is a rough estimate
                
                if (n.NamespaceURI == xmlns)
                {
                    int g, e, vr;
                    // read all subelements "dataelement"
                    foreach (XmlNode curnode in n.ChildNodes)
                    {
                        if (curnode.Name == "dataelement")
                        {
                            g = int.Parse(curnode.Attributes["group"].Value, System.Globalization.NumberStyles.HexNumber);
                            e = int.Parse(curnode.Attributes["element"].Value, System.Globalization.NumberStyles.HexNumber);
                            vr = int.Parse(curnode.Attributes["vr"].Value, System.Globalization.NumberStyles.HexNumber);
                            XmlNode lengthAttr = curnode.Attributes.GetNamedItem("length");
                            XmlNode parentID = curnode.Attributes.GetNamedItem("parent");
                            XmlNode nodeID = curnode.Attributes.GetNamedItem("id");

                            DataElement newDA = result.CreateDataElementByVR(vr, g, e, currentEncoding);
                            if (nodeID != null)
                            {
                                parentMap.Add(nodeID.Value, newDA);
                            }
                            if (parentID != null)
                            {
                                string myParent = parentID.Value;
                                if (parentMap.ContainsKey(myParent))
                                {
                                    ((DataElement)(parentMap[myParent])).Add(newDA);
                                }
                                else
                                {
                                    throw new Exception("Semantic error in node (" + newDA.GroupTag.ToString("x4") + ',' + newDA.ElementTag.ToString("x4") + "): missing parent " + myParent);
                                }
                            }
                            else
                            {
                                result.GetRootNode().Add(newDA);
                            }
                            XmlNode Values = curnode.FirstChild;
                            if (Values != null)
                            {
                                foreach (XmlNode Value in curnode.ChildNodes)
                                {
                                    if (Value.Name == "value")
                                    {
                                        // collect all properties
                                        // try to create a new value element
                                        // newDA.
                                        Hashtable h = new Hashtable(curnode.ChildNodes.Count);
                                        foreach (XmlNode prop in Value.ChildNodes)
                                        {
                                            XmlNode f = prop.FirstChild;
                                            // if no value exists, there is no text node.
                                            if (f != null)
                                            {
                                                // so add the value string if present
                                                h.Add(prop.Name, f.Value);
                                            }
                                            else
                                            {
                                                // add an empty string
                                                h.Add(prop.Name, "");
                                            }
                                        }
                                        newDA.AddValue(h);
                                    }
                                }
                            }
                            else
                            {
                                newDA.SetNoValues();
                            }
                            // it is important to set the ValueLength parameter AFTER ::SetNoValues,
                            // since ValueLength set to 0 by the SetNoValues(). The -1 defines a kind
                            // of Item Collection on (0x7FE0,0010) or SQ Data Elements.
                            if (lengthAttr != null)
                            {
                                if (lengthAttr.Value == "undefined")
                                {
                                    newDA.ValueLength = -1;
                                }
                            }
                        } // dataelement nodes
                    } // foreach
                } // if namespace okay
                else
                {
                    // catch this outside and try to find another XML interpretation
                    throw new Exception("Illegal XML notation or unknown namespace URI");
                }

                return result;

            }
        }

        #endregion

        #region sealed XMLWriter

        /// <summary>
        /// XMLReader is a class providing static functions to write
        /// the Gobosh.Dicom.XML format. 
        /// </summary>
        public class XMLWriter
        {
            const string xmlns = "http://gobosh.net/dicom/stream/xml";

            /// <summary>
            /// Writes a DICOM document to a file with a given filename
            /// </summary>
            /// <param name="filename">Name of the output file</param>
            /// <param name="rootNode">Reference to the root node of the DICOM document</param>
            public static void WriteToFile(string filename, DataElement rootNode)
            {
                FileStream s = System.IO.File.Create(filename);
                WriteToStream(s, rootNode);
                s.Close();
            }

            /// <summary>
            /// Writes a DICOM document to a stream
            /// </summary>
            /// <param name="outStream">An output stream</param>
            /// <param name="rootNode">Reference to the root node of the DICOM document</param>
            public static void WriteToStream(Stream outStream, DataElement rootNode)
            {
                if (rootNode is Gobosh.DICOM.DataElements.Root)
                {
                    XmlDocument z = new XmlDocument();

                    // current node
                    XmlNode n;
                    // get the root dataelement
                    DataElement d = (Gobosh.DICOM.DataElements.Root)(rootNode);

                    int id = 0; // no ids given yet
                    n = z.AppendChild(z.CreateElement("dicom", xmlns));
                    xmlAddAttribute(z, n, "datadictionary", d.GetDataDictionary().Name);
                    export(z, n, d, xmlns,ref id);

                    z.Save(outStream);
                }
                else
                {
                    throw new Exception("the given DataElement is not a root node!");
                }
            }

            #region no user servicable parts

            /// <summary>
            /// Adds an attribute to a given node, using a given document
            /// </summary>
            /// <param name="xmlDoc">the document the node belongs to</param>
            /// <param name="node">the node that gets a new attribute</param>
            /// <param name="name">the name of the new attribute</param>
            /// <param name="value">the value of the new attribute</param>
            private static void xmlAddAttribute(XmlDocument xmlDoc, XmlNode node, string name, string value)
            {
                XmlAttribute at = xmlDoc.CreateAttribute(name);
                at.Value = value;
                node.Attributes.Append(at);
            }

            /// <summary>
            /// Exports a dataElement recursivly.
            /// </summary>
            /// <remarks>This function calls itself recursivly for each DICOM recursion.</remarks>
            /// <param name="xmlDoc">the document the node belongs to</param>
            /// <param name="node">the node that gets the new XML nodes</param>
            /// <param name="dataElement">the data element to be exported</param>
            /// <param name="xmlns">the namespace of the Gobosh.DICOM.Xml document</param>
            private static void export(XmlDocument xmlDoc, XmlNode node, DataElement dataElement, string xmlns, ref int curid)
            {
                // remember the id of the root element
                int inID = curid;

                for (int i = 0; i < dataElement.Count; i++)
                {
                    DataElement da = dataElement.Item(i);
                    XmlNode n;
                    XmlNode val, prop;
                    DataElementValue daval;
                    VarProperty f;
                    int valcount;

                    n = node.AppendChild(xmlDoc.CreateElement("dataelement", xmlns));
                    xmlAddAttribute(xmlDoc, n, "group", da.GroupTag.ToString("x4"));
                    xmlAddAttribute(xmlDoc, n, "element", da.ElementTag.ToString("x4"));
                    xmlAddAttribute(xmlDoc, n, "vr", da.ValueRepresentation.ToString("x4"));
                    xmlAddAttribute(xmlDoc, n, "descr", da.GetTagDescription());
                    // the undefined length attribute shall be stored, since it can
                    // be necessary to distinguish things either it is undefined or it
                    // is defined length
                    if (da.IsUndefinedLength())
                    {
                        xmlAddAttribute(xmlDoc, n, "length", "undefined");
                    }
                    if (inID > 0)
                    {
                        xmlAddAttribute(xmlDoc, n, "parent", inID.ToString());
                    }

                   // write values
                    valcount = da.GetValueCount();
                    if (valcount > 0)
                    {
                        //vals = xmlDoc.CreateElement("values", xmlns);
                        //n.AppendChild(vals);
                        for (int j = 0; j < valcount; j++)
                        {
                            val = xmlDoc.CreateElement("value", xmlns);
                            n.AppendChild(val);
                            daval = (DataElementValue)da.GetValue(j);
                            foreach (string key in daval.Properties.Keys)
                            {
                                f = (VarProperty)(daval.Properties[key]);
                                prop = xmlDoc.CreateElement(f.Name, xmlns);
                                prop.AppendChild(xmlDoc.CreateTextNode(f.AsString()));
                                val.AppendChild(prop);
                            }
                        }
                    }
                    // start recursion if neccessary
                    if (da.Count > 0)
                    {
                        // write enter sequence
                        curid++;
                        xmlAddAttribute(xmlDoc, n, "id", curid.ToString());
                        export(xmlDoc, node, da, xmlns,ref curid);
                        // write leave sequence
                    }
                }
            }

            #endregion
        }
        #endregion
    }
}
