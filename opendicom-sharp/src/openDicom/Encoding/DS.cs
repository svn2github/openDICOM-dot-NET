/*
   
    openDICOM.NET openDICOM# 0.2

    openDICOM# provides a library for DICOM related development on Mono.
    Copyright (C) 2006-2007  Albert Gnandt

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA


    $Id$
*/
using System;
using System.Globalization;
using openDicom;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Decimal String (DS).
    /// </summary>
    public sealed class DecimalString: ValueRepresentation
    {
        public DecimalString(Tag tag): base("DS", tag) {}
        
        public override string ToLongString()
        {
            return "Decimal String (DS)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            decimal[] decimalValue = new decimal[multiValue.Length];
            for (int i = 0; i < decimalValue.Length; i++)
            {
                string item = multiValue[i];
                item = item.Trim();
                try
                {
                    if (item.Length > 0)
                        decimalValue[i] = decimal.Parse(item,
                            NumberStyles.Float, 
                            NumberFormatInfo.InvariantInfo);
                }
                catch (Exception e)
                {
                    throw new EncodingException(
                        "Decimal string format is invalid.",
                        Tag, Name + "/item", item);
                }
            }
            return decimalValue;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            decimal[] decimalValue = new decimal[multiValue.Length];
            for (int i = 0; i < decimalValue.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length <= 16)
                {
                    item = item.Trim();
                    try
                    {
                        if (item.Length > 0)
                            decimalValue[i] = decimal.Parse(item,
                                NumberStyles.Float, 
                                NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new EncodingException(
                            "Decimal string format is invalid.",
                            Tag, Name + "/item", item);
                    }
                }
                else
                    throw new EncodingException(
                        "A value of max. 16 bytes is only allowed.",
                        Tag, Name + "/item", item);
            }
            return decimalValue;
        }

        protected override byte[] Encode(Array array)
        {
            decimal[] decimalValue = array as decimal[];
            string[] multiValue = new string[decimalValue.Length];
            for (int i = 0; i < decimalValue.Length; i++)
            {
                string item = decimalValue[i].ToString();
                if (item.Length > 16)
                {
                    throw new EncodingException(
                        "A value of max. 16 bytes is only allowed.",
                        Tag, Name + "/item", item);
                }
                multiValue[i] = item;
            }
            string s = ToJointMultiValue(multiValue);
            return TransferSyntax.ToBytes(s);
        }
    }

}
