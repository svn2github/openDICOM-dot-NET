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


    $Id: $
*/
using System;


namespace openDicom.Image
{

    /// <summary>
    ///     Basic class for working with DICOM images. This class is an
    ///     additional layer on top of DICOM pixel data in order to provide
    ///     enhanced image processing routines.
    /// </summary>
    public sealed class Image
    {
        public Image ()
        {
        }

        /// <summary>
        ///     Implementation of the DICOM 2007 RLE Decoder.
        /// </summary>
        public byte[] DecodeRLE(byte[] buffer)
        {
            // RLE encoded image structure:
            //   [Header]
            //   [RLE Segment 1]
            //   [RLE Segment 2]
            //          :
            //   [RLE Segment N]

            // Header structure:
            //   [Number of Segments]
            //   [Offset of RLE Segment 1]
            //   [Offset of RLE Segment 2]
            //          :
            //   [Offset of RLE Segment N]
    
            // Max(N) = 15

            uint[] header = new uint[16];
            int i;
            // get header
            for (i = 0; i < header.Length; i++)
                header[i] = BitConverter.ToUInt32(buffer, i * 4);
            int numberOfSegments = 1;
            if (header[0] > 1 && header[0] <= (uint) header.Length - 1)
                numberOfSegments = (int) header[0];
            uint[] offsetOfSegment = new uint[numberOfSegments];
            Array.Copy(header, 1, offsetOfSegment, 0, numberOfSegments);

            uint[] sizeOfSegment = new uint[numberOfSegments];
            int sizeSum = 0;
            // calculate the size of each single RLE segment and the sum over all
            // RLE segments
            for (i = 0; i < numberOfSegments - 1; i++)
            {
                sizeOfSegment[i] = offsetOfSegment[i + 1] - offsetOfSegment[i];
                sizeSum += (int) sizeOfSegment[i];
            }
            sizeOfSegment[numberOfSegments - 1] =
                (uint) buffer.Length - offsetOfSegment[numberOfSegments - 1];
            sizeSum += (int) sizeOfSegment[numberOfSegments - 1];

            // we don't know the resulting size of the decoded segments
            // byte segments are the decoded RLE segments
            ArrayList[] byteSegmentBuffer = new ArrayList[numberOfSegments];

            int offset;
            int size;
            byte[] rleSegment;
            sbyte n;
            int rleIndex;
            int j;
            for (i = 0; i < numberOfSegments; i++)
            {
                offset = (int) offsetOfSegment[i];
                size = (int) sizeOfSegment[i];
                rleSegment = new byte[size];
                Buffer.BlockCopy(buffer, offset, rleSegment, 0, size);
                rleIndex = 0;
                byteSegmentBuffer[i] = new ArrayList();

                // the decoding algorithm
                while (rleIndex < size)
                {
                    n = (sbyte) rleSegment[rleIndex];
                    if (n >= 0 && n <= 127)
                    {
                        for (j = 0; j < n + 1; j++)
                        {
                            rleIndex++;
                            if (rleIndex >= size) break;
                            byteSegmentBuffer[i].Add(rleSegment[rleIndex]);
                        }
                    }
                    else if (n <= -1 && n >= -127)
                    {
                        rleIndex++;
                        if (rleIndex >= size) break;
                        for (j = 0; j < -n + 1; j++)
                            byteSegmentBuffer[i].Add(rleSegment[rleIndex]);
                    }
                    rleIndex++;
                }
            }
            
            // creates an image from the decoded byte segments
            // the composite pixel code image is a sequential merge of bytes
            // from the same position from all byte segments
           byte[][] byteSegment = new byte[numberOfSegments][];
           for (i = 0; i < numberOfSegments; i++)
           {
               byteSegment[i] = 
                   (byte[]) byteSegmentBuffer[i].ToArray(typeof(byte));
           }
           byteSegmentBuffer = null;
           byte[] compositePixelCodeImage = 
               new byte[numberOfSegments * byteSegment[0].Length];
           for (i = 0; i < byteSegment[0].Length; i++)
           {
               for (j = 0; j < numberOfSegments; j++)
                   compositePixelCodeImage[(i * numberOfSegments) + j] = 
                       byteSegment[j][i];            
           }
           byteSegment = null;
           System.GC.Collect();
           return compositePixelCodeImage;
		}
    }
}
