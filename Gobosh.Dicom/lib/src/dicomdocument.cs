/* Gobosh.DICOM
 * Document handling class
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
using System.IO;
using System.Xml;
using System.Globalization;
using System.Collections;
using Gobosh.DICOM;

namespace Gobosh
{
    namespace DICOM
    {
        /// <summary>
        /// DICOM.Document allows the management of any stream concerning
        /// the reading and writing of such a DICOM stream as well as
        /// transitions of Data Dictionaries or Transfer Syntaxes.
        /// 
        /// The Document holds a root element with all Data Elements
        /// in the Stream.
        /// 
        /// Usage:
        /// There are several ways to use it:
        /// 
        /// 1) Create the document directly from a existing stream, that
        ///    supports EOF. In this case the Document(stream*) constructor
        ///    shall be used.
        /// 2) Create the document from an partial stream (like TCP). In
        ///    this case the standard constructor shall be used and the
        ///    data shall be pushed by using the trio
        ///         PrepareToLoad()
        ///         PartialLoad()
        ///         FinishLoad()
        /// 3) Alternativly the document can be created and kept as a
        ///    reference all the time. In this case the LoadFromFile/SaveToFile
        ///    functions shall be used.
        /// 
        /// To export things to a file or a stream, the document can be
        /// configured and saved to a stream or file.
        /// 
        /// </summary>
        public class Document
        {
            #region Member variables

            /// <summary>
            /// The maximum size of the internal stream transferbuffer
            /// Adjust this to reduce the memory footprint of this object
            /// </summary>
#if true
            const long DocumentMaxTempMemory = 1 * 64 * 1024;       // 64kb only
#else
            const long DocumentMaxTempMemory = 64 * 1024 * 1024;
#endif

            /// <summary>
            /// The name of the current DataDictionary
            /// </summary>
            string ActiveDataDictionaryName;

            /// <summary>
            /// The current DataDictionary object
            /// </summary>
            DataDictionary ActiveDataDictionary;

            /// <summary>
            /// the parser class instance
            /// </summary>
            private Reader DicomStreamParser = null;

            /// <summary>
            /// true, when partial loading is active
            /// </summary>
            private bool IsPartialLoading = false;

            /// <summary>
            /// The Rootnode of the loaded document
            /// </summary>
            private DataElement RootNode = null;

            #endregion

            #region Document Constructors

            public Document()
            {
                RootNode = new DataElements.Root(null, 0, 0, 0, 0, null, null, true);
            }

            public Document(Stream inStream, string inDataDictionary)
            {
                RootNode = null;
                SetDataDictionary(inDataDictionary);
                LoadFromStream(inStream, inDataDictionary, true, false, true);
            }

            /// <summary>
            /// Constructs a document from a stream, inIsAFileSet allows
            /// to set the parser state
            /// </summary>
            /// <param name="inStream">a source stream</param>
            /// <param name="inIsAFileSet">flag true if stream is a file set</param>
            public Document(Stream inStream, string inDataDictionary, bool inIsAFileSet)
            {
                RootNode = null;
                SetDataDictionary(inDataDictionary);
                if (inIsAFileSet)
                {
                    // filesets start with preamble, explicit little endian transfersyntax
                    LoadFromStream(inStream, inDataDictionary, true, false, true);
                }
                else
                {
                    // other streams usually start with a group number and implicit little endian (standard)
                    LoadFromStream(inStream, inDataDictionary, false, true, true);
                }
            }

            #endregion

            #region Configuration methods 

            /// <summary>
            /// Returns the Transfer Syntax Flags for a given (0002,0010) Tag.
            /// The object must already exist in the object tree.
            /// </summary>
            /// <param name="isLittleEndian">reference to a bool variable that contains the endianess after the call</param>
            /// <param name="isImplicit">reference to a bool variable that contains the implicity after the call</param>
            public void GetTransferSyntaxFlags(out bool isImplicit, out bool isLittleEndian)
            {
                isLittleEndian = true;
                isImplicit = false;
                DataElement k = RootNode.GetChildByTag(DICOM.Consts.MetaGroupNumber, DICOM.Consts.MetaGroupTransferSyntax);
                if (k != null)
                {
                    string myTransferUID = k.GetValue().GetValueAsString();
                    if (myTransferUID == DICOM.Consts.ISOTransferSyntaxImplicitLittleEndian)
                    {
                        isLittleEndian = true;
                        isImplicit = true;
                    }
                    if (myTransferUID == DICOM.Consts.ISOTransferSyntaxExplicitLittleEndian)
                    {
                        isLittleEndian = true;
                        isImplicit = false;
                    }
                    if (myTransferUID == DICOM.Consts.ISOTransferSyntaxExplicitBigEndian)
                    {
                        isLittleEndian = false;
                        isImplicit = false;
                    }
                }
            }


            /// <summary>
            /// Changes the Data Dictionary if neccessary
            /// </summary>
            /// <param name="inDataDictionaryFilename"></param>
            public void SetDataDictionary(string inDataDictionaryFilename)
            {
                if (ActiveDataDictionaryName != inDataDictionaryFilename)
                {
                    ActiveDataDictionaryName = inDataDictionaryFilename;                    
                    if (RootNode != null)
                    {
                        ActiveDataDictionary = new DataDictionary(inDataDictionaryFilename);
                        RootNode.SetDataDictionary(ActiveDataDictionary);
                    }
                }
            }

            /// <summary>
            /// Returns the current Data Dictionary object
            /// </summary>
            /// <returns>Reference to the DataDictionary object</returns>
            /// <remarks>can return a null reference when no document is loaded!</remarks>
            public DataDictionary GetDataDictionary()
            {
                return this.ActiveDataDictionary;
            }

            /// <summary>
            /// Returns the root node of the loaded document
            /// </summary>
            /// <returns>Reference to the root node</returns>
            public DataElements.Root GetRootNode()
            {
                return (DataElements.Root) RootNode;
            }

            #endregion

            #region Loading Methods for DICOM

            /// <summary>
            /// Loads a Document from a file. The data dictionary name must be set before
            /// </summary>
            /// <param name="inFileName">The full path and filename of the dicom file</param>
            /// <exception cref="System.Exception">Thrown when the data dictionary is been set</exception>
            public void LoadFromFile(string inFileName)
            {
                if (ActiveDataDictionaryName != "")
                {
                    Stream myStream = File.OpenRead(inFileName);
                    LoadFromStream(myStream, ActiveDataDictionaryName, true, false, true);
                    myStream.Close();        // close and dispose the filestream
                    myStream = null;
                }
                else
                {
                    throw new Exception("Data Dictionary is not set");
                }
            }

            /// <summary>
            /// Reads inStream until EOF and
            /// creates a document from it. It uses a temporary buffer
            /// to load the file and pass it to the parser.
            /// </summary>
            /// <param name="inStream">The stream to read from</param>
            /// <param name="inDataDictionary">the name of the data dictionary to be used</param>
            /// <param name="inUsePreamble">true if preamble and DICM shall be searched</param>
            /// <param name="inExpectImplicit">true if implicit transfer syntax shall be expected</param>
            /// <param name="inExpectLittleEndian">true if little endian transfer syntax shall be expected</param>
            /// <exception cref="System.Exception">throws when stream is not readable or contains a DICOM syntax violation</exception>
            public void LoadFromStream(Stream inStream, string inDataDictionary, bool inUsePreamble, bool inExpectImplicit, bool inExpectLittleEndian)
            {
                if (inStream.CanRead)
                {
                    DicomStreamParser = new Reader();
                    // prepare the parser
                    DicomStreamParser.SetDataDictionary(inDataDictionary);
                    DicomStreamParser.setParserState(inUsePreamble, inExpectImplicit, inExpectLittleEndian);

                    // prepare the stream
                    long myBufferSize = inStream.Length;

                    // limit the temporary buffer
                    if (myBufferSize > DocumentMaxTempMemory)
                    {
                        myBufferSize = DocumentMaxTempMemory;
                    }
                    long myFileSize = inStream.Length;
                    long myBytesRead;

                    // create the temporary buffer
                    byte[] myBuffer = new byte[myBufferSize];

                    // System.Windows.Forms.MessageBox.Show("Total free: " + GC.GetTotalMemory(true).ToString() + " bytes");

                    // read until nothing can be read anymore
                    do
                    {
                        // transfer to the buffer
                        myBytesRead = inStream.Read(myBuffer, 0, (int)myBufferSize);
                        // pass the buffer to the parser
                        DicomStreamParser.parse(myBuffer, myBytesRead);
                    }
                    while (myBytesRead > 0);

                    // access the root node
                    RootNode = DicomStreamParser.GetRootNode();

                    // now check for accept state
                    if (!DicomStreamParser.Complete())
                    {
                        DicomStreamParser = null;
                        throw new Exception("Stream is not a compliant DICOM stream");
                    }

                    // release the parser, the root node stays alive since
                    // we referenced it
                    DicomStreamParser = null;
                }
                else
                {
                    throw new Exception("Stream is not readable");
                }
            }

            /// <summary>
            /// Prepares the partial loading and creates internal structures for calls to PartialLoad().
            /// The (clean) RootNode is available from this point on, so the programmer can access
            /// the partially loaded document stream, if neccessary
            /// </summary>
            /// <param name="inDataDictionary"></param>
            /// <param name="inUsePreamble"></param>
            /// <param name="inExpectImplicit"></param>
            /// <param name="inExpectLittleEndian"></param>
            public void PrepareToLoad(string inDataDictionary, bool inUsePreamble, bool inExpectImplicit, bool inExpectLittleEndian)
            {
                RootNode.Clear();
                IsPartialLoading = true;
                DicomStreamParser = new Reader();

                RootNode = DicomStreamParser.GetRootNode();
                // prepare the parser
                DicomStreamParser.SetDataDictionary(inDataDictionary);
                DicomStreamParser.setParserState(inUsePreamble, inExpectImplicit, inExpectLittleEndian);
            }

            /// <summary>
            /// Partially reads a byte[] buffer and passes it to the
            /// currently active DICOM binary parser. 
            /// </summary>
            /// <param name="buffer">reference to the buffer</param>
            /// <param name="size">the amount of valid data</param>
            /// <returns>true, if the document *could be* complete (see documentation)</returns>
            public bool PartialLoad(byte[] buffer, long size)
            {
                if (IsPartialLoading)
                {
                    DicomStreamParser.parse(buffer, size);
                    return DicomStreamParser.Complete();
                }
                else
                {
                    throw new Exception("Call PrepareToLoad() first.");
                }
            }

            /// <summary>
            /// Finishes the partial loading. This resets the internal states
            /// back and validates the RootNode object.
            /// </summary>
            public void FinishLoad()
            {
                if (IsPartialLoading)
                {
                    // reset the flag, this is the only function that shall do this
                    IsPartialLoading = false;

                    RootNode = DicomStreamParser.GetRootNode();
                    if (!DicomStreamParser.Complete())
                    {
                        DicomStreamParser = null;
                        throw new Exception("Stream is not a compliant DICOM stream");
                    }
                    DicomStreamParser = null;
                }
                else
                {
                    throw new Exception("Call PrepareToLoad() first.");
                }
            }

            #endregion

            #region Load Methods for XML

            #region Lb.Dicom XML variant
            /// <summary>
            /// Loads the document from an Lb.Dicom XML document. Since this XML
            /// variant does not contain any structural nodes all scanned nodes are
            /// attached to the rootnode except items, which get a SQ element in
            /// presequence.
            /// </summary>
            /// <param name="inDocument">reference to a XML document</param>
            public void LoadFromLbDicomXML(XmlDocument inDocument, string inCultureCode,string path)
            {
                RootNode = null;
                StringEncoding currentEncoding = new ISO_8859_1Encoding();
                RootNode = new DataElements.Root(ActiveDataDictionary, 0, 0, 0, 0, null, null, true);

                CultureInfo myCulture;
                myCulture = new CultureInfo(inCultureCode);

                foreach (XmlNode docnode in inDocument.ChildNodes)
                {
                    // document
                    if (docnode.Name == "DicomFile")
                    {
                        // iterate the nodes
                        foreach (XmlNode dataelement in docnode.ChildNodes)
                        {
                            // try to create an element from this xml node

                            DataElement newNode = CreateNodeByLbDicomXMLVR(dataelement, currentEncoding, myCulture,path);

                            // if a node was returned
                            if (newNode != null)
                            {
                                RootNode.Add(newNode);
                            }

                            //MessageBox.Show("gefunden: " + dataelement.Name, "element");
                        }
                    }
                }
            }

            private enum lbDecode { None, MultiValue, Value, Binary };

            /// <summary>
            /// Create a DataElement object by a given Lb.Dicom XML Node
            /// </summary>
            /// <param name="inDataelement">the XML node containing the Data Element</param>
            /// <param name="currentEncoding">the current encoding, usually ISO8859_1 since Lb.Dicom does not support anything else</param>
            /// <param name="myCulture">the culture the Lb.Dicom file was written in</param>
            /// <param name="path">the path to the extracted binary block files</param>
            /// <returns>a reference to a new DataElement block containing the values</returns>
            private DataElement CreateNodeByLbDicomXMLVR(XmlNode inDataelement, StringEncoding currentEncoding, CultureInfo myCulture, string path)
            {

                DataElement result = null;
                int Group, Element;
                XmlNode ValueNode = null;
                XmlNode MultiValueNode = null;
                Hashtable parameters;

                Group = 0; Element = 0;
                lbDecode UseGeneric = lbDecode.None;

                // read the group, element and locate the value/multivalue nodes
                foreach (XmlNode Childnode in inDataelement.ChildNodes)
                {
                    #region Read generic XML nodes (Group/Element...)
                    // TAG
                    if (Childnode.Name == "Tag")
                    {
                        foreach (XmlNode TagChild in Childnode.ChildNodes)
                        {
                            if (TagChild.Name == "Group") Group = int.Parse(TagChild.FirstChild.Value, NumberStyles.HexNumber);
                            if (TagChild.Name == "Element") Element = int.Parse(TagChild.FirstChild.Value, NumberStyles.HexNumber);
                        }
                    }

                    if (Childnode.Name == "Value")
                    {
                        ValueNode = Childnode;
                    }

                    if (Childnode.Name == "MultiValue")
                    {
                        MultiValueNode = Childnode;
                    }
                    #endregion
                }

                #region Create an DataElement object by given XML name
                switch (inDataelement.Name)
                {
                    // AgeString uses "Value"
                    // Value uses "Years", "Months", "Weeks", "Days" to encode the value
                    case "AgeString":
                        #region AgeString Decoding (Value/Years|Months|Weeks|Days)
                        string unit,number;
                        unit = "";
                        string nn; 
                        foreach (XmlNode asv in ValueNode.ChildNodes)
                        {
                            nn = asv.Name;
                            bool doSet = false;
                            if (nn == "Years")
                            {
                                unit = "Y";
                                doSet = true;
                            }
                            if (nn == "Months")
                            {
                                unit = "M";
                                doSet = true;
                            }
                            if (nn == "Weeks")
                            {
                                unit = "W";
                                doSet = true;
                            }
                            if (nn == "Days")
                            {
                                unit = "D";
                                doSet = true;
                            }
                            if ( doSet)
                            {
                                number = asv.FirstChild.Value;
                                result = new DataElements.AS(this.ActiveDataDictionary, Group, Element, ValueRepresentationConsts.AS, 0, null, currentEncoding,true);
                                result.AddValue(number.PadLeft(3,'0') + unit);
                                break;
                            }
                        }
                        #endregion
                        break;
                    case "ApplicationEntity":
                        #region ApplicationEntity Decoding (Multivalue/Value*)
                        result = new DataElements.AE(ActiveDataDictionary,Group,Element,ValueRepresentationConsts.AE,0,null,currentEncoding,true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "AttributeTag":
                        #region AttributeTag Decoding (Value/(Group+Element))
                        result = new DataElements.AT(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.AT, 0, null, currentEncoding, true);
                        parameters = new Hashtable();
                        foreach (XmlNode c in ValueNode)
                        {
                            if (c.Name == "Group" && c.HasChildNodes) parameters.Add("Group", c.FirstChild.Value);
                            if (c.Name == "Element" && c.HasChildNodes) parameters.Add("Element", c.FirstChild.Value);
                        }
                        result.AddValue(parameters);
                        parameters = null;
                        #endregion
                        break;
                    case "CodeString":
                        #region CodeString Decoding (Multivalue/Value*)
                        result = new DataElements.CS(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.CS, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "Date":
                        #region Date Decoding (MultiValue/Value/(Year+Month+Day))
                        result = new DataElements.DA(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.DA, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    parameters = new Hashtable(3);

                                    foreach ( XmlNode c in aev.ChildNodes)
                                    {
                                        if ( c.Name == "Year") parameters.Add("Year",c.FirstChild.Value);
                                        if ( c.Name == "Month") parameters.Add("Month",c.FirstChild.Value);
                                        if ( c.Name == "Day") parameters.Add("Day",c.FirstChild.Value);
                                    }
                                    result.AddValue(parameters);
                                }
                                else
                                {
                                    // Empty Strings are neccessary
                                    result.AddValue("");
                                }
                            }
                        }                        
                        #endregion
                        break;
                    case "DateTime":
                        #region DateTime Decoding (MultiValue/Value/(Year+Month+Day+Hour+Minutes+Seconds+Microseconds+UTC_Offset)
                        result = new DataElements.DT(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.DT, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    parameters = new Hashtable(8);

                                    foreach (XmlNode c in aev.ChildNodes)
                                    {
                                        // Translate the Lb.Dicom nodes to Gobosh.Dicom nodes
                                        if (c.Name == "Year") parameters.Add("Year", c.FirstChild.Value);
                                        if (c.Name == "Month") parameters.Add("Month", c.FirstChild.Value);
                                        if (c.Name == "Day") parameters.Add("Day", c.FirstChild.Value);
                                        if (c.Name == "Hour") parameters.Add("Hour", c.FirstChild.Value);
                                        if (c.Name == "Minutes") parameters.Add("Minute", c.FirstChild.Value);
                                        if (c.Name == "Seconds") parameters.Add("Second", c.FirstChild.Value);
                                        if (c.Name == "Microseconds") parameters.Add("Fractional", c.FirstChild.Value);
                                        if (c.Name == "UTC_Offset") parameters.Add("Timezone", c.FirstChild.Value);
                                    }
                                    result.AddValue(parameters);
                                }
                                else
                                {
                                    // Empty Strings are neccessary
                                    result.AddValue("");
                                }
                            }
                        }  
                        #endregion
                        break;
                    case "DecimalString":
                        #region DecimalString Decoding (MultiValue/Value*)
                        result = new DataElements.DS(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.DS, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    // translate numbers to invariant culture
                                    result.AddValue(double.Parse(aev.FirstChild.Value, myCulture).ToString(CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    result.AddValue("");
                                }
                            }
                        }
                        #endregion
                        break;
                    case "FloatingPointDouble":
                        #region FloatingPointDouble Decoding (MultiValue/Value*)
                        result = new DataElements.FD(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.FD, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    // translate numbers to invariant culture
                                    result.AddValue(double.Parse(aev.FirstChild.Value, myCulture).ToString(CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    result.AddValue("");
                                }
                            }
                        }
                        #endregion
                        break;
                    case "FloatingPointSingle":
                        #region FloatingPointSingle Decoding (MultiValue/Value*)
                        result = new DataElements.FL(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.FL, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    // translate numbers to invariant culture
                                    result.AddValue(float.Parse(aev.FirstChild.Value, myCulture).ToString(CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    result.AddValue("");
                                }
                            }
                        }
                        #endregion
                        break;
                    case "IntegerString":
                        #region IntegerString Decoding (MultiValue/Value*)
                        result = new DataElements.IS(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.IS, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    // translate numbers to invariant culture
                                    result.AddValue(int.Parse(aev.FirstChild.Value, myCulture).ToString(CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    result.AddValue("");
                                }
                            }
                        }
                        #endregion
                        break;
                    case "LongString":
                        #region LongString Decoding (MultiValue/Value*)
                        result = new DataElements.LO(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.LO, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "LongText":
                        #region LongText Decoding ( Value*)
                        result = new DataElements.LT(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.LT, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Value;
                        #endregion
                        break;
                    case "OtherByteString":
                        #region OtherByteString Decoding (FileName?)
                        result = new DataElements.OB(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.OB, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Binary;
                        #endregion
                        break;
                    case "OtherFloatString":
                        #region OtherFloatString Decoding (FileName?)
                        result = new DataElements.OF(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.OF, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Binary;
                        #endregion
                        break;
                    case "OtherWordString":
                        #region OtherWordString Decoding (FileName?)
                        result = new DataElements.OW(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.OW, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Binary;
                        #endregion
                        break;
                    case "PersonName":
                        #region Personname Decoding (Multivalue/Value/(FamilyName+GivenName+MiddleName+NamePrefix+NameSuffix)
                        result = new DataElements.PN(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.PN, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    parameters = new Hashtable(5);

                                    foreach (XmlNode c in aev.ChildNodes)
                                    {
                                        // Translate the Lb.Dicom nodes to Gobosh.Dicom nodes
                                        if (c.HasChildNodes)
                                        {
                                            if (c.Name == "FamilyName") parameters.Add("FamilyName", c.FirstChild.Value);
                                            if (c.Name == "GivenName") parameters.Add("GivenName", c.FirstChild.Value);
                                            if (c.Name == "MiddleName") parameters.Add("MiddleName", c.FirstChild.Value);
                                            if (c.Name == "NamePrefix") parameters.Add("NamePrefix", c.FirstChild.Value);
                                            if (c.Name == "NameSuffix") parameters.Add("NameSuffix", c.FirstChild.Value);
                                        }
                                    }
                                    result.AddValue(parameters);
                                }
                                else
                                {
                                    // Empty Strings are neccessary
                                    result.AddValue("");
                                }
                            }
                        }  
                        #endregion
                        break;
                    case "ShortString":
                        #region ShortString Decoding (MultiValue/Value*)
                        result = new DataElements.SH(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.SH, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "ShortText":
                        #region ShortText Decoding (MultiValue/Value*)
                        result = new DataElements.ST(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.ST, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Value;
                        #endregion
                        break;
                    case "SignedLong":
                        #region SignedLong Decoding (MultiValue/Value*)
                        result = new DataElements.SL(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.SL, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "SignedShort":
                        #region SignedShort Decoding (MultiValue/Value*)
                        result = new DataElements.SS(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.SS, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "Time":
                        #region Time Decoding (MultiValue/Value/(Hour+Minutes+Seconds+Microseconds)
                        result = new DataElements.TM(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.TM, 0, null, currentEncoding, true);
                        result.SetNoValues();
                        foreach (XmlNode aev in MultiValueNode)
                        {
                            if (aev.Name == "Value")
                            {
                                if (aev.HasChildNodes)
                                {
                                    parameters = new Hashtable(5);

                                    foreach (XmlNode c in aev.ChildNodes)
                                    {
                                        // Translate the Lb.Dicom nodes to Gobosh.Dicom nodes
                                        if (c.Name == "Hour") parameters.Add("Hours", c.FirstChild.Value);
                                        if (c.Name == "Minutes") parameters.Add("Minutes", c.FirstChild.Value);
                                        if (c.Name == "Seconds") parameters.Add("Seconds", c.FirstChild.Value);
                                        if (c.Name == "Microseconds") parameters.Add("Microseconds", c.FirstChild.Value);
                                    }
                                    result.AddValue(parameters);
                                }
                                else
                                {
                                    // Empty Strings are neccessary
                                    result.AddValue("");
                                }
                            }
                        }
                        #endregion
                        break;
                    case "UniqueID":
                        #region UniqueID Decoding (Value*)
                        result = new DataElements.UI(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.UI, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Value;
                        #endregion
                        break;
                    case "Unknown":
                        #region Unknown Decoding
                        result = new DataElements.UN(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.UN, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Binary;
                        #endregion
                        break;
                    case "UnlimitedText":
                        #region UnlimitedText Decoding (Value*)
                        result = new DataElements.UT(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.UT, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.Value;
                        #endregion
                        break;
                    case "UnsignedLong":
                        #region UnsignedLong Decoding (MultiValue/Value*)
                        result = new DataElements.UL(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.UL, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    case "UnsignedShort":
                        #region UnsignedShort Decoding (MultiValue/Value*)
                        result = new DataElements.US(ActiveDataDictionary, Group, Element, ValueRepresentationConsts.US, 0, null, currentEncoding, true);
                        UseGeneric = lbDecode.MultiValue;
                        #endregion
                        break;
                    default:
                        break;

                }
                #endregion

                #region Generic Methods to read an Lb.Dicom XML node and fill the DataElement object with values

                if (UseGeneric == lbDecode.Value)
                {
                    if (ValueNode.HasChildNodes)
                    {
                        result.SetValue(ValueNode.FirstChild.Value);
                    }
                    else
                    {
                        result.SetValue("");
                    }

                }
                if (UseGeneric == lbDecode.MultiValue)
                {
                    result.SetNoValues();
                    foreach (XmlNode aev in MultiValueNode)
                    {
                        if (aev.Name == "Value")
                        {
                            if (aev.HasChildNodes)
                            {
                                result.AddValue(aev.FirstChild.Value);
                            }
                            else
                            {
                                result.AddValue("");
                            }
                        }
                    }
                }
                if (UseGeneric == lbDecode.Binary)
                {
                    // FileName or Value
                    byte[] newBuffer = null;
                    int newLength = 0;

                    #region Load binary data from the file or external data
                    if (ValueNode != null)
                    {
                        if (ValueNode.HasChildNodes)
                        {
                            string k = ValueNode.FirstChild.Value;
                            int strLength = k.Length;
                            int decoded = 0;
                            newLength = (strLength + 1) / 3;
                            byte[] tmp = new byte[newLength];
                            // Read
                            bool haveFirst= false;
                            char f;
                            int i = 0;
                            string tmphex = "";
                            while (i < strLength)
                            {
                                f = k[i];
                                if (System.Uri.IsHexDigit(f))
                                {
                                    if (haveFirst)
                                    {
                                        tmphex += f;
                                        tmp[decoded++] = byte.Parse(tmphex, NumberStyles.HexNumber);
                                        haveFirst = false;
                                    }
                                    else
                                    {
                                        haveFirst = true;
                                        tmphex = ""+f;
                                    }
                                }
                                i++;
                            }

                            newBuffer = new byte[decoded];
                            Array.Copy(tmp, newBuffer, decoded);                            
                            result.setRawData(newBuffer, decoded);
                        }
                        else{
                            result.SetNoValues();
                        }
                    }
                    else
                    {
                        // find "FileName"
                        foreach (XmlNode c in inDataelement.ChildNodes)
                        {
                            if (c.Name == "FileName" && c.HasChildNodes)
                            {
                                string myFilename = path+"\\"+c.FirstChild.Value;
                                try
                                {
                                    
                                    FileStream f = File.OpenRead(myFilename);
                                    if (f != null)
                                    {
                                        newLength = (int)(f.Length);
                                        newBuffer = new byte[newLength];
                                        f.Read(newBuffer, 0, newLength);
                                        f.Close();
                                        result.setRawData(newBuffer, newLength);
                                    }
                                }
                                catch(Exception e)
                                {
                                    RootNode.Warnings.Add("File "+myFilename+" failed: "+e.Message);
                                    result.SetNoValues();
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                return result;
            }

            #endregion

            #endregion
            
            #region Write Methods for DICOM

            public void SaveToFile(string filename)
            {
                FileStream f = File.Create(filename);
                if (f != null )
                {
                    SaveDocument(f,true);
                }
                f.Close();
            }

            /// <summary>
            /// Saves a document (using the ExplicitVR/LittleEndian Transfersyntax)
            /// </summary>
            /// <param name="stream">reference to an output stream</param>
            /// <param name="usePreamble">true if the preamble shall be written (recommended for files)</param>
            public virtual void SaveDocument(Stream stream, bool usePreamble)
            {
                bool isImplicit, isLittleEndian;
                GetTransferSyntaxFlags(out isImplicit, out isLittleEndian);
                SaveDocument(stream, usePreamble, isImplicit, isLittleEndian);
            }

            public virtual void SaveDocument(Stream stream, bool usePreamble, bool useImplicit, bool useLittleEndian)
            {
                // TODO: remember where to change this
                PrepareDataElements(RootNode, useImplicit, useLittleEndian);

                Writer w = new Writer();
                w.SetInitialTransferSyntax(useLittleEndian, useImplicit);
                w.WriteToStream(RootNode, stream, usePreamble);
            }

            /// <summary>
            /// Sets the transfer syntax for the document. Creates or replaces the
            /// (0002,0010) Group Element, if present.
            /// (NOT IMPLEMENTED YET)
            /// </summary>
            /// <param name="dictionary">the Dictionary to use</param>
            /// <param name="useExplicit">Set true for Explicit Value Representation (Recommended)</param>
            /// <param name="useLittleEndian">Set true for little Endian encoding (Recommended)</param>
            public void SetTransferSyntax(DataDictionary dictionary, bool useExplicit, bool useLittleEndian)
            {
                
                this.ActiveDataDictionaryName = dictionary.Name;
                this.ActiveDataDictionary = dictionary;
                
                RootNode.SetDataDictionary(this.ActiveDataDictionary);
                // TODO: Fill with code to populate the transfer syntax                
            }

            /// <summary>
            /// Prepares the data elements for binary write. This includes
            /// the correction of any group lengths, any transfer syntax specific
            /// things as well as block hints like (0004,1400).
            /// </summary>
            /// <param name="rootElement"></param>
            /// <exception cref="">thrown when something is wrong</exception>
            private void PrepareDataElements(DataElement rootElement, bool isImplicit, bool isLittleEndian)
            {
                // Remove all 0008,0001/0000,0001 Data Elements (RET)
                // They can be removed, the calculation is not needed in
                // V3.0 of the DICOM standard
                RemoveRetiredObjects();

                // Populate Announced Transfer Syntax and DataDictionary
                // set the data dictionary to all nodes
                RootNode.SetDataDictionary(this.ActiveDataDictionary);

                // Bring the transfersyntax etc. down to the subnodes so
                // all lengths can be calculated correctly
                PopulateTransferSyntax(isImplicit, isLittleEndian);

                // Remove "not UL" group lengths
                // (1) remove wrong group lengths that are not UL and replace
                // them by an UL, so the length can be calculated later
                // Update all group lengths
                // (2) read all group length tags and items and let them
                // convert to raw, so a size estimation can be done. this
                // updates all group length/item etc. VRs/DataElements
                ModifyGroupLengthsToUL(rootElement, isImplicit);

                // TODO: Update all 0004,1400 Data Elements
                // (4) handle (0004,1400) DataElements
                // This is not implemented!
                // UpdateAll00041400Elements(RootNode);
            }

            // Remove known retired Data Elements we don't update
            private void RemoveRetiredObjects()
            {
                // Remove "Length To End" in the Messages
                RootNode.RemoveDataElements(0000, 0001);
                // Remove "Length To End" in the 0008 Group
                RootNode.RemoveDataElements(0008, 0001);
            }

            private void PopulateTransferSyntax(bool isImplicit, bool isLittleEndian)
            {
                // find the announced transfersyntax (if present)
                // default values: explicit, littleEndian

                // run through nodes and populate
                DataElement TransferSyntaxNode = RootNode.GetChildByTag(Consts.MetaGroupNumber, Consts.MetaGroupTransferSyntax);

                if (TransferSyntaxNode != null)
                {
                    string oldTransferSyntax = ((DataElementValue)(TransferSyntaxNode.GetValue())).GetValueAsString();

                    // TODO: Changing the (0002,0010) is not an easy task.
                    bool canChange = (oldTransferSyntax == Consts.ISOTransferSyntaxExplicitBigEndian || oldTransferSyntax == Consts.ISOTransferSyntaxExplicitLittleEndian || oldTransferSyntax == Consts.ISOTransferSyntaxImplicitLittleEndian);

                    if (canChange)
                    {
                        if (isImplicit)
                        {
                            if (isLittleEndian)
                            {
                                TransferSyntaxNode.SetValue(Consts.ISOTransferSyntaxImplicitLittleEndian);
                            }
                            else
                            {
                                throw new Exception("Transfersyntax Implicit BigEndian is not supported");
                            }
                        }
                        else
                        {
                            if (isLittleEndian)
                            {
                                TransferSyntaxNode.SetValue(Consts.ISOTransferSyntaxExplicitLittleEndian);
                            }
                            else
                            {
                                TransferSyntaxNode.SetValue(Consts.ISOTransferSyntaxExplicitBigEndian);
                            }
                        }
                    }
                    else
                    {
                        // otherwise we only allow Explicit LittleEndian encoding!
                        if (isImplicit || !isLittleEndian)
                        {
                            // See PS3.5-2006, Annex A4
                            throw new Exception("This Transfer Syntax can not be encoded!");
                        }
                    }
                }
                RootNode.SetEndianess(isLittleEndian);

            }

            private void ModifyGroupLengthsToUL(DataElement rootElement,bool isImplicit)
            {
                int i;                                          // iterate through element
                int currentGroup = -1;                          // the current group of elements (-1 to differ)
                int currentLength = 0;                          // the length of the current group

                DataElement currentObject;                      // ref to the current Object that gets scanned
                DataElement currentGroupObject = null;          // ref to the last Group Object (gggg,0000)

                // iterate through all elements
                for (i = 0; i < rootElement.Count; i++)
                {
                    currentObject = (rootElement.Item(i));

                    // (1) when the group changes, the Group Length and Group Offset objects
                    // must be updated
                    if (currentGroup != currentObject.GroupTag)
                    {
                        // update the last scanned GroupObject (currentGroupObject)
                        if (currentGroupObject != null)
                        {
                            if (currentGroupObject.ValueRepresentation != ValueRepresentationConsts.UL)
                            {
                                DataElement newGroup = CreateDataElementByVR(ValueRepresentationConsts.UL, currentGroup, 0, new ISO_8859_1Encoding());
                                rootElement.Swap(currentGroupObject, newGroup);
                            }
                            currentGroupObject.SetValue(currentLength.ToString());
                            currentGroupObject = null;
                            currentLength = 0;
                        }
                        currentGroup = currentObject.GroupTag;
                    }

                    // if it is a group length data element
                    if (currentObject.ElementTag == 0x0000)
                    {
                        // keep the reference for later update
                        currentGroupObject = currentObject;
                        // length is currently 0, the Group Length object 
                        // does not count into this
                        currentLength = 0;
                    }
                    else
                    {
                        // count the group length of all other objects
                        // it is intended not to count the length of the Group Offset object,
                        // this is done later, see (1)
                        if (currentGroupObject != null)
                        {
                            // add the object to the length
                            currentLength += currentObject.GetEncodedLength(isImplicit);
                        }

                    }

                    // recurse down
                    if (currentObject.Count > 0)
                    {
                        ModifyGroupLengthsToUL(currentObject, isImplicit);
                    }
                }
                // 

                // if there is a Group Data Element left over
                if (currentGroupObject != null)
                {
                    // update it
                    currentGroupObject.SetValue(currentLength.ToString());
                    currentGroupObject = null;
                }
            }

            #endregion

            #region Helper functions for tree management

            /// <summary>
            /// Create an appropriate DataElement object depending on the VR.
            /// </summary>
            /// <param name="valueRepresentation">The VR as int constant</param>
            /// <param name="group">The Group Tag</param>
            /// <param name="element">The Element Tag</param>
            /// <param name="encoding">The Encoding that shall be used</param>
            /// <returns>A reference to a new DataElement object</returns>
            public DataElement CreateDataElementByVR(int valueRepresentation,int group, int element, StringEncoding encoding)
            {
                DataElement result = null;
                DataDictionary dataDictionary = GetRootNode().GetDataDictionary();

                /* the following switch statement creates the appropiate type of an object
                 * according to a given VR. C# provides actually a very convenient way to 
                 * create types by a given type information via the Activator object.
                 * 
                 * Unfortunately it is not deeply implemented in the current .NET Compact Framework,
                 * therefore we use the more clumsy, but working way, even if it doesn't look
                 * very elegant.
                 */
                switch (valueRepresentation)
                {
                    case ValueRepresentationConsts.AE:
                        result = new DataElements.AE(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.AS:
                        result = new DataElements.AS(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.AT:
                        result = new DataElements.AT(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.CS:
                        result = new DataElements.CS(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.DA:
                        result = new DataElements.DA(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.DS:
                        result = new DataElements.DS(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.DT:
                        result = new DataElements.DT(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.FD:
                        result = new DataElements.FD(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.FL:
                        result = new DataElements.FL(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.IS:
                        result = new DataElements.IS(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.LO:
                        result = new DataElements.LO(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.LT:
                        result = new DataElements.LT(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.OB:
                        result = new DataElements.OB(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.OF:
                        result = new DataElements.OF(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.OW:
                        result = new DataElements.OW(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.PN:
                        result = new DataElements.PN(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.SH:
                        result = new DataElements.SH(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.SL:
                        result = new DataElements.SL(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.SQ:
                        result = new DataElements.SQ(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.SS:
                        result = new DataElements.SS(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.ST:
                        result = new DataElements.ST(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.TM:
                        result = new DataElements.TM(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.UI:
                        result = new DataElements.UI(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.UL:
                        result = new DataElements.UL(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.UN:
                        result = new DataElements.UN(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.US:
                        result = new DataElements.US(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    case ValueRepresentationConsts.UT:
                        result = new DataElements.US(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;
                    default:
                        result = new DataElements.Other(dataDictionary, group, element, valueRepresentation, 0, null, encoding, true);
                        break;

                }                
                return result;
            }

            #endregion
        }
    }
}