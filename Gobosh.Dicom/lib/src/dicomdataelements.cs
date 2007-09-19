/* Gobosh.DICOM
 * Specific Data Element handling. Contains all classes to handle specific 
 * DataElement and their Value payload.
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
using System.Text;
using System.Collections;
using Gobosh;
using Gobosh.DICOM;

namespace Gobosh
{
	namespace DICOM
	{
		/// <summary>
		/// Summary description for dicomdataelements.
		/// </summary>
		namespace DataElements
		{
			#region Abstract Base Type DataElements like DataElementStringType

			public abstract class DataElementStringType : DataElement
			{
//				protected string Data;
                /// <summary>
                /// defines if the DataElement allows multiple values. 
                /// If false, the 0x5C ("\") is not used to split the values
                /// </summary>
				protected bool AllowMultiValue;
                /// <summary>
                /// defines the maximum length of a single value
                /// </summary>
				protected int MaxLengthOfValue;

                /// <summary>
                /// defines if the value shall be padded by 0x20, otherwise
                /// the value shall be padded with 0x00.
                /// </summary>
                /// <remarks>Everything but the UI VR is being padded with blanks, UI shall
                /// be padded with 0x00 (see PS3.5, Table 6.2.1, page 30)</remarks>
                protected bool padWithBlank;

                // TODO: introduce a flag wether the string is allowed to be trimmed or not (UT shall not be trimmed on leading spaces)

				public DataElementStringType(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// default constructor
					AllowMultiValue = true;					
					MaxLengthOfValue = 0;		// Unlimited
                    padWithBlank = true;        // default is padding with 0x00
				}

                #region DataElement Interface to be overridden by descendants

                /// <summary>
                /// A overridable factory function to create the appropriate Value
                /// Element objects.
                /// </summary>
                /// <param name="Value">A string representing the value</param>
                /// <returns>an Value Element object</returns>
                public override object CreateValueElement(string Value)
                {
                    return new DataElementStringValue(Value);
                }

                /// <summary>
                /// An overridable factory function to create the appropriate
                /// Value Element object used as Value in this DataElement.
                /// Use this function to create an default Value object and
                /// access the properties to change it.
                /// </summary>
                /// <returns>A default Value Element object</returns>
                public override object CreateValueElement()
                {
                    return new DataElementStringValue();
                }

                #endregion

				/// <summary>
				/// Decodes the RawData buffer using the given endianess
				/// </summary>
				/// <param name="isLittleEndian">flag if data is little endian</param>
				public override void Decode(bool isLittleEndian)
				{
                    string MyData = Utils.DecodeString(RawData, ValueLength, this.UsedEncoding);
                    string[] StringValues;

                    if (AllowMultiValue)
                    {
                        StringValues = MyData.Split('\\');
                    }
                    else
                    {
                        StringValues = new string[1];
                        StringValues[0] = MyData;
                    }

                    // throw away old values
                    PurgeValues();
                    // prepare for new values
                    PrepareForAddValue();

                    foreach (string v in StringValues)
                    {
                        DoAddValue(v);
                    }
                    if ((!AllowMultiValue) && (Values.Count > 1))
                    {
                        // TODO: Emit Warning: This element does not allow multiple values
                        AddWarning(8, "this data element does not allow multiple values");
                        // throw new Exception("this data element does not allow multiple values");
                    }

					// call te base function for handling of eventing and states
					base.Decode(isLittleEndian);
				}

                protected override void PrepareRawBuffer()
                {
                    // assign new valuelength
                    StringBuilder n = new StringBuilder();
                    foreach (DataElementValue k in Values)
                    {
                        string myValue = k.GetValueAsString();
                        n.Append(myValue);
                        n.Append('\\');
                    }
                    // remove the trailing "\\"
                    n.Remove(n.Length - 1, 1);

                    bool hasBeenPadded = false;
                    // if uneven                    
                    if ((n.Length % 2) != 0)
                    {
                        hasBeenPadded = true;
                        n.Append(' ');
                    }
                    this.RawData = Utils.EncodeString(n.ToString(), UsedEncoding);

                    // update the trailing space by 0x00 if necessary
                    // this is because we can not append a 0x00 character to the string type
                    if (hasBeenPadded && !padWithBlank)
                    {
                        this.RawData[this.RawData.Length - 1] = 0x00;
                    }

                    this.ValueLength = RawData.Length;
                    
                }

                public override string GetHumanReadableString()
				{
					StringBuilder result = new StringBuilder(200);

					int i,j;
					j = GetValueCount();	// read it only once, it is not that cheap
					for ( i = 0; i < j ; i++ )
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
					if ( result.Length > 1 )
					{
						result.Remove(result.Length-1,1);
					}

                    return result.ToString();
				}
			}

			#endregion

            #region specific implementations of the Data Elements distinguished by VR

            #region AE (Application Entity) (string type)

            /// <summary>
			/// AE (Application Entity) Data Element Object
			/// Contains the name of the last application
			/// that modified the DataSet.
			///
			/// Definition ( PS3.5-2006, Page 25, Table 6.2-1 )
			/// A string of characters that identifies an
			/// Application Entity with leading and trailing
			/// spaces (20H) being non-significant. A value
			/// consisting solely of spaces shall not be used.
			///
			/// Character Repertoire
			/// Default Character Repertoire excluding character
			/// code 5CH (the BACKSLASH “\” in ISO-IR 6), and
			/// control characters LF, FF, CR and ESC.
			///
			/// Length of Value
			/// 16 bytes maximum
			/// </summary>
			public class AE : DataElementStringType
			{
				public AE(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// AE is only a single value
					AllowMultiValue = true;		// multi values are allowed, despite one could think it is not
					MaxLengthOfValue = 16;		// Length of Value maximum
				}
            }

            #endregion

            #region AS (special type)
            /// <summary>
			/// AS (Age String) Data Element Object
			/// Contains the age of a person (Patient)
			///
			/// Definition ( PS3.5-2006, Page 25, Table 6.2-1)
			/// A string of characters with one of the following
			/// formats -- nnnD, nnnW, nnnM, nnnY; where
			/// nnn shall contain the number of days for D,
			/// weeks for W, months for M, or years for Y.
			/// Example: “018M” would represent an age of
			/// 18 months.
			///
			/// Character Repertoire
			/// “0”-”9”, “D”, “W”,
			/// “M”, “Y” of Default
			/// Character
			/// Repertoire
			///
			/// Length of Value
			/// 4 bytes fixed
			/// </summary>
			public class AS : DataElementStringType
			{
				public AS(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed,isLittleEndian)
				{
					// AS Constructor
					AllowMultiValue = false;
					MaxLengthOfValue = 4;
				}

                protected override void PrepareRawBuffer()
                {
                    // encode all values we have. New length is valuecount times 4
                    int newLength = this.Values.Count * MaxLengthOfValue;
                    this.ValueLength = newLength;

                    // generate a new buffer
                    this.RawData = new byte[newLength];
                    StringBuilder z = new StringBuilder(newLength);
                    // iterate over the values
                    for (int i = 0; i < Values.Count; i++)
                    {
                        string unit;
                       
                        DataElementAgeStringValue k = (DataElementAgeStringValue)(Values[i]);
                        z.Append(k.GetProperty("AgeNumber").AsInteger().ToString("000", System.Globalization.CultureInfo.InvariantCulture));
                        unit = k.GetProperty("AgeUnit").AsString();
                        switch (unit)
                        {
                            case "Days":
                                z.Append('D');
                                break;
                            case "Weeks":
                                z.Append('W');
                                break;
                            case "Months":
                                z.Append('M');
                                break;
                            case "Years":
                                z.Append('Y');
                                break;
                            default:
                                z.Append('Y');
                                this.AddWarning(5, "Illegal unit!");
                                break;
                        }
                    }
                    if (z.Length != newLength)
                    {
                        throw new Exception("internal error in AS value encoding");
                    }
                    for (int i = 0; i < z.Length; i++)
                    {
                        this.RawData[i] = (byte) (z[i]);
                    }
                    this.ValueLength = newLength;
                }

                public override void Decode(bool isLittleEndian)
				{
                    string myValue;

					if ( ValueLength == 4)
					{
                        ISO_8859_1Encoding e = new ISO_8859_1Encoding();                        
                        myValue = e.ToUTF16(RawData, 0, 4);
                        e = null;
					}
					else
					{
                        // none of the data dictionaries defines a VM>1 for VR AS
                        this.AddWarning(5,"Illegal age string format or VM > 1");
                        myValue = "000Y";
						// TODO: Emit a warning or throw an exception for illegal (AS) Age String
					}
                    PurgeValues();
                    PrepareForAddValue();
                    Values.Add(new DataElementAgeStringValue(myValue));
                    this.DataValueState = DataState.IsRawAndDecoded;
				}

                /// <summary>
                /// An overridable factory function to create the appropriate
                /// Value Element object used as Value in this DataElement.
                /// </summary>
                /// <returns>A default Value Element object</returns>
                public override object CreateValueElement(string Value)
                {
                    return new DataElementAgeStringValue(Value); /**** jetzt oben einsetzen in Decode */
                }


                /// <summary>
                /// An overridable factory function to create the appropriate
                /// Value Element object used as Value in this DataElement.
                /// Use this function to create an default Value object and
                /// access the properties to change it.
                /// </summary>
                /// <returns>A default Value Element object</returns>
                public override object CreateValueElement()
                {
                    return new DataElementAgeStringValue();
                }

            }
            #endregion

            #region AT (Attribute Tag) ( Special Implementation ) (*)
            /// <summary>
			/// AT (Attribute Tag) Data Element Object
			///		Seems to be used to point to certain 
			///
			/// Definition ( PS3.5-2006, Page 25, Table 6.2-1)
			///		Ordered pair of 16-bit unsigned integers that is
			///		the value of a Data Element Tag.
			///		Example: A Data Element Tag of (0018,00FF)
			///		would be encoded as a series of 4 bytes in a
			///		Little-Endian Transfer Syntax as
			///		18H,00H,FFH,00H and in a Big-Endian
			///		Transfer Syntax as 00H,18H,00H,FFH.
			///		
			///		Note: The encoding of an AT value is exactly
			///		the same as the encoding of a Data
			///		Element Tag as defined in Section 7.
			///
			/// Character Repertoire
			///		not applicable
			///
			/// Length of Value
			///		4 bytes fixed (multiple VMs possible)
			/// </summary>
			public class AT : DataElement
			{
				public AT(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// AT Constructor
				}

                /// <summary>
                /// Decodes 4 byte group/number attributes
                /// </summary>
                /// <param name="isLittleEndian">true if data is little endian encoded</param>
                public override void Decode(bool isLittleEndian)
                {
                    int i,g,e;

                    PurgeValues();
                    PrepareForAddValue();

                    for (i = 0; i < ValueLength; i += 4)
                    {
                        g = Utils.ReadUInt16(RawData, i, isLittleEndian);
                        e = Utils.ReadUInt16(RawData, i + 2, isLittleEndian);
                        Values.Add(new DataElementATValue(g, e));
                    }
                    DataValueState = DataState.IsRawAndDecoded;
                }

                /// <summary>
                /// encodes the raw buffer for AT values
                /// </summary>
                protected override void PrepareRawBuffer()
                {
                    // create a new buffer
                    // the size is the number of encoded attribute values
                    // times 4 (a 16bit unsigned integer pair => 4 bytes)
                    int newBufferSize = this.Values.Count * 4;
                    if (newBufferSize > 0)
                    {
                        this.RawData = new byte[this.Values.Count * 4];
                        int index = 0;
                        // iterate through the Values
                        foreach (DataElementATValue a in Values)
                        {
                            index = a.WriteToRawBuffer(RawData, index, IsLittleEndian);
                        }
                    }
                    else
                    {
                        this.RawData = null;
                    }
                    ValueLength = newBufferSize;
                }


                /// <summary>
                /// An overridable factory function to create the appropriate
                /// Value Element object used as Value in this DataElement.
                /// Use this function to create an default Value object and
                /// access the properties to change it.
                /// </summary>
                /// <returns>A default Value Element object</returns>
                public override object CreateValueElement()
                {
                    return new DataElementATValue();
                }

                public override string GetHumanReadableString()
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

            }
            #endregion

            #region CS (Code String) (string type)
            /// <summary>
            /// CS (Code String) Data Element Object
            ///		a short code string used for types 
            ///		and identifiers
            ///
            /// Definition ( PS3.5-2006, Page 25, Table 6.2-1)
            ///		A string of characters with leading or trailing
            ///		spaces (20H) being non-significant.
            ///
            /// Character Repertoire
            ///		Uppercase characters, “0”-”9”, the SPACE
            ///		character, and underscore “_”, of the
            ///		Default Character Repertoire
            ///
            /// Length of Value
            ///		16 bytes maximum
            /// </summary>
            public class CS : DataElementStringType
			{
				public CS(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// CS Constructor
					AllowMultiValue = true;
					MaxLengthOfValue = 16;
				}
            }

            #endregion

            #region DA (Date) (date type)
            /// <summary>
			/// DA (Date) Data Element Object
			///
			/// Definition ( PS3.5-2006, Page 25, Table 6.2-1)
			/// A string of characters of the format yyyymmdd;
			/// where yyyy shall contain year, mm shall contain the month,
			/// and dd shall contain the day. This conforms to
			/// the ANSI HISPP MSDS Date common data type.
			///
			/// Example:
			/// “19930822” would represent August 22, 1993.
			///
			/// Notes: 1. For reasons of backward compatibility with
			/// versions of this standard prior to V3.0, it is recommended
			/// that implementations also support a string of characters of
			/// the format yyyy.mm.dd for this VR.
			///
			/// Character Repertoire
			/// “0”-”9” of Default Character Repertoire
			/// Note: For reasons specified in the previous column,
			/// implementations may wish to support the “.” character
			/// as well.
			///
			/// Length of Value
			/// 8 bytes fixed (10bytes fixed if old ACR/NEMA format)
			///
			/// Note: For reasons specified in the previous columns,
			/// implementations may also wish to support a 10 byte
			/// fixed length as well.
			/// </summary>
            public class DA : DataElementStringType
			{
				public DA(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// DA Constructor
				}

                /// <summary>
                /// Decodes the RawData buffer using the given endianess
                /// </summary>
                /// <param name="isLittleEndian">flag if data is little endian</param>
                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    // decode the string
                    string myData = Utils.DecodeString(RawData, ValueLength, UsedEncoding);
                    // split the multivalues

                    string[] myValues;
                    int elemSize = 10;
                    // check for "." characters. if present we have an old ACR/NEMA
                    // value with "yyyy.mm.dd" notation
                    if (myData.IndexOf('.') < 0)
                    {
                        elemSize = 8;
                    }
                    else
                    {
                        this.AddWarning(9, "DA is encoded oldfashioned." + elemSize.ToString());
                    }
                    // notation is "yyyymmdd", split it up to pieces of 8
                    int numElements = myData.Length / elemSize;

                    // check if valueLength matches our expectations
                    if ((myData.Length % elemSize) != 0)
                    {
                        this.AddWarning(5, "ValueLength does not match a multiple of " + elemSize.ToString());
                    }
                    
                    myValues = new string[numElements];
                    for (int i = 0; i < numElements; i ++)
                    {
                        myValues[i] = myData.Substring(i * elemSize, elemSize);
                    }

                    foreach (string val in myValues)
                    {
                        Values.Add(new DataElementDAValue(val));
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementDAValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementDAValue();
                }

                /// <summary>
                /// Writes all DA values to the raw data encoding and
                /// set ValueLength member.
                /// </summary>
                protected override void PrepareRawBuffer()
                {
                    // calculate the new size of the raw buffer
                    int newSize = Values.Count * 8;

                    // 8 bytes for each value (yyyymmdd)
                    this.RawData = new byte[newSize];

                    // iterating the values
                    for (int i = 0; i < Values.Count; i++)
                    {
                        // get the refs to an instance
                        DataElementDAValue k = (DataElementDAValue)(GetValue(i));
                        // write the raw encoding
                        k.WriteRawData(this.RawData, i * 8);
                    }
                    this.ValueLength = newSize;
                }
            }
            #endregion

            #region DS (Decimal String) (float & string type) (*)
            /// <summary>
			/// DS (Decimal String) Data Element Object
			///
			/// Definition ( PS3.5-2006, Page 26, Table 6.2-1)
			///		A string of characters representing either a
			///		fixed point number or a floating point number.
			///		A fixed point number shall contain only the
			///		characters 0-9 with an optional leading "+" or
			///		"-" and an optional "." to mark the decimal
			///		point. A floating point number shall be
			///		conveyed as defined in ANSI X3.9, with an "E"
			///		or "e" to indicate the start of the exponent.
			///		Decimal Strings may be padded with leading
			///		or trailing spaces. Embedded spaces are not
			///		allowed.
			///	
			///		Note: Data Elements with multiple values
			///		using this VR may not be properly
			///		encoded if Explicit-VR transfer syntax is
			///		used and the VL of this attribute
			///		exceeds 65534 bytes
			///
			/// Character Repertoire
			/// “0”-”9”, “+”, “-”, “E”, “e”, “." of
			/// Default Character Repertoire
			///
			/// Length of Value
			///		16 bytes maximum
			/// </summary>
			public class DS : DataElementStringType
			{
				public DS(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// DS Constructor
					this.AllowMultiValue = true;
					// (see PS3.5-2006, table 6.2-1, page 26)
					this.MaxLengthOfValue = 16;
				}

                /// <summary>
                /// A overridable factory function to create the appropriate Value
                /// Element objects.
                /// </summary>
                /// <param name="Value">A string representing the value</param>
                /// <returns>an Value Element object</returns>
                public override object CreateValueElement(string Value)
                {
					// examples are:
					// +384234.23234
					// -93093423.23423
					// +1231231398e-15
                    return new DataElementDoubleValue(Value);
                }

                /// <summary>
                /// An overridable factory function to create the appropriate
                /// Value Element object used as Value in this DataElement.
                /// Use this function to create an default Value object and
                /// access the properties to change it.
                /// </summary>
                /// <returns>A default Value Element object</returns>
                public override object CreateValueElement()
                {
                    return new DataElementDoubleValue();
                }
            }
            #endregion

            #region DT (Date Time) (*)
            /// <summary>
			/// DT (Date Time) Data Element Object
			///
			/// Definition ( PS3.5-2006, Page 26, Table 6.2-1)
			/// The Date Time common data type. Indicates a
			/// concatenated date-time ASCII string in the
			/// format: YYYYMMDDHHMMSS.FFFFFF&ZZZZ
			/// The components of this string, from left to
			/// right, are YYYY = Year, MM = Month, DD =
			/// Day, HH = Hour, MM = Minute, SS = Second,
			/// FFFFFF = Fractional Second, & = “+” or “-”,
			/// and ZZZZ = Hours and Minutes of offset.
			/// &ZZZZ is an optional suffix for plus/minus
			/// offset from Coordinated Universal Time. A
			/// component that is omitted from the string is
			/// termed a null component. Trailing null
			/// components of Date Time are ignored. Nontrailing
			/// null components are prohibited, given
			/// that the optional suffix is not considered as a
			/// component.
			///
			/// Note: For reasons of backward compatibility
			/// with versions of this standard prior to
			/// V3.0, many existing DICOM Data
			/// Elements use the separate DA and TM
			/// VRs. Standard and Private Data
			/// Elements defined in the future should
			/// use DT, when appropriate, to be more
			/// compliant with ANSI HISPP MSDS.
			///
			/// Character Repertoire
			/// "0"-"9", "+", "-", "." of Default Character Repertoire
			///
			/// Length of Value
			/// 26 bytes maximum
			/// </summary>
			public class DT : DataElementStringType
			{
				public DT(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// DT Constructor
				}

                public override object CreateValueElement(string Value)
                {
                    return new DataElementDTValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementDTValue();
                }
            }

            #endregion

            #region FL (Floating Point Single) (*)
            /// <summary>
			/// FL (Floating Point Single) Data Element Object
			///
			/// Definition ( PS3.5-2006, Page 26, Table 6.2-1)
			/// Single precision binary floating point number
			/// represented in IEEE 754:1985 32-bit Floating
			/// Point Number Format
			///
			/// Character Repertoire
			/// not applicable
			///
			/// Value of Length
			/// 4 bytes fixed
			/// </summary>
            /// <remarks>The internal float format of C# is compliant to IEEE 754:1985 standard. Tough Luck.</remarks>
			public class FL : DataElement
			{
				public FL(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// FL Constructor
				}

                public override void Decode(bool isLittleEndian)
                {
                    // base.Decode(isLittleEndian);
                    int i;
                    PurgeValues();
                    PrepareForAddValue();

                    for (i = 0; i < ValueLength; i += 4)
                    {
                        Values.Add(new DataElementFloatValue(Utils.ReadFloat(RawData, i, isLittleEndian)));
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }



                // TODO: Remove CreateValueElement() function or check if you need it
                public override object CreateValueElement(string Value)
                {
                    // throw new Exception("FL can not be based on string values");
                    return new DataElementFloatValue(Value);
                }

                public override object CreateValueElement()
                {
                    // throw new Exception("FL can not be based on string values");
                    return new DataElementFloatValue();
                }
            }
            #endregion

            #region FD (Floating Point Double) (*)
            /// <summary>
			/// FD (Floating Point Double) Data Element Object
			///
			/// Definition ( PS3.5-2006, Page 26, Table 6.2-1)
			/// Double precision binary floating point number
			/// represented in IEEE 754:1985 64-bit Floating
			/// Point Number Format
			///
			/// Character Repertoire
			/// not applicable
			///
			/// Value of Length
			/// 8 bytes fixed
			/// </summary>
			public class FD : DataElement
			{
				public FD(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// FD Constructor
				}

                public override void Decode(bool isLittleEndian)
                {
                    // base.Decode(isLittleEndian);
                    int i;
                    PurgeValues();
                    PrepareForAddValue();

                    for (i = 0; i < ValueLength; i += 8)
                    {
                        Values.Add(new DataElementFloatValue(Utils.ReadFloat(RawData, i, isLittleEndian)));
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementDoubleValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementDoubleValue();
                }

                public override string GetHumanReadableString()
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

            }
            #endregion

            #region IS (Integer String) (*)
            /// <summary>
            /// A string of characters representing an Integer
            /// in base-10 (decimal), shall contain only the
            /// characters 0 - 9, with an optional leading "+" or
            /// "-". It may be padded with leading and/or
            /// trailing spaces. Embedded spaces are not
            /// allowed.
            /// The integer, n, represented shall be in the
            /// range:
            ///     -2^31 lt n lt (2^31 - 1).
            /// </summary>
            public class IS : DataElementStringType
			{
				public IS(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// IS Constructor
                    AllowMultiValue = true;
				}

                /// <summary>
                /// Creates a Value element, if a value is present!
                /// An IS element can also be empty which results in NO value.
                /// </summary>
                /// <param name="Value">An DataElementIntegerValue object or null, if input string is empty.</param>
                /// <returns></returns>
                public override object CreateValueElement(string Value)
                {
                    if (Value.Length == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return new DataElementIntegerValue(Value);
                    }
                }

                public override object CreateValueElement()
                {
                    return new DataElementIntegerValue();
                }
            }
            #endregion

            #region LO (Long String) (*)
            /// <summary>
            /// Definition:
            /// A character string that may be padded with
            /// leading and/or trailing spaces. The character
            /// code 5CH (the BACKSLASH “\” in ISO-IR 6)
            /// shall not be present, as it is used as the
            /// delimiter between values in multiple valued
            /// data elements. The string shall not have
            /// Control Characters except for ESC.
            /// 
            /// Encoding
            ///     Default Character Repertoire and/or 
            ///     as defined by (0008,0005).
            /// 
            /// 64 characters maximum
            /// </summary>
            public class LO : DataElementStringType
			{
				public LO(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// LO Constructor
                    AllowMultiValue = true;
                    MaxLengthOfValue = 64;
				}
            }
            #endregion

            #region LT (Long Text) (*)
            /// <summary>
            /// A character string that may contain one or
            /// more paragraphs. It may contain the Graphic
            /// Character set and the Control Characters, CR,
            /// LF, FF, and ESC. It may be padded with
            /// trailing spaces, which may be ignored, but
            /// leading spaces are considered to be
            /// significant. Data Elements with this VR shall
            /// not be multi-valued and therefore character
            /// code 5CH (the BACKSLASH “\” in ISO-IR 6)
            /// may be used.
            /// </summary>
            public class LT : DataElementStringType
			{
				public LT(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// AS Constructor
                    AllowMultiValue = false;
				}
            }
            #endregion

            #region OB (Other Byte String)

            public class OB : DataElement
			{
				public OB(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// OB Constructor
                    if (ValueLength == -1)
                    {
                        DataValueState = DataState.IsIgnorable;
                    }
				}

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns></returns>
                public override string GetHumanReadableString()
                {
                    CallPrepareRawBuffer();
                    if (ValueLength < 0)
                    {
                        return "OB DataStream with undefined length";
                    }
                    else
                    {
                        return "OB DataStream with " + ValueLength.ToString() + " bytes";
                    }
                }

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    if (Values.Count == 1)
                    {
                        RawData = Convert.FromBase64String(((DataElementValue)(Values[0])).GetValueAsString());
                        ValueLength = RawData.Length;
                    }
                    else
                    {
                        RawData = null;
                        ValueLength = 0;
                    }
                }

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    if (ValueLength > 0)
                    {
                        DoAddValue(Convert.ToBase64String(RawData)); //, Base64FormattingOptions.InsertLineBreaks));
                    }
                    else
                    {
                        DoAddValue("");
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementStringValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementStringValue();
                }
            }

            #endregion

            #region OF (Other Float String)

            public class OF : DataElement
			{
				public OF(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// OF Constructor
				}

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns>A string</returns>
                public override string GetHumanReadableString()
                {
                    CallPrepareRawBuffer();
                    if (ValueLength < 0)
                    {
                        return "OF DataStream with undefined length";
                    }
                    else
                    {
                        return "OF DataStream with " + ValueLength.ToString() + " bytes";
                    }
                }

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    DoAddValue(Convert.ToBase64String(RawData)); //, Base64FormattingOptions.InsertLineBreaks));

                    DataValueState = DataState.IsRawAndDecoded;
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementStringValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementStringValue();
                }

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    if (Values.Count == 1)
                    {
                        RawData = Convert.FromBase64String(((DataElementValue)(Values[0])).GetValueAsString());
                        ValueLength = RawData.Length;
                    }
                    else
                    {
                        RawData = null;
                        ValueLength = 0;
                    }
                }

            }
            
            #endregion
            
            #region OW (Other Word String)

            public class OW : DataElement
			{
				public OW(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// OW Constructor
				}

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns></returns>
                public override string GetHumanReadableString()
                {
                    CallPrepareRawBuffer();
                    if (ValueLength < 0)
                    {
                        return "OW DataStream with undefined length";
                    }
                    else
                    {
                        return "OW DataStream with " + ValueLength.ToString() + " bytes";
                    }
                }

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    if ( Values.Count == 1)
                    {
                        RawData = Convert.FromBase64String(((DataElementValue)(Values[0])).GetValueAsString());
                        ValueLength = RawData.Length;
                    }
                    else
                    {
                        RawData = null;
                        ValueLength = 0;
                    }
                }

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    DoAddValue(Convert.ToBase64String(RawData)); //, Base64FormattingOptions.InsertLineBreaks));

                    DataValueState = DataState.IsRawAndDecoded;
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementStringValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementStringValue();
                }

            }
            #endregion

            #region PN (Person Name) (string type)
            /// <summary>
            /// A character string encoded using a 5
            /// component convention. The character code
            /// 5CH (the BACKSLASH “\” in ISO-IR 6) shall
            /// not be present, as it is used as the delimiter
            /// between values in multiple valued data
            /// elements. The string may be padded with
            /// trailing spaces. The five components in their
            /// order of occurrence are: family name complex,
            /// given name complex, middle name, name
            /// prefix, name suffix. Any of the five components
            /// may be an empty string. The component
            /// delimiter shall be the caret “^” character (5EH).
            /// Delimiters are required for interior null
            /// components. Trailing null components and
            /// their delimiters may be omitted. Multiple
            /// entries are permitted in each component and
            /// are encoded as natural text strings, in the
            /// format preferred by the named person. This
            /// conforms to the ANSI HISPP MSDS Person
            /// Name common data type.
            /// [...]
            /// Precise semantics are defined for each
            /// component group. See section 6.2.1.
            /// Examples:
            /// Rev. John Robert Quincy Adams, B.A.
            /// M.Div.
            /// “Adams^John Robert
            /// Quincy^^Rev.^B.A. M.Div.”
            /// [One family name; three given names;
            /// no middle name; one prefix; two
            /// suffixes.]
            /// Susan Morrison-Jones, Ph.D., Chief
            /// Executive Officer
            /// “Morrison-Jones^Susan^^^Ph.D., Chief
            /// Executive Officer”
            /// [Two family names; one given name; no
            /// middle name; no prefix; two suffixes.]
            /// John Doe
            /// “Doe^John”
            /// [One family name; one given name; no
            /// middle name, prefix, or suffix. Delimiters
            /// have been omitted for the three trailing
            /// null components.]
            /// </summary>
			public class PN : DataElementStringType
			{
				public PN(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// PN Constructor

					// multiple values are
					// (0008,1048) vr="PN" min="1" max="n" Physician(s) of Record
					// (0008,1050) vr="PN" min="1" max="n" Performing Physician's Name
					this.AllowMultiValue = true;
					// (see PS3.5-2006, table 6.2-1, page 27)
					this.MaxLengthOfValue = 64;
				}

                /// <summary>
                /// A overridable factory function to create the appropriate Value
                /// Element objects.
                /// </summary>
                /// <param name="Value">A string representing the value</param>
                /// <returns>an Value Element object</returns>
                public override object CreateValueElement(string Value)
                {
                    return new DataElementPNValue(Value);
                }


                /// <summary>
                /// A overridable factory function to create the appropriate Value
                /// Element objects.
                /// </summary>
                /// <returns>an default Value Element object</returns>
                public override object CreateValueElement()
                {
                    return new DataElementPNValue();
                }

            }
            #endregion

            #region SH (Short String) (string type)
            /// <summary>
			/// SH Short String
			/// contains a short string (max 16 bytes)
			///
			/// Definition ( PS3.5-2006, Page 25, Table 6.2-1 )
			/// A character string that may be padded with
			/// leading and/or trailing spaces. The character
			/// code 05CH (the BACKSLASH “\” in ISO-IR 6)
			/// shall not be present, as it is used as the
			/// delimiter between values for multiple data
			/// elements. The string shall not have Control
			/// Characters except ESC.
			///
			/// Default Character Repertoire:
			/// Default Character Repertoire and/or
			/// as defined by (0008,0005).
			///
			/// 16 chars maximum (see NOTE in 6.2)
			/// </summary>
			public class SH : DataElementStringType
			{
				public SH(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// SH Constructor
					this.AllowMultiValue = true;
					// (see NOTE in 6.2, PS3.5-2006, table 6.2-1, Page 29)
					this.MaxLengthOfValue = 16;
				}
            }
            #endregion

            #region SL (Signed Long)

            /// <summary>
            /// Signed binary integer 32 bits long in 2's
            /// complement form.
            /// Represents an integer, n, in the range:
            /// - 2^31 le n le (2^31 - 1).
            /// 
            /// Character Set:
            ///     not applicable 
            /// 
            /// Length:
            ///     4 bytes fixed
            /// </summary>
            public class SL : DataElement
			{
				public SL(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// SL Constructor
				}

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    for (int i = 0; i < ValueLength; i += 4)
                    {
                        Values.Add(new DataElementIntegerValue(Utils.ReadInt32(RawData, i, isLittleEndian)));
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }

                public override object CreateValueElement(string value)
                {
                    return new DataElementIntegerValue(value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementIntegerValue();
                }

                // 
                public override string GetHumanReadableString()
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

            }
            #endregion

            #region SQ (Sequence)

            public class SQ : DataElement
			{
				public SQ(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// SQ Constructor
                    DataValueState = DataState.IsIgnorable;
				}

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns></returns>
                public override string GetHumanReadableString()
                {
                    StringBuilder result = new StringBuilder(200);
                    result.Append("Sequence with ");
                    result.Append(Count.ToString());
                    result.Append(" items.");
                    return result.ToString();
                }

                public override object CreateValueElement()
                {
                    throw new Exception("SQ Elements shall have no actual Value!");
                }

#if false
                /// <summary>
                /// Get the encoded Length of the DataElement by given VR syntax including the length of all sub elements in their payload, if present
                /// </summary>
                /// <remarks>Calling GetEncodedLength() can be expensive since it
                /// causes the values to be encoded to raw format to determine the
                /// length of the value payload.</remarks>
                /// <param name="isImplicit">true for implicit encoding</param>
                /// <returns>The number of bytes the completely encoded Element would take</returns>
                public override int GetEncodedLength(bool isImplicit)
                {
                    int result;
                    int childLength = GetEncodedLengthOfChildren(isImplicit);
                    if (ValueLength != Consts.UndefinedLength)
                    {
                        ValueLength = childLength;
                    }
                    if (GroupTag == Consts.MetaGroupNumber)
                    {
                        isImplicit = false;
                    }
                    result = GetHeaderLen(isImplicit);
                    result += GetLength();
                    result += childLength;
                    return result;
                }
#endif
            }

            #endregion

            #region SS (Signed Short)
            /// <summary>
            /// Signed Short Data Element
            ///     Signed binary integer 16 bits long in 2's
            ///     complement form. Represents an integer n in
            ///     the range:
            ///     -215 le n le (215 - 1).
            /// 
            /// CharacterSet:
            ///     not applicable 
            /// 
            /// Length
            ///     2 bytes fixed
            /// </summary>
			public class SS : DataElement
			{
				public SS(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// SS Constructor
				}

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    for (int i = 0; i < ValueLength; i += 2)
                    {
                        Values.Add(new DataElementIntegerValue(Utils.ReadInt16(RawData, i, isLittleEndian)));
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    int newsize = Values.Count * 2;
                    if (newsize > 0)
                    {
                        RawData = new byte[newsize];
                        int i = 0;

                        foreach (object k in Values)
                        {
                            DataElementIntegerValue l = (DataElementIntegerValue)(k);
                            Utils.WriteInt16(l.getValueAsInteger(), RawData, i, IsLittleEndian);
                            i += 2;
                        }
                    }

                    ValueLength = newsize;
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementIntegerValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementIntegerValue();
                }

                public override string GetHumanReadableString()
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
            }
            #endregion

            #region ST (Short Text)

            public class ST : DataElementStringType
			{
				public ST(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// ST Constructor
					MaxLengthOfValue = 16;
					AllowMultiValue = true;
				}
            }
            #endregion

            #region TM (Time) (string type)
            /// <summary>
            /// A string of characters of the format
            ///    hhmmss.frac; where hh contains hours (range
            ///    "00" - "23"), mm contains minutes (range "00" -
            ///    "59"), ss contains seconds (range "00" -
            ///    "59"), and frac contains a fractional part of a
            ///    second as small as 1 millionth of a second
            ///    (range “000000” - “999999”). A 24 hour clock
            ///    is assumed. Midnight can be represented by
            ///    only “0000“ since “2400“ would violate the
            ///    hour range. The string may be padded with
            ///    trailing spaces. Leading and embedded
            ///    spaces are not allowed. One or more of the
            ///    components mm, ss, or frac may be
            ///    unspecified as long as every component to the
            ///    right of an unspecified component is also
            ///    unspecified. If frac is unspecified the
            ///    preceding “.” may not be included. Frac shall
            ///    be held to six decimal places or less to ensure
            ///    its format conforms to the ANSI HISPP MSDS
            ///    Time common data type.
            ///    Examples:
            ///    1. “070907.0705 ” represents a time of
            ///    7 hours, 9 minutes and 7.0705
            ///    seconds.
            ///    2. “1010” represents a time of 10 hours,
            ///    and 10 minutes.
            ///    3. “021 ” is an invalid value.
            ///    Notes: 1. For reasons of backward
            ///    compatibility with versions of this
            ///    standard prior to V3.0, it is
            ///    recommended that implementations
            ///    also support a string of characters of
            ///    the format hh:mm:ss.frac for this VR.
            ///    2. See also DT VR in this table.
            /// 
            /// Character Set Repertoire:
            ///     "0”-”9”, “." of Default Character Repertoire
            /// 
            /// 16 Bytes maximum
            /// </summary>
            public class TM : DataElement
			{
				/// <summary>
				/// TM Data Element Constructor. Use it when creating it from base values
				/// </summary>
				/// <param name="dataDictionary">The Data Dictionary that should be used to encode the string (can be null)</param>
				/// <param name="groupTag">The group tag of the element</param>
				/// <param name="elementTag">The element tag of the element</param>
				/// <param name="encodingUsed">The encoding that should be used</param>
				/// <param name="isLittleEndian">Shall the Element be encoded in little endianess</param>
				/// <param name="hour">The hour of the time data</param>
				/// <param name="minutes">The minute of the time data</param>
				/// <param name="seconds">The seconds of the time data</param>
				/// <param name="microseconds">The microseconds fraction of the time data</param>
				public TM(DataDictionary dataDictionary,int groupTag,int elementTag, StringEncoding encodingUsed, bool isLittleEndian, int hours, int minutes, int seconds, int microseconds)
					: base(dataDictionary,groupTag,elementTag,ValueRepresentationConsts.TM,0,null,encodingUsed,isLittleEndian)
				{
                    PurgeValues();
                    PurgeRawData();
					Values.Add(new DataElementTMValue(hours,minutes,seconds,microseconds));
					this.DataValueState = DataState.IsDecodedOnly;
                    
				}

				/// <summary>
				/// TM Data Element Constructor. Use this constructor to initialize it from a raw buffer.
				/// </summary>
				/// <param name="dataDictionary"></param>
				/// <param name="groupTag"></param>
				/// <param name="elementTag"></param>
				/// <param name="valueRepresentation"></param>
				/// <param name="valueLength"></param>
				/// <param name="rawBuffer"></param>
				/// <param name="encodingUsed"></param>
				/// <param name="isLittleEndian"></param>
				public TM(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// TM Constructor
				}

				/// <summary>
				/// Converts the member variables to a string
				/// </summary>
				protected override void PrepareRawBuffer()
				{
					/* prepare the RawBuffer in the given encoding
					 * 
					 * Allowed characters: [0-9] and [.]
					 * Every value has a maximum of 16 bytes and shall be
					 * padded with a trailing space for an even length. 
					 * Delimiter is the default Limiter 'backslash'.
					 * The complete RawBuffer must be also cheched for
					 * being even, so shall be padded with an additional 
					 * space character if neccessary.
					 * (see PS3.5-2006, Table 6.2-1, Page 30)
					 */
					StringBuilder myNewString = new StringBuilder(Values.Count*17);
					foreach ( DataElementTMValue k in Values )
					{
						// if there is already a value encoded,
						if ( myNewString.Length > 0 )
						{
							// add a delimiter
							myNewString.Append('\\');
						}
						myNewString.Append( k.toRawString() );
					}

					// if it is not even length, a trailing blank shall be added
					if ( ( myNewString.Length & 1 ) != 0 )
					{
						myNewString.Append(' ');
					}

					// transfer to raw buffer, adjust valuelength
					RawData = Utils.EncodeString( myNewString.ToString(), UsedEncoding );
					ValueLength = RawData.Length;
				}

				/// <summary>
				/// Decodes the RawData buffer using the given endianess
				/// </summary>
				/// <param name="isLittleEndian">flag if data is little endian</param>
				public override void Decode(bool isLittleEndian)
				{
					// decode the string
					string myData = Utils.DecodeString(RawData,ValueLength,UsedEncoding);
					// split the multivalues

                    string[] myValues = myData.Split('\\');

                    PurgeValues();
                    PrepareForAddValue();

					foreach ( string val in myValues )
					{
						Values.Add(new DataElementTMValue( val ) );
					}
					// Values.Add(new DataElementTMValue(
					DataValueState = DataState.IsRawAndDecoded;
				}

                public override string GetHumanReadableString()
				{
					if ( DataValueState != DataState.IsRawAndDecoded && DataValueState != DataState.IsDecodedOnly )
					{
						Decode(IsLittleEndian);
					}

					StringBuilder myNewString = new StringBuilder(Values.Count*17);
					foreach ( DataElementTMValue k in Values )
					{
						// if there is already a value encoded,
						if ( myNewString.Length > 0 )
						{
							// add a delimiter
							myNewString.Append('\\');
						}
						myNewString.Append( k.GetValueAsString() );
					}
					return myNewString.ToString();
				}

                public override object CreateValueElement(string Value)
                {
                    return new DataElementTMValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementTMValue();
                }
            }
            #endregion

            #region UI (Unique Identifier)
            /// <summary>
			/// Unique Identifier (UID)
			/// Contains an ISO identifier string that
			/// allows to announce transfer syntax encodings
			///
			/// Definition ( PS3.5-2006, Page 30, Table 6.2-1 )
			/// A character string containing a UID that is
			/// used to uniquely identify a wide variety of
			/// items. The UID is a series of numeric
			/// components separated by the period "."
			/// character. If a Value Field containing one or
			/// more UIDs is an odd number of bytes in
			/// length, the Value Field shall be padded with a
			/// single trailing NULL (00H) character to ensure
			/// that the Value Field is an even number of
			/// bytes in length. See Section 9 and Annex B
			/// for a complete specification and examples.
			///
			/// Character Set Repertoire
			/// "0”-”9”, “." of Default Character Repertoire
			///
			/// </summary>
			public class UI : DataElementStringType
			{
				public UI(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// UI Constructor
					// elements with VM > 1 are (0008,001A) vr="UI" min="1" max="n">Related General SOP Class UID
					this.AllowMultiValue = true;
					// 64 byte Element size
					// (see PS3.5-2006, table 6.2-1, page 30)
					this.MaxLengthOfValue = 64;
                    // pad the Value field with 0x00 (all other string types are being padded with 0x20)
                    // (see PS3.5-2006, table 6.2-1, page 30)
                    padWithBlank = false;
				}
            }
            #endregion

            #region UL (Unsigned Long)

            public class UL : DataElement
			{
				public UL(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// UL Constructor
				}

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    // TODO: Make this capable of Unsigned
                    for (int i = 0; i < ValueLength; i += 4)
                    {
                        Values.Add( new DataElementIntegerValue(Utils.ReadInt32(RawData,i,isLittleEndian)) );
                    }
                    this.DataValueState = DataState.IsRawAndDecoded;
                }

                public override object  CreateValueElement(string Value)
                {
                    return new DataElementIntegerValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementIntegerValue();
                }

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    int newsize = Values.Count * 4;
                    if (newsize > 0)
                    {
                        RawData = new byte[newsize];
                        int i = 0;

                        foreach (DataElementIntegerValue k in Values)
                        {
                            Utils.WriteInt32(k.getValueAsInteger(), RawData, i, IsLittleEndian);
                            i += 4;
                        }
                    }
                    ValueLength = newsize;
                }

                public override string GetHumanReadableString()
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
            }

            #endregion
            
            #region UN (Unknown) 

            public class UN : DataElement
			{
				public UN(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// UN Constructor
				}

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    if (Values.Count == 1)
                    {
                        RawData = Convert.FromBase64String(((DataElementValue)(Values[0])).GetValueAsString());
                        ValueLength = RawData.Length;
                    }
                    else
                    {
                        RawData = null;
                        ValueLength = 0;
                    }
                }

                public override void Decode(bool isLittleEndian)
                {
                    PurgeValues();
                    PrepareForAddValue();

                    if (ValueLength > 0)
                    {
                        DoAddValue(Convert.ToBase64String(RawData)); // , Base64FormattingOptions.InsertLineBreaks));
                    }
                    else
                    {
                        DoAddValue("");
                    }

                    DataValueState = DataState.IsRawAndDecoded;
                }

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns></returns>
                public override string GetHumanReadableString()
                {
                    StringBuilder result = new StringBuilder(200);
                    result.Append("Unknown DataStream (");
                    result.Append(ValueLength.ToString());
                    result.Append(" bytes): ");
                    if (DataValueState == DataState.IsRawOnly || DataValueState == DataState.IsRawAndDecoded)
                    {
                        for (int i = 0; (i < 20) && (i < ValueLength); i++)
                        {
                            result.Append("0x");
                            result.Append(RawData[i].ToString("X2").PadLeft(2, '0'));
                            result.Append(" ");
                        }
                    }
                    return result.ToString();
                }

                public override object CreateValueElement(string Value)
                {
                    return new DataElementStringValue(Value);
                }

                public override object CreateValueElement()
                {
                    return new DataElementStringValue();
                }

            }

            #endregion

            #region US (Unsigned Short)
            /// <summary>
            /// Definition:
            ///     Unsigned binary integer 16 bits long.
            ///     Represents integer n in the range
            ///     0 le n le 2^16-1
            /// 
            /// Character Repertoire:
            ///     not applicable
            /// 
            /// 2 bytes fixed
            /// </summary>
			public class US : DataElement
			{
				public US(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// UI Constructor
				}

				/// <summary>
				/// Decodes the RawData buffer using the given endianess.
				/// </summary>
				/// <param name="isLittleEndian">flag if data is little endian</param>
				public override void Decode(bool isLittleEndian)
				{
					PurgeValues();
                    PrepareForAddValue();
					
					int i = 0; 
					int j = this.ValueLength;
					while ( i < j )
					{
                        Values.Add(new DataElementIntegerValue(Utils.ReadUInt16(RawData, i, isLittleEndian)));
						i+=2;
					}

					// call te base function for handling of eventing and states
                    DataValueState = DataState.IsRawAndDecoded;
				}

                protected override void PrepareRawBuffer()
                {
                    PurgeRawData();
                    int newsize = Values.Count * 2;
                    RawData = new byte[newsize];
                    int i = 0;
                    foreach (DataElementIntegerValue k in Values)
                    {
                        Utils.WriteUInt16((UInt16) k.getValueAsInteger(), RawData, i, IsLittleEndian);
                        i += 2;
                    }
                    ValueLength = newsize;
                }

                public override string GetHumanReadableString()
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
                public override object CreateValueElement()
                {
                    return new DataElementIntegerValue();
                }


            }
            #endregion

            #region UT (Unlimited Text)

            // TODO: Find a file that contains an UT element
            /// <summary>
            /// Unlimited Text Data element
            ///     A character string that may contain one or more paragraphs.
            ///     It may contain the graphic character set and the control 
            ///     characters, CR, LF, FF and ESC. It may be padded with
            ///     trailing spaces, which may be ignored, but leading spaces
            ///     are considered to be significant.
            ///     Data Elements with this VR shall not be multi-valued and 
            ///     therefore Character Code 5CH (the BACKSLASH "\" in ISO_IR6)
            ///     may be used.
            /// 
            /// maximum size: 2^32-2 (0xffffffff is reserved)
            /// </summary>
            public class UT : DataElementStringType
			{
				public UT(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// UT Constructor
                    AllowMultiValue = false;    // prevent the splitting of values
				}
            }
            #endregion

            #region Root Data Element (special)
            /// <summary>
			/// This class is used to be the root node of the outmost DataSet frame in the DICOM stream
			/// </summary>
			public class Root : DataElement
			{
				// public System.Collections.ArrayList Warnings;

				public Root(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// Root Node
					Warnings = new System.Collections.ArrayList();
                    DataValueState = DataState.IsIgnorable;
				}

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns></returns>
                public override string GetHumanReadableString()
                {
                    return "Rootnode of the stream";
                }

                public override object CreateValueElement()
                {
                    throw new Exception("A root element shall have no Value!");
                }
            }
            #endregion

            #region Other Data Element (Items/Special)

            public class Other : DataElement
			{
				public Other(DataDictionary dataDictionary, int groupTag,int elementTag,int valueRepresentation, int valueLength, byte[] rawBuffer, StringEncoding encodingUsed, bool isLittleEndian)
					: base(dataDictionary,groupTag,elementTag,valueRepresentation,valueLength,rawBuffer,encodingUsed, isLittleEndian)
				{
					// Other Constructor

                    // since other data elements like items don't have a value representation,
                    // we give it a '??' so the display output is readable and not truncated due 0x0000 string limitation
                    ValueRepresentation = 0x3f3f;   // "??"
                    DataValueState = DataState.IsIgnorable;
				}

                /// <summary>
                /// Returns a string that shows the content of the DataElement in
                /// an human readable manner.
                /// </summary>
                /// <returns></returns>
                public override string GetHumanReadableString()
                {
                    return "Special Item without Value.";
                }

                public override object CreateValueElement()
                {
                    throw new Exception("Items or Special Elements shall have no value");
                }
            }
            #endregion

            #endregion
        }
	}
}