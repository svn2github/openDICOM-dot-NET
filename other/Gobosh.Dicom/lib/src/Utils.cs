/* Gobosh
 * Utils class to provide static cross-platform utility functions.
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

namespace Gobosh
{
	/// <summary>
	/// The Utils class provides several static functions to read and write distinct data types
	/// from a stream in a given endianess.
	/// </summary>
    /// <remarks>The Utils class is made public so it can be used even when the library is used as DLL</remarks>
	public sealed class Utils
	{

		/// <summary>
		/// read an 16bit signed integer from a buffer
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <param name="isLittleEndian">wether to read it little endian or big endian</param>
		/// <returns>the value in native endianess</returns>
		public static int ReadInt16(byte[] buffer, long index, bool isLittleEndian)
		{
			if ( isLittleEndian )
			{
				return ReadInt16LittleEndian(buffer,index);
			}
			else
			{
				return ReadInt16BigEndian(buffer,index);
			}
		}
		/// <summary>
		/// read an 32 bit signed integer from a buffer
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <param name="isLittleEndian">wether to read it little endian or big endian</param>
		/// <returns>the value in native endianess</returns>
		public static Int32 ReadInt32(byte[] buffer, long index, bool isLittleEndian)
		{
			if ( isLittleEndian )
			{
				return ReadInt32LittleEndian(buffer,index);
			}
			else
			{
				return ReadInt32BigEndian(buffer,index);
			}
		}

		public static UInt32 ReadUInt32(byte[] buffer, long index, bool isLittleEndian)
		{
			if ( isLittleEndian )
			{
				return ReadUInt32LittleEndian(buffer,index);
			}
			else
			{
				return ReadUInt32BigEndian(buffer,index);
			}
		}

		public static void WriteUInt32(UInt32 Value, byte[] buffer, long index, bool isLittleEndian)
		{
			if ( isLittleEndian )
			{
				WriteUInt32LittleEndian(Value,buffer,index);
			}
			else
			{
				WriteUInt32BigEndian(Value,buffer,index);
			}
		}

		public static int ReadUInt16(byte[] buffer, long index, bool isLittleEndian)
		{
			if ( isLittleEndian )
			{
				return ReadUInt16LittleEndian(buffer,index);
			}
			else
			{
				return ReadUInt16BigEndian(buffer,index);
			}
		}

		public static void WriteUInt16(UInt16 Value, byte[] buffer, long index, bool isLittleEndian)
		{
			if ( isLittleEndian )
			{
				WriteUInt16LittleEndian(Value,buffer,index);
			}
			else
			{
				WriteUInt16BigEndian(Value,buffer,index);
			}
		}

		/// <summary>
		/// Reads a signed 16 bit int Big Endian encoded
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static int ReadInt16BigEndian(byte[] buffer, long index)
		{
			int result = ( ((int)(buffer[index])<<8) | (int)(buffer[index+1] ) );
			if ( result > 32768 )
			{
				result -= 65536;
			}
			return result;
		}

		/// <summary>
		/// Writes a signed 16 bit int Big Endian encoded
		/// </summary>
		/// <param name="value"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void WriteInt16BigEndian(int Value, byte[] buffer, long index)
		{
			buffer[index] = (byte)(( Value >>8 ) & 0xFF00 );
			buffer[index+1] = (byte) (Value & 0xFF);
		}

		/// <summary>
		/// Reads a signed 16 bit int Little Endian encoded
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static int ReadInt16LittleEndian(byte[] buffer, long index)
		{
			int result = ( (((int)(buffer[index+1]))<<8) | (int)(buffer[index] ) );
			if ( result > 32768 )
			{
				result -= 65536;
			}
			return result;
		}

        /// <summary>
        /// Writes a signed 16 bit int
        /// </summary>
        /// <param name="value">the value to write</param>
        /// <param name="buffer">the target byte buffer</param>
        /// <param name="index">the index where to write</param>
        /// <param name="isLittleEndian">true if the value should be written in little endian</param>
        public static void WriteInt16(int value, byte[] buffer, long index, bool isLittleEndian)
        {
            if (isLittleEndian)
            {
                WriteInt16LittleEndian(value, buffer, index);
            }
            else
            {
                WriteInt16BigEndian(value, buffer, index);
            }
        }

		/// <summary>
		/// Writes a signed 16 bit int Little Endian encoded
		/// </summary>
		/// <param name="value"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void WriteInt16LittleEndian(int Value, byte[] buffer, long index)
		{
			buffer[index] = (byte) (Value & 0xFF);
			buffer[index+1] = (byte)(( Value & 0xFF00 ) >> 8 );			
		}

		/// <summary>
		/// Reads a signed Int32 Little Endian
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static Int32 ReadInt32LittleEndian(byte[] buffer, long index)
		{
			return ( 
                      ((Int32)(buffer[index])         ) 
                    | ((Int32)(buffer[index+1]) << 8  ) 
                    | ((Int32)(buffer[index+2]) << 16 ) 
                    | ((Int32)(buffer[index+3]) << 24 ) 
            );
		}

		/// <summary>
		/// Writes a signed Int32 Little Endian
		/// </summary>
		/// <param name="value">The value to be written</param>
		/// <param name="buffer">The buffer where to write to</param>
		/// <param name="index">The index in the buffer where to write to</param>
		public static void WriteInt32LittleEndian(Int32 Value, byte[] buffer, long index)
		{
			buffer[index+0] = (byte) ((Value & 0xFF));
			buffer[index+1] = (byte) ((Value & 0xFF00) >> 8);
			buffer[index+2] = (byte) ((Value & 0xFF0000) >> 16);
			buffer[index+3] = (byte) ((Value & 0xFF000000) >> 24);
		}

		/// <summary>
		/// Reads a signed Int32 Big Endian
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static Int32 ReadInt32BigEndian(byte[] buffer, long index)
		{
			return ( ((Int32)(buffer[index]) << 24) | ((Int32)(buffer[index+1]) << 16) | ((Int32)(buffer[index+2]) << 8) | ((Int32)(buffer[index+3])     ) );
		}

		/// <summary>
		/// Writes a signed Int32 Big Endian
		/// </summary>
		/// <param name="value">The value to be written</param>
		/// <param name="buffer">The buffer where to write to</param>
		/// <param name="index">The index in the buffer where to write to</param>
		public static void WriteInt32BigEndian(Int32 Value, byte[] buffer, long index)
		{
			buffer[index+0] = (byte) ((Value & 0xFF000000) >> 24);
			buffer[index+1] = (byte) ((Value & 0xFF0000) >> 16);
			buffer[index+2] = (byte) ((Value & 0xFF00) >> 8);
			buffer[index+3] = (byte) ((Value & 0xFF));
		}

        public static void WriteInt32(Int32 Value, byte[] buffer, long index, bool isLittleEndian)
        {
            if (isLittleEndian)
            {
                WriteInt32LittleEndian(Value, buffer, index);
            }
            else
            {
                WriteInt32BigEndian(Value, buffer, index);
            }
        }

		/// <summary>
		/// Reads a signed 16 bit int Big Endian encoded
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static int ReadUInt16BigEndian(byte[] buffer, long index)
		{
			return ( (((UInt16)(buffer[index]))<<8) | (UInt16)(buffer[index+1] ) );
		}

		public static void WriteUInt16BigEndian(UInt16 Value, byte[] buffer, long index)
		{
			buffer[index+0] = (byte)(( Value >> 8 ) & 0xFF);
			buffer[index+1] = (byte)(( Value ) & 0xFF);
		}

		public static void WriteUInt16LittleEndian(UInt16 Value, byte[] buffer, long index)
		{
			buffer[index+0] = (byte)(( Value ) & 0xFF);
			buffer[index+1] = (byte)(( Value >> 8 ) & 0xFF);
		}

		/// <summary>
		/// Reads a signed 16 bit int Little Endian encoded
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static int ReadUInt16LittleEndian(byte[] buffer, long index)
		{
			return ( (((UInt16)(buffer[index+1]))<<8) | (UInt16)(buffer[index] ) );
		}

		/// <summary>
		/// Reads a unsigned Int32 Little Endian
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static UInt32 ReadUInt32LittleEndian(byte[] buffer, long index)
		{
			return ( ((UInt32)(buffer[index])      ) | ((UInt32)(buffer[index+1]) << 8) | ((UInt32)(buffer[index+2]) << 16) | ((UInt32)(buffer[index+3]) << 24) );
		}

		/// <summary>
		/// Reads an unsigned Int32 Big Endian
		/// </summary>
		/// <param name="buffer">the byte buffer from where to read</param>
		/// <param name="index">the index of the first byte of the encoded value</param>
		/// <returns>the value in native endianess</returns>
		public static UInt32 ReadUInt32BigEndian(byte[] buffer, long index)
		{
			return ( ((UInt32)(buffer[index]) << 24) | ((UInt32)(buffer[index+1]) << 16) | ((UInt32)(buffer[index+2]) << 8) | ((UInt32)(buffer[index+3])     ) );
		}

		public static void WriteUInt32LittleEndian(UInt32 Value, byte[] buffer, long index)
		{
			buffer[index+0] = (byte) ((Value & 0xFF));
			buffer[index+1] = (byte) ((Value >> 8) & 0xFF);
			buffer[index+2] = (byte) ((Value >> 16) & 0xFF);
			buffer[index+3] = (byte) ((Value >> 24) & 0xFF);
		}

		public static void WriteUInt32BigEndian(UInt32 Value, byte[] buffer, long index)
		{
			buffer[index+0] = (byte) ((Value >> 24) & 0xFF);
			buffer[index+1] = (byte) ((Value >> 16) & 0xFF);
			buffer[index+2] = (byte) ((Value >> 8) & 0xFF);
			buffer[index+3] = (byte) ((Value & 0xFF));
        }

        #region Float (32bit) functions

        public static float ReadFloatLittleEndian(byte[] buffer, long index)
        {
            // use a ref
            byte[] myRef = buffer;
            long myIndex = index;

            if (!BitConverter.IsLittleEndian)
            {
                // create an additional buffer
                byte[] myBuf = new byte[4];
                myBuf[0 + 0] = buffer[index + 3];
                myBuf[0 + 1] = buffer[index + 2];
                myBuf[0 + 2] = buffer[index + 1];
                myBuf[0 + 3] = buffer[index + 0];
                // use the alternative buffer
                myRef = myBuf;
                myIndex = 0;
            }
            return BitConverter.ToSingle(myRef, (int)myIndex);
        }

        public static float ReadFloatBigEndian(byte[] buffer, long index)
        {
            // use a ref
            byte[] myRef = buffer;
            long myIndex = index;
            
            if ( BitConverter.IsLittleEndian )
            {
                // create an additional buffer
                byte[] myBuf = new byte[4];
                myBuf[0 + 0] = buffer[index + 3];
                myBuf[0 + 1] = buffer[index + 2];
                myBuf[0 + 2] = buffer[index + 1];
                myBuf[0 + 3] = buffer[index + 0];
                // use the alternative buffer
                myRef = myBuf;
                myIndex = 0;
            }
            return BitConverter.ToSingle(myRef, (int)myIndex);
        }

        public static float ReadFloat(byte[] buffer, long index, bool isLittleEndian)
        {
            if (isLittleEndian)
            {
                return ReadFloatLittleEndian(buffer, index);
            }
            else
            {
                return ReadFloatBigEndian(buffer, index);
            }
        }

        public static void WriteFloatLittleEndian(float value, byte[] buffer, long index)
        {
            byte[] myRef = buffer;
            long myIndex = index;

            if ( !BitConverter.IsLittleEndian)
            {
                myRef = new byte[4];
                myIndex = 0;
            }

            Array.Copy(BitConverter.GetBytes(value), 0, myRef, (int) myIndex, 4);

            if (!BitConverter.IsLittleEndian)
            {
                buffer[index + 0] = myRef[3];
                buffer[index + 1] = myRef[2];
                buffer[index + 2] = myRef[1];
                buffer[index + 3] = myRef[0];
            }
        }

        public static void WriteFloatBigEndian(float value, byte[] buffer, long index)
        {
            byte[] myRef = buffer;
            long myIndex = index;

            if (BitConverter.IsLittleEndian)
            {
                myRef = new byte[4];
                myIndex = 0;
            }

            Array.Copy(BitConverter.GetBytes(value), 0, myRef, (int) myIndex, 4);

            if (BitConverter.IsLittleEndian)
            {
                buffer[index + 0] = myRef[3];
                buffer[index + 1] = myRef[2];
                buffer[index + 2] = myRef[1];
                buffer[index + 3] = myRef[0];
            }
        }

        public static void WriteFloat(float value, byte[] buffer, long index, bool isLittleEndian)
        {
            if (isLittleEndian)
            {
                WriteFloatLittleEndian(value, buffer, index);
            }
            else
            {
                WriteFloatBigEndian(value, buffer, index);
            }
        }

        #endregion

        #region Double (64bit) functions

        // Double entspricht der Norm IEC 60559:1989 (IEEE 754) für binäre Gleitkomma-Arithmetik.

        public static double ReadDoubleLittleEndian(byte[] buffer, long index)
        {
            // use a ref
            byte[] myRef = buffer;
            long myIndex = index;

            if (!BitConverter.IsLittleEndian)
            {
                // create an additional buffer
                byte[] myBuf = new byte[8];
                myBuf[0 + 0] = buffer[index + 7];
                myBuf[0 + 1] = buffer[index + 6];
                myBuf[0 + 2] = buffer[index + 5];
                myBuf[0 + 3] = buffer[index + 4];
                myBuf[0 + 4] = buffer[index + 3];
                myBuf[0 + 5] = buffer[index + 2];
                myBuf[0 + 6] = buffer[index + 1];
                myBuf[0 + 7] = buffer[index + 0];
                // use the alternative buffer
                myRef = myBuf;
                myIndex = 0;
            }
            return BitConverter.ToSingle(myRef, (int)myIndex);
        }

        public static double ReadDoubleBigEndian(byte[] buffer, long index)
        {
            // use a ref
            byte[] myRef = buffer;
            long myIndex = index;

            if (BitConverter.IsLittleEndian)
            {
                // create an additional buffer
                byte[] myBuf = new byte[8];
                myBuf[0 + 0] = buffer[index + 7];
                myBuf[0 + 1] = buffer[index + 6];
                myBuf[0 + 2] = buffer[index + 5];
                myBuf[0 + 3] = buffer[index + 4];
                myBuf[0 + 4] = buffer[index + 3];
                myBuf[0 + 5] = buffer[index + 2];
                myBuf[0 + 6] = buffer[index + 1];
                myBuf[0 + 7] = buffer[index + 0];
                // use the alternative buffer
                myRef = myBuf;
                myIndex = 0;
            }
            return BitConverter.ToDouble(myRef, (int) myIndex);
        }

        public static double ReadDouble(byte[] buffer, long index, bool isLittleEndian)
        {
            if (isLittleEndian)
            {
                return ReadDoubleLittleEndian(buffer, index);
            }
            else
            {
                return ReadDoubleBigEndian(buffer, index);
            }
        }

        public static void WriteDoubleLittleEndian(double value, byte[] buffer, long index)
        {
            byte[] myRef = buffer;
            long myIndex = index;

            if (!BitConverter.IsLittleEndian)
            {
                myRef = new byte[8];
                myIndex = 0;
            }

            Array.Copy(BitConverter.GetBytes(value), 0, myRef, (int) myIndex, 4);

            if (!BitConverter.IsLittleEndian)
            {
                buffer[index + 0] = myRef[7];
                buffer[index + 1] = myRef[6];
                buffer[index + 2] = myRef[5];
                buffer[index + 3] = myRef[4];
                buffer[index + 4] = myRef[3];
                buffer[index + 5] = myRef[2];
                buffer[index + 6] = myRef[1];
                buffer[index + 7] = myRef[0];
            }
        }

        public static void WriteDoubleBigEndian(double value, byte[] buffer, long index)
        {
            byte[] myRef = buffer;
            long myIndex = index;

            if (BitConverter.IsLittleEndian)
            {
                myRef = new byte[8];
                myIndex = 0;
            }

            Array.Copy(BitConverter.GetBytes(value), 0, myRef, (int) myIndex, 4);

            if (BitConverter.IsLittleEndian)
            {
                buffer[index + 0] = myRef[7];
                buffer[index + 1] = myRef[6];
                buffer[index + 2] = myRef[5];
                buffer[index + 3] = myRef[4];
                buffer[index + 4] = myRef[3];
                buffer[index + 5] = myRef[2];
                buffer[index + 6] = myRef[1];
                buffer[index + 7] = myRef[0];
            }
        }

        public static void WriteDouble(double value, byte[] buffer, long index, bool isLittleEndian)
        {
            if (isLittleEndian)
            {
                WriteDoubleLittleEndian(value, buffer, index);
            }
            else
            {
                WriteDoubleBigEndian(value, buffer, index);
            }
        }

        #endregion

        /// <summary>
        /// returns true if Value is within [Lower,Higher].
        /// </summary>
        /// <param name="Value">The value to check</param>
        /// <param name="LowerBoundary">The lower including boundary</param>
        /// <param name="HigherBoundary">The upper including boundary</param>
        /// <returns>True, if value is in [Lower,Higher]</returns>
        public static bool CheckBounds(int Value, int LowerBoundary, int HigherBoundary)
		{
			return ( (Value >= LowerBoundary ) && ( Value <= HigherBoundary ) );
		}

		/// <summary>
		/// Decodes a byte array with given encoding
		/// </summary>
		/// <param name="buffer">the byte buffer</param>
		/// <param name="size">size of the string</param>
		/// <param name="encoding">the encoding of the string in the buffer</param>
		/// <returns>the decoded native string</returns>
		public static string DecodeString(byte []buffer, int size, StringEncoding encoding)
		{
			string result;
			if ( size == 0)
			{
				return "";
			}
			else
			{
				// crop a trailing 0x00 and 0x20 bytes
				while ( buffer[ size-1 ] == 0x00 || buffer[ size-1 ] == 0x20 )
				{
					size-=1;
				}
				try
				{
					// due the different encoding the number of bytes used can change
					int newSize = size; // encoding.GetCharCount(buffer,0,size);
					char[] tmp = new char[newSize];
					result = encoding.ToUTF16(buffer,0,size);
					//int encoded = encoding.GetChars(buffer,0,size,tmp,0);
					//result = new String(tmp,0,encoded);
					tmp = null;
				}
				catch(Exception e)
				{
					result = e.ToString();
				}
			}
			return result;
		}

		public static byte[] EncodeString(string stringToEncode, StringEncoding encoding)
		{
			byte[] result = encoding.FromUTF16(stringToEncode);
			return result;
		}
	}
}