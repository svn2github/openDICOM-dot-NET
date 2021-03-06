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
using System.IO;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Sequence of Items (SQ).
    /// </summary>
    public sealed class SequenceOfItems: ValueRepresentation
    {
        public SequenceOfItems(Tag tag): base("SQ", tag) {}
        
        public override string ToLongString()
        {
            return "Sequence Of Items (SQ)";
        }

        /// <summary>
        ///     Sequences are special cases that are normally taken care of by
        ///     the class Value, use this method if you know what you are doing.
        /// </summary>
        protected override Array DecodeImproper(byte[] bytes)
        {
            return DecodeProper(bytes);
        }
 
        /// <summary>
        ///     Sequences are special cases that are normally taken care of by
        ///     the class Value, use this method if you know what you are doing.
        /// </summary>
        protected override Array DecodeProper(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream(bytes);
            Sequence sq = new Sequence(memoryStream);
            return new Sequence[1] { sq };
        }
                
        /// <summary>
        ///     Sequences are special cases that are normally taken care of by
        ///     the class Value, use this method if you know what you are doing.
        /// </summary>
        protected override byte[] Encode (Array array)
        {
            Sequence sq = (array as Sequence[])[0];
            MemoryStream memoryStream = new MemoryStream();
            sq.SaveTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

}
