/* Gobosh.DICOM
 * Data Element base class. Works also as node class for the document tree.
 * All specific Data Element classes are derived from the class in here.
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
using System.Text;
using System.Collections;
using System.Globalization;

namespace Gobosh
{
	namespace DICOM
	{
        
		/// <summary>
		/// The external accessor for the elements
		/// of a DICOM stream as well as the base class for a
		/// specific DataElement. It contains as primary collection
		/// type a collection of sub elements (SQ DataElements i.e. 
        /// contain other DataElements).
		/// 
		/// Additionally each DataElement contains a collection of values
		/// in the Values member variable. Despite some Data Elements have
		/// a VM (Value Multiplicity) of 1, they share all the same 
		/// mechanism. Violations can be detected by checking against
		/// the DataDictionary which is referenced.
		/// </summary>
		public abstract class DataElement : System.Collections.CollectionBase
		{
			#region Declaration of Member Variables

			/// <summary>
			/// Flag true, if the DataElement has gone through Initialize()
			/// </summary>
			public bool IsInitialized = false;

			/// <summary>
			/// The group tag
			/// </summary>
			public int GroupTag;

			/// <summary>
			/// The Element tag
			/// </summary>
			public int ElementTag;

			/// <summary>
			/// The VR string as integer
			/// </summary>
			public int ValueRepresentation;

			/// <summary>
			/// The length of the RawBuffer (Value Length)
			/// </summary>
			public int ValueLength;

			// private DefaultCharacterSet;
			// TODO: change RawData to private later
			/// <summary>
			/// The reference to the raw data buffer
			/// </summary>
			public byte[] RawData = null;

			/// <summary>
			/// References to the parent element (needed for checks on Item Elements)
			/// </summary>
			protected DataElement Parent = null;

			/// <summary>
			/// References to the currently used Data Dictionary
			/// </summary>
			protected DataDictionary UsedDataDictionary;

			/// <summary>
			/// References to the used value encoding
			/// </summary>
			protected StringEncoding UsedEncoding;

			/// <summary>
			/// True, if RawBuffer be encoded little endian
			/// </summary>
			protected bool NodeIsLittleEndian;

			/// <summary>
			/// The values that are stored in the Data Element. Access is protected
			/// because the encoding state of the DataElement can interact with the
			/// contents or existence of the Values, therefore all access has to
			/// occur via member functions of the DataElement object.
			/// </summary>
			protected ArrayList Values = null;

			/// <summary>
			/// Any warnings that came up.
			/// </summary>
			public ArrayList Warnings = null;

			/// <summary>
			/// DataState presents the lifecycle of a DataElement.
			/// Since encoding/decoding can be expensive it is only
			/// done on request.
			/// </summary>
			protected enum DataState 
			{
				IsNotPresent = 0,   // no data present yet
				IsRawOnly,          // data is only present in the raw buffer
				IsRawAndDecoded,    // data is present as well in raw as in decoded state
				IsDecodedOnly,      // data is present in decoded state only
                IsIgnorable,        // data presentation does not need to be checked (SQ/ITEM)
			}

			/// <summary>
			/// The current life cycle state
			/// </summary>
			protected DataState DataValueState;

			#endregion

			#region DataElement Constructors and base stuff

			/// <summary>
			/// The standard DataElement constructor.
			/// </summary>
			public DataElement()
			{
				// standard encoding is ASCII
				UsedEncoding = null;
				DataValueState = DataState.IsNotPresent;
                ValueRepresentation = 0x3e3e;
			}

			/// <summary>
			/// The extended DataElement constructor. Already defines the group/element tag, the used valuerepresentation, etc.
			/// </summary>
			/// <param name="dataDictionary">A reference to the used data dictionary</param>
			/// <param name="groupTag">The group tag</param>
			/// <param name="elementTag">The element tag</param>
			/// <param name="valueRepresentation">The VR of the DataElement</param>
			/// <param name="valueLength">The Value Length</param>
			/// <param name="rawBuffer">byte[] Reference to the raw buffer</param>
			/// <param name="encodingUsed">The string encoding of the stream</param>
			/// <param name="isLittleEndian">raw buffer endianess</param>
			public DataElement(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
				: base()
			{
				Initialize(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed,isLittleEndian);
			}

			/// <summary>
			/// Assigns the given parameters to internal variables.
			/// </summary>
			/// <param name="dataDictionary">A reference to the used data dictionary</param>
			/// <param name="groupTag">The group tag</param>
			/// <param name="elementTag">The element tag</param>
			/// <param name="valueRepresentation">The VR of the DataElement</param>
			/// <param name="valueLength">The Value Length</param>
			/// <param name="rawBuffer">byte[] Reference to the raw buffer</param>
			/// <param name="encodingUsed">The string encoding of the stream</param>
			/// <param name="isLittleEndian">raw buffer endianess</param>
			private void Initialize(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
			{
				this.UsedDataDictionary = dataDictionary;
				this.GroupTag = groupTag;
				this.ElementTag = elementTag;
				this.ValueRepresentation = valueRepresentation;
				this.ValueLength = valueLength;
				this.RawData = rawBuffer;
				// internal state to isRawOnly IF any raw data is given
				if ( this.RawData != null)
				{
					DataValueState = DataState.IsRawOnly;
				}
				else
				{
					DataValueState = DataState.IsNotPresent;
				}
				this.IsInitialized = true;
				this.UsedEncoding = encodingUsed;
				this.NodeIsLittleEndian = isLittleEndian;
				//this.Values = new ArrayList();
			}

			#endregion

			#region DataElement Interface to Reader and Writer

            /// <summary>
            /// Adds a warning to the warnings list
            /// </summary>
			protected virtual void AddWarning(int level, string warningtext)
			{
				if ( Warnings == null )
				{
					Warnings = new ArrayList(1);
				}
				Warnings.Add(level.ToString()+" "+warningtext);
			}

			/// <summary>
			/// Purges the Raw Data if present.
			/// </summary>
			public virtual void PurgeRawData()
			{
				if ( DataValueState == DataState.IsRawOnly )
				{
					RawData = null;
					DataValueState = DataState.IsNotPresent;
				}

				if (  DataValueState == DataState.IsRawAndDecoded )
				{
					RawData = null;
					DataValueState = DataState.IsDecodedOnly;
				}
			}

			/// <summary>
			/// Purges all values if present.
			/// </summary>
			public virtual void PurgeValues()
			{
				if ( DataValueState == DataState.IsRawAndDecoded )
				{
					Values.Clear();
					Values = null;
					DataValueState = DataState.IsRawOnly;
				}

				if ( DataValueState == DataState.IsDecodedOnly )
				{
					Values.Clear();
					Values = null;
					DataValueState = DataState.IsNotPresent;
				}
			}

			/// <summary>
			/// Sets the raw Data buffer. If buffer is null, the data state
            /// is set to IsNotPresent. If you need to declare an Element with no
            /// Values, use the SetNoValues() function.
			/// </summary>
			/// <param name="buffer">A reference to a byte array</param>
			/// <param name="valuelength">the length of the value buffer</param>
			public virtual void setRawData(byte[] buffer, int valuelength)
			{				
				this.RawData = buffer;
				this.ValueLength = valuelength;
				if ( buffer == null)
				{
					DataValueState = DataState.IsNotPresent;
				}
				else
				{
					DataValueState = DataState.IsRawOnly;
				}
			}

            /// <summary>
            /// Exchanges the existent Data Element node with a new one.
            /// </summary>
            /// <param name="existentChild">the element to be replaced</param>
            /// <param name="newChild">the element that replaces</param>
            public void Swap(DataElement existentChild, DataElement newChild)
            {
                int k = List.IndexOf(existentChild);
                if (k < 0)
                {
                    List[k] = newChild; // existentChild will be garbage collected
                }
            }

            /// <summary>
            /// Get the Data Element child by group and element tag.
            /// Do not use this function to lookup Item Data Elements.
            /// </summary>
            /// <param name="group">The group tag of the Data Element to find</param>
            /// <param name="element">The element tag of the Data Element to find</param>
            /// <returns>A reference to the Data Element matching (group,element)</returns>
            /// <remarks>Shows undefined behaviour for Item DAs (like (fffe,e000), since there can be multiples</remarks>
            public DataElement GetChildByTag(int group, int element)
            {
                DataElement result = null;

                foreach (DataElement c in List)
                {
                    if (c.ElementTag == element)
                    {
                        if (c.GroupTag == group)
                        {
                            result = c;
                            break;
                        }
                    }
                }
                return result;
            }

            /// <summary>
            /// Get the Data Element child by group and element tag.
            /// Do not use this function to lookup Item Data Elements.
            /// </summary>
            /// <param name="group">The group tag of the Data Element to find</param>
            /// <param name="element">The element tag of the Data Element to find</param>
            /// <returns>A reference to the Data Element matching (group,element)</returns>
            /// <remarks>Shows undefined behaviour for Item DAs (like (fffe,e000), since there can be multiples</remarks>
            public int GetChildIndexByTag(int group, int element)
            {
                int result = -1;

                foreach (DataElement c in List)
                {
                    result++;
                    if (c.ElementTag == element)
                    {
                        if (c.GroupTag == group)
                        {
                            return result;
                        }
                    }
                }
                // not found
                return -1;
            }

			/// <summary>
			/// This is the internal function to prepare the raw data buffer
			/// if the state of the data element indicates that no raw buffer
			/// presentation of the value(s) is present, it calls the 
			/// function PrepareRawBuffer() which shall be overloaded by the
			/// descendants.
			/// </summary>
			/// <exception cref="System.Exception">Thrown when the index does not exist</exception>
			/// <exception cref="System.Exception">Thrown when the index does not exist</exception>
			protected void CallPrepareRawBuffer()
			{
				// if no data is present at all, we have to throw an exception
				if ( DataValueState == DataState.IsNotPresent )
				{
                    if ( ! ((ValueLength == 0) || (ValueLength == -1)) )
                    {
                        throw new Exception("The object has no values to encode, neither rawdata nor values are present.");
                    }
				}
				if ( DataValueState == DataState.IsDecodedOnly )
				{
					// perform conversion from Decoded to Raw
					PrepareRawBuffer();
					DataValueState = DataState.IsRawAndDecoded;
				}

				// TODO: Remove this check later. Since all Values
				// must be encoded in an even length, there must
				// be an error in the encoding routine, if not.
				if ( ( ValueLength % 1 ) != 0 )
				{
					throw new Exception("Internal error in PrepareRawBuffer: Value Length should always be EVEN!");
				}
			}

			void CallPrepareDecodedValues()
			{
				if ( DataValueState == DataState.IsNotPresent )
				{
					throw new Exception("The object has no values to encode, neither rawdata nor values are present.");
				}

				// if no values are decoded, decode them
				if ( DataValueState == DataState.IsRawOnly )
				{
					Decode( NodeIsLittleEndian );
					// now both representations exist
                    // TODO: check why this is commented out
                    // DataValueState = DataState.IsRawAndDecoded;
				}
			}

            /// <summary>
            /// Overloaded for raw buffer preparation. Must set ValueLength!
            /// </summary>
            /// <remarks>Must set ValueLength member variable.</remarks>
            protected virtual void PrepareRawBuffer()
			{
				/* prepare the RawData buffer if not present
				 * Should be overridden by descendend classes
				 */
			}

            /// <summary>
            /// Returns the length of the raw DataElement (override it in descendends).
            /// Will prepare the encoded raw buffer if not present!
            /// </summary>
            /// <returns>The length of the raw buffer</returns>
			public virtual int GetLength()
			{
                CallPrepareRawBuffer();
                if (RawData != null)
                {
                    return RawData.Length;
                }
                else
                {
                    return 0;
                }
			}

            /// <summary>
            /// Get the encoded Length of the DataElement by given VR syntax including the length of all sub elements in their payload, if present
            /// </summary>
            /// <remarks>Calling GetEncodedLength() can be expensive since it
            /// causes the values to be encoded to raw format to determine the
            /// length of the value payload.</remarks>
            /// <param name="isImplicit">true for implicit encoding</param>
            /// <returns>The number of bytes the completely encoded Element would take</returns>
            public virtual int GetEncodedLength(bool isImplicit)
            {
                int result;
                if (GroupTag == Consts.MetaGroupNumber)
                {
                    isImplicit = false;
                }
                result = GetHeaderLen(isImplicit);
                result += GetLength();
                result += GetEncodedLengthOfChildren(isImplicit);
                return result;
            }

            /// <summary>
            /// Get the encoded Length of all DataElement child nodes.
            /// </summary>
            /// <param name="isImplicit">true if implicit data encoding shall be used</param>
            /// <returns>the encoded length of all sub nodes</returns>
            protected int GetEncodedLengthOfChildren(bool isImplicit)
            {
                int result = 0;
                foreach (DataElement c in List)
                {
                    result += c.GetEncodedLength(isImplicit);
                }
                return result;
            }

            /// <summary>
            /// Calculates the length of the header bytes of this value element (without the payload)
            /// </summary>
            /// <returns>The number of bytes that this element encodes (excluding the payload)</returns>
            protected int GetHeaderLen(bool isImplicit)
            {
                int result;

                // Check for Item Tags, because they don't have any VR in any mode
                // It has also no Value Length, except 
                if (0xFFFE == GroupTag && (0xE000 == ElementTag || 0xE00D == ElementTag))
                {
                    /* Layout of Items
                     * (see PS3.5-2006, 7.5, Page 40 et seqq.)
                     * +--------+-----------+-----------+-------+
                     * | Group	| Element	| Value		| Value	|
                     * | Number	| Number	| Length	|		|
                     * |		|			|			|		|
                     * | UInt16	| UInt16	| UInt32	| ...	|
                     * | (1)	| (2)		| (3)		| (4)	|
                     * +--------+-----------+-----------+-------+
                     */

                    result = 8;
                }
                else
                {
                    /* Use the encoding rules defined in PS3.5-2006, 7.1.2, Page 36 et seqq.
                     * Note: Don't get confused: the code could also generate the combination 
                     * "implicit, big endian" which is not defined in the standard. Usually
                     * this won't happen since the encoding flags (isImplicit, isLittleEndian)
                     * are set according to the inital state or the given Transfer Syntax UID.
                     */
                    if (isImplicit)
                    {
                        /* Layout of Implicit VR
                         * (See PS3.5-2006, 7.1.2, Page 37)
                         * +--------+-----------+---------------+-----------------------+
                         * | Group	| Element	| Value Length	| Value					|
                         * | Number	| Number	|				| Even Number of Bytes	|
                         * |		|			|				|						|
                         * | UInt16	| UInt16	| UInt32		| Value Length			|
                         * | (1)	| (2)		| (3)			| (4)					|
                         * +--------+-----------+---------------+-----------------------+
                         */
                        result = 8;
                    }
                    else
                    {
                        /* The Explicit VR Transfer Syntax distinguishes between a 
                         * short (16 bit Value Length) and a long (32 bit Value Length)
                         * encoding. Usually one would check for  "! (VR in (SQ,UN,OB,OW,OF,UT))"
                         * but the standard defines there can be future unknown encodings
                         * which shall use the long variant then. Therefore we have to
                         * check for each VR that shall have the short encoding.
                         * (See PS3.5-2006, 7.1.1, Page 35, and 7.1.2, Page 36)
                         */
                        bool isShortVariant = (
                                (ValueRepresentationConsts.AE == ValueRepresentation)
                            || (ValueRepresentationConsts.AS == ValueRepresentation)
                            || (ValueRepresentationConsts.AT == ValueRepresentation)
                            || (ValueRepresentationConsts.CS == ValueRepresentation)
                            || (ValueRepresentationConsts.DA == ValueRepresentation)
                            || (ValueRepresentationConsts.DS == ValueRepresentation)
                            || (ValueRepresentationConsts.DT == ValueRepresentation)
                            || (ValueRepresentationConsts.FL == ValueRepresentation)
                            || (ValueRepresentationConsts.FD == ValueRepresentation)
                            || (ValueRepresentationConsts.IS == ValueRepresentation)
                            || (ValueRepresentationConsts.LO == ValueRepresentation)
                            || (ValueRepresentationConsts.LT == ValueRepresentation)
                            || (ValueRepresentationConsts.PN == ValueRepresentation)
                            || (ValueRepresentationConsts.SH == ValueRepresentation)
                            || (ValueRepresentationConsts.SL == ValueRepresentation)
                            || (ValueRepresentationConsts.SS == ValueRepresentation)
                            || (ValueRepresentationConsts.ST == ValueRepresentation)
                            || (ValueRepresentationConsts.TM == ValueRepresentation)
                            || (ValueRepresentationConsts.UI == ValueRepresentation)
                            || (ValueRepresentationConsts.UL == ValueRepresentation)
                            || (ValueRepresentationConsts.US == ValueRepresentation)
                            );

                        if (isShortVariant)
                        {
                            /* Layout of Explicit VR (short variant)
                             * (see PS3.5-2006, 7.1.2, Page 36 et seq)
                             * +--------+-----------+-----------+-----------+-----------+
                             * | Group	| Element	| VR		| Value		| Value		|
                             * | Number	| Number	| String	| Length	| ...		|
                             * |		|			|			|			|			|
                             * | UInt16	| UInt16	| UInt16	| UInt16	|			|
                             * | (1)	| (2)		| (3)		| (5)		| (6)		|
                             * | byte 0	| byte 2	| byte 4	| byte 6	| byte 8	|
                             * +--------+-----------+-----------+-----------+-----------+
                             * (yes, (4) is missing, no padding)
                             */
                            result = 8;
                        }
                        else
                        {
                            /* Layout of Explicit VR (long variant)
                             * (see PS3.5-2006, 7.1.2, Page 36 et seq)
                             * +--------+-----------+-----------+-----------+-----------+-----------+
                             * | Group	| Element	| VR		| Padding	| Value		| Value		|
                             * | Number	| Number	| String	| 0000		| Length	|			|
                             * |		|			|			|			|			|			|
                             * | UInt16	| UInt16	| UInt16	| UInt16	| UInt32	|			|
                             * | (1)	| (2)		| (3)		| (4)		| (5)		| (6)		|
                             * | byte 0	| byte 2	| byte 4	| byte 6	| byte 8	| byte 12	|
                             * +--------+-----------+-----------+-----------+-----------+-----------+
                             */
                            result = 12;
                        }
                    }
                }
                return result;
            }

            /// <summary>
            /// Sets the endianess of the current node and its child nodes.
            /// Converts all data necessary to be converted. If the Data Element
            /// is part of MetaData Group 0x0002, it is set to littleEndian despite
            /// the value of isLittleEndian.
            /// </summary>
            /// <param name="isLittleEndian">if the node shall be little endian</param>
            public void SetEndianess(bool isLittleEndian)
            {
                // metagroup is always little endian
                if (GroupTag == Consts.MetaGroupNumber)
                {
                    isLittleEndian = true;
                }

                /* if the endianess differs, the values need to
                 * be recoded. Therefore decoding is issued and
                 * the raw buffer is cleared. It will be recoded
                 * when necessary
                 */
                if (NodeIsLittleEndian != isLittleEndian)
                {
                    CallPrepareDecodedValues();
                    PurgeRawData();
                    NodeIsLittleEndian = isLittleEndian;
                }
                foreach (DataElement c in List)
                {
                    c.SetEndianess(isLittleEndian);
                }
            }

            /// <summary>
            /// Returns true when the ValueLength is -1 (= undefined length)
            /// </summary>
            /// <returns>true, when ValueLength = -1</returns>
            public bool IsUndefinedLength()
            {
                return (ValueLength == -1);
            }

			/// <summary>
			/// Creates a byte array and stores itself as DICOM DataElement
			/// using the Data Dictionary.
			/// The method is part of the export to stream mechanism.
			/// </summary>
            /// <param name="isLittleEndian">if the Data Element should be encoded in little endian</param>
            /// <param name="isImplicit">if the Data Element should use implicit VRs</param>
			/// <returns>a new byte[] array</returns>
			public virtual byte[] WriteToByteArray(bool isLittleEndian, bool isImplicit)
			{
				byte[] result;

                // Medadatagroup is definitely ExplicitVR,LittleEndian
                if (GroupTag == Consts.MetaGroupNumber)
                {
                    isLittleEndian = true;
                    isImplicit = false;
                }

                // check if the transfersyntax needs to be updated
                if (NodeIsLittleEndian != isLittleEndian)
                {
                    CallPrepareDecodedValues();
                    PurgeRawData();
                    NodeIsLittleEndian = isLittleEndian;
                }

				// check if Prepare Raw Buffer is neccessary
                // and calc the ValueLength (overridden in Items and Sequence)
				CallPrepareRawBuffer();

				long mySize = ValueLength;

				// Handle undefined (and items) same way as a zero byte value
				if ( mySize == -1 )
				{
					mySize = 0;
				}

				// Check for Item Tags, because they don't have any VR in any mode
				// It has also no Value Length, except 
				if ( 0xFFFE == GroupTag && ( 0xE000 == ElementTag || 0xE00D == ElementTag ) )
				{
					/* Layout of Items
					 * (see PS3.5-2006, 7.5, Page 40 et seqq.)
					 * +--------+-----------+-----------+-------+
					 * | Group	| Element	| Value		| Value	|
					 * | Number	| Number	| Length	|		|
					 * |		|			|			|		|
					 * | UInt16	| UInt16	| UInt32	| ...	|
					 * | (1)	| (2)		| (3)		| (4)	|
					 * +--------+-----------+-----------+-------+
					 */

                    /* the ValueLength of an Item with subnodes (like SQ Items) must 
                     * be computed here since the length depends on the Implicity 
                     * of the Transfer Syntax, which can not be determined whilst 
                     * the preparation of the raw buffers of the Data Elements, 
                     * since this information does not exist to that time, 
                     * except it is a data item (Count == 0)
                     */
                    if (ValueLength >= 0 && Count > 0)   // what ever the length is
                    {
                        ValueLength = GetEncodedLengthOfChildren(isImplicit);
                        mySize = 0;
                    }
					// If parent is a Pixel Data DataElement ...
					if ( 0x7FE0 == Parent.GroupTag && 0x0010 == Parent.ElementTag )
					{
						// ...the (FFFE,E00D) Item has Value Lengths
                        result = new byte[8 + mySize];
						Array.Copy(RawData,0,result,8,ValueLength);						// (4)
					}
					else
					{
						result = new byte[8];
					}
					Utils.WriteUInt16((UInt16) GroupTag,result,0,isLittleEndian);		// (1)
					Utils.WriteUInt16((UInt16) ElementTag,result,2,isLittleEndian);		// (2)
					Utils.WriteUInt32((UInt32) ValueLength,result,4,isLittleEndian);	// (3)
					
				}
				else
				{
                    if (ValueRepresentationConsts.SQ == ValueRepresentation)
                    {
                        // the ValueLength of an SQ with defined Length must be computed 
                        // here since the length depends on the Implicity of the Transfer Syntax,
                        // which can not be determined whilst the preparation of the
                        // raw buffers of the Data Elements, since this information does
                        // not exist to that time.
                        if (ValueLength >= 0)   // what ever the length is
                        {
                            // encode the Valuelength
                            ValueLength = GetEncodedLengthOfChildren(isImplicit);
                            // but do not aquire memory for the stream!
                            mySize = 0;
                        }
                    }
					/* Use the encoding rules defined in PS3.5-2006, 7.1.2, Page 36 et seqq.
					 * Note: Don't get confused: the code could also generate the combination 
					 * "implicit, big endian" which is not defined in the standard. Usually
					 * this won't happen since the encoding flags (isImplicit, isLittleEndian)
					 * are set according to the inital state or the given Transfer Syntax UID.
					 */
					if ( isImplicit )
					{
						/* Layout of Implicit VR
						 * (See PS3.5-2006, 7.1.2, Page 37)
						 * +--------+-----------+---------------+-----------------------+
						 * | Group	| Element	| Value Length	| Value					|
						 * | Number	| Number	|				| Even Number of Bytes	|
						 * |		|			|				|						|
						 * | UInt16	| UInt16	| UInt32		| Value Length			|
						 * | (1)	| (2)		| (3)			| (4)					|
						 * +--------+-----------+---------------+-----------------------+
						 */
                        result = new byte[8 + mySize];
						Utils.WriteUInt16((UInt16) GroupTag,result,0,isLittleEndian);		// (1)
						Utils.WriteUInt16((UInt16) ElementTag,result,2,isLittleEndian);		// (2)
						Utils.WriteUInt32((UInt32) ValueLength,result,4,isLittleEndian);	// (3)

						// now add the Raw Buffer, if present (SQ etc. not)
						if (RawData != null && mySize > 0)
						{
							Array.Copy(RawData,0,result,8,(int) mySize);							// (4)
						}
					}
					else
					{
						/* The Explicit VR Transfer Syntax distinguishes between a 
						 * short (16 bit Value Length) and a long (32 bit Value Length)
						 * encoding. Usually one would check for  "! (VR in (SQ,UN,OB,OW,OF,UT))"
						 * but the standard defines there can be future unknown encodings
						 * which shall use the long variant then. Therefore we have to
						 * check for each VR that shall have the short encoding.
						 * (See PS3.5-2006, 7.1.1, Page 35, and 7.1.2, Page 36)
						 */
						bool isShortVariant = (
								( ValueRepresentationConsts.AE == ValueRepresentation )
							||	( ValueRepresentationConsts.AS == ValueRepresentation )
							||	( ValueRepresentationConsts.AT == ValueRepresentation )
							||	( ValueRepresentationConsts.CS == ValueRepresentation )
							||	( ValueRepresentationConsts.DA == ValueRepresentation )
							||	( ValueRepresentationConsts.DS == ValueRepresentation )
							||	( ValueRepresentationConsts.DT == ValueRepresentation )
							||	( ValueRepresentationConsts.FL == ValueRepresentation )
							||	( ValueRepresentationConsts.FD == ValueRepresentation )
							||	( ValueRepresentationConsts.IS == ValueRepresentation )
							||	( ValueRepresentationConsts.LO == ValueRepresentation )
							||	( ValueRepresentationConsts.LT == ValueRepresentation )
							||	( ValueRepresentationConsts.PN == ValueRepresentation )
							||	( ValueRepresentationConsts.SH == ValueRepresentation )
							||	( ValueRepresentationConsts.SL == ValueRepresentation )
							||	( ValueRepresentationConsts.SS == ValueRepresentation )
							||	( ValueRepresentationConsts.ST == ValueRepresentation )
							||	( ValueRepresentationConsts.TM == ValueRepresentation )
							||	( ValueRepresentationConsts.UI == ValueRepresentation )
							||	( ValueRepresentationConsts.UL == ValueRepresentation )
							||	( ValueRepresentationConsts.US == ValueRepresentation )
							);

						int ValueLocation;				// the byte index to store the Value to
						const bool BigEndian = false;	// for readability (see (3))

						if ( isShortVariant )
						{
							/* Layout of Explicit VR (short variant)
							 * (see PS3.5-2006, 7.1.2, Page 36 et seq)
							 * +--------+-----------+-----------+-----------+-----------+
							 * | Group	| Element	| VR		| Value		| Value		|
							 * | Number	| Number	| String	| Length	| ...		|
							 * |		|			|			|			|			|
							 * | UInt16	| UInt16	| UInt16	| UInt16	|			|
							 * | (1)	| (2)		| (3)		| (5)		| (6)		|
							 * | byte 0	| byte 2	| byte 4	| byte 6	| byte 8	|
							 * +--------+-----------+-----------+-----------+-----------+
							 * (yes, (4) is missing, no padding)
							 */
							result = new byte[ 8 + mySize ];
							Utils.WriteUInt16((UInt16) ValueLength,result,6,isLittleEndian);	// (5)
							ValueLocation= 8;
						}
						else
						{
							/* Layout of Explicit VR (long variant)
							 * (see PS3.5-2006, 7.1.2, Page 36 et seq)
							 * +--------+-----------+-----------+-----------+-----------+-----------+
							 * | Group	| Element	| VR		| Padding	| Value		| Value		|
							 * | Number	| Number	| String	| 0000		| Length	|			|
							 * |		|			|			|			|			|			|
							 * | UInt16	| UInt16	| UInt16	| UInt16	| UInt32	|			|
							 * | (1)	| (2)		| (3)		| (4)		| (5)		| (6)		|
							 * | byte 0	| byte 2	| byte 4	| byte 6	| byte 8	| byte 12	|
							 * +--------+-----------+-----------+-----------+-----------+-----------+
							 */
							result = new byte[ 12 + mySize ];
							Utils.WriteUInt16(0,result,6,isLittleEndian);						// (4)
							Utils.WriteUInt32((UInt32) ValueLength,result,8,isLittleEndian);	// (5)
							ValueLocation = 12;
						}
						// The following elements are common to short and long encoding
						Utils.WriteUInt16((UInt16)GroupTag,result,0,isLittleEndian);			// (1)
						Utils.WriteUInt16((UInt16)ElementTag,result,2,isLittleEndian);			// (2)
						Utils.WriteUInt16((UInt16)ValueRepresentation,result,4,BigEndian);		// (3) ! Big Endian !

						/* Finally, copy the raw data to the new array
						 * This could be improved when all data would be written directly
						 * to the stream.
						 */
						if ( RawData != null && mySize > 0 )
						{
							Array.Copy(RawData,0,result,ValueLocation,(int) mySize);					// (6)
						}
					}
				}
				return result;
			}

			/// <summary>
			/// Decodes the RawData buffer using the given endianess.
            /// Overwrite this function to acchieve real decoding
			/// </summary>
            /// <example>public virtual void Decode(bool isLittleEndian)
            /// {
            ///   PurgeValues();
            ///   PrepareForAddValue();
            ///   ; *** decode *** and AddValue()
            ///   DataValueState = DataState.IsRawAndDecoded;
            /// }
            /// </example>
			/// <param name="isLittleEndian">flag if data is little endian</param>
			public virtual void Decode(bool isLittleEndian)
			{
				// TODO: emit a warning that an element was not parsed
				// TODO: mark this function as abstract later
				DataValueState = DataState.IsRawAndDecoded;
			}

			#endregion

			#region DataElement Interface to be overridden by descendants

			/// <summary>
			/// A overridable factory function to create the appropriate Value
			/// Element objects.
			/// </summary>
			/// <param name="Value">A string representing the value</param>
			/// <returns>an Value Element object</returns>
            /// <remarks>The function can return null, in this case, 
            /// no element could be created. Example is an IS DataElement
            /// with ValueLength of 0.</remarks>
			public virtual object CreateValueElement(string Value)
			{
                return Value; // new DataElementStringValue("Value", Value);
			}

            /// <summary>
            /// An overridable factory function to create the appropriate
            /// Value Element object used as Value in this DataElement.
            /// Use this function to create an default Value object and
            /// access the properties to change it.
            /// </summary>
            /// <returns>A default Value Element object</returns>
            public abstract object CreateValueElement();

            /// <summary>
            /// Returns a string that shows the content of the DataElement in
            /// an human readable manner.
            /// </summary>
            /// <returns></returns>
            public virtual string GetHumanReadableString()
            {
                StringBuilder result = new StringBuilder(ValueLength);

                int i, j;
                j = GetValueCount();	// read it only once, it is not that cheap
                for (i = 0; i < j; i++)
                {
                    object myValue = GetValue(i);
                    if (myValue is DataElementValue)
                    {
                        result.Append(((DataElementValue)myValue).GetValueAsString());
                    }
                    else
                    {
                        result.Append(GetValue(i));
                    }
                    result.Append('\\');
                }
                if (result.Length > 1)
                {
                    result.Remove(result.Length - 1, 1);
                }

                return result.ToString();
            }

			#endregion

			#region The Values interface to access the value list

			/// <summary>
			/// Sets a single Value (VM will be 1 afterwards)
			/// </summary>
			/// <param name="Value">A string representing the value</param>
			public virtual void SetValue(string Value)
			{
				PurgeValues();
				PurgeRawData();

				Values = new ArrayList();

				// calls the factory function for the element
				Values.Add( CreateValueElement(Value) );

				DataValueState = DataState.IsDecodedOnly;
			}

            /// <summary>
            /// Sets the Data Element object to RawAndDecoded data
            /// state to prevent false data states during interpretation.
            /// </summary>
            public virtual void SetNoValues()
            {
                if (DataValueState != DataState.IsIgnorable)
                {
                    PurgeRawData();
                    PurgeValues();
                    PrepareForAddValue();

                    RawData = null;
                    ValueLength = 0;

                    DataValueState = DataState.IsRawAndDecoded;
                }
            }

            /// <summary>
            /// Sets the Data Element to RawAndDecoded state
            /// by as well creating decoded values and rawdata.
            /// It does NOT just set the state flags. 
            /// 
            /// This function can be used to prepare the full
            /// working set to minimize unexpected performance loss
            /// during accessing value and or import/export functions
            /// 
            /// The function does also call all its child nodes,
            /// so a call to the root node creates a fully expanded
            /// node tree.
            /// </summary>
            public virtual void SetRawAndDecodedState()
            {
                if (DataValueState != DataState.IsIgnorable)
                {
                    if (DataValueState == DataState.IsNotPresent)
                    {
                        SetNoValues();
                    }
                    CallPrepareDecodedValues();
                    CallPrepareRawBuffer();
                    CallPrepareDecodedValues();
                }
                for (int i = 0; i < List.Count; i++)
                {
                    Item(i).SetRawAndDecodedState();
                }
            }

            /// <summary>
            /// Sets a single Value at a certain Index
            /// </summary>
            /// <param name="index">The index where the value should be stored</param>
            /// <param name="Value">A string representing the value</param>
            public virtual void SetValue(int index, string Value)
			{
				PurgeRawData();

				if ( Utils.CheckBounds(index,0,Values.Count-1) )
				{					
					Values[index] = CreateValueElement(Value);
				}
				else
				{
					throw new System.ArgumentOutOfRangeException("index","No value at index "+index);
				}
			}

            /// <summary>
            /// Prepares the addition of values or the translation. Does not change the DataValueState
            /// </summary>
            protected virtual void PrepareForAddValue()
            {
                if ((DataValueState == DataState.IsNotPresent) || (DataValueState == DataState.IsRawOnly))
                {
                    Values = new ArrayList(1);
                }
            }

			/// <summary>
			/// Adds a single object to the values without modifying the DataState. Use this during encoding/decoding.
			/// </summary>
			/// <param name="Value">A string representing the value</param>
			protected virtual void DoAddValue(string Value)
			{
                object z = CreateValueElement(Value);
                if (z != null)
                {
                    Values.Add(z);
                }
			}

			/// <summary>
			/// Adds a single value to the values list
			/// </summary>
			/// <param name="Value">A string representing the value</param>
			public virtual void AddValue(string Value)
			{
				PurgeRawData();
                PrepareForAddValue();
				DoAddValue(Value);

				DataValueState = DataState.IsDecodedOnly;
			}

            public virtual void AddValue(Hashtable inProperties)
            {
                PurgeRawData();
                PrepareForAddValue();

                DataElementValue v = (DataElementValue) CreateValueElement();
                foreach (string key in inProperties.Keys)
                {
                    string value = (inProperties[key]).ToString();
                    ((VarProperty)(v.Properties[key])).Set(value);
                }
                Values.Add(v);

                DataValueState = DataState.IsDecodedOnly;

            }

			/// <summary>
			/// Clears all values and also raw buffers, call it
			/// as preparation before calling AddValue
			/// </summary>
			public virtual void ClearValues()
			{
				PurgeRawData();
				PurgeValues();
			}

			/// <summary>
			/// Gets the only first value, usually this shouldn't be used!
			/// </summary>
			/// <returns>The object of the first value</returns>
			public virtual DataElementValue GetValue()
			{
				CallPrepareDecodedValues();
                return (DataElementValue)(Values[0]);
			}


			/// <summary>
			/// Gets the value from a given index.
			/// </summary>
			/// <param name="index">The index from where the value should be retrieved from</param>
			/// <returns>The object at the current place</returns>
            public virtual DataElementValue GetValue(int index)
			{
				CallPrepareDecodedValues();
				if ( Utils.CheckBounds(index,0,Values.Count-1) )
				{
                    return (DataElementValue)(Values[index]);
				}
				else
				{
					throw new System.ArgumentOutOfRangeException("index","no value at index "+index);
				}
			}

			/// <summary>
			/// Get the number of values. This function triggers decoding of the raw buffer if neccessary, therefore it is not a cheap function.
			/// </summary>
			/// <returns>The number of (decoded) values</returns>
			public virtual int GetValueCount()
			{
				CallPrepareDecodedValues();
                if (Values == null)
                {
                    return 0;
                }
                else
                {
                    return Values.Count;
                }
			}

			#endregion

			#region The virtual functions of the Collection Interface

			/// <summary>
			/// Adds a DataElement Object to the current List
			/// </summary>
			/// <param name="aDataElement">a reference to the new DataElement child object</param>
			public void Add(DataElement aDataElement)
			{
				aDataElement.Parent = this;
				List.Add(aDataElement);
			}



            /// <summary>
            /// Removes an item from the current List
            /// </summary>
            /// <param name="Element">The reference to an DataElement object</param>
            public virtual void Remove(DataElement Element)
            {
                List.Remove(Element);
            }
			/// <summary>
			/// Removes an items from the current LIst
			/// </summary>
			/// <param name="Index">the index at which an object should be removed</param>
			/// <exception cref="System.Exception">Thrown when the index does not exist</exception>
			public virtual void Remove(int Index)
			{
				// Check to see if there is an item at the supplied index.
				if (Index > Count - 1 || Index < 0)
					// If an item does not exist, the operation is cancelled by exception
				{
					throw new Exception("index overflow in DataElement");
				}
				else
				{
					List.RemoveAt(Index);
				}
			}

			/// <summary>
			/// Retrieves an Item at a certain index
			/// </summary>
			/// <param name="Index">the index from where the object should be returned</param>
			/// <returns></returns>
			public DataElement Item(int Index)
			{
				return (DataElement) List[Index];
			}

			#endregion

            /// <summary>
            /// Generates a human readable description of the DataElement including
            /// the tag, the vr as well as a size and the description from the data dictionary
            /// </summary>
			public string GetDescription()
			{
				System.Text.StringBuilder result = new System.Text.StringBuilder("(",1024);

				result.Append(this.GroupTag.ToString("X4"));
				result.Append(",");
				result.Append(this.ElementTag.ToString("X4"));
				result.Append(") [");
				result.Append((char) ((this.ValueRepresentation & 0xFF00) >> 8));
				result.Append((char) ((this.ValueRepresentation & 0xFF)));
				result.Append("] ");
                if (this.DataValueState != DataState.IsDecodedOnly)
                {
                    result.Append("(");
                    result.Append(this.ValueLength.ToString().PadLeft(5));
                    result.Append(") ");
                }
				result.Append(UsedDataDictionary.getValueDescription(this.GroupTag,this.ElementTag));

				return result.ToString();
			}
			
			public string GetValueRepresentationAsString()
			{
				StringBuilder result = new StringBuilder(4);
				result.Append((char) ((this.ValueRepresentation & 0xFF00) >> 8));
				result.Append((char) ((this.ValueRepresentation & 0xFF)));
				
				return result.ToString();
			}


            /// <summary>
            /// Get the currently used Data Dictionary
            /// </summary>
            /// <returns>a reference to a DataDictionary object</returns>
            public DataDictionary GetDataDictionary()
            {
                return UsedDataDictionary;
            }

            /// <summary>
            /// Gets the endianess of the node
            /// </summary>
            /// <returns>true, if the node is little endian</returns>
            public bool IsLittleEndian()
            {
                return NodeIsLittleEndian;
            }

            /// <summary>
            /// Populates the dictionary to be used to the node and all child nodes.
            /// </summary>
            /// <param name="dictionary">a reference to the used dictionary</param>
            public void SetDataDictionary(DataDictionary dictionary)
            {
                this.UsedDataDictionary = dictionary;
                foreach (DataElement c in List)
                {
                    c.SetDataDictionary(dictionary);
                }
            }

            /// <summary>
            /// Remove all DataElements that match (group,element).
            /// The function does work recursive into the subtree
            /// </summary>
            /// <param name="group">The group tag</param>
            /// <param name="element">The element tag</param>
            public void RemoveDataElements(int group, int element)
            {
                // TODO: Optimize this algorithm, currently it is enumerating the List twice
                // The Remove() function can not be used in the foreach() clause
                // since this breaks the iterators.
                int f = GetChildIndexByTag(group,element);
                if ( f >= 0 )
                {
                    Remove(f);
                }
                foreach (DataElement c in List)
                {                       
                    c.RemoveDataElements(group, element);
                }
            }
            
            /// <summary>
            /// Gets a human readable description for an element by using the current Data Dictionary
            /// </summary>
            /// <returns>human readable string</returns>
            public string GetTagDescription()
            {
                return UsedDataDictionary.getValueDescription(this.GroupTag, this.ElementTag);
            }

            #region TODO: REMOVE THE FUNCTIONS IN HERE


            /// <summary>
			/// Returns true if the element could be Image Data
			/// </summary>
			/// <returns>true, when Data Element could be Image Data</returns>
			public bool isImageData()
			{

				// (7FE0,0010) without 
				if ( GroupTag == 0x7FE0 && ElementTag == 0x0010 && this.Count == 0)
				{
                    CallPrepareRawBuffer();
                    return true;
				}

				if ( ( GroupTag == 0xFFFE && ElementTag == 0xE000 )&&
					(Parent.GroupTag == 0x7FE0 && Parent.ElementTag == 0x0010))
				{
                    CallPrepareRawBuffer();
					return true;
				}
				return false;
			}

			#endregion

            public DataElementValue Valueslist
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                }
            }

        }
	}
}
