/* Gobosh.DICOM
 * Consts for usage all over the Gobosh.DICOM namespace.
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
    namespace DICOM
    {
        class Consts
        {
            public const int UndefinedLength = -1;
            public const int MetaGroupNumber = 0x0002;
            public const int MetaGroupTransferSyntax = 0x0010;
            public const string ISOTransferSyntaxImplicitLittleEndian = "1.2.840.10008.1.2";
            public const string ISOTransferSyntaxExplicitLittleEndian = "1.2.840.10008.1.2.1";
            public const string ISOTransferSyntaxExplicitBigEndian = "1.2.840.10008.1.2.2";
            
        }
    }
}