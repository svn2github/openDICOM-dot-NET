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
using System.IO;
using openDicom.Encoding;


namespace openDicom.DataStructure
{

    /// <summary>
    ///     DICOM member stream interface for classes that participate in a
    ///     DICOM data stream according to the DICOM standard. If a DICOM class
    ///     is binary represented on a DICOM stream, it will have to implement
    ///     this interface.
    /// </summary>      
    public interface IDicomStreamMember
    {
        /// <summary>
        ///     DICOM transfer syntax.
        /// </summary>      
        TransferSyntax TransferSyntax { get; }

        /// <summary>
        ///     Position within a DICOM data stream.
        /// </summary>      
        long StreamPosition { get; }

        /// <summary>
        ///     Re-creates a DICOM stream member instance from a specified
        ///     DICOM output stream.
        /// </summary>      
        void LoadFrom(Stream stream);

        /// <summary>
        ///     Saves a DICOM stream member instance to a specified
        ///     DICOM input stream.
        /// </summary>      
        void SaveTo(Stream stream);
    }
    
}
