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
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;

namespace openDicom.Image
{

    /// <summary>
    ///     Basic class for working with DICOM Palette Color Lookup Tables.
    /// </summary>
    public sealed class PaletteColorLookupTable
    {
        /// <summary>
        ///     DICOM tag (0028,1101).
        /// </summary>
        public static readonly Tag RedPaletteColorLutDescriptorTag  = 
            new Tag("0028", "1101");

        /// <summary>
        ///     DICOM tag (0028,1102).
        /// </summary>
        public static readonly Tag GreenPaletteColorLutDescriptorTag  = 
            new Tag("0028", "1102");

        /// <summary>
        ///     DICOM tag (0028,1103).
        /// </summary>
        public static readonly Tag BluePaletteColorLutDescriptorTag  = 
            new Tag("0028", "1103");

        /// <summary>
        ///     DICOM tag (0028,1201).
        /// </summary>
        public static readonly Tag RedPaletteColorLutDataTag  = 
            new Tag("0028", "1201");

        /// <summary>
        ///     DICOM tag (0028,1202).
        /// </summary>
        public static readonly Tag GreenPaletteColorLutDataTag  = 
            new Tag("0028", "1202");

        /// <summary>
        ///     DICOM tag (0028,1203).
        /// </summary>
        public static readonly Tag BluePaletteColorLutDataTag  = 
            new Tag("0028", "1203");

        /// <summary>
        ///     Creates a palette color LUT instance from the specified data
        ///     set.
        /// </summary>
        public PaletteColorLookupTable (DataSet dataSet)
        {
            LoadFrom(dataSet);
        }

        /// <summary>
        ///     Re-creates a palette color LUT instance from the specified data
        ///     set.
        /// </summary>
        public void LoadFrom(DataSet dataSet)
        {
            if (dataSet != null)
            {
                foreach (DataElement element in dataSet)
                {
                    if (element.Tag.Equals(RedPaletteColorLutDescriptorTag))
                        samplesPerPixel = ToValue(element);
                    else if (element.Tag.Equals(GreenPaletteColorLutDescriptorTag))
                        planarConfiguration = ToValue(element);
                    else if (element.Tag.Equals(BluePaletteColorLutDescriptorTag))
                        rows = ToValue(element);
                    else if (element.Tag.Equals(RedPaletteColorLutDataTag))
                        columns = ToValue(element);
                    else if (element.Tag.Equals(GreenPaletteColorLutDataTag))
                        bitsAllocated = ToValue(element);
                    else if (element.Tag.Equals(BluePaletteColorLutDataTag))
                        bitsStored = ToValue(element);
                }
            }
            else
                throw new DicomException("Data set is null.", "dataSet");
        }

        /// <summary>
        ///     Determines whether specified data set contains the minimum
        ///     of necessary content for working with palette color LUT.
        /// </summary>
        public static bool IsValid(DataSet dataSet)
        {
            if (dataSet != null)
                return dataSet.Contains(RedPaletteColorLutDescriptorTag)
                    && dataSet.Contains(GreenPaletteColorLutDescriptorTag)
                    && dataSet.Contains(BluePaletteColorLutDescriptorTag)
                    && dataSet.Contains(RedPaletteColorLutDataTag)
                    && dataSet.Contains(GreenPaletteColorLutDataTag)
                    && dataSet.Contains(BluePaletteColorLutDataTag);
            else
                return false;
        }

	    public byte[] ColorizeImage(byte[] grayImage, 
            int grayValueBits)
        {
            // Are Look-Up-Tables available?
            Tag[] lutDescriptorTag =  { 
                RedPaletteColorLutDescriptorTag, 
                GreenPaletteColorLutDescriptorTag, 
                BluePaletteColorLutDescriptorTag };
            Tag[] lutDataTag = { 
                RedPaletteColorLutDataTag, 
                GreenPaletteColorLutDataTag, 
                BluePaletteColorLutDataTag };
       	    bool isColorized = IsValid();
            int[] entryCount = new int[3];
            int[] startValue = new int[3];
            int[] lutBits = new int[3];
            ushort[][] lutData = new ushort[3][];
            ushort[] minLutValue = { ushort.MaxValue, ushort.MaxValue, 
                ushort.MaxValue };
            ushort[] maxLutValue = { ushort.MinValue, ushort.MinValue,
                ushort.MinValue };
            ushort[] lutValueRange = new ushort[3];
            if (isColorized)
            {
                for (int i = 0; i < 3; i++)
                {
                    entryCount[i] =
                       (int) (ushort) DicomFile.DataSet[lutDescriptorTag[i]].
                            Value[0];
                    startValue[i] =
                       (int) (ushort) DicomFile.DataSet[lutDescriptorTag[i]].
                            Value[1];
                    lutBits[i] =
                       (int) (ushort) DicomFile.DataSet[lutDescriptorTag[i]].
                            Value[2];
                    lutData[i] =
                       (ushort[]) DicomFile.DataSet[lutDataTag[i]].Value[0];
                    for (int k = 0; k < lutData[i].Length; k++)
                    {
                        if (minLutValue[i] > lutData[i][k]) 
                            minLutValue[i] = lutData[i][k];
                        if (maxLutValue[i] < lutData[i][k]) 
                            maxLutValue[i] = lutData[i][k];
                    }
                    lutValueRange[i] = (ushort) (maxLutValue[i] - minLutValue[i]);
                }
            }

            byte[] rgbImage;
            if (grayValueBits <= 8)
            {
                rgbImage = new byte[grayImage.Length * 3];
                for (int i = 0; i < grayImage.Length; i++)
                {
                    if (isColorized)
                    {
                        rgbImage[i * 3] = (byte) Math.Round(
                            ((lutData[0][grayImage[i]] - minLutValue[0]) * 
                                (double) byte.MaxValue) / 
                            (double) lutValueRange[0]);
                        rgbImage[i * 3 + 1] = (byte) Math.Round(
                            ((lutData[1][grayImage[i]] - minLutValue[1]) * 
                                (double) byte.MaxValue) / 
                            (double) lutValueRange[1]);
                        rgbImage[i * 3 + 2] = (byte) Math.Round(
                            ((lutData[2][grayImage[i]] - minLutValue[2]) * 
                                (double) byte.MaxValue) / 
                            (double) lutValueRange[2]);
                    }
                    else
                    {
                        rgbImage[i * 3] = grayImage[i];
                        rgbImage[i * 3 + 1] = grayImage[i];
                        rgbImage[i * 3 + 2] = grayImage[i];
                    }
                }
            }
            else if (grayValueBits <= 16)
            {
                rgbImage = new byte[grayImage.Length * 3/2];
                ushort[] words = new ushort[grayImage.Length / 2];
                byte reducedValue;
                ushort minWordValue = ushort.MaxValue;
                ushort maxWordValue = ushort.MinValue;
                int i;
                for (i = 0; i < words.Length; i++)
                {
                    words[i] = BitConverter.ToUInt16(grayImage, i * 2);
                    if (minWordValue > words[i]) minWordValue = words[i];
                    if (maxWordValue < words[i]) maxWordValue = words[i];
                }
                ushort wordRange = (ushort) (maxWordValue - minWordValue);
                for (i = 0; i < words.Length; i++)
                {
                    if (isColorized)
                    {
                        rgbImage[i * 3] = (byte) Math.Round(
                            ((lutData[0][words[i]] - minLutValue[0]) * 
                                (double) byte.MaxValue) / 
                            (double) lutValueRange[0]);
                        rgbImage[i * 3 + 1] = (byte) Math.Round(
                            ((lutData[1][words[i]] - minLutValue[1]) * 
                                (double) byte.MaxValue) / 
                            (double) lutValueRange[1]);
                        rgbImage[i * 3 + 2] = (byte) Math.Round(
                            ((lutData[2][words[i]] - minLutValue[2]) * 
                                (double) byte.MaxValue) /
                            (double) lutValueRange[2]);
                    }
                    else
                    {
                        reducedValue = (byte) Math.Round(
                            ((words[i] - minWordValue) * 
                                (double) byte.MaxValue) / (double) wordRange);
                        rgbImage[i * 3] = reducedValue;
                        rgbImage[i * 3 + 1] = reducedValue;
                        rgbImage[i * 3 + 2] = reducedValue;
                    }
                }
            }
            else
            {
                rgbImage = new byte[grayImage.Length];
                rgbImage = grayImage;
            }
            return rgbImage;
        }
    }
}
