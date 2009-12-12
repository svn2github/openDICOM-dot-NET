/* Gobosh.DICOM
 * DataElement Value Handling. This file contains all specific value classes
 * and provide access to their scalar properties.
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
using System.Globalization;

namespace Gobosh
{
	namespace DICOM
    {
        #region BaseTypeCompatibility
        public enum BaseTypeCompatibility
		{
			Structure, // root object or other
			Integer, // AT,IS,SL,SS,UL,US,
			Float, // DS,FL,DF,
			Date, // AS,DA,DT,TM,
			String, // AE,CS,LO,LT,PN,SH,ST,UI,UT
			Image, // OB,OW
			Other // OF,OW,SQ,UN,
        }

        #endregion

        #region DataElementValue Base Declaration
        /// <summary>
		/// base class for data element values
		/// </summary>
		public abstract class DataElementValue
		{
            /// <summary>
            /// Basetype Compatibility
            /// </summary>
			private BaseTypeCompatibility ValueType;

            /// <summary>
            /// A Hashtable of VarProperty objects for this value            
            /// </summary>
            /// <remarks>A Hashtable is used instead of Dictionary since compatibility with .NET 1.1 shall be achieved</remarks>
            public Hashtable Properties;

			public DataElementValue(BaseTypeCompatibility valueType)
			{
				ValueType = valueType;
			}

			public abstract string GetValueAsString();

            /// <summary>
            /// Clear the property table and rebuild it.
            /// </summary>
            /// <param name="capacity">Capacity to be reserved on creation</param>
            protected void ClearProperties(int capacity)
            {
                Properties = new Hashtable(capacity);
            }

            /// <summary>
            /// Adds a VarProperty object to the list of properties
            /// </summary>
            /// <param name="newProperty">The VarProperty object to be added</param>
            protected void AddProperty(VarProperty newProperty)
            {
                Properties.Add(newProperty.Name, newProperty);
            }

            /// <summary>
            /// Get the property object by name
            /// </summary>
            /// <param name="name">the name of the property</param>
            /// <returns>a VarProperty object or null if not existent</returns>
            public VarProperty GetProperty(string name)
            {
                if (Properties.ContainsKey(name))
                {
                    return (VarProperty)(Properties[name]);
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region INTEGER - DataElementIntegerValue

        /// <summary>
		/// An Integer Value for data elements
		/// </summary>
		public class DataElementIntegerValue : DataElementValue
		{
            private IntProperty Number; // = new IntProperty("Number");

			public DataElementIntegerValue()
				: base(BaseTypeCompatibility.Integer)
			{
                Number = null;
                Number = new IntProperty("int");
                Number.Set(0);
                ClearProperties(1);
                AddProperty(Number);
			}

			public DataElementIntegerValue(int number)
				: base(BaseTypeCompatibility.Integer)
			{
                Number = null;
                Number = new IntProperty("int");
                Number.Set(number);
                ClearProperties(1);
                AddProperty(Number);
            }

            public DataElementIntegerValue(string number)
                : base(BaseTypeCompatibility.Integer)
            {
                Number = null;
                Number = new IntProperty("int");
                Number.Set(number);
                ClearProperties(1);
                AddProperty(Number);
            }

			public int getValueAsInteger()
			{
                return Number.AsInteger();
			}

			public override string GetValueAsString()
			{
                return Number.AsString();
			}
        }
        #endregion

        #region FLOAT - DataElementFloatValue class
        public class DataElementFloatValue : DataElementValue
		{
            private FloatProperty Number;

            public DataElementFloatValue()
                : this(0.0f)
            {
            }

			public DataElementFloatValue(string number)
				: base(BaseTypeCompatibility.Float)
			{
                Number = null;
                Number = new FloatProperty("float");
                Number.Set(number);
                ClearProperties(1);
                AddProperty(Number);
			}

            public DataElementFloatValue(float number)
				: base(BaseTypeCompatibility.Float)
			{
                Number = null;
                Number = new FloatProperty("float");
                Number.Set(number); 
                ClearProperties(1);
                AddProperty(Number); 
			}

			public double getValueAsFloat()
			{
				return Number.AsFloat();
			}

			public override string GetValueAsString()
			{
				return Number.AsString();
			}
        }
        #endregion

        #region DOUBLE - DataElementDoubleValue class

        /// <summary>
        /// A property representing a 64bit double precision floating point number.
        /// </summary>
        public class DataElementDoubleValue : DataElementValue
        {
            private DoubleProperty Number;

            public DataElementDoubleValue()
                : base(BaseTypeCompatibility.Float)
            {
                Number = null;
                Number = new DoubleProperty("double");
                Number.Set(0.0);
                ClearProperties(1);
                AddProperty(Number);
            }

            public DataElementDoubleValue(double number)
                : base(BaseTypeCompatibility.Float)
            {
                Number = null;
                Number = new DoubleProperty("double");
                Number.Set(number);
                ClearProperties(1);
                AddProperty(Number);
            }

            public DataElementDoubleValue(string number)
                : base(BaseTypeCompatibility.Float)
            {
                Number = null;
                Number = new DoubleProperty("double");
                Number.Set(number);
                ClearProperties(1);
                AddProperty(Number);
            }

            public double getValueAsFloat()
            {
                return Number.AsFloat();
            }


            public double getValueAsDouble()
            {
                return Number.AsDouble();
            }

            public override string GetValueAsString()
            {
                return Number.AsString();
            }
        }
        #endregion

        #region STRING - DataElementIntegerValue

        /// <summary>
        /// An Integer Value for data elements
        /// </summary>
        public class DataElementStringValue : DataElementValue
        {
            private StringProperty TextString; // = new IntProperty("Number");

            public DataElementStringValue()
                : base(BaseTypeCompatibility.String)
            {
                TextString = null;
                TextString = new StringProperty("string");
                TextString.Set("");
                ClearProperties(1);
                AddProperty(TextString);
            }

            public DataElementStringValue(string text)
                : base(BaseTypeCompatibility.Integer)
            {
                TextString = null;
                TextString = new StringProperty("string");
                TextString.Set(text);
                ClearProperties(1);
                AddProperty(TextString);
            }

            public override string GetValueAsString()
            {
                return TextString.AsString();
            }
        }
        #endregion

        #region AS (Age String) - DataElementAgeStringValue

        public class DataElementAgeStringValue : DataElementValue
        {
            private IntProperty AgeNumber;

            private StringProperty AgeUnit;

            public DataElementAgeStringValue()
                : this(0,"Years")
            {
            }

            public DataElementAgeStringValue(int age, string ageUnit)
                : base(BaseTypeCompatibility.String)
            {
                AgeNumber = new IntProperty("AgeNumber");
                AgeUnit = new StringProperty("AgeUnit");
                ClearProperties(2);
                AddProperty(AgeNumber);
                AddProperty(AgeUnit);

                AgeNumber.Set(age);
                AgeUnit.Set(ageUnit);
            }

            /// <summary>
            /// Creates a new DataElementAgeStringValue object and sets its
            /// value according to the given valuestring. The format for the
            /// string is 000N, where 000 is a three-digit-number and N
            /// is a specifier with valid values of "D","W","M","Y"
            /// </summary>
            /// <param name="Value">The age description as 000N</param>
            public DataElementAgeStringValue(string Value)
                : this(0,"Years")
            {
                if (Value.Length == 4)
                {
                    int thenumber;
                    string theunit;
                    char scale = Value[3];

                    thenumber = (((Value[0] - (int)'0') * 100) + ((Value[1] - (int)'0') * 10) + (Value[2] - (int)'0'));
                    switch (scale)
                    {
                        case 'D':
                            theunit = "Days";
                            break;
                        case 'W':
                            theunit = "Weeks";
                            break;
                        case 'M':
                            theunit = "Months";
                            break;
                        case 'Y':
                            theunit = "Years";
                            break;
                        default:
                            throw new Exception("Illegal time specifier: Should have been D,W,M or Y");
                            // theunit = "Years";
                            // break;
                    }
                    // set the properties
                    AgeNumber.Set(thenumber);
                    AgeUnit.Set(theunit);
                }
                else
                {
                    throw new Exception("Invalid encoding");
                }
				
            }

            public override string GetValueAsString()
            {
                return AgeNumber.AsString() + " " + AgeUnit.AsString();
            }
        }

        #endregion

        #region AT (Attribute) Value - DataElementATValue

        public class DataElementATValue : DataElementValue
        {
            private IntProperty GroupNumber;
            private IntProperty ElementNumber;

            public DataElementATValue()
                : this(0,0)
            {
            }

            public DataElementATValue(int group, int element)
                : base(BaseTypeCompatibility.Structure)
            {
                GroupNumber = new IntProperty("Group", group);
                ElementNumber = new IntProperty("Element", element);
                ClearProperties(2);
                AddProperty(GroupNumber);
                AddProperty(ElementNumber);
            }

            public override string GetValueAsString()
            {
                StringBuilder result = new StringBuilder(11);
                result.Append('(');
                result.Append(GroupNumber.AsInteger().ToString("x4"));
                result.Append(',');
                result.Append(ElementNumber.AsInteger().ToString("x4"));
                result.Append(')');
                return result.ToString();
            }

            public int WriteToRawBuffer(byte[] buffer, int index, bool isLittleEndian)
            {
                Utils.WriteUInt16((ushort)this.GroupNumber.AsInteger(), buffer, index, isLittleEndian);
                Utils.WriteUInt16((ushort) this.ElementNumber.AsInteger(), buffer, index + 2, isLittleEndian);
                return index + 4;
            }

        }

        #endregion

        #region DA Value

        public class DataElementDAValue : DataElementValue
        {
            public IntProperty Year;
            public IntProperty Month;
            public IntProperty Day;

            public DataElementDAValue()
                : this("19000101")
            {
            }

            public DataElementDAValue(string dateString)
                : base(BaseTypeCompatibility.Date)
            {
                Year = new IntProperty("Year");
                Month = new IntProperty("Month");
                Day = new IntProperty("Day");
                ClearProperties(3);
                AddProperty(Year);
                AddProperty(Month);
                AddProperty(Day);
                SetValueFromString(dateString);
            }

            /// <summary>
            /// Set the current value by an encoded string of the date.
            /// </summary>
            /// <param name="dateString">A date as string in yyyymmdd notation</param>
            public void SetValueFromString(string dateString)
            {
                /*
                 * A string of characters of the format yyyymmdd;
                 * where yyyy shall contain year, mm shall
                 * contain the month, and dd shall contain the
                 * day. This conforms to the ANSI HISPP MSDS
                 * Date common data type.
                 * Example:
                 * “19930822” would represent August 22,1993.
                 * Notes: 1. For reasons of backward
                 * compatibility with versions of this
                 * standard prior to V3.0, it is
                 * recommended that implementations
                 * also support a string of characters of
                 * the format yyyy.mm.dd for this VR.
                 * */

                // remove any "." from DICOM files prior to V3.0
                // (See PS3.5-2006, 6.2-1, Page 25, DA Definition)
                dateString = dateString.Replace(".", "");
                if (dateString.Length == 8)
                {
                    Year.Set(Int32.Parse(dateString.Substring(0, 4)));
                    Month.Set(Int32.Parse(dateString.Substring(4, 2)));
                    Day.Set(Int32.Parse(dateString.Substring(6, 2)));
                }
                else
                {
                    // TODO: Warning or Exception on DA string error?
                    Year.Set(1900);
                    Month.Set(1);
                    Day.Set(1);
                    throw new Exception("Date has no valid syntax!");
                }
            }

            /// <summary>
            /// A static string array for converting the month number
            /// to a string.
            /// </summary>
            private static string[] Months = {
                "Jan","Feb","Mar","Apr","May","Jun","Jul",
                "Aug","Sep","Oct","Nov","Dec"
            };

            /// <summary>
            /// Returns the date as human readable string.
            /// Formatting is "dd mmm yyyy", the common invariant culture
            /// </summary>
            /// <returns>a string in format dd mmm yyyy</returns>
            public override string GetValueAsString()
            {
                StringBuilder result = new StringBuilder(10);
                //result.Append(Month.AsString());
                //result.Append('/');
                if (!Utils.CheckBounds(Day.AsInteger(), 1, 31))
                {
                    Day.Set(1);
                }
                if (!Utils.CheckBounds(Month.AsInteger(), 1, 12))
                {
                    Month.Set(1);
                }
                result.Append(Day.AsString());
                result.Append(' ');
                result.Append(Months[Month.AsInteger() - 1]);   // Month number -1 = index of array "Months"
                result.Append(' ');
                result.Append(Year.AsString());
                return result.ToString();
            }

            /// <summary>
            /// Writes the raw data representation of the current date
            /// </summary>
            /// <param name="buffer">the buffer where to write to</param>
            /// <param name="index">the index where to write to</param>
            public void WriteRawData(byte[] buffer, int index)
            {
                StringBuilder result = new StringBuilder(8);
                result.Append(Year.AsString().PadLeft(4, '0'));
                result.Append(Month.AsString().PadLeft(2, '0'));
                result.Append(Day.AsString().PadLeft(2, '0'));
                for (int i = 0; i < 8; i++)
                {
                    buffer[index + i] = (byte)(result[i]);
                }
            }
        }

        #endregion

        #region PN (PersonName) - DataElementPNValue
        /// <summary>
        /// Represents one value of a PN (Person Name) DataElement.
        /// This Data Element type is used for patient names or
        /// performing physicians.
        /// The raw data contains a (up to) 5 components string.
        /// (see PS3.5-2006, table 6.2-1, page 27)
        /// </summary>
        public class DataElementPNValue : DataElementValue
        {
            /// <summary>
            /// Represents the familyname of a person
            /// </summary>
            public StringProperty FamilyName = new StringProperty("FamilyName");
            /// <summary>
            /// Represents the given name of a person
            /// </summary>
            public StringProperty GivenName = new StringProperty("GivenName");
            /// <summary>
            /// Represents the middle name or char of a person
            /// </summary>
            public StringProperty MiddleName = new StringProperty("MiddleName");
            /// <summary>
            /// Represents a name prefix, like "Reverend"
            /// </summary>
            public StringProperty NamePrefix = new StringProperty("NamePrefix");
            /// <summary>
            /// Represents a name suffix, like "Ph.D., Chief Execution Officer"
            /// </summary>
            public StringProperty NameSuffix = new StringProperty("NameSuffix");

            /// <summary>
            /// Represents the ideographic group of the name
            /// </summary>
            public StringProperty Ideographic = new StringProperty("Ideographic");
            /// <summary>
            /// Represents the phonetic group of the name
            /// </summary>
            public StringProperty Phonetic = new StringProperty("Phontic");


            /// <summary>
            /// Constructs a DataElementPNValue object. The namestring
            /// should have the format:
            /// "Adams^John Robert^Quincy^Rev.^B.A. M.Div=ideographic=phonetic"
            /// </summary>
            /// <param name="namestring">a 3 group, 5 component string</param>
            public DataElementPNValue()
                : base(BaseTypeCompatibility.String)
            {
                ClearProperties(7);
                AddProperty(FamilyName);
                AddProperty(GivenName);
                AddProperty(MiddleName);
                AddProperty(NamePrefix);
                AddProperty(NameSuffix);
                AddProperty(Ideographic);
                AddProperty(Phonetic);
            }

            /// <summary>
            /// Constructs a DataElementPNValue object. The namestring
            /// should have the format:
            /// "Adams^John Robert^Quincy^Rev.^B.A. M.Div=ideographic=phonetic"
            /// </summary>
            /// <param name="namestring">a 3 group, 5 component string</param>
            public DataElementPNValue(string namestring)
                : base(BaseTypeCompatibility.String)
            {
                // collect the properties for a value                                
                ClearProperties(7);
                AddProperty(FamilyName);
                AddProperty(GivenName);
                AddProperty(MiddleName);
                AddProperty(NamePrefix);
                AddProperty(NameSuffix);
                AddProperty(Ideographic);
                AddProperty(Phonetic);

                /* split the component into the groups
                 * afterwards split the first group into components
                 */
                string[] groups = namestring.Split('=');
                // some SIEMENS devices sometimes use "!" instead of "^", maybe this
                // should be more like this:
                char[] splitchars = new char[] { '^', '!' };
                string[] components = groups[0].Split(splitchars);
                // totally standard conform:
                //* string[] components = groups[0].Split('^');

                FamilyName.Set("");
                GivenName.Set("");
                MiddleName.Set("");
                NamePrefix.Set("");
                NameSuffix.Set("");
                Ideographic.Set("");
                Phonetic.Set("");

                if (groups.Length >= 3)
                {
                    Phonetic.Set(groups[2]);
                }
                if (groups.Length >= 2)
                {
                    Ideographic.Set(groups[1]);
                }
                if (components.Length >= 5)
                {
                    NameSuffix.Set(components[4]);
                }
                if (components.Length >= 4)
                {
                    NamePrefix.Set(components[3]);
                }
                if (components.Length >= 3)
                {
                    MiddleName.Set(components[2]);
                }
                if (components.Length >= 2)
                {
                    GivenName.Set(components[1]);
                }
                if (components.Length >= 1)
                {
                    FamilyName.Set(components[0]);
                }
            }

            public override string GetValueAsString()
            {
                // 100 characters should be enough for a lot of name variations
                StringBuilder result = new StringBuilder(100);
                if (NamePrefix.Length > 0)
                {
                    result.Append(NamePrefix.AsString());
                    result.Append(' ');
                }
                if (GivenName.Length > 0)
                {
                    result.Append(GivenName.AsString());
                    result.Append(' ');
                }
                if (MiddleName.Length > 0)
                {
                    result.Append(MiddleName.AsString());
                    result.Append(' ');
                }
                result.Append(FamilyName.AsString());
                if (NameSuffix.Length > 0)
                {
                    result.Append(", ");
                    result.Append(NameSuffix.AsString());
                }
                return result.ToString();
            }
        }
        #endregion

        #region DT Value

        public class DataElementDTValue : DataElementValue
        {
            // YYYYMMDDHHMMSS.FFFFFF&ZZZZ
            public IntProperty Year;
            public IntProperty Month;
            public IntProperty Day;
            public IntProperty Hour;
            public IntProperty Minute;
            public IntProperty Second;
            public IntProperty Fractional;
            public IntProperty Timezone;

            public DataElementDTValue()
                : base(BaseTypeCompatibility.Date)
            {
                Year = new IntProperty("Year", 1900);
                Month = new IntProperty("Month", 1);
                Day = new IntProperty("Day", 1);
                Hour = new IntProperty("Hour", 0);
                Minute = new IntProperty("Minute", 0);
                Second = new IntProperty("Second", 0);
                Fractional = new IntProperty("Fractional", 0);
                Timezone = new IntProperty("Timezone", 0);
                ClearProperties(8);
                AddProperty(Year);
                AddProperty(Month);
                AddProperty(Day);
                AddProperty(Hour);
                AddProperty(Minute);
                AddProperty(Second);
                AddProperty(Fractional);
                AddProperty(Timezone);
            }

            public DataElementDTValue(string value)
                : base(BaseTypeCompatibility.Date)
            {                
                Year = new IntProperty("Year", 1900);
                Month = new IntProperty("Month", 1);
                Day = new IntProperty("Day", 1);
                Hour = new IntProperty("Hour", 0);
                Minute = new IntProperty("Minute", 0);
                Second = new IntProperty("Second", 0);
                Fractional = new IntProperty("Fractional", 0);
                Timezone = new IntProperty("Timezone", 0);
                ClearProperties(8);
                AddProperty(Year);
                AddProperty(Month);
                AddProperty(Day);
                AddProperty(Hour);
                AddProperty(Minute);
                AddProperty(Second);
                AddProperty(Fractional);
                AddProperty(Timezone);
                SetValue(value);

            }

            public void SetValue(string value)
            {
                // 01234567890123456789012345
                //           1111111111222222
                // YYYYMMDDHHMMSS.FFFFFF&ZZZZ

                // YYYY
                if (value.Length >= 4)
                {
                    Year.Set(int.Parse(value.Substring(0, 4)));
                }
                else
                {
                    Year.Set(1900);
                }
                if (value.Length >= 6)
                {
                    Month.Set(int.Parse(value.Substring(4, 2)));
                }
                else
                {
                    Month.Set(1);
                }
                if (value.Length >= 8)
                {
                    Day.Set(int.Parse(value.Substring(6, 2)));
                }
                else
                {
                    Day.Set(1);
                }
                if (value.Length >= 10)
                {
                    Hour.Set(int.Parse(value.Substring(8, 2)));
                }
                else
                {
                    Hour.Set(0);
                }
                if (value.Length >= 12)
                {
                    Minute.Set(int.Parse(value.Substring(10, 2)));
                }
                else
                {
                    Minute.Set(0);
                }
                if (value.Length >= 14)
                {
                    Second.Set(int.Parse(value.Substring(12,2)));
                }
                else
                {
                    Second.Set(0);
                }
                if (value.Length >= 20)
                {
                    Fractional.Set(int.Parse(value.Substring(15, 6)));
                }
                else
                {
                    Fractional.Set(0);
                }
                if (value.Length >= 26)
                {
                    Timezone.Set(int.Parse(value.Substring(21, 5)));
                }
                else
                {
                    Timezone.Set(0);
                }
            }

            public string GetRawString()
            {
                StringBuilder result = new StringBuilder(26);
                result.Append(Year.AsString().PadLeft(4, '0'));
                result.Append(Month.AsString().PadLeft(2, '0'));
                result.Append(Day.AsString().PadLeft(2, '0'));
                result.Append(Hour.AsString().PadLeft(2, '0'));
                result.Append(Minute.AsString().PadLeft(2, '0'));
                result.Append(Second.AsString().PadLeft(2, '0'));
                result.Append('.');
                result.Append(Fractional.AsString().PadLeft(6, '0'));
                if (Timezone.AsInteger() < 0)
                {
                    result.Append("+");
                    result.Append(Timezone.AsString().PadLeft(5, '0'));
                }
                else
                {
                    result.Append("-");
                    int x = -(Timezone.AsInteger());
                    result.Append(x.ToString().PadLeft(5, '0'));

                }
                return result.ToString();
            }

            /// <summary>
            /// A static string array for converting the month number
            /// to a string.
            /// </summary>
            private static string[] Months = {
                "Jan","Feb","Mar","Apr","May","Jun","Jul",
                "Aug","Sep","Oct","Nov","Dec"
            };

            public override string GetValueAsString()
            {
                StringBuilder result = new StringBuilder(50);

                result.Append(Day.AsString());
                result.Append(' ');
                result.Append(Months[Month.AsInteger() - 1]);
                result.Append(' ');
                result.Append(Year.AsString());
                result.Append(' ');
                result.Append(Hour.AsString().PadLeft(2, '0'));
                result.Append(':');
                result.Append(Minute.AsString().PadLeft(2, '0'));
                result.Append(':');
                result.Append(Second.AsString().PadLeft(2, '0'));
                result.Append('.');
                result.Append(Fractional.AsString().PadLeft(6, '0'));
                if (Timezone.AsInteger() < 0)
                {
                    result.Append("+");
                    result.Append(Timezone.AsString().PadLeft(5, '0'));
                }
                else
                {
                    result.Append("-");
                    int x = -(Timezone.AsInteger());
                    result.Append(x.ToString().PadLeft(5, '0'));

                }
                return result.ToString();
            }

        }

        #endregion

        #region TM Value
        /// <summary>
        /// A TM Valuetype
        /// </summary>
        public class DataElementTMValue : DataElementValue
        {
            public IntProperty Hour;
            public IntProperty Minute;
            public IntProperty Second;
            public IntProperty Microsecond;

            public DataElementTMValue()
                : this(0,0,0,0)
            {
            }

            public DataElementTMValue(int hours, int minutes, int seconds, int microseconds)
                : base(BaseTypeCompatibility.Date)
            {
                Hour = new IntProperty("Hours", hours);
                Minute = new IntProperty("Minutes", minutes);
                Second = new IntProperty("Seconds", seconds);
                Microsecond = new IntProperty("Microseconds", microseconds);
                ClearProperties(4);
                AddProperty(Hour);
                AddProperty(Minute);
                AddProperty(Second);
                AddProperty(Microsecond);
            }

            public DataElementTMValue(string timestring)
                : this(0,0,0,0)
            {
                // Initialize

                /* DICOM prior to v3 allowed the format
                 * "hh:mm:ss.frac", therefore we remove all
                 * ":" characters since then the result is
                 * according to the expected hhmmss.frac
                 * (See PS3.5-2006, table 6.2-1, page 30)
                 * 
                 * Such a thing exists also for DA.
                 */
                if (timestring.IndexOf(':') > 0)
                {
                    timestring = timestring.Replace(":", "");
                }

                // if there are more than 2 Characters
                if (timestring.Length >= 2)
                {
                    Hour.Set(Int32.Parse(timestring.Substring(0, 2)));
                }
                else
                {
                    // TODO: emit a warning about formatting error
                }
                if (timestring.Length >= 4)
                {
                    Minute.Set(Int32.Parse(timestring.Substring(2, 2)));
                }
                if (timestring.Length >= 6)
                {
                    Second.Set(Int32.Parse(timestring.Substring(4, 2)));
                }
                if (timestring.Length >= 8)
                {
                    if (timestring[6] == '.')
                    {
                        Microsecond.Set(Int32.Parse(timestring.Substring(7, timestring.Length - 7)));
                    }
                    else
                    {
                        // TODO: Emit warning for invalid timestring format                        
                    }
                }
            }

            public override string GetValueAsString()
            {
                StringBuilder result = new StringBuilder(50);
                result.Append(Hour.AsString().PadLeft(2, '0'));
                result.Append(':');
                result.Append(Minute.AsString().PadLeft(2, '0'));
                result.Append(':');
                result.Append(Second.AsString().PadLeft(2, '0'));
                result.Append('.');
                result.Append(Microsecond.AsString());
                return result.ToString();
            }

            /// <summary>
            /// returns the current encoding as string ready for the conversion
            /// to the raw buffer representation. The return value will be already
            /// padded to an even length.
            /// </summary>
            /// <returns>The encoded and to even length padded string</returns>
            public string toRawString()
            {
                StringBuilder result = new StringBuilder(16);
                result.Append(Hour.AsString().PadLeft(2, '0'));
                if (Minute.AsInteger() != 0 || Second.AsInteger() != 0 || Microsecond.AsInteger() != 0)
                {
                    result.Append(Minute.AsString().PadLeft(2, '0'));
                    if (Second.AsInteger() != 0 || Microsecond.AsInteger() != 0)
                    {
                        result.Append(Second.AsString().PadLeft(2, '0'));
                        if (Microsecond.AsInteger() != 0)
                        {
                            result.Append('.');
                            string micros = Microsecond.AsString();
                            if (micros.Length > 6)
                            {
                                micros = micros.Substring(0, 6);
                            }
                            result.Append(micros);
                        }
                    }
                }

                // padding even length values
                if ((result.Length & 1) != 0)
                {
                    result.Append(' ');
                }
                return result.ToString();
            }
        }

        #endregion

    }
}
