/* Gobosh.DICOM
 * Gobosh.Dicom.Reader to parse native DICOM 3.0 files.
 * It is based to the 2006 standard of DICOM.
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
using Gobosh;
using Gobosh.DICOM;

namespace Gobosh
{
	namespace DICOM
	{		
		#region ParserStackEntry Class
		sealed class ParserStackEntry
		{
			public int ItemLength;
			public bool TerminateByDelimitationItem;
			public bool IsImplicitVR;
			public bool IsLittleEndian;
			public int ExpectedSequenceLength;
			public UInt32 BytesReadInSequence;
			public bool IsSequence;
			public bool IsPixelData;
			public DataElement ParentNode;

			public ParserStackEntry(bool isSequence,bool isPixelData,int announcedLength,bool needDelimitationItem, int currentExpectedSequenceLength,UInt32 currentBytesReadInSequence, bool implicitvr, bool littleendian, DataElement parentNode )
			{
				// transfer the parameters to the member variables
				ParentNode = parentNode;
				IsSequence = isSequence;
				IsPixelData = isPixelData;
				ItemLength = announcedLength;
				TerminateByDelimitationItem = needDelimitationItem;
				IsImplicitVR = implicitvr;
				IsLittleEndian = littleendian;
				BytesReadInSequence = currentBytesReadInSequence;
				ExpectedSequenceLength = currentExpectedSequenceLength;
			}
		}
		#endregion

		#region Reader Class
		/// <summary>
		/// the dicom parser extracts the neccessary informations
		/// out of a DICOM stream to create a DICOM DataElement sequence..
		/// It is a multilevel parser that takes care of several syntactic and
		/// semantic rules regarding the DICOM standard.
		/// </summary>
		public sealed class Reader
		{
			public const int kUndefinedLength = -1;

			#region Reader State

			/// <summary>
			/// the size of the intermediate buffer to store unparsed bytes
			/// </summary>
			const int kStillReserveSize = 4096; // we can store 4096 bytes still reserve

			/// <summary>
			/// a const for the META groupnumber, used for readability
			/// </summary>
			const int kMetaGroupNumber = 2; 
			
			/// <summary>
			/// tParserState does reflect the state of the transfer syntax parser.
			/// </summary>
			enum SyntaxParserState 
			{
				kStateUndefined = 0,
				kStateError,					// parser is in unrecoverable error state
				kSkipPreamble,					// read 128 byte preamble
				kReadDICMSignature,				// read DICM signature
				// --------- Implicit Value Representation States
				kReadIVRGroupNumber,			// read Group Tag (2 Bytes)
				kReadIVRElementNumber,			// read Element Number (2 Bytes)
				kReadIVRFromDataDictionary,		// intermediate State: lookup the VR from the DataDictionary
				kReadIVR32BitValueLength,		// read 32bit Value Length (little Endian)
				kReadIVRValue,					// read the Value of VL size
				// --------- sequence (SQ) and item (FFFE,E000) handling (IVR)
				kPushIVRSequence,				// begin a new sequence
				kReadIVRItemLength,				// read Item Length
				kReadIVRDelimitationVL,			// read the Sequence Delimitation Item (VL = 0x00000000)
				kPopIVRSequence,				// end a sequence

				// --------- Explicit Value Representation States
				kReadEVRGroupNumber,			// read Group Tag (2 Bytes)
				kReadEVRElementNumber,			// read Element Number (2 Bytes)
				kReadEVRValueRepresentation,	// read the Value Representation
				kReadEVRTwoBytePadding,			// read 2 byte padding (if neccessary)
				kReadEVR32BitValueLength,		// read 32bit Value Length (big Endian)
				kReadEVR16BitValueLength,		// read 16bit Value Length (big Endian)
				kReadEVRValue,					// read the Value of VL size
				// --------- sequence (SQ) and item (FFFE,E000) handling (EVR)
				kPushEVRSequence,				// begin a new sequence
				kReadEVRItemLength,				// read Item Length
				kReadEVRDelimitationVL,			// read the Sequence Delimitation Item (VL = 0x00000000)
				kPopEVRSequence					// end a sequence
			};

			// these private members reflect the current stream state
			// concerning the syntax parser

			/// <summary>
			/// reflects the state of the parser
			/// </summary>
			private SyntaxParserState parserState;
			/// <summary>
			/// the number of bytes expected for the next complete element
			/// </summary>
			private long bytesExpected;

			/// <summary>
			/// the number of bytes are expected for the current sequence length
			/// </summary>
			private int expectedSequenceLength;

			private UInt32 bytesReadSequence;

			/// <summary>
			/// the current endianess flag
			/// </summary>
			private bool isStreamLittleEndian;

			/// <summary>
			/// the flag if the stream is Implicit VR or Explicit VR
			/// </summary>
			private bool isStreamImplicitVR;

			/// <summary>
			/// the current encoding according to (0008,0005)
			/// </summary>
			private StringEncoding CurrentEncoding;

			/// <summary>
			/// the announced encoding, gets valid after the tag found
			/// </summary>
			private StringEncoding AnnouncedEncoding;
			
			/// <summary>
			/// the current group number
			/// </summary>
			private int expectedGroup;

            // TODO: implement groupLengthNeeded for sanity checks
            ///// <summary>
            ///// indicates that the current group expects a group length DataElement
            ///// </summary>
            //private bool groupLengthNeeded;

			/// <summary>
			/// the number of bytes the expected for the current group
			/// </summary>
			private UInt32 expectedGroupLength;

            /// <summary>
            /// internal flag to show if a group is complete and the parser _could_ end.
            /// </summary>
            private bool groupCompleteFlag;

            /// <summary>
			/// the number of bytes being read from the current Data Element
			/// </summary>
			private UInt32 bytesReadDataElement;

			/// <summary>
			/// the number of bytes being read from the current group
			/// </summary>
			private UInt32 bytesReadGroupLength;

			/// <summary>
			/// stores the announced VR Transfersyntax, see (0002,0010)
			/// </summary>
			private bool nextGroupWillBeImplicitVR;

			/// <summary>
			/// stores the announced Endianess Transfersyntax, see (0002,0010)
			/// </summary>
			private bool nextGroupWillBeLittleEndian;

			// the over/underrun stream handling
			// if parts of the bytestream can not be read due insufficient number
			// of bytes the unparsed bytes get stored in the stillReserve buffer
			// until enough bytes come in to read the next element
			
			/// <summary>
			/// the buffer remembering unparsed bytes
			/// </summary>			
			private byte[] stillReserve = new byte[kStillReserveSize];
			/// <summary>
			/// how many bytes are in the stillReserve buffer
			/// </summary>
			private long stillReserveLevel;

			// -----------------------------------------------------
			// the following temporarily variables are used to collect
			// the properties of the currently read DataElement.

			/// <summary>
			/// group number of the current element
			/// </summary>
			private int dataElementGroupNumber;

			/// <summary>
			/// the element number of the current element
			/// </summary>
			private int dataElementElementNumber;

			/// <summary>
			/// the value representation 
			/// </summary>
			private byte[] dataElementValueRepresentation = new byte[2];

			/// <summary>
			/// the value representation as int ('UN' = 0x554E)
			/// the first byte is MSB, the second LSB of the lowest 16 bit
			/// of the int (32 bit)
			/// </summary>
			private int dataElementVRAsDWord;

			/// <summary>
			/// the value length
			/// </summary>
			private Int32 dataElementValueLength;

			/// <summary>
			/// the number of bytes alread read for VL
			/// </summary>
			private long dataElementValueRead;

			/// <summary>
			/// the current raw data value buffer of an Data Element
			/// the parser will create this buffer as soon as the Value Length
			/// is determined and fills it. the createElement function will
			/// create an Element and passes the reference to the buffer.
			/// Deletetition is in responsibility of the Data Element Object
			/// </summary>
			private byte[] dataElementValueBuffer;

			/// <summary>
			/// the reference to the selected data dictionary
			/// </summary>
			DataDictionary currentDictionary;			

			#endregion

			#region Reader Sequence Stack
			/// <summary>
			/// the Sequence Stack, used by nested DataSets / Items
			/// </summary>
			Stack SequenceStack;
			#endregion

			#region Reader Document Tree

			/// <summary>
			/// The Root Node Data Element for the current Reader Stream
			/// </summary>
			DataElement RootNode;

			/// <summary>
			/// The current node where new parsed elements are attached to
			/// </summary>
			DataElement CurrentNode;

			#endregion

			#region Syntax Reader 
			public Reader()
			{
				
				stillReserveLevel = 0;									// no unparsed bytes
				setSyntaxState(SyntaxParserState.kSkipPreamble,128);	// setting up stream state (expect 128 byte preamble)
				
				dataElementGroupNumber = -1;							// no current group
				dataElementElementNumber = -1;							// no current element
				
				expectedGroup = kMetaGroupNumber;						// expect the META group including a group length
//				groupLengthNeeded = true;								// which is required for group 0002
				//groupLengthRead = false;								// but not read yet (gggg,0000)
				expectedGroupLength = 0;

                groupCompleteFlag = false;

				isStreamImplicitVR = false;								// file streams are Explicit VR,
				isStreamLittleEndian = true;							// Big Endian (at least meta group 0002)

				nextGroupWillBeImplicitVR = isStreamImplicitVR;			// next group should be same transfer syntax, except announced otherwise
				nextGroupWillBeLittleEndian = isStreamLittleEndian;		// next group should be same endianess, except announced otherwise

				dataElementValueBuffer = null;							// no buffer yet.

				currentDictionary = null;								// no data dictionary loaded yet

				// create the stack
				SequenceStack = new Stack();							// the sequence nesting stack

				bytesReadSequence = 0;									// no bytes read in current sequence
				expectedSequenceLength = 0;								// no sequence size expected

				CurrentEncoding = new ISO_8859_1Encoding();             // ASCII (Using Superset ISO_R 100)

				// create the Root Node
				RootNode = new DataElements.Root(this.currentDictionary, -1,-1,0,0,null,CurrentEncoding,System.BitConverter.IsLittleEndian);
				CurrentNode = RootNode;

			}

            /// <summary>
            /// Sets the Syntax Parser State to appropriate options.
            /// If the usage of the preamble is indicated, the endianess and explicity is
            /// being set according to Part 10 (Filesets) otherwise the parser expects the
            /// DICOM stream to begin with a Group Tag.
            /// </summary>
            /// <param name="inUsePreamble">Should the parser expect a preamble and DICM signature</param>
            /// <param name="inExpectImplicit">Is the stream is implicit VR? (ignored if inUsePreamble is true)</param>
            /// <param name="inExpectLittleEndian">Is the stream little endian encoded? (ignored if inUsePreamble is true)</param>
            public void setParserState(bool inUsePreamble,bool inExpectImplicit, bool inExpectLittleEndian)
            {
                if (inUsePreamble)
                {
                    setSyntaxState(SyntaxParserState.kSkipPreamble, 128);
                    isStreamImplicitVR = inExpectImplicit;
                    isStreamLittleEndian = inExpectLittleEndian;
                }
                else
                {
                    if (inExpectImplicit)
                    {
                        setSyntaxState(SyntaxParserState.kReadIVRGroupNumber, 2);
                    }
                    else
                    {
                        setSyntaxState(SyntaxParserState.kReadEVRGroupNumber, 2);
                    }
                    isStreamImplicitVR = inExpectImplicit;
                    isStreamLittleEndian = inExpectLittleEndian;
                }
            }

			public void SetDataDictionary(string filename)
			{
                // if the filename is the same, don't reload the data dictionary
                if (filename.Length > 0 )
                {
                    AddWarning(0, "using Data Dictionary file: " + filename);
                    currentDictionary = new DataDictionary(filename);
                }
                RootNode.SetDataDictionary(currentDictionary);
			}

            public void SetDataDictionary(DataDictionary inDataDictionary)
            {
                currentDictionary = inDataDictionary;
                RootNode.SetDataDictionary(currentDictionary);
            }

			// private void appendBufferToStillReserve
			/// <summary>
			/// Parses buffer[] completely. If there are unparsed bytes left
			/// this function will keep it in our silentReserve unil the parser
			/// can be fed again with it.
			/// </summary>
			/// <param name="buffer">A reference to byte array</param>
			/// <param name="size">The total number of bytes in buffer[]</param>
			/// <returns>Nothing</returns>
			public void parse(byte[] buffer, long size)
			{
				if ( size == 0 )
				{
					return;
				}

				if ( currentDictionary == null)
				{
					throw new Exception("no datadictionary set");
				}
				// data comes in and if we have already data in the buffer
				// we have to concat the element 
				// THIS function handles the still reserve and guarantees
				// that parseSyntax() is called with enough bytes to
				// read at least one syntactical element

				long currentIndex = 0;

				// now we try to empty the still reserve as long as it is filled and
				// we have still bytes left in the current provided source buffer
				while ( ( this.stillReserveLevel > 0 ) && ( currentIndex < size ) )
				{
					// calculate how many bytes are expected from the inner parser
					int byteToFillUp = (int) Math.Min( this.bytesExpected-this.stillReserveLevel, size );

					if ( byteToFillUp <0 )
					{
						throw new Exception("there is an syntactical error, maybe wrong length");
					}

					// complete silent Reserve to 1 complete Element
					Array.Copy(
						buffer,0,									// source
						this.stillReserve,(int)this.stillReserveLevel,	// destination
						(int)byteToFillUp								// enough to complete
						);

					// move the stillReserveLevel forward
					currentIndex += byteToFillUp;
					stillReserveLevel += byteToFillUp;

					// now parse the (completed) element from the the start of 
					// the still reserve
					long bytesLeft = parse(stillReserve,0,stillReserveLevel);

					if ( bytesLeft > 0 )
					{
						// still ReserveLevel can be still > 0 if not all bytes were parsed
						if ( bytesLeft != stillReserveLevel)
						{
							// WARNING CONDITION!
							// Cast a warning

							// move what is left in the still reserve to the beginning
                            Array.Copy(stillReserve, (int) stillReserveLevel, stillReserve, 0, (int)stillReserveLevel);
						}
					}

					// the new reserve level was filled up to one elemenet and
					// should by completely read
					stillReserveLevel = bytesLeft;

				}
				// now the still reserve is completely done
				if ( currentIndex < size )
				{
					// now parse from the source buffer (so it is not needed to copy it)
					long bytesLeft = parse(buffer,currentIndex,size);

					if ( bytesLeft > 0 )
					{
						if ( stillReserveLevel+bytesLeft < kStillReserveSize )
						{
                            Array.Copy(buffer, (int) (size - bytesLeft), stillReserve, (int)stillReserveLevel, (int)bytesLeft);
							stillReserveLevel += bytesLeft;
						}
						else
						{
							// ERROR STATE
							// too much bytes unparsed, no space left in the stillReserveBuffer
							
							// TODO: this can be restructured to a error forgiving algorithm by
							// checking this in a while loop and increasing the stillReserve buffer
							// by the amount needed. Actually this should not be neccessary, since
							// the number of unparsed bytes should not be very large
							throw new Exception("element to large");
						}
					}
				} 
			}

			/// <summary>
			/// sets the state of the syntax parser
			/// </summary>
			/// <param name="newState">the new state of the statemachine</param>
			/// <param name="bytesExpectedForNextElement">the number of bytes that must be received for the next syntactical element</param>
			private void setSyntaxState(SyntaxParserState newState, long bytesExpectedForNextElement)
			{
				this.parserState = newState;
				this.bytesExpected = bytesExpectedForNextElement;
			}

			/// <summary>
			/// Parses buffer[] from index up to size-1
			/// </summary>
			/// <param name="buffer">A reference to byte array</param>
			/// <param name="index">The index from where to parse</param>
			/// <param name="size">The total number of bytes in buffer[]</param>
			/// <returns>returns the number of bytes that left unparsed</returns>
			public long parse(byte[] buffer, long index, long size)
			{
                // as long there is something to read, a group is not necessarily finished
                groupCompleteFlag = false;

				long result = 0;
				int myItemLength = 0;		// needed to check for the length of the Sequence Delimitation Item

				long bytesLeft = size-index;

				// we have at least a buffer of bytesExpected
				if ( bytesLeft < this.bytesExpected )
				{
					return bytesLeft;
				}
				
				// 
				// as long we have enough bytes in our buffer
				while ( bytesLeft >= bytesExpected )
				{
					// parse the next element
					//Console.Write("state: ");
					//Console.WriteLine(parserState);

					switch( this.parserState )
					{
						case SyntaxParserState.kSkipPreamble:
							// bytes expected: 128
							bytesLeft -= 128;	bytesReadDataElement+=128;
							index += 128;
							// behind the preamble we expect the DICM signature
							setSyntaxState(SyntaxParserState.kReadDICMSignature,4);
							break;
						case SyntaxParserState.kReadDICMSignature:
							// check for DCIM signature;
							if (   buffer[index] == 'D' 
								&& buffer[index+1] == 'I'
								&& buffer[index+2] == 'C'
								&& buffer[index+3] == 'M'
								)
							{
								bytesLeft -= 4;
								index += 4;
								bytesReadDataElement += 4;
								// depending on the endianess, the state differs now
								if ( isStreamImplicitVR )
								{
									setSyntaxState(SyntaxParserState.kReadIVRGroupNumber,2);
								}
								else
								{
									setSyntaxState(SyntaxParserState.kReadEVRGroupNumber,2);
								}
							}
							else
							{
								this.parserState = SyntaxParserState.kStateError;
								AddWarning(2,"DICM signature not found, not a file");
								throw new Exception("DICM signature not found");
							}
							break;

						case SyntaxParserState.kReadIVRGroupNumber:							
							// read group number little endian (16bit)
							int myLastGroup = dataElementGroupNumber;
							dataElementGroupNumber = Utils.ReadUInt16(buffer,index,isStreamLittleEndian);
							bytesReadDataElement = 2;

							index += 2;		bytesLeft -= 2;		//bytesReadDataElement += 2;

							if ( this.expectedGroup >= 0)
							{
								if ( dataElementGroupNumber != myLastGroup )
								{
									if ( this.expectedGroup != dataElementGroupNumber )
									{
										throw new Exception("unexpected group number");
									}
									this.expectedGroup = -1;
								}
							}

							setSyntaxState(SyntaxParserState.kReadIVRElementNumber,2);
							break;

						case SyntaxParserState.kReadEVRGroupNumber:
							myLastGroup = dataElementGroupNumber;
							dataElementGroupNumber = Utils.ReadUInt16(buffer,index,isStreamLittleEndian);

							bytesReadDataElement = 2;
							index += 2;		bytesLeft -= 2;		//bytesReadDataElement += 2;
							if ( this.expectedGroup >= 0)
							{
								if ( dataElementGroupNumber != myLastGroup )
								{
									if ( this.expectedGroup != dataElementGroupNumber )
									{
										AddWarning(2,"Unexpected Group Number");
										throw new Exception("unexpected group number");
									}
									this.expectedGroup = -1;
								}
							}

							setSyntaxState(SyntaxParserState.kReadEVRElementNumber,2);							
							break;

							// read the Element number (16 bit, unsigned)
						case SyntaxParserState.kReadIVRElementNumber:							
							dataElementElementNumber = Utils.ReadUInt16(buffer,index,isStreamLittleEndian);
							index += 2;
							bytesLeft -= 2;
							bytesReadDataElement += 2;
							// PS 3.5-2006, Page 42
							// check for delimitation items
							if ( isSequenceDelimitationItem() )
							{
								if ( kUndefinedLength != this.expectedSequenceLength )
								{
									AddWarning(8,"Warning: Sequence Delimitation Item detected, but size was announced");
									throw new Exception("unexpected Sequence Delimitation Item parsed");
								}
								// Console.WriteLine("Sequence delimitation item detected");
								setSyntaxState(SyntaxParserState.kReadIVRDelimitationVL,4);
							}
							else
							{
								if ( isItemDelimitationItem() )
								{
									if ( kUndefinedLength != this.expectedSequenceLength )
									{
										AddWarning(8,"Warning: Sequence Delimitation Item detected, but size was announced");
										throw new Exception("unexpected Sequence Delimitation Item parsed");
									}
									// Console.WriteLine("Item delimitation item detected");
									setSyntaxState(SyntaxParserState.kReadIVRDelimitationVL,4);
								}
								else
								{
									if ( isItemTag() )
									{
										if ( ((ParserStackEntry)SequenceStack.Peek()).IsPixelData )
										{
											setSyntaxState(SyntaxParserState.kReadIVR32BitValueLength,4);
										}
										else
										{
											setSyntaxState(SyntaxParserState.kReadIVRItemLength,4);
										}
									}
									else
									{
										setSyntaxState(SyntaxParserState.kReadIVRFromDataDictionary,2);
									}
								}
							}
							break;

							// read the Element number (16 bit, unsigned)
						case SyntaxParserState.kReadEVRElementNumber:
							dataElementElementNumber = Utils.ReadUInt16(buffer,index,isStreamLittleEndian);
							index += 2;
							bytesLeft -= 2;
							bytesReadDataElement += 2;
							// PS 3.5-2006, Page 42
							// check for delimitation items
							if ( isSequenceDelimitationItem() )
							{
								// Console.WriteLine("Sequence delimitation item detected");
								if ( kUndefinedLength != this.expectedSequenceLength )
								{
									AddWarning(8,"Warning: Sequence Delimitation Item detected, but size was announced");
									throw new Exception("unexpected Sequence Delimitation Item parsed");
								}
								setSyntaxState(SyntaxParserState.kReadEVRDelimitationVL,4);
							}
							else
							{
								if ( isItemDelimitationItem() )
								{
									// Console.WriteLine("Item delimitation item detected");
									if ( kUndefinedLength != this.expectedSequenceLength )
									{
										AddWarning(8,"Warning: Sequence Delimitation Item detected, but size was announced");
										throw new Exception("unexpected Sequence Delimitation Item parsed");
									}
									setSyntaxState(SyntaxParserState.kReadEVRDelimitationVL,4);
								}
								else
								{
									if ( isItemTag() )
									{
										if ( ((ParserStackEntry)SequenceStack.Peek()).IsPixelData )
										{
											setSyntaxState(SyntaxParserState.kReadEVR32BitValueLength,4);
										}
										else
										{
											setSyntaxState(SyntaxParserState.kReadEVRItemLength,4);
										}
									}
									else
									{
										setSyntaxState(SyntaxParserState.kReadEVRValueRepresentation,2);
									}
								}
							}
							break;

						case SyntaxParserState.kReadIVRFromDataDictionary:
							// get the current VR from the dictionary
							// (since we of course have to use 1 the 
							// getValueRepresentation function returns the FIRST
							// DataElement definition in the dictionary
							if ( this.currentDictionary != null )
							{
								this.dataElementValueRepresentation = this.currentDictionary.getValueRepresentation(dataElementGroupNumber,dataElementElementNumber);
								this.dataElementVRAsDWord = ( dataElementValueRepresentation[0] << 8) | dataElementValueRepresentation[1];
								// *see PS3.5 2006, page 37
								// 7.13 (Data Element Structure With Explicit VR)
								setSyntaxState(SyntaxParserState.kReadIVR32BitValueLength,4);
							}
							else
							{
								throw new Exception("no data dictionary set, but stream is implicit");
							}
							break;

						case SyntaxParserState.kReadIVRItemLength:
							dataElementValueLength = Utils.ReadInt32(buffer,index,isStreamLittleEndian);
							index += 4;
							bytesLeft -= 4;
							bytesReadDataElement += 4;
							dataElementVRAsDWord = 0;
							// dataElementValueRead = 0;
							setSyntaxState(SyntaxParserState.kPushIVRSequence,0);
							break;

						case SyntaxParserState.kReadEVRItemLength:
							dataElementValueLength = Utils.ReadInt32(buffer,index,isStreamLittleEndian);
							index += 4;
							bytesLeft -= 4;
							bytesReadDataElement += 4;
							dataElementVRAsDWord = 0;
							setSyntaxState(SyntaxParserState.kPushEVRSequence,0);
							break;

						case SyntaxParserState.kReadEVRValueRepresentation:
						
							byte b0,b1;
							b0 = buffer[index];
							b1 = buffer[index+1];
							dataElementVRAsDWord = ( b0 << 8 ) | b1;
							dataElementValueRepresentation[0] = b0;
							dataElementValueRepresentation[1] = b1;
							index += 2;
							bytesLeft -= 2;
							bytesReadDataElement += 2;

						switch(dataElementVRAsDWord)
						{
								// check if the VR is in { OB,OW,OF, SQ, UT,UN }
								// *see PS3.5 2006, page 36
								// 7.1.2 (Data Element Structure With Explicit VR)
								// Table 7.1-1 ("Data Element with explicit VR of OB,OW,OF,SQ,UT or UN")
							case ValueRepresentationConsts.OB:
							case ValueRepresentationConsts.OW:
							case ValueRepresentationConsts.OF:
							case ValueRepresentationConsts.SQ:
							case ValueRepresentationConsts.UT:
							case ValueRepresentationConsts.UN:
								// skip the 2 byte padding and read 32bit VL
								setSyntaxState(SyntaxParserState.kReadEVRTwoBytePadding,2);
								break;
								// all other VRs have 16bit value length (in Explicit VR)
								// Table 7.1-2
							case ValueRepresentationConsts.AE:
							case ValueRepresentationConsts.AS:
							case ValueRepresentationConsts.AT:
							case ValueRepresentationConsts.CS:
							case ValueRepresentationConsts.DA:
							case ValueRepresentationConsts.DS:
							case ValueRepresentationConsts.DT:
							case ValueRepresentationConsts.FD:
							case ValueRepresentationConsts.FL:
							case ValueRepresentationConsts.IS:
							case ValueRepresentationConsts.LO:
							case ValueRepresentationConsts.LT:
							case ValueRepresentationConsts.PN:
							case ValueRepresentationConsts.SH:
							case ValueRepresentationConsts.SL:
							case ValueRepresentationConsts.SS:
							case ValueRepresentationConsts.ST:
							case ValueRepresentationConsts.TM:
							case ValueRepresentationConsts.UL:
							case ValueRepresentationConsts.US:
							case ValueRepresentationConsts.UI:
								setSyntaxState(SyntaxParserState.kReadEVR16BitValueLength,2);
								break;
							default:
								// In case of an unknown VR, the 4byte VL encoding should be used
								// (see PS3.5-2006, 6.2:
								// "All new VRs defined in future versions of DICOM shall be of 
								// the same Data Element Structure as defined in Section 7.1.2 
								// (i.e. following the format for VRs such as OB,OW,SQ,UN)

								// skip the 2 byte padding and read 32bit VL
								setSyntaxState(SyntaxParserState.kReadEVRTwoBytePadding,2);
								// unknown !!

								// TODO: emit a warning through the (todo) event mechanism
								AddWarning(9,"Found an unknown Value Representation:"
									+dataElementValueRepresentation[0]+dataElementValueRepresentation[1]);
								break;
						}						
							break;
						

						case SyntaxParserState.kReadEVRTwoBytePadding:
							if ( Utils.ReadUInt16(buffer,index,isStreamLittleEndian) != 0 )
							{
								// error/warning: two byte padding is not zero!
								AddWarning(8,"Error in Stream: (ReadEVRTwoBytePadding) Bytes are not zero");
								// throw new Exception("error in stream: state: ReadEVRTwoBytePadding: bytes are not zero");
							}
							index += 2;
							bytesLeft -= 2;
							bytesReadDataElement += 2;
							setSyntaxState(SyntaxParserState.kReadEVR32BitValueLength,4);
							break;

						case SyntaxParserState.kReadEVR16BitValueLength:
							dataElementValueLength = Utils.ReadInt16(buffer,index,isStreamLittleEndian);
							index += 2;
							bytesLeft -= 2;
							bytesReadDataElement += 2;
							dataElementValueRead = 0;
							// allocate the buffer
							dataElementValueBuffer = new byte[dataElementValueLength];
							if ( dataElementValueLength < (kStillReserveSize / 2) )
							{
								setSyntaxState(SyntaxParserState.kReadEVRValue,dataElementValueLength);
							}
							else
							{
								setSyntaxState(SyntaxParserState.kReadEVRValue,1);
							}
							break;

						case SyntaxParserState.kReadIVR32BitValueLength:
							dataElementValueLength = Utils.ReadInt32(buffer,index,isStreamLittleEndian);
							index += 4;
							bytesLeft -= 4;
							bytesReadDataElement += 4;
							dataElementValueRead = 0;

							// PS 3.5-2006, 7.5, page 40
							// Nesting of Datasets
							// if the VR is 'SQ' we push an Sequence Entry up the Sequence Stack
							// instead of allocating space for a value element
							if ( ValueRepresentationConsts.SQ == dataElementVRAsDWord  )
							{								
								setSyntaxState(SyntaxParserState.kPushIVRSequence,0);
							}
							else
							{
								// (7FE0,0010) [OB] -> Sequencing Encoded Frames
								if ( (( ValueRepresentationConsts.OB == dataElementVRAsDWord  ) 
									|| (ValueRepresentationConsts.OW == dataElementVRAsDWord) )
									&& isPixelData()
									&& ( kUndefinedLength == dataElementValueLength )
									)
								{
									setSyntaxState(SyntaxParserState.kPushIVRSequence,0);
								}
								else
								{
									// allocate the buffer
									dataElementValueBuffer = new byte[dataElementValueLength];
									if ( dataElementValueLength < (kStillReserveSize / 2) )
									{
										setSyntaxState(SyntaxParserState.kReadIVRValue,dataElementValueLength);
									}
									else
									{
										setSyntaxState(SyntaxParserState.kReadIVRValue,1);
									}
								}
							}
							break;

						case SyntaxParserState.kReadEVR32BitValueLength:
							dataElementValueLength = Utils.ReadInt32(buffer,index,isStreamLittleEndian);
							index += 4;
							bytesLeft -= 4;
							bytesReadDataElement += 4;
							dataElementValueRead = 0;
							// allocate the buffer

							// SQ will be used with Items of Data Elements
							// PS 3.5-2006, Page XXX
							if ( ValueRepresentationConsts.SQ == dataElementVRAsDWord )
							{
								setSyntaxState(SyntaxParserState.kPushEVRSequence,0);
							}
							else
							{
								// (7FE0,0010) [OB] -> Sequencing Encoded Frames
								// PS 3.5-2006, Page 64
								//- The Length of the Data Element (7FE0,0010) shall be set to the Value for Undefined
								//  Length (FFFFFFFFH).
								//- Each Data Stream Fragment encoded according to the specific encoding process
								//  shall be encapsulated as a DICOM Item with a specific Data Element Tag of Value
								//  (FFFE,E000). The Item Tag is followed by a 4 byte Item Length field encoding the
								//  explicit number of bytes of the Item.
								if ( (( ValueRepresentationConsts.OB == dataElementVRAsDWord  ) 
									|| (ValueRepresentationConsts.OW == dataElementVRAsDWord) )
									&& isPixelData()
									&& ( kUndefinedLength == dataElementValueLength )
									)
								{
									setSyntaxState(SyntaxParserState.kPushEVRSequence,0);
								}
								else
								{
									dataElementValueBuffer = new byte[dataElementValueLength];
									if ( dataElementValueLength < (kStillReserveSize / 2) )
									{
										setSyntaxState(SyntaxParserState.kReadEVRValue,dataElementValueLength);
									}
									else
									{
										setSyntaxState(SyntaxParserState.kReadEVRValue,1);
									}
								}
							}
							break;

						case SyntaxParserState.kReadEVRValue:
						case SyntaxParserState.kReadIVRValue:
							// first check determines how much bytes should be read at maximum
							long myLength = dataElementValueLength - dataElementValueRead;
							// if there are not enough bytes left in the current buffer
							if ( myLength > bytesLeft )
							{
								myLength = bytesLeft;
							}
							// transfer from the buffer
							System.Buffer.BlockCopy( buffer,(int)index,dataElementValueBuffer,(int)dataElementValueRead,(int)myLength);
							// Array.Copy(	buffer,index,dataElementValueBuffer,dataElementValueRead,myLength);
							if ( myLength <= bytesLeft )
							{
								// complete
								dataElementValueRead += myLength;
								index += myLength;
								bytesReadDataElement += (UInt32)myLength;
								bytesLeft -= myLength;
							}
							else
							{
								dataElementValueRead += bytesLeft;
								index += bytesLeft;
								bytesReadDataElement += (UInt32) bytesLeft;
								bytesLeft = 0;
								
							}
							//Console.WriteLine("({0},{1})",dataElementGroupNumber,dataElementElementNumber);
							// is all data value read then start to create an element
							if ( dataElementValueRead >= dataElementValueLength )
							{
								// add the current DataElement size to the group size
								bytesReadGroupLength += bytesReadDataElement;
								bytesReadSequence += bytesReadDataElement;

								// create an Element (and assign a reference to the buffer)
								CreateDataElement(dataElementValueBuffer,0,dataElementValueLength);
								// delete our reference
								dataElementValueBuffer = null;

								if ( ( SequenceStack.Count > 0 ) && ( this.expectedSequenceLength >= 0) )
								{
									if ( this.bytesReadSequence >= this.expectedSequenceLength )
									{
										// sequence is ended by announced length
										if ( this.bytesReadSequence > this.expectedSequenceLength )
										{
											throw new Exception("Warning: more data elements read than bytes announced in sequence");
										}
										// if the next object on the stack is a sequence, just pop the sequence
										if ( ((ParserStackEntry)SequenceStack.Peek()).IsSequence )
										{
											popSequenceStack();
										}
										else
										{
											// pop the item from the stack
											popSequenceStack();
											// if the item/sequence was announced by value length
											if ( this.expectedSequenceLength>=0 )
											{
												// if it was the last item and the sequence is filled, pop the sequence object, too
												if ( this.bytesReadSequence >= this.expectedSequenceLength )
												{
													// sequence is ended by announced length
													if ( this.bytesReadSequence > this.expectedSequenceLength )
													{
														AddWarning(8,"Warning: more data elements read than bytes announced in sequence");
														throw new Exception("Warning: more data elements read than bytes announced in sequence");
													}
													popSequenceStack();
												}
											}
										}
									}
								}
								// since the implicit/explicit state could have changed
								// in ::CreateDataElement(), the next state depends on 
								// the current configuration.
								if ( this.isStreamImplicitVR )
								{
									setSyntaxState(SyntaxParserState.kReadIVRGroupNumber,2);
								}
								else
								{
									setSyntaxState(SyntaxParserState.kReadEVRGroupNumber,2);
								}
							}
							break;

						case SyntaxParserState.kPushIVRSequence:
							// set parser to read the first element of the Item Sequence
							setSyntaxState(SyntaxParserState.kReadIVRGroupNumber,2);
							// push the stack and set expectedSequenceLength
							pushSequenceStack();
							break;

						case SyntaxParserState.kPushEVRSequence:
							// set parser to read the first element of the Item Sequence
							setSyntaxState(SyntaxParserState.kReadEVRGroupNumber,2);
							// push the stack and set expectedSequenceLength
							pushSequenceStack();
							break;

						case SyntaxParserState.kReadIVRDelimitationVL:
							myItemLength = Utils.ReadInt32(buffer,index,isStreamLittleEndian);
							index += 4;
							bytesLeft -= 4;
							bytesReadDataElement += 4;
							if ( myItemLength != 0 )
							{
								// WARNING: this should be 0
								// see PS3.5-2006, Page 43 7.5-3
								AddWarning(8,"WARNING: Item Delimitation Item not zero length");
							}
							if ( isItemDelimitationItem() )
							{
                                // FFFE,E00D
								setSyntaxState(SyntaxParserState.kPopIVRSequence,0);
							}
							else
							{
                                // FFFE,E0DD
                                popSequenceStack();
								// next state is Continuation
								setSyntaxState(SyntaxParserState.kReadIVRGroupNumber,2);
                                
							}
							// end of the sequence							
							break;

						case SyntaxParserState.kPopIVRSequence:							
							popSequenceStack();
							setSyntaxState(SyntaxParserState.kReadIVRGroupNumber,2);
							break;

						case SyntaxParserState.kPopEVRSequence:
							popSequenceStack();
							setSyntaxState(SyntaxParserState.kReadEVRGroupNumber,2);
							break;

						case SyntaxParserState.kReadEVRDelimitationVL:
							myItemLength = Utils.ReadInt32(buffer,index,isStreamLittleEndian);
							index += 4;
							bytesLeft -= 4;
							bytesReadDataElement += 4;
							if ( myItemLength != 0 )
							{
								// WARNING: this should be 0
								// see PS3.5-2006, Page 43 7.5-3
								AddWarning(8,"WARNING: Item Delimitation Item not zero length");
							}
							if ( isItemDelimitationItem()  || isSequenceDelimitationItem() )
							{
								setSyntaxState(SyntaxParserState.kPopEVRSequence,0);
							}
							else
							{
								// next state is Continuation
								setSyntaxState(SyntaxParserState.kReadEVRGroupNumber,2);
							}
							// end of the sequence
							// popSequenceStack();
							break;
						default:
							throw new Exception("unhandled stream state!");
					}
				}
				result = bytesLeft; //  size - bytesLeft;
				return result;
			}

			/// <summary>
			/// creates a new Element on the Sequence stack
			/// </summary>
			private void pushSequenceStack()
			{
				if ( 0 == dataElementValueLength )
				{
					// ignore the sequence. It came with defined length 0, so 
					// there is no delimitation item
				}
				else
				{					
					// add the bytes read for the SQ item
					this.bytesReadSequence += bytesReadDataElement;
					// call create element to create the SQ item
					DataElement aNewElement = CreateDataElement(dataElementValueBuffer,0,dataElementValueLength);

					SequenceStack.Push(	new ParserStackEntry(
						!isItemTag(),
						isPixelData(),
						dataElementValueLength,
						(dataElementValueLength == kUndefinedLength),
						expectedSequenceLength,
						bytesReadSequence,
						isStreamImplicitVR,
						isStreamLittleEndian,
						CurrentNode));
					// 
					CurrentNode = aNewElement;
					// the new expected sequence length that is being announced
					this.expectedSequenceLength = dataElementValueLength;
					// no data read yet
					this.bytesReadSequence = 0;
					this.bytesReadDataElement = 0;
				}
			}

			/// <summary>
			/// removes the last Element on the Sequence stack
			/// </summary>
			private void popSequenceStack()
			{
				ParserStackEntry entry = (ParserStackEntry) SequenceStack.Pop();
				// restore the implicity/endianess 
				this.isStreamImplicitVR = entry.IsImplicitVR;
				this.isStreamLittleEndian = entry.IsLittleEndian;

				// restore the current Sequence Length Counter and expected Sequence
				this.expectedSequenceLength = entry.ExpectedSequenceLength;
				this.bytesReadSequence += entry.BytesReadInSequence;

				// leave the tree
				CurrentNode = entry.ParentNode;
			}

            /// <summary>
            /// Checks if a stream is an acceptance situation. Usually a DICOM stream
            /// shall end with an empty sequence stack and parser state shall be
            /// (explicit or implicit) reading a group number of the tag.
            /// The state of the parser will not be modified.
            /// </summary>
            /// <returns>Returns true if the parser is in acceptance situation</returns>
			public bool Complete()
			{
				bool result = false;

				// acceptance is reached when no element is on the stack
				result = ( ( SequenceStack.Count == 0 ) 
					&& ( ( SyntaxParserState.kReadIVRGroupNumber == parserState )
					    || (SyntaxParserState.kReadEVRGroupNumber == parserState ))
                    && ( ( groupCompleteFlag ) 
                        || ( expectedGroupLength == 0 ))
                );
				return result;
			}

			public DataElement GetRootNode()
			{
				return RootNode;
			}

			public void DumpDocumentToConsole() 
			{
				DumpDataElement(RootNode,0);
			}

			private void DumpDataElement(DataElement dataElement, int level)
			{
				foreach (DataElement elem in dataElement)
				{
					StringBuilder myLine = new StringBuilder("".PadLeft(level),1024);
					myLine.Append("(");
					myLine.Append(elem.GroupTag.ToString("X4"));
					myLine.Append(",");
					myLine.Append(elem.ElementTag.ToString("X4"));
					myLine.Append(") [");
					myLine.Append((char) ((elem.ValueRepresentation & 0xFF00) >> 8));
					myLine.Append((char) ((elem.ValueRepresentation & 0xFF)));
					myLine.Append("] (");
					myLine.Append(elem.ValueLength.ToString().PadLeft(5));
					myLine.Append(") ");
					myLine.Append(this.currentDictionary.getValueDescription(elem.GroupTag,elem.ElementTag));
					Console.WriteLine(myLine);
					if ( elem.Count > 0 )
					{
						DumpDataElement(elem,level+1);
					}
				}
			}

			#endregion

			#region Data Element Handling

			private DataElement CreateDataElement(byte[] buffer, long index, long size)
			{				
				// check if it is a group length element
				if ( dataElementElementNumber == 0 )
				{
					if ( this.expectedGroupLength == 0 )
					{
						if ( dataElementVRAsDWord == ValueRepresentationConsts.UL )
						{
							expectedGroupLength = Utils.ReadUInt32(buffer,index,isStreamLittleEndian);
							this.bytesReadGroupLength = 0;
						}
						else
						{
                            StringBuilder z = new StringBuilder(50);

                            z.Append("Group Length (");
                            z.Append(this.dataElementGroupNumber.ToString("x4"));
                            z.Append(',');
                            z.Append(this.dataElementElementNumber.ToString("x4"));
                            z.Append(") is not using UL as VR (see PS3.5-2006, 7.2, Page 37)");
							// throw new Exception("Group Length in wrong Value Representation");
							AddWarning(9,z.ToString());
                            // this might be something for "strict" mode

                            // overwrite the valuerepresentation if valuelength is 4, since
                            // then it is just encoded in a false manner (Siemens *sic*)
                            if (this.dataElementValueLength == 4)
                            {

                                dataElementVRAsDWord = ValueRepresentationConsts.UL;
                            }
						}
					}
					else
					{
						// ERROR STATE
						AddWarning(9,"Group Length redefined, not allowed (see PS3.5-2006, ch7)");
						throw new Exception("Group Length already defined");
					}
				}

				#region Check for transfer syntax

				// check if the transfer syntax gets announced
				// (0002,0010) Transfer Syntax UID UI 1
				// 1.2.840.10008.1.2		Implicit VR Little Endian: Default Transfer Syntax for DICOM
				// 1.2.840.10008.1.2.1		Explicit VR Little Endian
				// 1.2.840.10008.1.2.1.99	Deflated Explicit VR Little Endian
				// 1.2.840.10008.1.2.2		Explicit VR Big Endian
				if ( dataElementGroupNumber == Consts.MetaGroupNumber )
				{
                    // TODO: Remove Magic Numbers
					if ( dataElementElementNumber == Consts.MetaGroupTransferSyntax )
					{
						// ToString ist falsch!!
						if ( dataElementVRAsDWord != ValueRepresentationConsts.UI )
							//if (dataElementValueRepresentation[0] != 'U' && dataElementValueRepresentation[1] != 'I')
						{

							// WARNING STATE
							Console.WriteLine("\tWARNING: UID without UI VR");
						}
						
						String dataContent;
						
						// crop a tailing 0 byte
						if ( buffer [ size-1 ] == 0x00 )
						{
							size -= 1;
						}
						char[] myData = new char[size];
						Encoding.ASCII.GetChars(buffer,(int)index,(int)size,myData,0);
                        dataContent = new String(myData).Trim(); ;
						// string dataContent = 
						bool handled = false;

                        if (dataContent == Consts.ISOTransferSyntaxImplicitLittleEndian)
						{
							this.nextGroupWillBeImplicitVR = true;
							this.nextGroupWillBeLittleEndian = true;
							handled = true;
							AddWarning(0,"Encoding: Implicit Little Endian");
							Console.WriteLine(" Implicit Little Endian");
						}
						if ( dataContent == Consts.ISOTransferSyntaxExplicitLittleEndian )
						{
							this.nextGroupWillBeImplicitVR = false;
							this.nextGroupWillBeLittleEndian = true;
							Console.WriteLine(" Explicit Little Endian");
							AddWarning(0,"Encoding: Explicit Little Endian");
							handled = true;
						}
						if ( dataContent == "1.2.840.10008.1.2.1.99" )
						{
							this.nextGroupWillBeImplicitVR = false;
							this.nextGroupWillBeLittleEndian = true;
							handled = true;
							AddWarning(0,"Encoding: Deflate Explicit Little Endian");
							// TODO: implement deflate filter
							throw new Exception("Deflate Explicit VR is not supported yet");
						}
                        if (dataContent == Consts.ISOTransferSyntaxExplicitBigEndian)
						{
							this.nextGroupWillBeImplicitVR = false;
							this.nextGroupWillBeLittleEndian = false;
							AddWarning(0,"Encoding: Explicit Big Endian");
							Console.WriteLine(" Explicit Big Endian");
							handled = true;
						}
						if ( !handled )
						{
							// WARNING STATE!!
							Console.WriteLine("!! Transfer Syntax not supported: "+dataContent);
						}
					}
				}

				#endregion

                #region Creation of new DataElement object by fabric function

                // create the appropriate element
				DataElement aNewDataElement = CreateDataElementByValueRepresentation();
				// append it to the current object
				CurrentNode.Add(aNewDataElement);

				if ( aNewDataElement.Warnings != null )
				{
					foreach ( string k in aNewDataElement.Warnings )
					{
						AddWarning(9,k);
					}
                }

                #endregion

                #region Collect announced specific character sets

                // check for (0008,0005) CS (1-n): Specific Character Set
				if ( 0x0008 == dataElementGroupNumber && 0x0005 == dataElementElementNumber )
				{
					string announcedEncodingTag ;
					// TODO: Accept also Multiple Values, in this case we need Value 1
					string[] list = aNewDataElement.GetHumanReadableString().Split('\\');
					if ( list.Length > 0 )
					{
                        AddWarning(0, "(0008,0005) lists these Specific Character Sets:");
						foreach ( string k in list )
						{
							// attach to warning list
                            AddWarning(0, "  "+ k);
						}
						announcedEncodingTag = list[0];
					}
					else
					{
						announcedEncodingTag = "us-ascii";
					}
					
					AnnouncedEncoding = GetEncodingFromEncodingString( announcedEncodingTag);										
					CurrentEncoding = AnnouncedEncoding;
                }

                #endregion

                #region Check if group is complete by length ( Meta )
                // check if group has changed
				if ( this.expectedGroupLength > 0 )
				{
					if ( bytesReadGroupLength >= expectedGroupLength )
					{
						// group end reached
						// update the transfer syntax
						this.isStreamImplicitVR = this.nextGroupWillBeImplicitVR;
						this.isStreamLittleEndian = this.nextGroupWillBeLittleEndian;

						// resetting the group length
						this.expectedGroupLength = 0;
						this.bytesReadGroupLength = 0;

                        groupCompleteFlag = (bytesReadGroupLength == expectedGroupLength);

					}

                }

                #endregion

                return aNewDataElement;
			}
			
			/// <summary>
			/// Returns an Encoding Class Reference from System.Text.Encoding by translating
			/// the DICOM Character Set to the Registration Strings. The DICOM strings are
			/// defined in PS3.3-2006, Page 816, Table C.12-2
			/// 
			/// The code strings for the Encoding object can be found here:
			/// http://msdn2.microsoft.com/de-de/library/system.text.encoding.aspx
			/// </summary>
			/// <param name="announcedEncoding">DICOM string for encoding</param>
			/// <returns>An Encoding Class</returns>
			private StringEncoding GetEncodingFromEncodingString(string announcedEncoding)
			{
				string encodingString = "us-ascii";

				if ( announcedEncoding == "ISO_IR 100" )		encodingString = "iso-8859-1";	// Westeuropäisch (ISO)
				if ( announcedEncoding == "ISO_IR 101" )		encodingString = "iso-8859-2";	// Mitteleuropäisch (ISO)
				if ( announcedEncoding == "ISO_IR 109" )		encodingString = "iso-8859-3";	// Lateinisch 3 (ISO)
				if ( announcedEncoding == "ISO_IR 110" )		encodingString = "iso-8859-4";	// Baltisch (ISO)
				if ( announcedEncoding == "ISO_IR 144" )		encodingString = "iso-8859-5";	// Kyrillisch (ISO)
				if ( announcedEncoding == "ISO_IR 127" )		encodingString = "iso-8859-6";	// Arabisch (ISO)
				if ( announcedEncoding == "ISO_IR 126" )		encodingString = "iso-8859-7";	// Griechisch (ISO)
				if ( announcedEncoding == "ISO_IR 138" )		encodingString = "iso-8859-8";	// Hebrew (ISO-Visual)
				if ( announcedEncoding == "ISO_IR 148" )		encodingString = "iso-8859-9";	// Türkisch (ISO) (Latin 5)
				if ( announcedEncoding == "ISO_IR 13"  )		encodingString = "iso-2022-jp";	// IBM EBCDIC (Japanisch Katakana)
				if ( announcedEncoding == "ISO_IR 166" )		encodingString = "x-cp20949";	// IBM EBCDIC (Thailändisch)
				if ( announcedEncoding == "ISO 2022 IR 100" )	encodingString = "iso-8859-1";	// Westeuropäisch (ISO)
				if ( announcedEncoding == "ISO 2022 IR 101" )	encodingString = "iso-8859-2";	// Mitteleuropäisch (ISO)
				if ( announcedEncoding == "ISO 2022 IR 109" )	encodingString = "iso-8859-3";	// Lateinisch 3 (ISO)
				if ( announcedEncoding == "ISO 2022 IR 110" )	encodingString = "iso-8859-4";	// Baltisch (ISO)
				if ( announcedEncoding == "ISO 2022 IR 144" )	encodingString = "iso-8859-5";	// Kyrillisch (ISO)
				if ( announcedEncoding == "ISO 2022 IR 127" )	encodingString = "iso-8859-6";	// Arabisch (ISO)
				if ( announcedEncoding == "ISO 2022 IR 126" )	encodingString = "iso-8859-7";	// Griechisch (ISO)
				if ( announcedEncoding == "ISO 2022 IR 138" )	encodingString = "iso-8859-8";	// Hebrew (ISO-Visual)
				if ( announcedEncoding == "ISO 2022 IR 148" )	encodingString = "iso-8859-9";	// Türkisch (ISO) (Latin 5)
				if ( announcedEncoding == "ISO 2022 IR 13"  )	encodingString = "iso-2022-jp";		// IBM EBCDIC (Japanisch Katakana)
				if ( announcedEncoding == "ISO 2022 IR 166" )	encodingString = "IBM-Thai";	// IBM EBCDIC (Thailändisch)
				if ( announcedEncoding == "ISO 2022 IR 87"  )	encodingString = "unsupp";		// JIS X 0208 Kanji
				if ( announcedEncoding == "ISO 2022 IR 159"  )	encodingString = "unsupp";		// JIS X 0212 Supplementary Kanji set
				if ( announcedEncoding == "ISO 2022 IR 149"  )	encodingString = "unsupp";		// KS X 1001: Hangul and Hanja

				// unicode
				if ( announcedEncoding == "ISO_IR 192" )		encodingString = "utf-8";		// UTF 8 Unicode
				if ( announcedEncoding == "GB18030" )			encodingString = "GB18030";		// Chinesisch, Vereinfacht

				if ( "us-ascii" == encodingString )
				{
					// TODO: emit warning of "not supported"
					AddWarning(8,"Unsupported Text Encoding: "+encodingString);
				}

				return StringEncoding.GetStringEncoding(encodingString);
			}

			private DataElement CreateDataElementByValueRepresentation()
			{
				DataElement aNewDataElement = null;

				switch ( dataElementVRAsDWord )
				{
					case ValueRepresentationConsts.AE:
						aNewDataElement = new DataElements.AE(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.AS:
						aNewDataElement = new DataElements.AS(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.AT:
						aNewDataElement = new DataElements.AT(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.CS:
						aNewDataElement = new DataElements.CS(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.DA:
						aNewDataElement = new DataElements.DA(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.DS:
						aNewDataElement = new DataElements.DS(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.DT:
						aNewDataElement = new DataElements.DT(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.FL:
						aNewDataElement = new DataElements.FL(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.FD:
						aNewDataElement = new DataElements.FD(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.IS:
						aNewDataElement = new DataElements.IS(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.LO:
						aNewDataElement = new DataElements.LO(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.LT:
						aNewDataElement = new DataElements.LT(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.OB:
						aNewDataElement = new DataElements.OB(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.OF:
						aNewDataElement = new DataElements.OF(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.OW:
						aNewDataElement = new DataElements.OW(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.PN:
						aNewDataElement = new DataElements.PN(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.SH:
						aNewDataElement = new DataElements.SH(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.SL:
						aNewDataElement = new DataElements.SL(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.SQ:
						aNewDataElement = new DataElements.SQ(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.SS:
						aNewDataElement = new DataElements.SS(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.ST:
						aNewDataElement = new DataElements.ST(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.TM:
						aNewDataElement = new DataElements.TM(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.UI:
						aNewDataElement = new DataElements.UI(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.UL:
						aNewDataElement = new DataElements.UL(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.UN:
						aNewDataElement = new DataElements.UN(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.US:
						aNewDataElement = new DataElements.US(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					case ValueRepresentationConsts.UT:
						aNewDataElement = new DataElements.UT(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
					default:
                        // TODO: distinguish between DAs with unknown VRs and ITEMS (that simply have no VR)
						aNewDataElement = new DataElements.Other(currentDictionary, dataElementGroupNumber,dataElementElementNumber,dataElementVRAsDWord,dataElementValueLength,dataElementValueBuffer,CurrentEncoding,isStreamLittleEndian);
						break;
				}
				// calling the Decode function
				// aNewDataElement.Decode(isStreamLittleEndian);
				return aNewDataElement;
			}

			private void AddWarning(int level, string text)
			{
				StringBuilder newString = new StringBuilder(256);
				newString.Append(level);
				newString.Append(": ");
				newString.Append(text);
				((DataElements.Root)RootNode).Warnings.Add(newString.ToString());
			}
			#endregion

			#region Helper functions to detect certain special DataElement tags

			private bool isSequenceDelimitationItem()	
			{
				return ( ( 0xFFFE == dataElementGroupNumber ) && ( 0xE0DD == dataElementElementNumber) );
			}

			/// <summary>
			/// returns if the current Item is a delimitation item
			/// </summary>
			/// <returns>xx</returns>
			private bool isItemDelimitationItem()	
			{
				return ( ( 0xFFFE == dataElementGroupNumber ) && ( 0xE00D == dataElementElementNumber) );
			}

			private bool isItemTag()	
			{
				return ( ( 0xFFFE == dataElementGroupNumber ) && ( 0xE000 == dataElementElementNumber) );
			}

			private bool isPixelData()
			{
				return ( ( 0x7FE0 == dataElementGroupNumber ) && ( 0x0010 == dataElementElementNumber) );
			}
			#endregion
		}
		#endregion
	}
}
