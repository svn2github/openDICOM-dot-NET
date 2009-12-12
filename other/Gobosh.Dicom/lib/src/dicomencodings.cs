/* Gobosh
 * String Encoding classes to handle international string conversion
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

namespace Gobosh
{
    //namespace DICOM
    //{
			
		/// <summary>
		/// Encodings is the base class for the different DICOM Encodings.
		/// They support encoding and decoding to utf16, the native .NET
		/// string format.
		/// </summary>
        /// <remarks>You need to override FromUTF16() and ToUTF16() to create a new encoding</remarks>
		public class StringEncoding
		{
			public StringEncoding()
			{
				//
				// TODO: Add constructor logic here
				//
			}
			public virtual string ToUTF16(byte[] buffer, int start , int length)
			{
				return "not implemented";
			}

			public virtual byte[] FromUTF16(string value)
			{
				return null;
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
            public static StringEncoding GetEncodingFromEncodingString(string announcedEncoding)
            {
                string encodingString = "us-ascii";

                if (announcedEncoding == "ISO_IR 100") encodingString = "iso-8859-1";	// Westeuropäisch (ISO)
                if (announcedEncoding == "ISO_IR 101") encodingString = "iso-8859-2";	// Mitteleuropäisch (ISO)
                if (announcedEncoding == "ISO_IR 109") encodingString = "iso-8859-3";	// Lateinisch 3 (ISO)
                if (announcedEncoding == "ISO_IR 110") encodingString = "iso-8859-4";	// Baltisch (ISO)
                if (announcedEncoding == "ISO_IR 144") encodingString = "iso-8859-5";	// Kyrillisch (ISO)
                if (announcedEncoding == "ISO_IR 127") encodingString = "iso-8859-6";	// Arabisch (ISO)
                if (announcedEncoding == "ISO_IR 126") encodingString = "iso-8859-7";	// Griechisch (ISO)
                if (announcedEncoding == "ISO_IR 138") encodingString = "iso-8859-8";	// Hebrew (ISO-Visual)
                if (announcedEncoding == "ISO_IR 148") encodingString = "iso-8859-9";	// Türkisch (ISO) (Latin 5)
                if (announcedEncoding == "ISO_IR 13") encodingString = "iso-2022-jp";	// IBM EBCDIC (Japanisch Katakana)
                if (announcedEncoding == "ISO_IR 166") encodingString = "x-cp20949";	// IBM EBCDIC (Thailändisch)
                if (announcedEncoding == "ISO 2022 IR 100") encodingString = "iso-8859-1";	// Westeuropäisch (ISO)
                if (announcedEncoding == "ISO 2022 IR 101") encodingString = "iso-8859-2";	// Mitteleuropäisch (ISO)
                if (announcedEncoding == "ISO 2022 IR 109") encodingString = "iso-8859-3";	// Lateinisch 3 (ISO)
                if (announcedEncoding == "ISO 2022 IR 110") encodingString = "iso-8859-4";	// Baltisch (ISO)
                if (announcedEncoding == "ISO 2022 IR 144") encodingString = "iso-8859-5";	// Kyrillisch (ISO)
                if (announcedEncoding == "ISO 2022 IR 127") encodingString = "iso-8859-6";	// Arabisch (ISO)
                if (announcedEncoding == "ISO 2022 IR 126") encodingString = "iso-8859-7";	// Griechisch (ISO)
                if (announcedEncoding == "ISO 2022 IR 138") encodingString = "iso-8859-8";	// Hebrew (ISO-Visual)
                if (announcedEncoding == "ISO 2022 IR 148") encodingString = "iso-8859-9";	// Türkisch (ISO) (Latin 5)
                if (announcedEncoding == "ISO 2022 IR 13") encodingString = "iso-2022-jp";		// IBM EBCDIC (Japanisch Katakana)
                if (announcedEncoding == "ISO 2022 IR 166") encodingString = "IBM-Thai";	// IBM EBCDIC (Thailändisch)
                if (announcedEncoding == "ISO 2022 IR 87") encodingString = "unsupp";		// JIS X 0208 Kanji
                if (announcedEncoding == "ISO 2022 IR 159") encodingString = "unsupp";		// JIS X 0212 Supplementary Kanji set
                if (announcedEncoding == "ISO 2022 IR 149") encodingString = "unsupp";		// KS X 1001: Hangul and Hanja

                // unicode
                if (announcedEncoding == "ISO_IR 192") encodingString = "utf-8";		// UTF 8 Unicode
                if (announcedEncoding == "GB18030") encodingString = "GB18030";		// Chinesisch, Vereinfacht

                if ("us-ascii" == encodingString)
                {
                    // TODO: emit warning of "not supported"
                    throw new Exception("Unsupported Text Encoding: " + encodingString);
                }

                return StringEncoding.GetStringEncoding(encodingString);
            }

            public static StringEncoding GetStringEncoding(string encoding)
			{
				StringEncoding result = null;
				// if ( encoding == "us-ascii" )   result = new ISO_8859_1Encoding();
				if ( encoding == "iso-8859-1" ) result = new ISO_8859_1Encoding();
				if ( encoding == "iso-8859-2" ) result = new ISO_8859_2Encoding();
				if ( encoding == "iso-8859-3" ) result = new ISO_8859_3Encoding();
				if ( encoding == "iso-8859-4" ) result = new ISO_8859_4Encoding();
				if ( encoding == "iso-8859-5" ) result = new ISO_8859_5Encoding();
				if ( encoding == "iso-8859-6" ) result = new ISO_8859_6Encoding();
				if ( encoding == "iso-8859-7" ) result = new ISO_8859_7Encoding();
				if ( encoding == "iso-8859-8" ) result = new ISO_8859_8Encoding();
				if ( encoding == "iso-8859-9" ) result = new ISO_8859_9Encoding();
				if ( encoding == "utf-8" )		result = new UTF_8_Encoding();
				if ( encoding == "iso-2022-jp") result = new ISO_2022_JP_Encoding();
				if ( encoding == "GB18030"    )	result = new GB18030_Encoding();

				if ( result == null )
				{
					// trying standard decoding
					result = new ISO_8859_1Encoding();
				}
				return result;
			}
		}

		/// <summary>
		/// base class for ISO_8859_x encodings with a single matching table
		/// Descendands only need to supply their own mapping array
		/// </summary>
		public class ISO_8859_Base : StringEncoding
		{
			protected static int[,] CurrentEncodingTable;  // static?
            /// <summary>
            /// The reverse map of CurrentEncodingTable. Since we have 
            /// UTF16/Unicode characters but only 256 (or less)
            /// target cells. To save Memory a Hashtable is used.
            /// The Hashtable is created by the CreateHashtable()
            /// function. Call it in your constructor
            /// </summary>
            protected Hashtable CurrentRecodingTable;

			public ISO_8859_Base()
			{
				// descendend classes have to set CurrentEncodingTable to their encoding
                CurrentRecodingTable = new Hashtable(256);
			}

            /// <summary>
            /// Protected function to create the reverse hash table of the Encoding table.
            /// Key of the Hashtable is the Unicode character value, 
            /// Value is the 8bit character value from the Encoding table.
            /// </summary>
            protected void CreateHashtable()
            {
                for (int i = 0; i < (CurrentEncodingTable.Length/2); i++)
                {
                    CurrentRecodingTable.Add(CurrentEncodingTable[i, 1], CurrentEncodingTable[i, 0]);
                }
            }

            public override string ToUTF16(byte[] buffer, int start , int length)
			{
				StringBuilder result = new StringBuilder(length);
				int i,j;
				char c;
				for ( i = start ; i < length ; i++)
				{
					c = '?';
					for( j = 0; j < (CurrentEncodingTable.Length/2) ; j++ )
					{

						try
						{
							if ( CurrentEncodingTable[j,0] == buffer[i] )
							{
								c = (char) CurrentEncodingTable[j,1];
								break;
							}
						}
						catch (IndexOutOfRangeException)
						{
							c = '?';
							break;
						}
					}
					result.Append(c);
				}
				return result.ToString();
			}

			public override byte[] FromUTF16(string utf16String)
			{
				// return null;
                int i, j, len;
                len = utf16String.Length;
                byte[] result = new byte[len];
                byte c;
                char ch;

                for (i = 0; i < len; i++)
                {
                    c = (byte)'?';
                    for (j = 0; j < (CurrentEncodingTable.Length / 2); j++)
                    {
                        ch = utf16String[i];
                        try
                        {
                            if (CurrentEncodingTable[j, 1] == ch)
                            {
                                c = (byte)CurrentEncodingTable[j, 0];
                                break;
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            c = (byte)'?';
                            break;
                        }
                    }
                    result[i] = c;
                }

                return result;

                // throw new Exception("FromUTF16() not implemented in class " + this.GetType().ToString());

			}
		}

		public class SystemEncoding : StringEncoding
		{
			protected System.Text.Encoding UsedEncoding;

			public SystemEncoding()
			{
			}

			public override string ToUTF16(byte[] buffer, int start , int length)
			{
				char[] tmp = new char[ UsedEncoding.GetCharCount(buffer,start,length) ];
				int myLength = UsedEncoding.GetChars(buffer,start,length,tmp,0);
				string result = new String(tmp);
				tmp = null;
				return result;
			}
		}

		public class ISO_8859_1Encoding : ISO_8859_Base
		{
			#region ISO 8859-1 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x00A1 },	// INVERTED EXCLAMATION MARK
				{ 0xA2, 0x00A2 },	// CENT SIGN
				{ 0xA3, 0x00A3 },	// POUND SIGN
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xA5, 0x00A5 },	// YEN SIGN
				{ 0xA6, 0x00A6 },	// BROKEN BAR
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x00A9 },	// COPYRIGHT SIGN
				{ 0xAA, 0x00AA },	// FEMININE ORDINAL INDICATOR
				{ 0xAB, 0x00AB },	// LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xAC, 0x00AC },	// NOT SIGN
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x00AE },	// REGISTERED SIGN
				{ 0xAF, 0x00AF },	// MACRON
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x00B1 },	// PLUS-MINUS SIGN
				{ 0xB2, 0x00B2 },	// SUPERSCRIPT TWO
				{ 0xB3, 0x00B3 },	// SUPERSCRIPT THREE
				{ 0xB4, 0x00B4 },	// ACUTE ACCENT
				{ 0xB5, 0x00B5 },	// MICRO SIGN
				{ 0xB6, 0x00B6 },	// PILCROW SIGN
				{ 0xB7, 0x00B7 },	// MIDDLE DOT
				{ 0xB8, 0x00B8 },	// CEDILLA
				{ 0xB9, 0x00B9 },	// SUPERSCRIPT ONE
				{ 0xBA, 0x00BA },	// MASCULINE ORDINAL INDICATOR
				{ 0xBB, 0x00BB },	// RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xBC, 0x00BC },	// VULGAR FRACTION ONE QUARTER
				{ 0xBD, 0x00BD },	// VULGAR FRACTION ONE HALF
				{ 0xBE, 0x00BE },	// VULGAR FRACTION THREE QUARTERS
				{ 0xBF, 0x00BF },	// INVERTED QUESTION MARK
				{ 0xC0, 0x00C0 },	// LATIN CAPITAL LETTER A WITH GRAVE
				{ 0xC1, 0x00C1 },	// LATIN CAPITAL LETTER A WITH ACUTE
				{ 0xC2, 0x00C2 },	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				{ 0xC3, 0x00C3 },	// LATIN CAPITAL LETTER A WITH TILDE
				{ 0xC4, 0x00C4 },	// LATIN CAPITAL LETTER A WITH DIAERESIS
				{ 0xC5, 0x00C5 },	// LATIN CAPITAL LETTER A WITH RING ABOVE
				{ 0xC6, 0x00C6 },	// LATIN CAPITAL LETTER AE
				{ 0xC7, 0x00C7 },	// LATIN CAPITAL LETTER C WITH CEDILLA
				{ 0xC8, 0x00C8 },	// LATIN CAPITAL LETTER E WITH GRAVE
				{ 0xC9, 0x00C9 },	// LATIN CAPITAL LETTER E WITH ACUTE
				{ 0xCA, 0x00CA },	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX
				{ 0xCB, 0x00CB },	// LATIN CAPITAL LETTER E WITH DIAERESIS
				{ 0xCC, 0x00CC },	// LATIN CAPITAL LETTER I WITH GRAVE
				{ 0xCD, 0x00CD },	// LATIN CAPITAL LETTER I WITH ACUTE
				{ 0xCE, 0x00CE },	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				{ 0xCF, 0x00CF },	// LATIN CAPITAL LETTER I WITH DIAERESIS
				{ 0xD0, 0x00D0 },	// LATIN CAPITAL LETTER ETH
				{ 0xD1, 0x00D1 },	// LATIN CAPITAL LETTER N WITH TILDE
				{ 0xD2, 0x00D2 },	// LATIN CAPITAL LETTER O WITH GRAVE
				{ 0xD3, 0x00D3 },	// LATIN CAPITAL LETTER O WITH ACUTE
				{ 0xD4, 0x00D4 },	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				{ 0xD5, 0x00D5 },	// LATIN CAPITAL LETTER O WITH TILDE
				{ 0xD6, 0x00D6 },	// LATIN CAPITAL LETTER O WITH DIAERESIS
				{ 0xD7, 0x00D7 },	// MULTIPLICATION SIGN
				{ 0xD8, 0x00D8 },	// LATIN CAPITAL LETTER O WITH STROKE
				{ 0xD9, 0x00D9 },	// LATIN CAPITAL LETTER U WITH GRAVE
				{ 0xDA, 0x00DA },	// LATIN CAPITAL LETTER U WITH ACUTE
				{ 0xDB, 0x00DB },	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX
				{ 0xDC, 0x00DC },	// LATIN CAPITAL LETTER U WITH DIAERESIS
				{ 0xDD, 0x00DD },	// LATIN CAPITAL LETTER Y WITH ACUTE
				{ 0xDE, 0x00DE },	// LATIN CAPITAL LETTER THORN
				{ 0xDF, 0x00DF },	// LATIN SMALL LETTER SHARP S
				{ 0xE0, 0x00E0 },	// LATIN SMALL LETTER A WITH GRAVE
				{ 0xE1, 0x00E1 },	// LATIN SMALL LETTER A WITH ACUTE
				{ 0xE2, 0x00E2 },	// LATIN SMALL LETTER A WITH CIRCUMFLEX
				{ 0xE3, 0x00E3 },	// LATIN SMALL LETTER A WITH TILDE
				{ 0xE4, 0x00E4 },	// LATIN SMALL LETTER A WITH DIAERESIS
				{ 0xE5, 0x00E5 },	// LATIN SMALL LETTER A WITH RING ABOVE
				{ 0xE6, 0x00E6 },	// LATIN SMALL LETTER AE
				{ 0xE7, 0x00E7 },	// LATIN SMALL LETTER C WITH CEDILLA
				{ 0xE8, 0x00E8 },	// LATIN SMALL LETTER E WITH GRAVE
				{ 0xE9, 0x00E9 },	// LATIN SMALL LETTER E WITH ACUTE
				{ 0xEA, 0x00EA },	// LATIN SMALL LETTER E WITH CIRCUMFLEX
				{ 0xEB, 0x00EB },	// LATIN SMALL LETTER E WITH DIAERESIS
				{ 0xEC, 0x00EC },	// LATIN SMALL LETTER I WITH GRAVE
				{ 0xED, 0x00ED },	// LATIN SMALL LETTER I WITH ACUTE
				{ 0xEE, 0x00EE },	// LATIN SMALL LETTER I WITH CIRCUMFLEX
				{ 0xEF, 0x00EF },	// LATIN SMALL LETTER I WITH DIAERESIS
				{ 0xF0, 0x00F0 },	// LATIN SMALL LETTER ETH
				{ 0xF1, 0x00F1 },	// LATIN SMALL LETTER N WITH TILDE
				{ 0xF2, 0x00F2 },	// LATIN SMALL LETTER O WITH GRAVE
				{ 0xF3, 0x00F3 },	// LATIN SMALL LETTER O WITH ACUTE
				{ 0xF4, 0x00F4 },	// LATIN SMALL LETTER O WITH CIRCUMFLEX
				{ 0xF5, 0x00F5 },	// LATIN SMALL LETTER O WITH TILDE
				{ 0xF6, 0x00F6 },	// LATIN SMALL LETTER O WITH DIAERESIS
				{ 0xF7, 0x00F7 },	// DIVISION SIGN
				{ 0xF8, 0x00F8 },	// LATIN SMALL LETTER O WITH STROKE
				{ 0xF9, 0x00F9 },	// LATIN SMALL LETTER U WITH GRAVE
				{ 0xFA, 0x00FA },	// LATIN SMALL LETTER U WITH ACUTE
				{ 0xFB, 0x00FB },	// LATIN SMALL LETTER U WITH CIRCUMFLEX
				{ 0xFC, 0x00FC },	// LATIN SMALL LETTER U WITH DIAERESIS
				{ 0xFD, 0x00FD },	// LATIN SMALL LETTER Y WITH ACUTE
				{ 0xFE, 0x00FE },	// LATIN SMALL LETTER THORN
				{ 0xFF, 0x00FF }	// LATIN SMALL LETTER Y WITH DIAERESIS

																	  };
			#endregion

			public ISO_8859_1Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}

		}

		public class ISO_8859_2Encoding : ISO_8859_Base
		{
			#region ISO 8859-2 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x0104 },	// LATIN CAPITAL LETTER A WITH OGONEK
				{ 0xA2, 0x02D8 },	// BREVE
				{ 0xA3, 0x0141 },	// LATIN CAPITAL LETTER L WITH STROKE
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xA5, 0x013D },	// LATIN CAPITAL LETTER L WITH CARON
				{ 0xA6, 0x015A },	// LATIN CAPITAL LETTER S WITH ACUTE
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x0160 },	// LATIN CAPITAL LETTER S WITH CARON
				{ 0xAA, 0x015E },	// LATIN CAPITAL LETTER S WITH CEDILLA
				{ 0xAB, 0x0164 },	// LATIN CAPITAL LETTER T WITH CARON
				{ 0xAC, 0x0179 },	// LATIN CAPITAL LETTER Z WITH ACUTE
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x017D },	// LATIN CAPITAL LETTER Z WITH CARON
				{ 0xAF, 0x017B },	// LATIN CAPITAL LETTER Z WITH DOT ABOVE
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x0105 },	// LATIN SMALL LETTER A WITH OGONEK
				{ 0xB2, 0x02DB },	// OGONEK
				{ 0xB3, 0x0142 },	// LATIN SMALL LETTER L WITH STROKE
				{ 0xB4, 0x00B4 },	// ACUTE ACCENT
				{ 0xB5, 0x013E },	// LATIN SMALL LETTER L WITH CARON
				{ 0xB6, 0x015B },	// LATIN SMALL LETTER S WITH ACUTE
				{ 0xB7, 0x02C7 },	// CARON
				{ 0xB8, 0x00B8 },	// CEDILLA
				{ 0xB9, 0x0161 },	// LATIN SMALL LETTER S WITH CARON
				{ 0xBA, 0x015F },	// LATIN SMALL LETTER S WITH CEDILLA
				{ 0xBB, 0x0165 },	// LATIN SMALL LETTER T WITH CARON
				{ 0xBC, 0x017A },	// LATIN SMALL LETTER Z WITH ACUTE
				{ 0xBD, 0x02DD },	// DOUBLE ACUTE ACCENT
				{ 0xBE, 0x017E },	// LATIN SMALL LETTER Z WITH CARON
				{ 0xBF, 0x017C },	// LATIN SMALL LETTER Z WITH DOT ABOVE
				{ 0xC0, 0x0154 },	// LATIN CAPITAL LETTER R WITH ACUTE
				{ 0xC1, 0x00C1 },	// LATIN CAPITAL LETTER A WITH ACUTE
				{ 0xC2, 0x00C2 },	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				{ 0xC3, 0x0102 },	// LATIN CAPITAL LETTER A WITH BREVE
				{ 0xC4, 0x00C4 },	// LATIN CAPITAL LETTER A WITH DIAERESIS
				{ 0xC5, 0x0139 },	// LATIN CAPITAL LETTER L WITH ACUTE
				{ 0xC6, 0x0106 },	// LATIN CAPITAL LETTER C WITH ACUTE
				{ 0xC7, 0x00C7 },	// LATIN CAPITAL LETTER C WITH CEDILLA
				{ 0xC8, 0x010C },	// LATIN CAPITAL LETTER C WITH CARON
				{ 0xC9, 0x00C9 },	// LATIN CAPITAL LETTER E WITH ACUTE
				{ 0xCA, 0x0118 },	// LATIN CAPITAL LETTER E WITH OGONEK
				{ 0xCB, 0x00CB },	// LATIN CAPITAL LETTER E WITH DIAERESIS
				{ 0xCC, 0x011A },	// LATIN CAPITAL LETTER E WITH CARON
				{ 0xCD, 0x00CD },	// LATIN CAPITAL LETTER I WITH ACUTE
				{ 0xCE, 0x00CE },	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				{ 0xCF, 0x010E },	// LATIN CAPITAL LETTER D WITH CARON
				{ 0xD0, 0x0110 },	// LATIN CAPITAL LETTER D WITH STROKE
				{ 0xD1, 0x0143 },	// LATIN CAPITAL LETTER N WITH ACUTE
				{ 0xD2, 0x0147 },	// LATIN CAPITAL LETTER N WITH CARON
				{ 0xD3, 0x00D3 },	// LATIN CAPITAL LETTER O WITH ACUTE
				{ 0xD4, 0x00D4 },	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				{ 0xD5, 0x0150 },	// LATIN CAPITAL LETTER O WITH DOUBLE ACUTE
				{ 0xD6, 0x00D6 },	// LATIN CAPITAL LETTER O WITH DIAERESIS
				{ 0xD7, 0x00D7 },	// MULTIPLICATION SIGN
				{ 0xD8, 0x0158 },	// LATIN CAPITAL LETTER R WITH CARON
				{ 0xD9, 0x016E },	// LATIN CAPITAL LETTER U WITH RING ABOVE
				{ 0xDA, 0x00DA },	// LATIN CAPITAL LETTER U WITH ACUTE
				{ 0xDB, 0x0170 },	// LATIN CAPITAL LETTER U WITH DOUBLE ACUTE
				{ 0xDC, 0x00DC },	// LATIN CAPITAL LETTER U WITH DIAERESIS
				{ 0xDD, 0x00DD },	// LATIN CAPITAL LETTER Y WITH ACUTE
				{ 0xDE, 0x0162 },	// LATIN CAPITAL LETTER T WITH CEDILLA
				{ 0xDF, 0x00DF },	// LATIN SMALL LETTER SHARP S
				{ 0xE0, 0x0155 },	// LATIN SMALL LETTER R WITH ACUTE
				{ 0xE1, 0x00E1 },	// LATIN SMALL LETTER A WITH ACUTE
				{ 0xE2, 0x00E2 },	// LATIN SMALL LETTER A WITH CIRCUMFLEX
				{ 0xE3, 0x0103 },	// LATIN SMALL LETTER A WITH BREVE
				{ 0xE4, 0x00E4 },	// LATIN SMALL LETTER A WITH DIAERESIS
				{ 0xE5, 0x013A },	// LATIN SMALL LETTER L WITH ACUTE
				{ 0xE6, 0x0107 },	// LATIN SMALL LETTER C WITH ACUTE
				{ 0xE7, 0x00E7 },	// LATIN SMALL LETTER C WITH CEDILLA
				{ 0xE8, 0x010D },	// LATIN SMALL LETTER C WITH CARON
				{ 0xE9, 0x00E9 },	// LATIN SMALL LETTER E WITH ACUTE
				{ 0xEA, 0x0119 },	// LATIN SMALL LETTER E WITH OGONEK
				{ 0xEB, 0x00EB },	// LATIN SMALL LETTER E WITH DIAERESIS
				{ 0xEC, 0x011B },	// LATIN SMALL LETTER E WITH CARON
				{ 0xED, 0x00ED },	// LATIN SMALL LETTER I WITH ACUTE
				{ 0xEE, 0x00EE },	// LATIN SMALL LETTER I WITH CIRCUMFLEX
				{ 0xEF, 0x010F },	// LATIN SMALL LETTER D WITH CARON
				{ 0xF0, 0x0111 },	// LATIN SMALL LETTER D WITH STROKE
				{ 0xF1, 0x0144 },	// LATIN SMALL LETTER N WITH ACUTE
				{ 0xF2, 0x0148 },	// LATIN SMALL LETTER N WITH CARON
				{ 0xF3, 0x00F3 },	// LATIN SMALL LETTER O WITH ACUTE
				{ 0xF4, 0x00F4 },	// LATIN SMALL LETTER O WITH CIRCUMFLEX
				{ 0xF5, 0x0151 },	// LATIN SMALL LETTER O WITH DOUBLE ACUTE
				{ 0xF6, 0x00F6 },	// LATIN SMALL LETTER O WITH DIAERESIS
				{ 0xF7, 0x00F7 },	// DIVISION SIGN
				{ 0xF8, 0x0159 },	// LATIN SMALL LETTER R WITH CARON
				{ 0xF9, 0x016F },	// LATIN SMALL LETTER U WITH RING ABOVE
				{ 0xFA, 0x00FA },	// LATIN SMALL LETTER U WITH ACUTE
				{ 0xFB, 0x0171 },	// LATIN SMALL LETTER U WITH DOUBLE ACUTE
				{ 0xFC, 0x00FC },	// LATIN SMALL LETTER U WITH DIAERESIS
				{ 0xFD, 0x00FD },	// LATIN SMALL LETTER Y WITH ACUTE
				{ 0xFE, 0x0163 },	// LATIN SMALL LETTER T WITH CEDILLA
				{ 0xFF, 0x02D9 },	// DOT ABOVE
			};
			#endregion

			public ISO_8859_2Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}
		
		public class ISO_8859_3Encoding : ISO_8859_Base
		{
			#region ISO 8859-3 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x0126 },	// LATIN CAPITAL LETTER H WITH STROKE
				{ 0xA2, 0x02D8 },	// BREVE
				{ 0xA3, 0x00A3 },	// POUND SIGN
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xA6, 0x0124 },	// LATIN CAPITAL LETTER H WITH CIRCUMFLEX
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x0130 },	// LATIN CAPITAL LETTER I WITH DOT ABOVE
				{ 0xAA, 0x015E },	// LATIN CAPITAL LETTER S WITH CEDILLA
				{ 0xAB, 0x011E },	// LATIN CAPITAL LETTER G WITH BREVE
				{ 0xAC, 0x0134 },	// LATIN CAPITAL LETTER J WITH CIRCUMFLEX
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAF, 0x017B },	// LATIN CAPITAL LETTER Z WITH DOT ABOVE
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x0127 },	// LATIN SMALL LETTER H WITH STROKE
				{ 0xB2, 0x00B2 },	// SUPERSCRIPT TWO
				{ 0xB3, 0x00B3 },	// SUPERSCRIPT THREE
				{ 0xB4, 0x00B4 },	// ACUTE ACCENT
				{ 0xB5, 0x00B5 },	// MICRO SIGN
				{ 0xB6, 0x0125 },	// LATIN SMALL LETTER H WITH CIRCUMFLEX
				{ 0xB7, 0x00B7 },	// MIDDLE DOT
				{ 0xB8, 0x00B8 },	// CEDILLA
				{ 0xB9, 0x0131 },	// LATIN SMALL LETTER DOTLESS I
				{ 0xBA, 0x015F },	// LATIN SMALL LETTER S WITH CEDILLA
				{ 0xBB, 0x011F },	// LATIN SMALL LETTER G WITH BREVE
				{ 0xBC, 0x0135 },	// LATIN SMALL LETTER J WITH CIRCUMFLEX
				{ 0xBD, 0x00BD },	// VULGAR FRACTION ONE HALF
				{ 0xBF, 0x017C },	// LATIN SMALL LETTER Z WITH DOT ABOVE
				{ 0xC0, 0x00C0 },	// LATIN CAPITAL LETTER A WITH GRAVE
				{ 0xC1, 0x00C1 },	// LATIN CAPITAL LETTER A WITH ACUTE
				{ 0xC2, 0x00C2 },	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				{ 0xC4, 0x00C4 },	// LATIN CAPITAL LETTER A WITH DIAERESIS
				{ 0xC5, 0x010A },	// LATIN CAPITAL LETTER C WITH DOT ABOVE
				{ 0xC6, 0x0108 },	// LATIN CAPITAL LETTER C WITH CIRCUMFLEX
				{ 0xC7, 0x00C7 },	// LATIN CAPITAL LETTER C WITH CEDILLA
				{ 0xC8, 0x00C8 },	// LATIN CAPITAL LETTER E WITH GRAVE
				{ 0xC9, 0x00C9 },	// LATIN CAPITAL LETTER E WITH ACUTE
				{ 0xCA, 0x00CA },	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX
				{ 0xCB, 0x00CB },	// LATIN CAPITAL LETTER E WITH DIAERESIS
				{ 0xCC, 0x00CC },	// LATIN CAPITAL LETTER I WITH GRAVE
				{ 0xCD, 0x00CD },	// LATIN CAPITAL LETTER I WITH ACUTE
				{ 0xCE, 0x00CE },	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				{ 0xCF, 0x00CF },	// LATIN CAPITAL LETTER I WITH DIAERESIS
				{ 0xD1, 0x00D1 },	// LATIN CAPITAL LETTER N WITH TILDE
				{ 0xD2, 0x00D2 },	// LATIN CAPITAL LETTER O WITH GRAVE
				{ 0xD3, 0x00D3 },	// LATIN CAPITAL LETTER O WITH ACUTE
				{ 0xD4, 0x00D4 },	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				{ 0xD5, 0x0120 },	// LATIN CAPITAL LETTER G WITH DOT ABOVE
				{ 0xD6, 0x00D6 },	// LATIN CAPITAL LETTER O WITH DIAERESIS
				{ 0xD7, 0x00D7 },	// MULTIPLICATION SIGN
				{ 0xD8, 0x011C },	// LATIN CAPITAL LETTER G WITH CIRCUMFLEX
				{ 0xD9, 0x00D9 },	// LATIN CAPITAL LETTER U WITH GRAVE
				{ 0xDA, 0x00DA },	// LATIN CAPITAL LETTER U WITH ACUTE
				{ 0xDB, 0x00DB },	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX
				{ 0xDC, 0x00DC },	// LATIN CAPITAL LETTER U WITH DIAERESIS
				{ 0xDD, 0x016C },	// LATIN CAPITAL LETTER U WITH BREVE
				{ 0xDE, 0x015C },	// LATIN CAPITAL LETTER S WITH CIRCUMFLEX
				{ 0xDF, 0x00DF },	// LATIN SMALL LETTER SHARP S
				{ 0xE0, 0x00E0 },	// LATIN SMALL LETTER A WITH GRAVE
				{ 0xE1, 0x00E1 },	// LATIN SMALL LETTER A WITH ACUTE
				{ 0xE2, 0x00E2 },	// LATIN SMALL LETTER A WITH CIRCUMFLEX
				{ 0xE4, 0x00E4 },	// LATIN SMALL LETTER A WITH DIAERESIS
				{ 0xE5, 0x010B },	// LATIN SMALL LETTER C WITH DOT ABOVE
				{ 0xE6, 0x0109 },	// LATIN SMALL LETTER C WITH CIRCUMFLEX
				{ 0xE7, 0x00E7 },	// LATIN SMALL LETTER C WITH CEDILLA
				{ 0xE8, 0x00E8 },	// LATIN SMALL LETTER E WITH GRAVE
				{ 0xE9, 0x00E9 },	// LATIN SMALL LETTER E WITH ACUTE
				{ 0xEA, 0x00EA },	// LATIN SMALL LETTER E WITH CIRCUMFLEX
				{ 0xEB, 0x00EB },	// LATIN SMALL LETTER E WITH DIAERESIS
				{ 0xEC, 0x00EC },	// LATIN SMALL LETTER I WITH GRAVE
				{ 0xED, 0x00ED },	// LATIN SMALL LETTER I WITH ACUTE
				{ 0xEE, 0x00EE },	// LATIN SMALL LETTER I WITH CIRCUMFLEX
				{ 0xEF, 0x00EF },	// LATIN SMALL LETTER I WITH DIAERESIS
				{ 0xF1, 0x00F1 },	// LATIN SMALL LETTER N WITH TILDE
				{ 0xF2, 0x00F2 },	// LATIN SMALL LETTER O WITH GRAVE
				{ 0xF3, 0x00F3 },	// LATIN SMALL LETTER O WITH ACUTE
				{ 0xF4, 0x00F4 },	// LATIN SMALL LETTER O WITH CIRCUMFLEX
				{ 0xF5, 0x0121 },	// LATIN SMALL LETTER G WITH DOT ABOVE
				{ 0xF6, 0x00F6 },	// LATIN SMALL LETTER O WITH DIAERESIS
				{ 0xF7, 0x00F7 },	// DIVISION SIGN
				{ 0xF8, 0x011D },	// LATIN SMALL LETTER G WITH CIRCUMFLEX
				{ 0xF9, 0x00F9 },	// LATIN SMALL LETTER U WITH GRAVE
				{ 0xFA, 0x00FA },	// LATIN SMALL LETTER U WITH ACUTE
				{ 0xFB, 0x00FB },	// LATIN SMALL LETTER U WITH CIRCUMFLEX
				{ 0xFC, 0x00FC },	// LATIN SMALL LETTER U WITH DIAERESIS
				{ 0xFD, 0x016D },	// LATIN SMALL LETTER U WITH BREVE
				{ 0xFE, 0x015D },	// LATIN SMALL LETTER S WITH CIRCUMFLEX
				{ 0xFF, 0x02D9 },	// DOT ABOVE
			};
			#endregion

			public ISO_8859_3Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}
		
		public class ISO_8859_4Encoding : ISO_8859_Base
		{
			#region ISO 8859-4 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x0104 },	// LATIN CAPITAL LETTER A WITH OGONEK
				{ 0xA2, 0x0138 },	// LATIN SMALL LETTER KRA
				{ 0xA3, 0x0156 },	// LATIN CAPITAL LETTER R WITH CEDILLA
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xA5, 0x0128 },	// LATIN CAPITAL LETTER I WITH TILDE
				{ 0xA6, 0x013B },	// LATIN CAPITAL LETTER L WITH CEDILLA
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x0160 },	// LATIN CAPITAL LETTER S WITH CARON
				{ 0xAA, 0x0112 },	// LATIN CAPITAL LETTER E WITH MACRON
				{ 0xAB, 0x0122 },	// LATIN CAPITAL LETTER G WITH CEDILLA
				{ 0xAC, 0x0166 },	// LATIN CAPITAL LETTER T WITH STROKE
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x017D },	// LATIN CAPITAL LETTER Z WITH CARON
				{ 0xAF, 0x00AF },	// MACRON
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x0105 },	// LATIN SMALL LETTER A WITH OGONEK
				{ 0xB2, 0x02DB },	// OGONEK
				{ 0xB3, 0x0157 },	// LATIN SMALL LETTER R WITH CEDILLA
				{ 0xB4, 0x00B4 },	// ACUTE ACCENT
				{ 0xB5, 0x0129 },	// LATIN SMALL LETTER I WITH TILDE
				{ 0xB6, 0x013C },	// LATIN SMALL LETTER L WITH CEDILLA
				{ 0xB7, 0x02C7 },	// CARON
				{ 0xB8, 0x00B8 },	// CEDILLA
				{ 0xB9, 0x0161 },	// LATIN SMALL LETTER S WITH CARON
				{ 0xBA, 0x0113 },	// LATIN SMALL LETTER E WITH MACRON
				{ 0xBB, 0x0123 },	// LATIN SMALL LETTER G WITH CEDILLA
				{ 0xBC, 0x0167 },	// LATIN SMALL LETTER T WITH STROKE
				{ 0xBD, 0x014A },	// LATIN CAPITAL LETTER ENG
				{ 0xBE, 0x017E },	// LATIN SMALL LETTER Z WITH CARON
				{ 0xBF, 0x014B },	// LATIN SMALL LETTER ENG
				{ 0xC0, 0x0100 },	// LATIN CAPITAL LETTER A WITH MACRON
				{ 0xC1, 0x00C1 },	// LATIN CAPITAL LETTER A WITH ACUTE
				{ 0xC2, 0x00C2 },	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				{ 0xC3, 0x00C3 },	// LATIN CAPITAL LETTER A WITH TILDE
				{ 0xC4, 0x00C4 },	// LATIN CAPITAL LETTER A WITH DIAERESIS
				{ 0xC5, 0x00C5 },	// LATIN CAPITAL LETTER A WITH RING ABOVE
				{ 0xC6, 0x00C6 },	// LATIN CAPITAL LETTER AE
				{ 0xC7, 0x012E },	// LATIN CAPITAL LETTER I WITH OGONEK
				{ 0xC8, 0x010C },	// LATIN CAPITAL LETTER C WITH CARON
				{ 0xC9, 0x00C9 },	// LATIN CAPITAL LETTER E WITH ACUTE
				{ 0xCA, 0x0118 },	// LATIN CAPITAL LETTER E WITH OGONEK
				{ 0xCB, 0x00CB },	// LATIN CAPITAL LETTER E WITH DIAERESIS
				{ 0xCC, 0x0116 },	// LATIN CAPITAL LETTER E WITH DOT ABOVE
				{ 0xCD, 0x00CD },	// LATIN CAPITAL LETTER I WITH ACUTE
				{ 0xCE, 0x00CE },	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				{ 0xCF, 0x012A },	// LATIN CAPITAL LETTER I WITH MACRON
				{ 0xD0, 0x0110 },	// LATIN CAPITAL LETTER D WITH STROKE
				{ 0xD1, 0x0145 },	// LATIN CAPITAL LETTER N WITH CEDILLA
				{ 0xD2, 0x014C },	// LATIN CAPITAL LETTER O WITH MACRON
				{ 0xD3, 0x0136 },	// LATIN CAPITAL LETTER K WITH CEDILLA
				{ 0xD4, 0x00D4 },	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				{ 0xD5, 0x00D5 },	// LATIN CAPITAL LETTER O WITH TILDE
				{ 0xD6, 0x00D6 },	// LATIN CAPITAL LETTER O WITH DIAERESIS
				{ 0xD7, 0x00D7 },	// MULTIPLICATION SIGN
				{ 0xD8, 0x00D8 },	// LATIN CAPITAL LETTER O WITH STROKE
				{ 0xD9, 0x0172 },	// LATIN CAPITAL LETTER U WITH OGONEK
				{ 0xDA, 0x00DA },	// LATIN CAPITAL LETTER U WITH ACUTE
				{ 0xDB, 0x00DB },	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX
				{ 0xDC, 0x00DC },	// LATIN CAPITAL LETTER U WITH DIAERESIS
				{ 0xDD, 0x0168 },	// LATIN CAPITAL LETTER U WITH TILDE
				{ 0xDE, 0x016A },	// LATIN CAPITAL LETTER U WITH MACRON
				{ 0xDF, 0x00DF },	// LATIN SMALL LETTER SHARP S
				{ 0xE0, 0x0101 },	// LATIN SMALL LETTER A WITH MACRON
				{ 0xE1, 0x00E1 },	// LATIN SMALL LETTER A WITH ACUTE
				{ 0xE2, 0x00E2 },	// LATIN SMALL LETTER A WITH CIRCUMFLEX
				{ 0xE3, 0x00E3 },	// LATIN SMALL LETTER A WITH TILDE
				{ 0xE4, 0x00E4 },	// LATIN SMALL LETTER A WITH DIAERESIS
				{ 0xE5, 0x00E5 },	// LATIN SMALL LETTER A WITH RING ABOVE
				{ 0xE6, 0x00E6 },	// LATIN SMALL LETTER AE
				{ 0xE7, 0x012F },	// LATIN SMALL LETTER I WITH OGONEK
				{ 0xE8, 0x010D },	// LATIN SMALL LETTER C WITH CARON
				{ 0xE9, 0x00E9 },	// LATIN SMALL LETTER E WITH ACUTE
				{ 0xEA, 0x0119 },	// LATIN SMALL LETTER E WITH OGONEK
				{ 0xEB, 0x00EB },	// LATIN SMALL LETTER E WITH DIAERESIS
				{ 0xEC, 0x0117 },	// LATIN SMALL LETTER E WITH DOT ABOVE
				{ 0xED, 0x00ED },	// LATIN SMALL LETTER I WITH ACUTE
				{ 0xEE, 0x00EE },	// LATIN SMALL LETTER I WITH CIRCUMFLEX
				{ 0xEF, 0x012B },	// LATIN SMALL LETTER I WITH MACRON
				{ 0xF0, 0x0111 },	// LATIN SMALL LETTER D WITH STROKE
				{ 0xF1, 0x0146 },	// LATIN SMALL LETTER N WITH CEDILLA
				{ 0xF2, 0x014D },	// LATIN SMALL LETTER O WITH MACRON
				{ 0xF3, 0x0137 },	// LATIN SMALL LETTER K WITH CEDILLA
				{ 0xF4, 0x00F4 },	// LATIN SMALL LETTER O WITH CIRCUMFLEX
				{ 0xF5, 0x00F5 },	// LATIN SMALL LETTER O WITH TILDE
				{ 0xF6, 0x00F6 },	// LATIN SMALL LETTER O WITH DIAERESIS
				{ 0xF7, 0x00F7 },	// DIVISION SIGN
				{ 0xF8, 0x00F8 },	// LATIN SMALL LETTER O WITH STROKE
				{ 0xF9, 0x0173 },	// LATIN SMALL LETTER U WITH OGONEK
				{ 0xFA, 0x00FA },	// LATIN SMALL LETTER U WITH ACUTE
				{ 0xFB, 0x00FB },	// LATIN SMALL LETTER U WITH CIRCUMFLEX
				{ 0xFC, 0x00FC },	// LATIN SMALL LETTER U WITH DIAERESIS
				{ 0xFD, 0x0169 },	// LATIN SMALL LETTER U WITH TILDE
				{ 0xFE, 0x016B },	// LATIN SMALL LETTER U WITH MACRON
				{ 0xFF, 0x02D9 },	// DOT ABOVE

			};
			#endregion

			public ISO_8859_4Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class ISO_8859_5Encoding : ISO_8859_Base
		{
			#region ISO 8859-5 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x0452 },	// CYRILLIC SMALL LETTER DJE
				{ 0xA2, 0x0453 },	// CYRILLIC SMALL LETTER GJE
				{ 0xA3, 0x0451 },	// CYRILLIC SMALL LETTER IO
				{ 0xA4, 0x0454 },	// CYRILLIC SMALL LETTER UKRAINIAN IE
				{ 0xA5, 0x0455 },	// CYRILLIC SMALL LETTER DZE
				{ 0xA6, 0x0456 },	// CYRILLIC SMALL LETTER BYELORUSSIAN-UKRAINIAN I
				{ 0xA7, 0x0457 },	// CYRILLIC SMALL LETTER YI
				{ 0xA8, 0x0458 },	// CYRILLIC SMALL LETTER JE
				{ 0xA9, 0x0459 },	// CYRILLIC SMALL LETTER LJE
				{ 0xAA, 0x045A },	// CYRILLIC SMALL LETTER NJE
				{ 0xAB, 0x045B },	// CYRILLIC SMALL LETTER TSHE
				{ 0xAC, 0x045C },	// CYRILLIC SMALL LETTER KJE
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x045E },	// CYRILLIC SMALL LETTER SHORT U
				{ 0xAF, 0x045F },	// CYRILLIC SMALL LETTER DZHE
				{ 0xB0, 0x2116 },	// NUMERO SIGN
				{ 0xB1, 0x0402 },	// CYRILLIC CAPITAL LETTER DJE
				{ 0xB2, 0x0403 },	// CYRILLIC CAPITAL LETTER GJE
				{ 0xB3, 0x0401 },	// CYRILLIC CAPITAL LETTER IO
				{ 0xB4, 0x0404 },	// CYRILLIC CAPITAL LETTER UKRAINIAN IE
				{ 0xB5, 0x0405 },	// CYRILLIC CAPITAL LETTER DZE
				{ 0xB6, 0x0406 },	// CYRILLIC CAPITAL LETTER BYELORUSSIAN-UKRAINIAN I
				{ 0xB7, 0x0407 },	// CYRILLIC CAPITAL LETTER YI
				{ 0xB8, 0x0408 },	// CYRILLIC CAPITAL LETTER JE
				{ 0xB9, 0x0409 },	// CYRILLIC CAPITAL LETTER LJE
				{ 0xBA, 0x040A },	// CYRILLIC CAPITAL LETTER NJE
				{ 0xBB, 0x040B },	// CYRILLIC CAPITAL LETTER TSHE
				{ 0xBC, 0x040C },	// CYRILLIC CAPITAL LETTER KJE
				{ 0xBD, 0x00A4 },	// CURRENCY SIGN
				{ 0xBE, 0x040E },	// CYRILLIC CAPITAL LETTER SHORT U
				{ 0xBF, 0x040F },	// CYRILLIC CAPITAL LETTER DZHE
				{ 0xC0, 0x044E },	// CYRILLIC SMALL LETTER YU
				{ 0xC1, 0x0430 },	// CYRILLIC SMALL LETTER A
				{ 0xC2, 0x0431 },	// CYRILLIC SMALL LETTER BE
				{ 0xC3, 0x0446 },	// CYRILLIC SMALL LETTER TSE
				{ 0xC4, 0x0434 },	// CYRILLIC SMALL LETTER DE
				{ 0xC5, 0x0435 },	// CYRILLIC SMALL LETTER IE
				{ 0xC6, 0x0444 },	// CYRILLIC SMALL LETTER EF
				{ 0xC7, 0x0433 },	// CYRILLIC SMALL LETTER GHE
				{ 0xC8, 0x0445 },	// CYRILLIC SMALL LETTER HA
				{ 0xC9, 0x0438 },	// CYRILLIC SMALL LETTER I
				{ 0xCA, 0x0439 },	// CYRILLIC SMALL LETTER SHORT I
				{ 0xCB, 0x043A },	// CYRILLIC SMALL LETTER KA
				{ 0xCC, 0x043B },	// CYRILLIC SMALL LETTER EL
				{ 0xCD, 0x043C },	// CYRILLIC SMALL LETTER EM
				{ 0xCE, 0x043D },	// CYRILLIC SMALL LETTER EN
				{ 0xCF, 0x043E },	// CYRILLIC SMALL LETTER O
				{ 0xD0, 0x043F },	// CYRILLIC SMALL LETTER PE
				{ 0xD1, 0x044F },	// CYRILLIC SMALL LETTER YA
				{ 0xD2, 0x0440 },	// CYRILLIC SMALL LETTER ER
				{ 0xD3, 0x0441 },	// CYRILLIC SMALL LETTER ES
				{ 0xD4, 0x0442 },	// CYRILLIC SMALL LETTER TE
				{ 0xD5, 0x0443 },	// CYRILLIC SMALL LETTER U
				{ 0xD6, 0x0436 },	// CYRILLIC SMALL LETTER ZHE
				{ 0xD7, 0x0432 },	// CYRILLIC SMALL LETTER VE
				{ 0xD8, 0x044C },	// CYRILLIC SMALL LETTER SOFT SIGN
				{ 0xD9, 0x044B },	// CYRILLIC SMALL LETTER YERU
				{ 0xDA, 0x0437 },	// CYRILLIC SMALL LETTER ZE
				{ 0xDB, 0x0448 },	// CYRILLIC SMALL LETTER SHA
				{ 0xDC, 0x044D },	// CYRILLIC SMALL LETTER E
				{ 0xDD, 0x0449 },	// CYRILLIC SMALL LETTER SHCHA
				{ 0xDE, 0x0447 },	// CYRILLIC SMALL LETTER CHE
				{ 0xDF, 0x044A },	// CYRILLIC SMALL LETTER HARD SIGN
				{ 0xE0, 0x042E },	// CYRILLIC CAPITAL LETTER YU
				{ 0xE1, 0x0410 },	// CYRILLIC CAPITAL LETTER A
				{ 0xE2, 0x0411 },	// CYRILLIC CAPITAL LETTER BE
				{ 0xE3, 0x0426 },	// CYRILLIC CAPITAL LETTER TSE
				{ 0xE4, 0x0414 },	// CYRILLIC CAPITAL LETTER DE
				{ 0xE5, 0x0415 },	// CYRILLIC CAPITAL LETTER IE
				{ 0xE6, 0x0424 },	// CYRILLIC CAPITAL LETTER EF
				{ 0xE7, 0x0413 },	// CYRILLIC CAPITAL LETTER GHE
				{ 0xE8, 0x0425 },	// CYRILLIC CAPITAL LETTER HA
				{ 0xE9, 0x0418 },	// CYRILLIC CAPITAL LETTER I
				{ 0xEA, 0x0419 },	// CYRILLIC CAPITAL LETTER SHORT I
				{ 0xEB, 0x041A },	// CYRILLIC CAPITAL LETTER KA
				{ 0xEC, 0x041B },	// CYRILLIC CAPITAL LETTER EL
				{ 0xED, 0x041C },	// CYRILLIC CAPITAL LETTER EM
				{ 0xEE, 0x041D },	// CYRILLIC CAPITAL LETTER EN
				{ 0xEF, 0x041E },	// CYRILLIC CAPITAL LETTER O
				{ 0xF0, 0x041F },	// CYRILLIC CAPITAL LETTER PE
				{ 0xF1, 0x042F },	// CYRILLIC CAPITAL LETTER YA
				{ 0xF2, 0x0420 },	// CYRILLIC CAPITAL LETTER ER
				{ 0xF3, 0x0421 },	// CYRILLIC CAPITAL LETTER ES
				{ 0xF4, 0x0422 },	// CYRILLIC CAPITAL LETTER TE
				{ 0xF5, 0x0423 },	// CYRILLIC CAPITAL LETTER U
				{ 0xF6, 0x0416 },	// CYRILLIC CAPITAL LETTER ZHE
				{ 0xF7, 0x0412 },	// CYRILLIC CAPITAL LETTER VE
				{ 0xF8, 0x042C },	// CYRILLIC CAPITAL LETTER SOFT SIGN
				{ 0xF9, 0x042B },	// CYRILLIC CAPITAL LETTER YERU
				{ 0xFA, 0x0417 },	// CYRILLIC CAPITAL LETTER ZE
				{ 0xFB, 0x0428 },	// CYRILLIC CAPITAL LETTER SHA
				{ 0xFC, 0x042D },	// CYRILLIC CAPITAL LETTER E
				{ 0xFD, 0x0429 },	// CYRILLIC CAPITAL LETTER SHCHA
				{ 0xFE, 0x0427 },	// CYRILLIC CAPITAL LETTER CHE
				{ 0xFF, 0x042A },	// CYRILLIC CAPITAL LETTER HARD SIGN
			};
			#endregion

			public ISO_8859_5Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class ISO_8859_6Encoding : ISO_8859_Base
		{
			#region ISO 8859-6 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xAC, 0x060C },	// ARABIC COMMA
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xBB, 0x061B },	// ARABIC SEMICOLON
				{ 0xBF, 0x061F },	// ARABIC QUESTION MARK
				{ 0xC1, 0x0621 },	// ARABIC LETTER HAMZA
				{ 0xC2, 0x0622 },	// ARABIC LETTER ALEF WITH MADDA ABOVE
				{ 0xC3, 0x0623 },	// ARABIC LETTER ALEF WITH HAMZA ABOVE
				{ 0xC4, 0x0624 },	// ARABIC LETTER WAW WITH HAMZA ABOVE
				{ 0xC5, 0x0625 },	// ARABIC LETTER ALEF WITH HAMZA BELOW
				{ 0xC6, 0x0626 },	// ARABIC LETTER YEH WITH HAMZA ABOVE
				{ 0xC7, 0x0627 },	// ARABIC LETTER ALEF
				{ 0xC8, 0x0628 },	// ARABIC LETTER BEH
				{ 0xC9, 0x0629 },	// ARABIC LETTER TEH MARBUTA
				{ 0xCA, 0x062A },	// ARABIC LETTER TEH
				{ 0xCB, 0x062B },	// ARABIC LETTER THEH
				{ 0xCC, 0x062C },	// ARABIC LETTER JEEM
				{ 0xCD, 0x062D },	// ARABIC LETTER HAH
				{ 0xCE, 0x062E },	// ARABIC LETTER KHAH
				{ 0xCF, 0x062F },	// ARABIC LETTER DAL
				{ 0xD0, 0x0630 },	// ARABIC LETTER THAL
				{ 0xD1, 0x0631 },	// ARABIC LETTER REH
				{ 0xD2, 0x0632 },	// ARABIC LETTER ZAIN
				{ 0xD3, 0x0633 },	// ARABIC LETTER SEEN
				{ 0xD4, 0x0634 },	// ARABIC LETTER SHEEN
				{ 0xD5, 0x0635 },	// ARABIC LETTER SAD
				{ 0xD6, 0x0636 },	// ARABIC LETTER DAD
				{ 0xD7, 0x0637 },	// ARABIC LETTER TAH
				{ 0xD8, 0x0638 },	// ARABIC LETTER ZAH
				{ 0xD9, 0x0639 },	// ARABIC LETTER AIN
				{ 0xDA, 0x063A },	// ARABIC LETTER GHAIN
				{ 0xE0, 0x0640 },	// ARABIC TATWEEL
				{ 0xE1, 0x0641 },	// ARABIC LETTER FEH
				{ 0xE2, 0x0642 },	// ARABIC LETTER QAF
				{ 0xE3, 0x0643 },	// ARABIC LETTER KAF
				{ 0xE4, 0x0644 },	// ARABIC LETTER LAM
				{ 0xE5, 0x0645 },	// ARABIC LETTER MEEM
				{ 0xE6, 0x0646 },	// ARABIC LETTER NOON
				{ 0xE7, 0x0647 },	// ARABIC LETTER HEH
				{ 0xE8, 0x0648 },	// ARABIC LETTER WAW
				{ 0xE9, 0x0649 },	// ARABIC LETTER ALEF MAKSURA
				{ 0xEA, 0x064A },	// ARABIC LETTER YEH
				{ 0xEB, 0x064B },	// ARABIC FATHATAN
				{ 0xEC, 0x064C },	// ARABIC DAMMATAN
				{ 0xED, 0x064D },	// ARABIC KASRATAN
				{ 0xEE, 0x064E },	// ARABIC FATHA
				{ 0xEF, 0x064F },	// ARABIC DAMMA
				{ 0xF0, 0x0650 },	// ARABIC KASRA
				{ 0xF1, 0x0651 },	// ARABIC SHADDA
				{ 0xF2, 0x0652 },	// ARABIC SUKUN
			};
			#endregion

			public ISO_8859_6Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class ISO_8859_7Encoding : ISO_8859_Base
		{
			#region ISO 8859-7 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x02BD },	// MODIFIER LETTER REVERSED COMMA
				{ 0xA2, 0x02BC },	// MODIFIER LETTER APOSTROPHE
				{ 0xA3, 0x00A3 },	// POUND SIGN
				{ 0xA6, 0x00A6 },	// BROKEN BAR
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x00A9 },	// COPYRIGHT SIGN
				{ 0xAB, 0x00AB },	// LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xAC, 0x00AC },	// NOT SIGN
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAF, 0x2015 },	// HORIZONTAL BAR
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x00B1 },	// PLUS-MINUS SIGN
				{ 0xB2, 0x00B2 },	// SUPERSCRIPT TWO
				{ 0xB3, 0x00B3 },	// SUPERSCRIPT THREE
				{ 0xB4, 0x0384 },	// GREEK TONOS
				{ 0xB5, 0x0385 },	// GREEK DIALYTIKA TONOS
				{ 0xB6, 0x0386 },	// GREEK CAPITAL LETTER ALPHA WITH TONOS
				{ 0xB7, 0x00B7 },	// MIDDLE DOT
				{ 0xB8, 0x0388 },	// GREEK CAPITAL LETTER EPSILON WITH TONOS
				{ 0xB9, 0x0389 },	// GREEK CAPITAL LETTER ETA WITH TONOS
				{ 0xBA, 0x038A },	// GREEK CAPITAL LETTER IOTA WITH TONOS
				{ 0xBB, 0x00BB },	// RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xBC, 0x038C },	// GREEK CAPITAL LETTER OMICRON WITH TONOS
				{ 0xBD, 0x00BD },	// VULGAR FRACTION ONE HALF
				{ 0xBE, 0x038E },	// GREEK CAPITAL LETTER UPSILON WITH TONOS
				{ 0xBF, 0x038F },	// GREEK CAPITAL LETTER OMEGA WITH TONOS
				{ 0xC0, 0x0390 },	// GREEK SMALL LETTER IOTA WITH DIALYTIKA AND TONOS
				{ 0xC1, 0x0391 },	// GREEK CAPITAL LETTER ALPHA
				{ 0xC2, 0x0392 },	// GREEK CAPITAL LETTER BETA
				{ 0xC3, 0x0393 },	// GREEK CAPITAL LETTER GAMMA
				{ 0xC4, 0x0394 },	// GREEK CAPITAL LETTER DELTA
				{ 0xC5, 0x0395 },	// GREEK CAPITAL LETTER EPSILON
				{ 0xC6, 0x0396 },	// GREEK CAPITAL LETTER ZETA
				{ 0xC7, 0x0397 },	// GREEK CAPITAL LETTER ETA
				{ 0xC8, 0x0398 },	// GREEK CAPITAL LETTER THETA
				{ 0xC9, 0x0399 },	// GREEK CAPITAL LETTER IOTA
				{ 0xCA, 0x039A },	// GREEK CAPITAL LETTER KAPPA
				{ 0xCB, 0x039B },	// GREEK CAPITAL LETTER LAMDA
				{ 0xCC, 0x039C },	// GREEK CAPITAL LETTER MU
				{ 0xCD, 0x039D },	// GREEK CAPITAL LETTER NU
				{ 0xCE, 0x039E },	// GREEK CAPITAL LETTER XI
				{ 0xCF, 0x039F },	// GREEK CAPITAL LETTER OMICRON
				{ 0xD0, 0x03A0 },	// GREEK CAPITAL LETTER PI
				{ 0xD1, 0x03A1 },	// GREEK CAPITAL LETTER RHO
				{ 0xD3, 0x03A3 },	// GREEK CAPITAL LETTER SIGMA
				{ 0xD4, 0x03A4 },	// GREEK CAPITAL LETTER TAU
				{ 0xD5, 0x03A5 },	// GREEK CAPITAL LETTER UPSILON
				{ 0xD6, 0x03A6 },	// GREEK CAPITAL LETTER PHI
				{ 0xD7, 0x03A7 },	// GREEK CAPITAL LETTER CHI
				{ 0xD8, 0x03A8 },	// GREEK CAPITAL LETTER PSI
				{ 0xD9, 0x03A9 },	// GREEK CAPITAL LETTER OMEGA
				{ 0xDA, 0x03AA },	// GREEK CAPITAL LETTER IOTA WITH DIALYTIKA
				{ 0xDB, 0x03AB },	// GREEK CAPITAL LETTER UPSILON WITH DIALYTIKA
				{ 0xDC, 0x03AC },	// GREEK SMALL LETTER ALPHA WITH TONOS
				{ 0xDD, 0x03AD },	// GREEK SMALL LETTER EPSILON WITH TONOS
				{ 0xDE, 0x03AE },	// GREEK SMALL LETTER ETA WITH TONOS
				{ 0xDF, 0x03AF },	// GREEK SMALL LETTER IOTA WITH TONOS
				{ 0xE0, 0x03B0 },	// GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND TONOS
				{ 0xE1, 0x03B1 },	// GREEK SMALL LETTER ALPHA
				{ 0xE2, 0x03B2 },	// GREEK SMALL LETTER BETA
				{ 0xE3, 0x03B3 },	// GREEK SMALL LETTER GAMMA
				{ 0xE4, 0x03B4 },	// GREEK SMALL LETTER DELTA
				{ 0xE5, 0x03B5 },	// GREEK SMALL LETTER EPSILON
				{ 0xE6, 0x03B6 },	// GREEK SMALL LETTER ZETA
				{ 0xE7, 0x03B7 },	// GREEK SMALL LETTER ETA
				{ 0xE8, 0x03B8 },	// GREEK SMALL LETTER THETA
				{ 0xE9, 0x03B9 },	// GREEK SMALL LETTER IOTA
				{ 0xEA, 0x03BA },	// GREEK SMALL LETTER KAPPA
				{ 0xEB, 0x03BB },	// GREEK SMALL LETTER LAMDA
				{ 0xEC, 0x03BC },	// GREEK SMALL LETTER MU
				{ 0xED, 0x03BD },	// GREEK SMALL LETTER NU
				{ 0xEE, 0x03BE },	// GREEK SMALL LETTER XI
				{ 0xEF, 0x03BF },	// GREEK SMALL LETTER OMICRON
				{ 0xF0, 0x03C0 },	// GREEK SMALL LETTER PI
				{ 0xF1, 0x03C1 },	// GREEK SMALL LETTER RHO
				{ 0xF2, 0x03C2 },	// GREEK SMALL LETTER FINAL SIGMA
				{ 0xF3, 0x03C3 },	// GREEK SMALL LETTER SIGMA
				{ 0xF4, 0x03C4 },	// GREEK SMALL LETTER TAU
				{ 0xF5, 0x03C5 },	// GREEK SMALL LETTER UPSILON
				{ 0xF6, 0x03C6 },	// GREEK SMALL LETTER PHI
				{ 0xF7, 0x03C7 },	// GREEK SMALL LETTER CHI
				{ 0xF8, 0x03C8 },	// GREEK SMALL LETTER PSI
				{ 0xF9, 0x03C9 },	// GREEK SMALL LETTER OMEGA
				{ 0xFA, 0x03CA },	// GREEK SMALL LETTER IOTA WITH DIALYTIKA
				{ 0xFB, 0x03CB },	// GREEK SMALL LETTER UPSILON WITH DIALYTIKA
				{ 0xFC, 0x03CC },	// GREEK SMALL LETTER OMICRON WITH TONOS
				{ 0xFD, 0x03CD },	// GREEK SMALL LETTER UPSILON WITH TONOS
				{ 0xFE, 0x03CE },	// GREEK SMALL LETTER OMEGA WITH TONOS
			};
			#endregion

			public ISO_8859_7Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class ISO_8859_8Encoding : ISO_8859_Base
		{
			#region ISO 8859-8 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA2, 0x00A2 },	// CENT SIGN
				{ 0xA3, 0x00A3 },	// POUND SIGN
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xA5, 0x00A5 },	// YEN SIGN
				{ 0xA6, 0x00A6 },	// BROKEN BAR
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x00A9 },	// COPYRIGHT SIGN
				{ 0xAA, 0x00D7 },	// MULTIPLICATION SIGN
				{ 0xAB, 0x00AB },	// LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xAC, 0x00AC },	// NOT SIGN
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x00AE },	// REGISTERED SIGN
				{ 0xAF, 0x203E },	// OVERLINE
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x00B1 },	// PLUS-MINUS SIGN
				{ 0xB2, 0x00B2 },	// SUPERSCRIPT TWO
				{ 0xB3, 0x00B3 },	// SUPERSCRIPT THREE
				{ 0xB4, 0x00B4 },	// ACUTE ACCENT
				{ 0xB5, 0x00B5 },	// MICRO SIGN
				{ 0xB6, 0x00B6 },	// PILCROW SIGN
				{ 0xB7, 0x00B7 },	// MIDDLE DOT
				{ 0xB8, 0x00B8 },	// CEDILLA
				{ 0xB9, 0x00B9 },	// SUPERSCRIPT ONE
				{ 0xBA, 0x00F7 },	// DIVISION SIGN
				{ 0xBB, 0x00BB },	// RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xBC, 0x00BC },	// VULGAR FRACTION ONE QUARTER
				{ 0xBD, 0x00BD },	// VULGAR FRACTION ONE HALF
				{ 0xBE, 0x00BE },	// VULGAR FRACTION THREE QUARTERS
				{ 0xDF, 0x2017 },	// DOUBLE LOW LINE
				{ 0xE0, 0x05D0 },	// HEBREW LETTER ALEF
				{ 0xE1, 0x05D1 },	// HEBREW LETTER BET
				{ 0xE2, 0x05D2 },	// HEBREW LETTER GIMEL
				{ 0xE3, 0x05D3 },	// HEBREW LETTER DALET
				{ 0xE4, 0x05D4 },	// HEBREW LETTER HE
				{ 0xE5, 0x05D5 },	// HEBREW LETTER VAV
				{ 0xE6, 0x05D6 },	// HEBREW LETTER ZAYIN
				{ 0xE7, 0x05D7 },	// HEBREW LETTER HET
				{ 0xE8, 0x05D8 },	// HEBREW LETTER TET
				{ 0xE9, 0x05D9 },	// HEBREW LETTER YOD
				{ 0xEA, 0x05DA },	// HEBREW LETTER FINAL KAF
				{ 0xEB, 0x05DB },	// HEBREW LETTER KAF
				{ 0xEC, 0x05DC },	// HEBREW LETTER LAMED
				{ 0xED, 0x05DD },	// HEBREW LETTER FINAL MEM
				{ 0xEE, 0x05DE },	// HEBREW LETTER MEM
				{ 0xEF, 0x05DF },	// HEBREW LETTER FINAL NUN
				{ 0xF0, 0x05E0 },	// HEBREW LETTER NUN
				{ 0xF1, 0x05E1 },	// HEBREW LETTER SAMEKH
				{ 0xF2, 0x05E2 },	// HEBREW LETTER AYIN
				{ 0xF3, 0x05E3 },	// HEBREW LETTER FINAL PE
				{ 0xF4, 0x05E4 },	// HEBREW LETTER PE
				{ 0xF5, 0x05E5 },	// HEBREW LETTER FINAL TSADI
				{ 0xF6, 0x05E6 },	// HEBREW LETTER TSADI
				{ 0xF7, 0x05E7 },	// HEBREW LETTER QOF
				{ 0xF8, 0x05E8 },	// HEBREW LETTER RESH
				{ 0xF9, 0x05E9 },	// HEBREW LETTER SHIN
				{ 0xFA, 0x05EA },	// HEBREW LETTER TAV
			};
			#endregion

			public ISO_8859_8Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class ISO_8859_9Encoding : ISO_8859_Base
		{
			#region ISO 8859-9 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x00A1 },	// INVERTED EXCLAMATION MARK
				{ 0xA2, 0x00A2 },	// CENT SIGN
				{ 0xA3, 0x00A3 },	// POUND SIGN
				{ 0xA4, 0x00A4 },	// CURRENCY SIGN
				{ 0xA5, 0x00A5 },	// YEN SIGN
				{ 0xA6, 0x00A6 },	// BROKEN BAR
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x00A8 },	// DIAERESIS
				{ 0xA9, 0x00A9 },	// COPYRIGHT SIGN
				{ 0xAA, 0x00AA },	// FEMININE ORDINAL INDICATOR
				{ 0xAB, 0x00AB },	// LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xAC, 0x00AC },	// NOT SIGN
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x00AE },	// REGISTERED SIGN
				{ 0xAF, 0x00AF },	// MACRON
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x00B1 },	// PLUS-MINUS SIGN
				{ 0xB2, 0x00B2 },	// SUPERSCRIPT TWO
				{ 0xB3, 0x00B3 },	// SUPERSCRIPT THREE
				{ 0xB4, 0x00B4 },	// ACUTE ACCENT
				{ 0xB5, 0x00B5 },	// MICRO SIGN
				{ 0xB6, 0x00B6 },	// PILCROW SIGN
				{ 0xB7, 0x00B7 },	// MIDDLE DOT
				{ 0xB8, 0x00B8 },	// CEDILLA
				{ 0xB9, 0x00B9 },	// SUPERSCRIPT ONE
				{ 0xBA, 0x00BA },	// MASCULINE ORDINAL INDICATOR
				{ 0xBB, 0x00BB },	// RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
				{ 0xBC, 0x00BC },	// VULGAR FRACTION ONE QUARTER
				{ 0xBD, 0x00BD },	// VULGAR FRACTION ONE HALF
				{ 0xBE, 0x00BE },	// VULGAR FRACTION THREE QUARTERS
				{ 0xBF, 0x00BF },	// INVERTED QUESTION MARK
				{ 0xC0, 0x00C0 },	// LATIN CAPITAL LETTER A WITH GRAVE
				{ 0xC1, 0x00C1 },	// LATIN CAPITAL LETTER A WITH ACUTE
				{ 0xC2, 0x00C2 },	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				{ 0xC3, 0x00C3 },	// LATIN CAPITAL LETTER A WITH TILDE
				{ 0xC4, 0x00C4 },	// LATIN CAPITAL LETTER A WITH DIAERESIS
				{ 0xC5, 0x00C5 },	// LATIN CAPITAL LETTER A WITH RING ABOVE
				{ 0xC6, 0x00C6 },	// LATIN CAPITAL LETTER AE
				{ 0xC7, 0x00C7 },	// LATIN CAPITAL LETTER C WITH CEDILLA
				{ 0xC8, 0x00C8 },	// LATIN CAPITAL LETTER E WITH GRAVE
				{ 0xC9, 0x00C9 },	// LATIN CAPITAL LETTER E WITH ACUTE
				{ 0xCA, 0x00CA },	// LATIN CAPITAL LETTER E WITH CIRCUMFLEX
				{ 0xCB, 0x00CB },	// LATIN CAPITAL LETTER E WITH DIAERESIS
				{ 0xCC, 0x00CC },	// LATIN CAPITAL LETTER I WITH GRAVE
				{ 0xCD, 0x00CD },	// LATIN CAPITAL LETTER I WITH ACUTE
				{ 0xCE, 0x00CE },	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				{ 0xCF, 0x00CF },	// LATIN CAPITAL LETTER I WITH DIAERESIS
				{ 0xD0, 0x011E },	// LATIN CAPITAL LETTER G WITH BREVE
				{ 0xD1, 0x00D1 },	// LATIN CAPITAL LETTER N WITH TILDE
				{ 0xD2, 0x00D2 },	// LATIN CAPITAL LETTER O WITH GRAVE
				{ 0xD3, 0x00D3 },	// LATIN CAPITAL LETTER O WITH ACUTE
				{ 0xD4, 0x00D4 },	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				{ 0xD5, 0x00D5 },	// LATIN CAPITAL LETTER O WITH TILDE
				{ 0xD6, 0x00D6 },	// LATIN CAPITAL LETTER O WITH DIAERESIS
				{ 0xD7, 0x00D7 },	// MULTIPLICATION SIGN
				{ 0xD8, 0x00D8 },	// LATIN CAPITAL LETTER O WITH STROKE
				{ 0xD9, 0x00D9 },	// LATIN CAPITAL LETTER U WITH GRAVE
				{ 0xDA, 0x00DA },	// LATIN CAPITAL LETTER U WITH ACUTE
				{ 0xDB, 0x00DB },	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX
				{ 0xDC, 0x00DC },	// LATIN CAPITAL LETTER U WITH DIAERESIS
				{ 0xDD, 0x0130 },	// LATIN CAPITAL LETTER I WITH DOT ABOVE
				{ 0xDE, 0x015E },	// LATIN CAPITAL LETTER S WITH CEDILLA
				{ 0xDF, 0x00DF },	// LATIN SMALL LETTER SHARP S
				{ 0xE0, 0x00E0 },	// LATIN SMALL LETTER A WITH GRAVE
				{ 0xE1, 0x00E1 },	// LATIN SMALL LETTER A WITH ACUTE
				{ 0xE2, 0x00E2 },	// LATIN SMALL LETTER A WITH CIRCUMFLEX
				{ 0xE3, 0x00E3 },	// LATIN SMALL LETTER A WITH TILDE
				{ 0xE4, 0x00E4 },	// LATIN SMALL LETTER A WITH DIAERESIS
				{ 0xE5, 0x00E5 },	// LATIN SMALL LETTER A WITH RING ABOVE
				{ 0xE6, 0x00E6 },	// LATIN SMALL LETTER AE
				{ 0xE7, 0x00E7 },	// LATIN SMALL LETTER C WITH CEDILLA
				{ 0xE8, 0x00E8 },	// LATIN SMALL LETTER E WITH GRAVE
				{ 0xE9, 0x00E9 },	// LATIN SMALL LETTER E WITH ACUTE
				{ 0xEA, 0x00EA },	// LATIN SMALL LETTER E WITH CIRCUMFLEX
				{ 0xEB, 0x00EB },	// LATIN SMALL LETTER E WITH DIAERESIS
				{ 0xEC, 0x00EC },	// LATIN SMALL LETTER I WITH GRAVE
				{ 0xED, 0x00ED },	// LATIN SMALL LETTER I WITH ACUTE
				{ 0xEE, 0x00EE },	// LATIN SMALL LETTER I WITH CIRCUMFLEX
				{ 0xEF, 0x00EF },	// LATIN SMALL LETTER I WITH DIAERESIS
				{ 0xF0, 0x011F },	// LATIN SMALL LETTER G WITH BREVE
				{ 0xF1, 0x00F1 },	// LATIN SMALL LETTER N WITH TILDE
				{ 0xF2, 0x00F2 },	// LATIN SMALL LETTER O WITH GRAVE
				{ 0xF3, 0x00F3 },	// LATIN SMALL LETTER O WITH ACUTE
				{ 0xF4, 0x00F4 },	// LATIN SMALL LETTER O WITH CIRCUMFLEX
				{ 0xF5, 0x00F5 },	// LATIN SMALL LETTER O WITH TILDE
				{ 0xF6, 0x00F6 },	// LATIN SMALL LETTER O WITH DIAERESIS
				{ 0xF7, 0x00F7 },	// DIVISION SIGN
				{ 0xF8, 0x00F8 },	// LATIN SMALL LETTER O WITH STROKE
				{ 0xF9, 0x00F9 },	// LATIN SMALL LETTER U WITH GRAVE
				{ 0xFA, 0x00FA },	// LATIN SMALL LETTER U WITH ACUTE
				{ 0xFB, 0x00FB },	// LATIN SMALL LETTER U WITH CIRCUMFLEX
				{ 0xFC, 0x00FC },	// LATIN SMALL LETTER U WITH DIAERESIS
				{ 0xFD, 0x0131 },	// LATIN SMALL LETTER DOTLESS I
				{ 0xFE, 0x015F },	// LATIN SMALL LETTER S WITH CEDILLA
				{ 0xFF, 0x00FF },	// LATIN SMALL LETTER Y WITH DIAERESIS
			};
			#endregion

			public ISO_8859_9Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class ISO_8859_10Encoding : ISO_8859_Base
		{
			#region ISO 8859-10 to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
				{ 0x20, 0x0020 },	// SPACE
				{ 0x21, 0x0021 },	// EXCLAMATION MARK
				{ 0x22, 0x0022 },	// QUOTATION MARK
				{ 0x23, 0x0023 },	// NUMBER SIGN
				{ 0x24, 0x0024 },	// DOLLAR SIGN
				{ 0x25, 0x0025 },	// PERCENT SIGN
				{ 0x26, 0x0026 },	// AMPERSAND
				{ 0x27, 0x0027 },	// APOSTROPHE
				{ 0x28, 0x0028 },	// LEFT PARENTHESIS
				{ 0x29, 0x0029 },	// RIGHT PARENTHESIS
				{ 0x2A, 0x002A },	// ASTERISK
				{ 0x2B, 0x002B },	// PLUS SIGN
				{ 0x2C, 0x002C },	// COMMA
				{ 0x2D, 0x002D },	// HYPHEN-MINUS
				{ 0x2E, 0x002E },	// FULL STOP
				{ 0x2F, 0x002F },	// SOLIDUS
				{ 0x30, 0x0030 },	// DIGIT ZERO
				{ 0x31, 0x0031 },	// DIGIT ONE
				{ 0x32, 0x0032 },	// DIGIT TWO
				{ 0x33, 0x0033 },	// DIGIT THREE
				{ 0x34, 0x0034 },	// DIGIT FOUR
				{ 0x35, 0x0035 },	// DIGIT FIVE
				{ 0x36, 0x0036 },	// DIGIT SIX
				{ 0x37, 0x0037 },	// DIGIT SEVEN
				{ 0x38, 0x0038 },	// DIGIT EIGHT
				{ 0x39, 0x0039 },	// DIGIT NINE
				{ 0x3A, 0x003A },	// COLON
				{ 0x3B, 0x003B },	// SEMICOLON
				{ 0x3C, 0x003C },	// LESS-THAN SIGN
				{ 0x3D, 0x003D },	// EQUALS SIGN
				{ 0x3E, 0x003E },	// GREATER-THAN SIGN
				{ 0x3F, 0x003F },	// QUESTION MARK
				{ 0x40, 0x0040 },	// COMMERCIAL AT
				{ 0x41, 0x0041 },	// LATIN CAPITAL LETTER A
				{ 0x42, 0x0042 },	// LATIN CAPITAL LETTER B
				{ 0x43, 0x0043 },	// LATIN CAPITAL LETTER C
				{ 0x44, 0x0044 },	// LATIN CAPITAL LETTER D
				{ 0x45, 0x0045 },	// LATIN CAPITAL LETTER E
				{ 0x46, 0x0046 },	// LATIN CAPITAL LETTER F
				{ 0x47, 0x0047 },	// LATIN CAPITAL LETTER G
				{ 0x48, 0x0048 },	// LATIN CAPITAL LETTER H
				{ 0x49, 0x0049 },	// LATIN CAPITAL LETTER I
				{ 0x4A, 0x004A },	// LATIN CAPITAL LETTER J
				{ 0x4B, 0x004B },	// LATIN CAPITAL LETTER K
				{ 0x4C, 0x004C },	// LATIN CAPITAL LETTER L
				{ 0x4D, 0x004D },	// LATIN CAPITAL LETTER M
				{ 0x4E, 0x004E },	// LATIN CAPITAL LETTER N
				{ 0x4F, 0x004F },	// LATIN CAPITAL LETTER O
				{ 0x50, 0x0050 },	// LATIN CAPITAL LETTER P
				{ 0x51, 0x0051 },	// LATIN CAPITAL LETTER Q
				{ 0x52, 0x0052 },	// LATIN CAPITAL LETTER R
				{ 0x53, 0x0053 },	// LATIN CAPITAL LETTER S
				{ 0x54, 0x0054 },	// LATIN CAPITAL LETTER T
				{ 0x55, 0x0055 },	// LATIN CAPITAL LETTER U
				{ 0x56, 0x0056 },	// LATIN CAPITAL LETTER V
				{ 0x57, 0x0057 },	// LATIN CAPITAL LETTER W
				{ 0x58, 0x0058 },	// LATIN CAPITAL LETTER X
				{ 0x59, 0x0059 },	// LATIN CAPITAL LETTER Y
				{ 0x5A, 0x005A },	// LATIN CAPITAL LETTER Z
				{ 0x5B, 0x005B },	// LEFT SQUARE BRACKET
				{ 0x5C, 0x005C },	// REVERSE SOLIDUS
				{ 0x5D, 0x005D },	// RIGHT SQUARE BRACKET
				{ 0x5E, 0x005E },	// CIRCUMFLEX ACCENT
				{ 0x5F, 0x005F },	// LOW LINE
				{ 0x60, 0x0060 },	// GRAVE ACCENT
				{ 0x61, 0x0061 },	// LATIN SMALL LETTER A
				{ 0x62, 0x0062 },	// LATIN SMALL LETTER B
				{ 0x63, 0x0063 },	// LATIN SMALL LETTER C
				{ 0x64, 0x0064 },	// LATIN SMALL LETTER D
				{ 0x65, 0x0065 },	// LATIN SMALL LETTER E
				{ 0x66, 0x0066 },	// LATIN SMALL LETTER F
				{ 0x67, 0x0067 },	// LATIN SMALL LETTER G
				{ 0x68, 0x0068 },	// LATIN SMALL LETTER H
				{ 0x69, 0x0069 },	// LATIN SMALL LETTER I
				{ 0x6A, 0x006A },	// LATIN SMALL LETTER J
				{ 0x6B, 0x006B },	// LATIN SMALL LETTER K
				{ 0x6C, 0x006C },	// LATIN SMALL LETTER L
				{ 0x6D, 0x006D },	// LATIN SMALL LETTER M
				{ 0x6E, 0x006E },	// LATIN SMALL LETTER N
				{ 0x6F, 0x006F },	// LATIN SMALL LETTER O
				{ 0x70, 0x0070 },	// LATIN SMALL LETTER P
				{ 0x71, 0x0071 },	// LATIN SMALL LETTER Q
				{ 0x72, 0x0072 },	// LATIN SMALL LETTER R
				{ 0x73, 0x0073 },	// LATIN SMALL LETTER S
				{ 0x74, 0x0074 },	// LATIN SMALL LETTER T
				{ 0x75, 0x0075 },	// LATIN SMALL LETTER U
				{ 0x76, 0x0076 },	// LATIN SMALL LETTER V
				{ 0x77, 0x0077 },	// LATIN SMALL LETTER W
				{ 0x78, 0x0078 },	// LATIN SMALL LETTER X
				{ 0x79, 0x0079 },	// LATIN SMALL LETTER Y
				{ 0x7A, 0x007A },	// LATIN SMALL LETTER Z
				{ 0x7B, 0x007B },	// LEFT CURLY BRACKET
				{ 0x7C, 0x007C },	// VERTICAL LINE
				{ 0x7D, 0x007D },	// RIGHT CURLY BRACKET
				{ 0x7E, 0x007E },	// TILDE
				{ 0xA0, 0x00A0 },	// NO-BREAK SPACE
				{ 0xA1, 0x0104 },	// LATIN CAPITAL LETTER A WITH OGONEK
				{ 0xA2, 0x0112 },	// LATIN CAPITAL LETTER E WITH MACRON
				{ 0xA3, 0x0122 },	// LATIN CAPITAL LETTER G WITH CEDILLA
				{ 0xA4, 0x012A },	// LATIN CAPITAL LETTER I WITH MACRON
				{ 0xA5, 0x0128 },	// LATIN CAPITAL LETTER I WITH TILDE
				{ 0xA6, 0x0136 },	// LATIN CAPITAL LETTER K WITH CEDILLA
				{ 0xA7, 0x00A7 },	// SECTION SIGN
				{ 0xA8, 0x013B },	// LATIN CAPITAL LETTER L WITH CEDILLA
				{ 0xA9, 0x0110 },	// LATIN CAPITAL LETTER D WITH STROKE
				{ 0xAA, 0x0160 },	// LATIN CAPITAL LETTER S WITH CARON
				{ 0xAB, 0x0166 },	// LATIN CAPITAL LETTER T WITH STROKE
				{ 0xAC, 0x017D },	// LATIN CAPITAL LETTER Z WITH CARON
				{ 0xAD, 0x00AD },	// SOFT HYPHEN
				{ 0xAE, 0x016A },	// LATIN CAPITAL LETTER U WITH MACRON
				{ 0xAF, 0x014A },	// LATIN CAPITAL LETTER ENG
				{ 0xB0, 0x00B0 },	// DEGREE SIGN
				{ 0xB1, 0x0105 },	// LATIN SMALL LETTER A WITH OGONEK
				{ 0xB2, 0x0113 },	// LATIN SMALL LETTER E WITH MACRON
				{ 0xB3, 0x0123 },	// LATIN SMALL LETTER G WITH CEDILLA
				{ 0xB4, 0x012B },	// LATIN SMALL LETTER I WITH MACRON
				{ 0xB5, 0x0129 },	// LATIN SMALL LETTER I WITH TILDE
				{ 0xB6, 0x0137 },	// LATIN SMALL LETTER K WITH CEDILLA
				{ 0xB7, 0x00B7 },	// MIDDLE DOT
				{ 0xB8, 0x013C },	// LATIN SMALL LETTER L WITH CEDILLA
				{ 0xB9, 0x0111 },	// LATIN SMALL LETTER D WITH STROKE
				{ 0xBA, 0x0161 },	// LATIN SMALL LETTER S WITH CARON
				{ 0xBB, 0x0167 },	// LATIN SMALL LETTER T WITH STROKE
				{ 0xBC, 0x017E },	// LATIN SMALL LETTER Z WITH CARON
				{ 0xBD, 0x2015 },	// HORIZONTAL BAR
				{ 0xBE, 0x016B },	// LATIN SMALL LETTER U WITH MACRON
				{ 0xBF, 0x014B },	// LATIN SMALL LETTER ENG
				{ 0xC0, 0x0100 },	// LATIN CAPITAL LETTER A WITH MACRON
				{ 0xC1, 0x00C1 },	// LATIN CAPITAL LETTER A WITH ACUTE
				{ 0xC2, 0x00C2 },	// LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				{ 0xC3, 0x00C3 },	// LATIN CAPITAL LETTER A WITH TILDE
				{ 0xC4, 0x00C4 },	// LATIN CAPITAL LETTER A WITH DIAERESIS
				{ 0xC5, 0x00C5 },	// LATIN CAPITAL LETTER A WITH RING ABOVE
				{ 0xC6, 0x00C6 },	// LATIN CAPITAL LETTER AE
				{ 0xC7, 0x012E },	// LATIN CAPITAL LETTER I WITH OGONEK
				{ 0xC8, 0x010C },	// LATIN CAPITAL LETTER C WITH CARON
				{ 0xC9, 0x00C9 },	// LATIN CAPITAL LETTER E WITH ACUTE
				{ 0xCA, 0x0118 },	// LATIN CAPITAL LETTER E WITH OGONEK
				{ 0xCB, 0x00CB },	// LATIN CAPITAL LETTER E WITH DIAERESIS
				{ 0xCC, 0x0116 },	// LATIN CAPITAL LETTER E WITH DOT ABOVE
				{ 0xCD, 0x00CD },	// LATIN CAPITAL LETTER I WITH ACUTE
				{ 0xCE, 0x00CE },	// LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				{ 0xCF, 0x00CF },	// LATIN CAPITAL LETTER I WITH DIAERESIS
				{ 0xD0, 0x00D0 },	// LATIN CAPITAL LETTER ETH
				{ 0xD1, 0x0145 },	// LATIN CAPITAL LETTER N WITH CEDILLA
				{ 0xD2, 0x014C },	// LATIN CAPITAL LETTER O WITH MACRON
				{ 0xD3, 0x00D3 },	// LATIN CAPITAL LETTER O WITH ACUTE
				{ 0xD4, 0x00D4 },	// LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				{ 0xD5, 0x00D5 },	// LATIN CAPITAL LETTER O WITH TILDE
				{ 0xD6, 0x00D6 },	// LATIN CAPITAL LETTER O WITH DIAERESIS
				{ 0xD7, 0x0168 },	// LATIN CAPITAL LETTER U WITH TILDE
				{ 0xD8, 0x00D8 },	// LATIN CAPITAL LETTER O WITH STROKE
				{ 0xD9, 0x0172 },	// LATIN CAPITAL LETTER U WITH OGONEK
				{ 0xDA, 0x00DA },	// LATIN CAPITAL LETTER U WITH ACUTE
				{ 0xDB, 0x00DB },	// LATIN CAPITAL LETTER U WITH CIRCUMFLEX
				{ 0xDC, 0x00DC },	// LATIN CAPITAL LETTER U WITH DIAERESIS
				{ 0xDD, 0x00DD },	// LATIN CAPITAL LETTER Y WITH ACUTE
				{ 0xDE, 0x00DE },	// LATIN CAPITAL LETTER THORN
				{ 0xDF, 0x00DF },	// LATIN SMALL LETTER SHARP S
				{ 0xE0, 0x0101 },	// LATIN SMALL LETTER A WITH MACRON
				{ 0xE1, 0x00E1 },	// LATIN SMALL LETTER A WITH ACUTE
				{ 0xE2, 0x00E2 },	// LATIN SMALL LETTER A WITH CIRCUMFLEX
				{ 0xE3, 0x00E3 },	// LATIN SMALL LETTER A WITH TILDE
				{ 0xE4, 0x00E4 },	// LATIN SMALL LETTER A WITH DIAERESIS
				{ 0xE5, 0x00E5 },	// LATIN SMALL LETTER A WITH RING ABOVE
				{ 0xE6, 0x00E6 },	// LATIN SMALL LETTER AE
				{ 0xE7, 0x012F },	// LATIN SMALL LETTER I WITH OGONEK
				{ 0xE8, 0x010D },	// LATIN SMALL LETTER C WITH CARON
				{ 0xE9, 0x00E9 },	// LATIN SMALL LETTER E WITH ACUTE
				{ 0xEA, 0x0119 },	// LATIN SMALL LETTER E WITH OGONEK
				{ 0xEB, 0x00EB },	// LATIN SMALL LETTER E WITH DIAERESIS
				{ 0xEC, 0x0117 },	// LATIN SMALL LETTER E WITH DOT ABOVE
				{ 0xED, 0x00ED },	// LATIN SMALL LETTER I WITH ACUTE
				{ 0xEE, 0x00EE },	// LATIN SMALL LETTER I WITH CIRCUMFLEX
				{ 0xEF, 0x00EF },	// LATIN SMALL LETTER I WITH DIAERESIS
				{ 0xF0, 0x00F0 },	// LATIN SMALL LETTER ETH
				{ 0xF1, 0x0146 },	// LATIN SMALL LETTER N WITH CEDILLA
				{ 0xF2, 0x014D },	// LATIN SMALL LETTER O WITH MACRON
				{ 0xF3, 0x00F3 },	// LATIN SMALL LETTER O WITH ACUTE
				{ 0xF4, 0x00F4 },	// LATIN SMALL LETTER O WITH CIRCUMFLEX
				{ 0xF5, 0x00F5 },	// LATIN SMALL LETTER O WITH TILDE
				{ 0xF6, 0x00F6 },	// LATIN SMALL LETTER O WITH DIAERESIS
				{ 0xF7, 0x0169 },	// LATIN SMALL LETTER U WITH TILDE
				{ 0xF8, 0x00F8 },	// LATIN SMALL LETTER O WITH STROKE
				{ 0xF9, 0x0173 },	// LATIN SMALL LETTER U WITH OGONEK
				{ 0xFA, 0x00FA },	// LATIN SMALL LETTER U WITH ACUTE
				{ 0xFB, 0x00FB },	// LATIN SMALL LETTER U WITH CIRCUMFLEX
				{ 0xFC, 0x00FC },	// LATIN SMALL LETTER U WITH DIAERESIS
				{ 0xFD, 0x00FD },	// LATIN SMALL LETTER Y WITH ACUTE
				{ 0xFE, 0x00FE },	// LATIN SMALL LETTER THORN
				{ 0xFF, 0x0138 },	// LATIN SMALL LETTER KRA
			};
			#endregion

			public ISO_8859_10Encoding()
			{
				CurrentEncodingTable = EncodingTable;
                CreateHashtable();
			}
		}

		public class UTF_8_Encoding : SystemEncoding
		{

			public UTF_8_Encoding()
			{
				UsedEncoding = System.Text.Encoding.UTF8;
			}
		}

		public class ISO_2022_JP_Encoding : SystemEncoding
		{
			public ISO_2022_JP_Encoding()
			{
				UsedEncoding = System.Text.Encoding.GetEncoding("iso-2022-jp");
			}
		}

		public class GB18030_Encoding : SystemEncoding
		{
			public GB18030_Encoding()
			{
				UsedEncoding = System.Text.Encoding.GetEncoding("GB18030");
			}
		}


		/*
		 * template for ISO class
		 * 
		public class ISO_8859_XEncoding : ISO_8859_Base
		{
			#region ISO 8859-X to UTF-16 mapping table

			private static readonly int[,] EncodingTable = new int[,] {
																	  };
			#endregion

			public ISO_8859_XEncoding()
			{
				CurrentEncodingTable = EncodingTable;
			}
		}
		*/

//	}
}
