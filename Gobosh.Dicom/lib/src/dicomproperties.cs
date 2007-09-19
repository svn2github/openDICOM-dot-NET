/* Gobosh.DICOM
 * Scalar property value handling
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

namespace Gobosh
{
    namespace DICOM
    {
        #region VarProperty - base class for Properties

        /// <summary>
        /// VarProperty is a base class for properties of an DataElementValue
        /// </summary>
        public abstract class VarProperty
        {
            /// <summary>
            /// a string storing the name of the Property
            /// </summary>
            private string PropName;

            /// <summary>
            /// a string property for the name
            /// </summary>
            public string Name
            {
                set { PropName = Name; }
                get { return PropName; }
            }

            // constructor
            public VarProperty(string name)
            {
                PropName = name;
            }

            // ---------------- the getter functions
            /// <summary>
            /// Returns the value as an integer
            /// </summary>
            /// <returns>Value as 32bit int</returns>
            abstract public Int32 AsInteger();

            /// <summary>
            /// Returns the value as a float
            /// </summary>
            /// <returns>Value as 32 bit float</returns>
            abstract public float AsFloat();

            /// <summary>
            /// Returns the value as a double
            /// </summary>
            /// <returns>Value as 80 bit double</returns>
            abstract public double AsDouble();

            /// <summary>
            /// Returns a true string representation as unicode stirng
            /// </summary>
            /// <returns>Unicode string of the value</returns>
            abstract public string AsString();

            // ---------------- the setter functions


            abstract public void Set(Int32 newValue);
            abstract public void Set(float newValue);
            abstract public void Set(double newValue);
            abstract public void Set(string newValue);
        }
        #endregion

        #region Integer
        public class IntProperty : VarProperty
        {

            public IntProperty(string name)
                : base(name)
            {
            }

            public IntProperty(string name, Int32 number)
                : base(name)
            {
                mInteger = number;
            }

            private Int32 mInteger;

            /// <summary>
            /// Returns the value as an integer
            /// </summary>
            /// <returns>Value as 32bit int</returns>
            override public Int32 AsInteger()
            {
                return mInteger;
            }

            /// <summary>
            /// Returns the value as a float
            /// </summary>
            /// <returns>Value as 32 bit float</returns>
            override public float AsFloat()
            {
                return (mInteger * 1.0f);
            }

            /// <summary>
            /// Returns the value as a double
            /// </summary>
            /// <returns>Value as 80 bit double</returns>
            override public double AsDouble()
            {
                return (mInteger * 1.0);
            }

            /// <summary>
            /// Returns a true string representation as unicode stirng
            /// </summary>
            /// <returns>Unicode string of the value</returns>
            override public string AsString()
            {
                return mInteger.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            override public void Set(Int32 newValue)
            {
                mInteger = newValue;
            }
            override public void Set(float newValue)
            {
                mInteger = (Int32)Math.Round(newValue);
                // TODO: Emit a warning about possible format conversion
            }

            override public void Set(double newValue)
            {
                mInteger = (Int32)Math.Round(newValue);
                // TODO: Emit a warning about possible format conversion
            }

            override public void Set(string newValue)
            {
                mInteger = Int32.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        #endregion

        #region Float
        public class FloatProperty : VarProperty
        {

            public FloatProperty(string name)
                : base(name)
            {
            }

            private float mFloat;

            /// <summary>
            /// Returns the value as an integer
            /// </summary>
            /// <returns>Value as 32bit int</returns>
            override public Int32 AsInteger()
            {
                // TODO: Emit error about rounding problems
                throw new Exception("FloatProperty can not be easily converted to Integer: Use explicit conversion!");
                // return (Int32) Math.Round(mFloat);
            }

            /// <summary>
            /// Returns the value as a float
            /// </summary>
            /// <returns>Value as 32 bit float</returns>
            override public float AsFloat()
            {
                return mFloat;
            }

            /// <summary>
            /// Returns the value as a double
            /// </summary>
            /// <returns>Value as 80 bit double</returns>
            override public double AsDouble()
            {
                return mFloat;
            }

            /// <summary>
            /// Returns a true string representation as unicode stirng
            /// </summary>
            /// <returns>Unicode string of the value</returns>
            override public string AsString()
            {
                return mFloat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            override public void Set(Int32 newValue)
            {
                mFloat = newValue * 1.0f;
            }
            override public void Set(float newValue)
            {
                mFloat = newValue;
                // TODO: Emit a warning about possible format conversion
            }

            override public void Set(double newValue)
            {
                throw new Exception("Double assigned to a float value, try to use an explicit conversion"); 
                //mFloat = newValue * 1.0f;
                // TODO: Emit a warning about possible format conversion
            }

            override public void Set(string newValue)
            {
                mFloat = float.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        #endregion

        #region Double
        public class DoubleProperty : VarProperty
        {
            public DoubleProperty(string name)
                : base(name)
            {
                mDouble = 0.0;
            }

            public DoubleProperty(string name, double value)
                : base(name)
            {
                mDouble = value;
            }

            public DoubleProperty(string name, string value)
                : base(name)
            {
                mDouble = double.Parse(value, 
                    System.Globalization.NumberStyles.AllowDecimalPoint, 
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            private double mDouble;

            /// <summary>
            /// Returns the value as an integer
            /// </summary>
            /// <returns>Value as 32bit int</returns>
            override public Int32 AsInteger()
            {
                // TODO: Emit error about rounding problems
                throw new Exception("FloatProperty can not be easily converted to Integer: Use explicit conversion!");
                // return (Int32) Math.Round(mFloat);
            }

            /// <summary>
            /// Returns the value as a float
            /// </summary>
            /// <returns>Value as 32 bit float</returns>
            override public float AsFloat()
            {
                throw new Exception("Double shouldn't be converted to Float directly: Use explicit AsDouble() instead!");
            }

            /// <summary>
            /// Returns the value as a double
            /// </summary>
            /// <returns>Value as 80 bit double</returns>
            override public double AsDouble()
            {
                return mDouble;
            }

            /// <summary>
            /// Returns a true string representation as unicode stirng
            /// </summary>
            /// <returns>Unicode string of the value</returns>
            override public string AsString()
            {
                return mDouble.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            override public void Set(Int32 newValue)
            {
                mDouble = newValue * 1.0f;
            }
            override public void Set(float newValue)
            {
                mDouble = newValue;
            }

            override public void Set(double newValue)
            {
                mDouble = newValue;
            }

            override public void Set(string newValue)
            {
                mDouble = double.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        #endregion

        #region String
        public class StringProperty : VarProperty
        {

            public StringProperty(string name)
                : base(name)
            {
                mString = "";
            }

            private string mString;

            /// <summary>
            /// Returns the value as an integer
            /// </summary>
            /// <returns>Value as 32bit int</returns>
            override public Int32 AsInteger()
            {
                return Int32.Parse(mString, System.Globalization.CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Returns the value as a float
            /// </summary>
            /// <returns>Value as 32 bit float</returns>
            override public float AsFloat()
            {
                return float.Parse(mString, System.Globalization.CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Returns the value as a double
            /// </summary>
            /// <returns>Value as 80 bit double</returns>
            override public double AsDouble()
            {
                return double.Parse(mString, System.Globalization.CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Returns a true string representation as unicode stirng
            /// </summary>
            /// <returns>Unicode string of the value</returns>
            override public string AsString()
            {
                return mString;
            }

            override public void Set(Int32 newValue)
            {
                mString = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            override public void Set(float newValue)
            {
                mString = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            override public void Set(double newValue)
            {
                mString = newValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            override public void Set(string newValue)
            {
                mString = newValue;
            }

            public int Length
            {
                get
                {
                    return mString.Length;
                }
            }

        }
        #endregion
    }
}