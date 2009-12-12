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
using openDicom.DataStructure;
using System.Text.RegularExpressions;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Date (DA).
    /// </summary>
    public sealed class Date: ValueRepresentation
    {
        public Date(Tag tag): base("DA", tag) {}
        
        public override string ToLongString()
        {
            return "Date (DA)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            System.DateTime[] date = new System.DateTime[multiValue.Length];
            for (int i = 0; i < date.Length; i++)
            {
                string item = multiValue[i];
                if (Regex.IsMatch(item, "^[0-9]{4}\\.?[0-9]{2}\\.?[0-9]{2}$"))
                {
                    item = item.Replace(".", null);
                    string year = item.Substring(0, 4);
                    string month = item.Substring(4, 2);
                    string day = item.Substring(6, 2);
                    try
                    {
                        date[i] = new System.DateTime(int.Parse(year), 
                            int.Parse(month), int.Parse(day));
                    }
                    catch (Exception e)
                    {
                        throw new EncodingException("Date format is invalid.",
                            Tag, Name + "/item", item);
                    }
                }
            }
            return date;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            System.DateTime[] date = new System.DateTime[multiValue.Length];
            for (int i = 0; i < date.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length > 0)
                {
                    if (Regex.IsMatch(item, "^[0-9]{4}\\.?[0-9]{2}\\.?[0-9]{2}$"))
                    {
                        item = item.Replace(".", null);
                        string year = item.Substring(0, 4);
                        string month = item.Substring(4, 2);
                        string day = item.Substring(6, 2);
                        try
                        {
                            date[i] = new System.DateTime(int.Parse(year), 
                                int.Parse(month), int.Parse(day));
                        }
                        catch (Exception e)
                        {
                            throw new EncodingException("Date format is invalid.",
                                Tag, Name + "/item", item);
                        }
                    }
                    else
                        throw new EncodingException("Date format is invalid.",
                            Tag, Name + "/item", item);
                }
            }
            return date;
        }

        protected override byte[] Encode(Array array)
        {
            System.DateTime[] date = array as System.DateTime[];            
            string[] multiValue = new string[date.Length];
            for (int i = 0; i < date.Length; i++)
            {
                System.DateTime d = date[i];
                multiValue[i] = 
                    d.Year.ToString() +
                    d.Month.ToString() + 
                    d.Day.ToString();
            }
            string s = ToJointMultiValue(multiValue);
            return TransferSyntax.ToBytes(s);
        }
    }

}
