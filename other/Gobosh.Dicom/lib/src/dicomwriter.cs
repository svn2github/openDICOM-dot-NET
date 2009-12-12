/* Gobosh.DICOM
 * Gobosh.Dicom.Writer class to write native DICOM 3.0 files/streams.
 * It is based to the 2006 standard of DICOM.
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
using Gobosh.DICOM;

namespace Gobosh
{
	namespace DICOM
	{

        /// <summary>
		/// Summary description for dicomwriter.
		/// </summary>
		public sealed class Writer
		{

            /// <summary>
            /// a const for the META groupnumber, used for readability
            /// </summary>
            const int kMetaGroupNumber = 2;
            
            const int UndefinedLength = -1;

			private bool IsLittleEndian;
			private bool IsLittleEndianAnnounced;
			private bool IsImplicitVR;
			private bool IsImplicitVRAnnounced;
			private int LastGroup;

			public Writer()
			{
				// default values for writing
				SetInitialTransferSyntax(true,false);
				LastGroup = -1;
			}

			public void SetInitialTransferSyntax(bool isLittleEndian, bool isImplicit)
			{
				IsLittleEndian = isLittleEndian;
				IsLittleEndianAnnounced = isLittleEndian;
				IsImplicitVR = isImplicit;
				IsImplicitVR = IsImplicitVRAnnounced;
			}


            /// <summary>
            /// Writes an element and subelements to output stream
            /// </summary>
            /// <param name="rootElement">the root element to write</param>
            /// <param name="targetStream">the target stream</param>
            /// <param name="usePreamble">if an preamble should be written (true for files)</param>
            /// <returns>true for successful writing</returns>
            public bool WriteToStream(DataElement rootElement, Stream targetStream, bool usePreamble)
			{
                // prepare the data elements before writing.
//                PrepareDataElements(rootElement);

				// write DICOM preamble
                byte[] buffer = new byte[128];

                // shall the preamble be used
				if ( usePreamble )
				{
                    // set all bytes to 0 (default value of byte is 0 
                    // according to the C# programming handbook)
					buffer.Initialize();

					targetStream.Write(buffer,0,128);
				}

				// write DICOM Signature reusing the preamble buffer
				buffer[0] = 0x44;	// D
				buffer[1] = 0x49;	// I
				buffer[2] = 0x43;	// C
				buffer[3] = 0x4D;	// M
				targetStream.Write(buffer,0,4);

				// begin with first element
				foreach ( DataElement n in rootElement )
				{
					WriteElementToStream(n,targetStream,0);
				}
				return true;
			}

			private void WriteElementToStream(DataElement element, Stream targetStream,int level)
			{
				// Write To Stream
				// Check Encoding and group change to detect changes
				if ( element.GroupTag != LastGroup )
				{
					IsLittleEndian = IsLittleEndianAnnounced;
					IsImplicitVR = IsImplicitVRAnnounced;
					LastGroup = element.GroupTag;
				}

				// check for announced encoding

                // TODO: move this to "PrepareDataElements()"
				if ( kMetaGroupNumber == element.GroupTag && 0x0010 == element.ElementTag )
				{
					if ( level > 0 )
					{
						// TODO: Emit a Warning?!?
					}
					String dataContent = element.GetValue().GetValueAsString();
						
					// string dataContent = 
					bool handled = false;

					if ( dataContent == Consts.ISOTransferSyntaxImplicitLittleEndian)
					{
						IsImplicitVRAnnounced = true;
						IsLittleEndianAnnounced = true;
						handled = true;
						// Console.WriteLine(" Implicit Little Endian");
					}
					if ( dataContent == Consts.ISOTransferSyntaxExplicitLittleEndian)
					{
						IsImplicitVRAnnounced = false;
						IsLittleEndianAnnounced = true;
						// Console.WriteLine(" Explicit Little Endian");
						
                        handled = true;
					}
					if ( dataContent == "1.2.840.10008.1.2.1.99" )
					{
						IsImplicitVRAnnounced = false;
						IsLittleEndianAnnounced = true;
						handled = true;
						// TODO: implement deflate filter
						throw new Exception("Deflate Explicit VR is not supported yet");
					}
					if ( dataContent == Consts.ISOTransferSyntaxExplicitBigEndian )
					{
						IsImplicitVRAnnounced = false;
						IsLittleEndianAnnounced = false;
						// Console.WriteLine(" Explicit Big Endian");
						handled = true;
					}
					if ( !handled )
					{
						// WARNING STATE!!
                        // Actually this happens to a lot of other transfer syntaxes
						Console.WriteLine("!! Transfer Syntax not supported: "+dataContent);
						// throw new Exception("Unsupported Transfersyntax: "+dataContent);
                        // IsImplicitVRAnnounced = true;
                        // IsLittleEndianAnnounced = true;
                    }
				}

				// WriteToByteArray evaluates the Children and defines an appropriate
				// ValueLength to write to. It also checks that elements below take care
				// of their internal representation.
				byte[] aByteSequence = element.WriteToByteArray(IsLittleEndian,IsImplicitVR);
				targetStream.Write(aByteSequence,0,aByteSequence.Length);
				if ( element.Count > 0 )
				{
					foreach ( DataElement child in element)
					{
						WriteElementToStream(child,targetStream,level+1);
					}
					// check if we need to write an Sequence Delimitation Item
					if ( ( ValueRepresentationConsts.SQ == element.ValueRepresentation) 
						|| ( 0xFFFE == element.GroupTag && 0xE000 == element.ElementTag )
						|| ( 0x7FE0 == element.GroupTag && 0x0010 == element.ElementTag )	// Pixel Data with Items
						)
					{
						if ( element.ValueLength == UndefinedLength )
						{
                            // write an Sequence Delimitation Item

							byte[] tmp = new byte[8];
							Utils.WriteUInt16(0xFFFE,tmp,0,IsLittleEndian);                            
                            if ( (ValueRepresentationConsts.SQ == element.ValueRepresentation)
                                || ( 0x7FE0 == element.GroupTag && 0x0010 == element.ElementTag ) )
                            {
                                // End of Sequence or 
                                // End of Pixel Data Sequence (see PS3.5-2006, Table A.4-1, p66)
                                Utils.WriteUInt16(0xE0DD, tmp, 2, IsLittleEndian);
                            }
                            else
                            {
                                // End of Item
                                Utils.WriteUInt16(0xE00D, tmp, 2, IsLittleEndian);
                            }
							Utils.WriteUInt32(0,tmp,4,IsLittleEndian);
							targetStream.Write(tmp,0,tmp.Length);
						}
					}
				}
			}
		}
	}
}
