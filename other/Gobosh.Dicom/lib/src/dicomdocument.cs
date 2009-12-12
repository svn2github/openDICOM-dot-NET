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