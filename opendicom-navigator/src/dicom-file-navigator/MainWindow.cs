/*
    openDICOM.NET Navigator 0.1.1

    Simple GTK ACR-NEMA and DICOM Viewer for Mono / .NET based on the 
    openDICOM.NET library.

    Copyright (C) 2006-2007  Albert Gnandt

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA


    $Id$
*/
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using Gtk;
using Glade;
using Gdk;
using GLib;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding.Type;
using openDicom.File;


public sealed class MainWindow: GladeWidget
{
    public new Gtk.Window Self
    {
        get { return (Gtk.Window) base.Self; }
    }

    private AcrNemaFile dicomFile = null;
    public AcrNemaFile DicomFile
    {
        get { return dicomFile; }
    }

    private string defaultTitle = null;

    private byte[][] images;
    private int imageIndex = 0;
    private double scaleFactor = 1.0;
    private bool isSlideCycling = false;
    private uint slideCyclingIdleHandler;

    private bool treeViewLoaded = false;

    private const double scaleStep = 0.10;
    private const double minScaleFactor = 0.10;
    private const double maxScaleFactor = 8.0;

    private const double brightnessStep = 0.05;
    private const double minBrightnessFactor = 0.0;
    private const double maxBrightnessFactor = 2.0;

    private bool CorrectIndex
    {
        get { return DicomFile.PixelData.Data.Value.IsSequence; }
    }

    // officially not part of the DICOM data dictionary, but nice to have
    private static readonly string[,] groupDescription =
    {
        { "0002", "File Meta Information" },
        { "0003", "Directory Structuring" },
        { "0008", "Identification" },
        { "0010", "Patient Data" },
        { "0012", "Clinical Trial" },
        { "0013", "Modifying Physician" },
        { "0018", "Acquisition" },
        { "0020", "Image" },
        { "0022", "Ophthalmologistic Data" },
        { "0028", "Image Presentation" },
        { "0032", "Study" },
        { "0038", "Patient Administration" },
        { "003A", "Waveform" },
        { "0040", "Procedure" },
        { "0050", "Calibration" },
        { "0054", "Nuclear Acquisition" },
        { "0060", "Histogram" },
        { "0070", "Graphic" },
        { "0088", "Storage" },
        { "0100", "Authorization" },
        { "0400", "Encryption" },
        { "2000", "Media" },
        { "2010", "Film Box" },
        { "2020", "???" },
        { "2030", "Annotation" },
        { "2040", "Image Overlay Box" },
        { "2050", "Presentation Look-Up-Table" },
        { "2100", "Print Job" },
        { "2110", "Printer Status" },
        { "2120", "Printer Queue Status" },
        { "2130", "Print Management" },
        { "3002", "Radiotherapy Image" },
        { "3004", "Dose Volume Histogram" },
        { "3006", "Region Of Interest" },
        { "3008", "Dose Control" },
        { "300A", "Radiotherapy Acquisition" },
        { "300C", "Radiotherapy Reference" },
        { "300E", "Approval" },
        { "4008", "Result" },
        { "4FFE", "MAC Parameter" },
        { "50xx", "Curve Data" },
        { "5200", "Functional Group" },
        { "5400", "Waveform" },
        { "5600", "Spectroscopy" },
        { "60xx", "Overlay" },
        { "7FE0", "Pixel Data" },
        { "FFFA", "Digital Signature" },
        { "FFFC", "Trailing Padding" },
        { "FFFE", "Item" }
    };


    [WidgetAttribute]
    Notebook MainNotebook;

    [WidgetAttribute]
    TreeView MainTreeView;

    [WidgetAttribute]
    TreeView MainFileInfoTreeView;

    [WidgetAttribute]
    TextView ContentTextView;

    [WidgetAttribute]
    Gtk.Image ImageViewImage;

    [WidgetAttribute]
    Viewport ImageViewViewport;

    [WidgetAttribute]
    Statusbar ImageViewStatusbar;

    [WidgetAttribute]
    HScale CyclingSpeedHScale;
    
    [WidgetAttribute]
    Label TimeUnitLabel;

    [WidgetAttribute]
    ToolButton FirstSlideToolButton;

    [WidgetAttribute]
    ToolButton PrevSlideToolButton;

    [WidgetAttribute]
    ToolButton CycleSlidesToolButton;

    [WidgetAttribute]
    ToolButton NextSlideToolButton;

    [WidgetAttribute]
    ToolButton LastSlideToolButton;

    [WidgetAttribute]
    ToolButton ZoomInToolButton;

    [WidgetAttribute]
    ToolButton OriginalSizeToolButton;

    [WidgetAttribute]
    ToolButton FitWindowToolButton;

    [WidgetAttribute]
    ToolButton ZoomOutToolButton;

    [WidgetAttribute]
    ToolButton OpenToolButton;

    [WidgetAttribute]
    ToolButton PreferencesToolButton;

    [WidgetAttribute]
    ToolButton SaveImageToolButton;

    [WidgetAttribute]
    ToolButton ExportAsToolButton;

    [WidgetAttribute]
    ToolButton BrightenToolButton;
    
    [WidgetAttribute]
    ToolButton DarkenToolButton;

    [WidgetAttribute]
    ToolButton GimpToolButton;

    [WidgetAttribute]
    ImageMenuItem OpenMenuItem;

    [WidgetAttribute]
    ImageMenuItem SaveImageMenuItem;

    [WidgetAttribute]
    ImageMenuItem PreferencesMenuItem;

    [WidgetAttribute]
    ImageMenuItem ExportAsMenuItem;

    [WidgetAttribute]
    MenuItem ExpandTreeMenuItem;

    [WidgetAttribute]
    MenuItem CollapseTreeMenuItem;


    public MainWindow(string fileName): base("MainWindow")
    {
        Self.Icon = Pixbuf.LoadFromResource(Resources.IconResource);
        BrightenToolButton.IconWidget = Gtk.Image.LoadFromResource(
            Resources.StockBrightenResource);
        BrightenToolButton.IconWidget.Show();
        DarkenToolButton.IconWidget = Gtk.Image.LoadFromResource(
            Resources.StockDarkenResource);
        DarkenToolButton.IconWidget.Show();
        GimpToolButton.IconWidget = Gtk.Image.LoadFromResource(
            Resources.GimpIconResource);
        GimpToolButton.IconWidget.Show();
        defaultTitle = Self.Title;
        CyclingSpeedHScale.Value = Configuration.Global.SlideCyclingSpeed;
        SaveImageMenuItem.Sensitive = false;
        SaveImageToolButton.Sensitive = false;
        ExportAsMenuItem.Sensitive = false;
        ExportAsToolButton.Sensitive = false;
        FirstSlideToolButton.Sensitive = false;
        PrevSlideToolButton.Sensitive = false;
        CycleSlidesToolButton.Sensitive = false;
        TimeUnitLabel.Sensitive = false;
        NextSlideToolButton.Sensitive = false;
        LastSlideToolButton.Sensitive = false;
        ZoomInToolButton.Sensitive = false;
        OriginalSizeToolButton.Sensitive = false;
        FitWindowToolButton.Sensitive = false;
        ZoomOutToolButton.Sensitive = false;
        CyclingSpeedHScale.Sensitive = false;
        BrightenToolButton.Sensitive = false;
        DarkenToolButton.Sensitive = false;
        GimpToolButton.Sensitive = false;
        MainTreeView.AppendColumn("Tag", new CellRendererText(), "text", 0);
        MainTreeView.AppendColumn("Description", new CellRendererText(), 
            "text", 1);
        MainTreeView.AppendColumn("Value", new CellRendererText(), "text", 
            2);
        MainTreeView.AppendColumn("Value Representation", 
            new CellRendererText(), "text", 3);
        MainTreeView.AppendColumn("Value Multiplicity",
            new CellRendererText(), "text", 4);
        MainTreeView.AppendColumn("Value Length", new CellRendererText(),
            "text", 5);
        MainTreeView.AppendColumn("System Type", new CellRendererText(),
            "text", 6);
        MainTreeView.AppendColumn("Stream Position", new CellRendererText(),
            "text", 7);
        // Does not work with Glade!
        MainTreeView.CursorChanged += OnMainTreeViewCursorChanged;
        MainFileInfoTreeView.AppendColumn("Key", new CellRendererText(),
            "text", 0);
        MainFileInfoTreeView.AppendColumn("Value", new CellRendererText(),
            "text", 1);
        ContentTextView.ModifyFont(
            Pango.FontDescription.FromString("Monospace"));
        // raises page switch event
        MainNotebook.Page = 1;
        LoadDicomFile(fileName);
    }

    private void OnMainWindowDeleteEvent(object o, DeleteEventArgs args)
    {
        Configuration.Global.SlideCyclingSpeed = (int) CyclingSpeedHScale.Value;
        Application.Quit();
        args.RetVal = true;
    }

    private void OnQuitMenuItemActivate(object o, EventArgs args) 
    {
        Configuration.Global.SlideCyclingSpeed = (int) CyclingSpeedHScale.Value;
        Application.Quit();
    }

    private void OnExpandTreeMenuItemActivate(object o, EventArgs args) 
    {
        MainTreeView.ExpandAll();
    }

    private void OnCollapseTreeMenuItemActivate(object o, EventArgs args) 
    {
        MainTreeView.CollapseAll();
    }

    private void OnAboutMenuItemActivate(object o, EventArgs args) 
    {
        AboutDialog d = new AboutDialog();
        d.Self.Run();
    }

    private void LoadDicomFile(string fileName)
    {
        if (fileName != null && fileName != "")
        {
            if (File.Exists(fileName))
            {
                if (Configuration.Global.AreDictionariesAvailable)
                {
                    bool useStrictDecoding = 
                        Configuration.Global.UseStrictDecoding;
                    try
                    {
                        if (openDicom.File.DicomFile.IsDicomFile(fileName))
                        {
                            dicomFile = new DicomFile(fileName, 
                                useStrictDecoding);
                            PostDicomFileLoad(fileName);
                        }
                        else if (AcrNemaFile.IsAcrNemaFile(fileName))
                        {
                            dicomFile = new AcrNemaFile(fileName,
                                useStrictDecoding);
                            PostDicomFileLoad(fileName);
                        }
                        else if (XmlFile.IsXmlFile(fileName))
                        {
                            MessageDialog(MessageType.Error,
                                "Found DICOM-/ACR-NEMA-XML file instead of " +
                                "DICOM file. This function is not implemented.");
                        }
                        else
                        {
                            MessageDialog(MessageType.Error,
                                "User specified file is whether " +
                                "a DICOM, ACR-NEMA nor a compliant XML file.");
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionDialog d = new ExceptionDialog(
                            "Unexpected problems loading file.", e);
                        d.Self.Run();
                    }
                }
                else
                    MessageDialog(MessageType.Error,
                        "Please select valid data dictionaries " +
                        "before loading. Use the preferences dialog for " +
                        "registration.");
            }
            else
                MessageDialog(MessageType.Error,
                    "User specified file does not exists.");
        }
    }

    private void PrepareImages()
    {
        if (DicomFile.HasPixelData)
            images = DicomFile.PixelData.ToBytesArray();
        Tag numberOfFramesTag = new Tag("0028", "0008");
        if (DicomFile.DataSet.Contains(numberOfFramesTag))
        {
            int frames = int.Parse(
                DicomFile.DataSet[numberOfFramesTag].Value[0].ToString());
            if (frames > 1 && 
                ((CorrectIndex && images.Length == 2) || images.Length == 1))
            {
                byte[] buffer = images[images.Length - 1];
                int size = buffer.Length / frames;
                byte[][] results = new byte[frames][];
                int i = 0;
                for (i = 0; i < frames; i++)
                {
                    results[i] = new byte[size];
                    Array.Copy(buffer, i * size, results[i], 0, size);
                }
                if (CorrectIndex)
                {
                    buffer = images[0];
                    images = new byte[frames + 1][];
                    images[0] = buffer;
                    for (i = 0; i < frames; i++)
                        images[i + 1] = results[i];
                }
                else    
                    images = results;
            }
        }
        if (DicomFile.DataSet.TransferSyntax.Uid.Equals("1.2.840.10008.1.2.5"))
        {
            // RLE
            int startIndex = CorrectIndex ? 1 : 0;
            byte[][] tempImages = images;
            try
            {
                for (int i = startIndex; i < images.Length; i++)
                    images[i] = DecodeRLE(images[i]);
            }
            catch (Exception e)
            {
                images = tempImages;
                tempImages = null;
                System.GC.Collect();
                MessageDialog(MessageType.Error, 
                    "Unable to RLE decode images.");
            }
        }
    }

    private byte[] DecodeRLE(byte[] buffer)
    {
        // Implementation of the DICOM 3.0 2004 RLE Decoder
        ulong[] header = new ulong[16];
        for (int i = 0; i < header.Length; i++)
            header[i] = BitConverter.ToUInt64(buffer, i * 4);
        int numberOfSegments = 1;
        if (header[0] > 1 && header[0] <= (ulong) header.LongLength - 1)
            numberOfSegments = (int) header[0];
        ulong[] offsetOfSegment = new ulong[numberOfSegments];
        Buffer.BlockCopy(header, 1, offsetOfSegment, 0, numberOfSegments);
        ulong[] sizeOfSegment = new ulong[numberOfSegments];
        int sizeSum = 0;
        for (int i = 0; i < numberOfSegments - 1; i++)
        {
            sizeOfSegment[i] = offsetOfSegment[i + 1] - offsetOfSegment[i];
            sizeSum += (int) sizeOfSegment[i];
        }
        sizeOfSegment[numberOfSegments - 1] =
            (ulong) buffer.LongLength - offsetOfSegment[numberOfSegments - 1];
        sizeSum += (int) sizeOfSegment[numberOfSegments - 1];
        ArrayList resultBuffer = new ArrayList(2 * sizeSum);
        ArrayList byteSegment = new ArrayList();
        for (int i = 0; i < numberOfSegments; i++)
        {
            int offset = (int) offsetOfSegment[i];
            int size = (int) sizeOfSegment[i];
            byte[] rleSegment = new byte[size];
            Buffer.BlockCopy(buffer, offset, rleSegment, 0, size);
            byteSegment.Capacity = 2 * size;
            sbyte n;
            int rleIndex = 0;
            while (rleIndex < size)
            {
                n = (sbyte) rleSegment[rleIndex];
                if (n >= 0 && n <= 127)
                {
                    for (int j = 0; j < n; j++)
                    {
                        rleIndex++;
                        if (rleIndex >= size) break;
                        byteSegment.Add(rleSegment[rleIndex]);
                    }
                }
                else if (n <= -1 && n >= -127)
                {
                    rleIndex++;
                    if (rleIndex >= size) break;
                    for (int j = 0; j < -n; j++)
                        byteSegment.Add(rleSegment[rleIndex]);
                }
                rleIndex++;
            }
            resultBuffer.AddRange(byteSegment);
            byteSegment.Clear();
        }
        byte[] result = (byte[]) resultBuffer.ToArray(typeof(byte));
        resultBuffer.Clear();
        return result;
    }

    private void PostDicomFileLoad(string fileName)
    {
        Self.Title = defaultTitle + " - " + Path.GetFileName(fileName);
        ImageViewImage.Pixbuf = null;
        RefreshFileInfoTreeView(fileName);
        RefreshDicomTreeView();
        PrepareImages();
        imageIndex = 0;
        scaleFactor = 1.0;
        ShowImage();
        if (images.Length == 1 || (CorrectIndex && images.Length == 2))
        {
            FirstSlideToolButton.Sensitive = false;
            PrevSlideToolButton.Sensitive = false;
            CycleSlidesToolButton.Sensitive = false;
            NextSlideToolButton.Sensitive = false;
            LastSlideToolButton.Sensitive = false;
            CyclingSpeedHScale.Sensitive = false;
            TimeUnitLabel.Sensitive = false;
        }
        else
        {
            FirstSlideToolButton.Sensitive = true;
            PrevSlideToolButton.Sensitive = true;
            CycleSlidesToolButton.Sensitive = true;
            NextSlideToolButton.Sensitive = true;
            LastSlideToolButton.Sensitive = true;
            CyclingSpeedHScale.Sensitive = true;
            TimeUnitLabel.Sensitive = true;
        }
        ExportAsMenuItem.Sensitive = true;
        ExportAsToolButton.Sensitive = true;
    }

    private void OnOpenMenuItemActivate(object o, EventArgs args) 
    {
        if (Configuration.Global.AreDictionariesAvailable)
        {
            GenericOpenFileChooserDialog openFileChooserDialog = 
                new GenericOpenFileChooserDialog("Open ACR-NEMA or DICOM file");
            openFileChooserDialog.Self.Run();
            if (File.Exists(openFileChooserDialog.FileName))
                LoadDicomFile(openFileChooserDialog.FileName);
        }
        else
            MessageDialog(MessageType.Error,
                "Please select valid data dictionaries at " +
                "preferences before loading.");
    }

    private void OnSaveImageMenuItemActivate(object o, EventArgs args) 
    {
        SaveImageFileChooserDialog saveImageFileChooserDialog = 
            new SaveImageFileChooserDialog();
        int response = saveImageFileChooserDialog.Self.Run();
        if (response == -1)
        {
            string imageFileName = saveImageFileChooserDialog.FileName;
            string imageFileType = saveImageFileChooserDialog.FileType;
            try
            {
                Pixbuf pixbuf = new Pixbuf(images[imageIndex],
                    DicomFile.PixelData.Columns,
                    DicomFile.PixelData.Rows);
                pixbuf.Save(imageFileName, imageFileType);
            }
            catch (Exception e)
            {
                ExceptionDialog d = new ExceptionDialog(
                  "Unexpected problems saving image.", e);
                d.Self.Run();
            }
        }
    }

    private void DeactivateImageView()
    {
        if (isSlideCycling)
            OnCycleSlidesToolButtonClicked(null, null);
        SaveImageMenuItem.Sensitive = false;
        SaveImageToolButton.Sensitive = false;
        //BrightenToolButton.Sensitive = false;
        //DarkenToolButton.Sensitive = false;
        GimpToolButton.Sensitive = false;
        ZoomInToolButton.Sensitive = false;
        OriginalSizeToolButton.Sensitive = false;
        FitWindowToolButton.Sensitive = false;
        ZoomOutToolButton.Sensitive = false;
    }

    private void ActivateImageView()
    {
        if ( ! isSlideCycling)
        {
            if (MainNotebook.Page == 2)
            {
                // only image view allows to select and save single images
                SaveImageMenuItem.Sensitive = true;
                SaveImageToolButton.Sensitive = true;
            }
            //BrightenToolButton.Sensitive = true;
            //DarkenToolButton.Sensitive = true;
            GimpToolButton.Sensitive = true;
        }
        ZoomInToolButton.Sensitive = true;
        OriginalSizeToolButton.Sensitive = true;
        FitWindowToolButton.Sensitive = true;
        ZoomOutToolButton.Sensitive = true;
    }

    private Gdk.Image BrightenImage(Gdk.Image src, double brightnessFactor)
    {
        // untested!
        Gdk.Image image = src;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                uint col = image.GetPixel(x, y);
                col = (uint) (((double) col) * brightnessFactor);
                image.PutPixel(x, y, col);
            }
        }
        return image;
    }

    private void ShowImage()
    {
        if (DicomFile.HasPixelData)
        {
            try
            {
                if (imageIndex < 0)
                    imageIndex = 0;
                else if (imageIndex >=  images.Length)
                    imageIndex = images.Length - 1;
                if (CorrectIndex && imageIndex == 0)
                    imageIndex = 1;
                if (scaleFactor < minScaleFactor)
                    scaleFactor = minScaleFactor;
                else if (scaleFactor > maxScaleFactor)
                    scaleFactor = maxScaleFactor;
                if (Configuration.Global.ImageBrightnessFactor < 
                    minBrightnessFactor)
                    Configuration.Global.ImageBrightnessFactor = 
                        minBrightnessFactor;
                else if (Configuration.Global.ImageBrightnessFactor > 
                    maxBrightnessFactor)
                    Configuration.Global.ImageBrightnessFactor = 
                        maxBrightnessFactor;
                ImageViewStatusbar.Push(0, string.Format(
                    "Image: {0}/{1}, Zoom: {2}%, Brightness: {3}%", 
                    CorrectIndex ? imageIndex : imageIndex + 1,
                    CorrectIndex ? images.Length - 1 : images.Length,
                    (int) (scaleFactor * 100),
                    (int) ((Configuration.Global.ImageBrightnessFactor / 2) * 
                        100)));
                bool isJpegNotSupported = false;
                Pixbuf pixbuf = null;
                try
                {
                    pixbuf = new Pixbuf(images[imageIndex],
                            DicomFile.PixelData.Columns,
                            DicomFile.PixelData.Rows);
                }
                catch (Exception exception)
                {
                    // fallback solution
                    if (DicomFile.PixelData.IsJpeg)
                    {
                        isJpegNotSupported = true;
                        DeactivateImageView();
                        MessageDialog(MessageType.Error,
                            string.Format("JPEG format of image {0}/{1} is " +
                                "not supported.",
                                CorrectIndex ? imageIndex : imageIndex + 1, 
                                CorrectIndex ? images.Length - 1 : 
                                    images.Length));
                    }
                    else
                    {
                        pixbuf = new Pixbuf(images[imageIndex],
                            false,
                            DicomFile.PixelData.BitsStored,
                            DicomFile.PixelData.Columns,
                            DicomFile.PixelData.Rows,
                            DicomFile.PixelData.Columns * 
                                (DicomFile.PixelData.BitsAllocated / 8),
                            null);
                    }
                }
                if (pixbuf != null)
                {
                    // TODO: How to get gdk-image from gtk-image?
                    //Gdk.Image image = BrightenImage(
                    //    image, Configuration.Global.ImageBrightnessFactor);
                    //pixbuf = pixbuf.GetFromImage(image, image.Colormap, 
                    //    0, 0, 0, 0, image.Width, image.Height);
                    ImageViewImage.Pixbuf = pixbuf.ScaleSimple(
                        (int) Math.Round(pixbuf.Width * scaleFactor),
                        (int) Math.Round(pixbuf.Height * scaleFactor),
                        InterpType.Bilinear);
                    // very important! prevents swapping to harddisk
                    // tested: no feelable latency time between images
                    System.GC.Collect();
                    ActivateImageView();
                }
                else if ( ! isJpegNotSupported)
                {
                    DeactivateImageView();
                    MessageDialog(MessageType.Error,
                        string.Format("Unable to load image {0}/{1}.",
                            CorrectIndex ? imageIndex : imageIndex + 1, 
                            CorrectIndex ? images.Length - 1 : images.Length));
                }
            }
            catch (Exception e)
            {
                DeactivateImageView();
                ExceptionDialog d = new ExceptionDialog(
                    string.Format("Problems processing image {0}/{1}:\n{2}",
                        CorrectIndex ? imageIndex : imageIndex + 1, 
                        CorrectIndex ? images.Length - 1 : images.Length,
                        e.Message),
                    e);
                d.Self.Run();
            }
        }
    }

    private TreeIter AppendNode(TreeStore store, DataElement element)
    {
        return AppendNode(TreeIter.Zero, store, element, false);
    }

    private TreeIter AppendNode(TreeIter parentNode, TreeStore store,
        DataElement element)
    {
        return AppendNode(parentNode, store, element, true);
    }

    private TreeIter AppendNode(TreeIter parentNode, TreeStore store,
        DataElement element, bool useParentNode)
    {
        TreeIter node;
        if (element.Value.IsMultiValue)
        {
            string multiValue = "";
            foreach (object o in element.Value)
            {
                if (multiValue == "")
                    multiValue = o.ToString();
                else
                    multiValue += "\\" + o.ToString();
            }
            if (useParentNode)
                node = store.AppendValues(
                    parentNode,
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    multiValue,
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
            else
                node = store.AppendValues(
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    multiValue,
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
            foreach (object o in element.Value)
            {
                store.AppendValues(
                    node,
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    o.ToString(),
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    "",
                    o.GetType().ToString(),
                    element.Value.StreamPosition.ToString());
            }
        }
        else if (element.Value.IsDate)
        {
            if (useParentNode)
                node = store.AppendValues(
                    parentNode,
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    ((DateTime) element.Value[0]).ToShortDateString(),
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
            else    
                node = store.AppendValues(
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    ((DateTime) element.Value[0]).ToShortDateString(),
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
        }
        else if (element.Value.IsArray)
        {
            // shows the first n bytes of binary content as hex values
            string hexDigest = ""; 
            int n = 8;
            byte[] bytes = new byte[n];
            if (element.Value[0] is ushort[])
                bytes = openDicom.Encoding.ByteConvert.ToBytes(
                    (ushort[]) element.Value[0]);
            else if (element.Value[0] is short[])
                bytes = openDicom.Encoding.ByteConvert.ToBytes(
                    (short[]) element.Value[0]);
            else
            {
                int len = (element.Value[0] as byte[]).Length;
                if (len > n) len = n + 1; // because of the "..." :)
                bytes = new byte[len];
                Array.Copy((byte[]) element.Value[0], 0, bytes, 0, len);
            }
            bool continues = bytes.Length > n;
            if ( ! continues) n = bytes.Length;
            for (int i = 0; i < n; i++)
            {
                if (hexDigest == "")
                    hexDigest = string.Format("0x{0:X2}", bytes[i]);
                else
                    hexDigest += string.Format(" 0x{0:X2}", bytes[i]);
            }
            if (hexDigest != "" && continues) hexDigest += " ...";
            if (useParentNode)
                node = store.AppendValues(
                    parentNode,
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    hexDigest,
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());            
            else
                node = store.AppendValues(
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    hexDigest,
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
        }
        else
        {
            if (useParentNode)
                node = store.AppendValues(
                    parentNode,
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    (element.Value.IsEmpty || element.Value.IsSequence || 
                        element.Value.IsNestedDataSet) ? 
                        "" : 
                        element.Value[0].ToString(),
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value.IsEmpty ?
                        "" : 
                        element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
            else
                node = store.AppendValues(
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    (element.Value.IsEmpty || element.Value.IsSequence || 
                        element.Value.IsNestedDataSet) ? 
                        "" : 
                        element.Value[0].ToString(),
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    element.Value.ValueLength.ToString(),
                    element.Value.IsEmpty ?
                        "" : 
                        element.Value[0].GetType().ToString(),
                    element.StreamPosition.ToString());
            if (element.Value.IsUid)
            {
                store.AppendValues(
                    node,
                    element.Tag.ToString(),
                    element.VR.Tag.GetDictionaryEntry().Description,
                    (element.Value[0] as Uid).GetDictionaryEntry().Name +
                    " { " + 
                    (element.Value[0] as Uid).GetDictionaryEntry().Type + " }",
                    element.VR.ToLongString(),
                    element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                    "",
                    element.Value[0].GetType().ToString(),
                    element.Value.StreamPosition.ToString());
            }
            else if (element.Value.IsPersonName)
            {
                PersonName pn = (PersonName) element.Value[0];
                for (int i = 0; i < 5; i++)
                {
                    store.AppendValues(
                        node,
                        element.Tag.ToString(),
                        element.VR.Tag.GetDictionaryEntry().Description,
                        pn[i],
                        element.VR.ToLongString(),
                        element.VR.Tag.GetDictionaryEntry().VM.ToString(),
                        "",
                        element.Value[0].GetType().ToString(),
                        element.Value.StreamPosition.ToString());
                }
            }
        }
        return node;
    }

    private void AppendAllSubnodes(TreeIter parentNode, TreeStore store,
        DataElement element)
    {
        if (element.Value.IsSequence)
        {
            foreach (DataElement d in (Sequence) element.Value[0])
            {
                TreeIter node = AppendNode(parentNode, store, d);
                AppendAllSubnodes(node, store, d);
            }
        }
        else if (element.Value.IsNestedDataSet)
        {
            foreach (DataElement d in (NestedDataSet) element.Value[0])
            {
                TreeIter node = AppendNode(parentNode, store, d);
                AppendAllSubnodes(node, store, d);
            }
        }
    }

    private void RefreshDicomTreeView()
    {
        if (DicomFile != null)
        {
            TreeStore store = new TreeStore(typeof(string), typeof(string), 
               typeof(string), typeof(string), typeof(string),
               typeof(string), typeof(string), typeof(string));
            MainTreeView.Model = store;
            try
            {
                string lastTagGroup = "";
                string description = "";
                TreeIter node = TreeIter.Zero;
                TreeIter groupNode = TreeIter.Zero;
                foreach (DataElement d in DicomFile.GetJointDataSets())
                {
                    if (lastTagGroup != d.Tag.Group)
                    {
                        lastTagGroup = d.Tag.Group;
                        description = "Unknown";
                        for (int i = 0; i < groupDescription.Length / 2; i++)
                        {
                            if (lastTagGroup.Equals(groupDescription[i, 0]))
                            {
                                description = groupDescription[i, 1];
                                break;
                            }
                            else if (Regex.IsMatch(groupDescription[i, 0], 
                                "(50xx|60xx)"))
                            {
                                if (Regex.IsMatch(lastTagGroup, 
                                    "(50|60)[0-9A-F]{2}"))
                                {
                                    description = groupDescription[i, 1];
                                    break;
                                }
                            }
                        }
                        groupNode = store.AppendValues(
                            "(" + lastTagGroup + ")",
                            description,
                            "",
                            "",
                            "",
                            "",
                            "",
                            d.StreamPosition.ToString());
                    } 
                    node = AppendNode(groupNode, store, d);
                    AppendAllSubnodes(node, store, d);
                }
                ContentTextView.Buffer.Clear();
                ExpandTreeMenuItem.Sensitive = true;
                CollapseTreeMenuItem.Sensitive = true;
                treeViewLoaded = true;
            }
            catch (Exception e)
            {
                treeViewLoaded = false;
                ExceptionDialog d = new ExceptionDialog(
                    "Unexpected problems refreshing data sets view.", e);
                d.Self.Run();
            }
        }
    }

    private void OnMainTreeViewCursorChanged(object o, EventArgs args)
    {
        TreeModel model;
        TreeIter node;
        TreeSelection selection = MainTreeView.Selection;
        if (selection.GetSelected(out model, out node))
        {
            string tag = (string) model.GetValue(node, 0);
            string description = (string) model.GetValue(node, 1);
            string value = (string) model.GetValue(node, 2);
            string vr = (string) model.GetValue(node, 3);
            string vm = (string) model.GetValue(node, 4);
            string valueLength = (string) model.GetValue(node, 5);
            string systemType = (string) model.GetValue(node, 6);
            string streamPosition = (string) model.GetValue(node, 7);

            bool isGroup = (vr == "");            
            bool isUid = (vr == "Unique Identifier (UI)" && valueLength == "");
            bool isPersonName = (vr == "Person Name (PN)" && valueLength == "");
            bool isMultiValue = (valueLength == "" && ! isGroup && ! isUid && 
                ! isPersonName);
            int index = 0;
            if (isMultiValue)
            {
                TreePath path = model.GetPath(node);
                if (path.Indices.Length > 0)
                    // row index in relation to lowest tree level
                    index = path.Indices[path.Indices.Length - 1];
                ContentTextView.Buffer.Text =  string.Format(
                    "Tag:             {0}\n" +
                    "Description:     {1}\n" +
                    "Value{2,-12}{3}\n" +
                    "VR:              {4}\n" +
                    "VM:              {5}\n" +
                    "System Type:     {6}\n" +
                    "Stream Position: {7}", 
                    tag, description, "[" + index.ToString() + "]:", value, vr,
                    vm, systemType, streamPosition);
            }
            else if (isUid)
            {
                TreePath path = model.GetPath(node);
                path.Up();
                model.GetIter(out node, path);
                string uid = (string) model.GetValue(node, 2);
                bool userDefined = (new Uid(uid)).IsUserDefined;
                bool retired = false;
                string s = "";
                if (userDefined)
                    s = "This UID is user defined.";
                else if ((new Uid(uid)).GetDictionaryEntry().IsRetired)
                    s = "This UID is retired.";
                ContentTextView.Buffer.Text =  string.Format(
                    "Tag:             {0}\n" +
                    "Description:     {1}\n" +
                    "UID Description: {2}\n" +
                    "VR:              {3}\n" +
                    "VM:              {4}\n" +
                    "System Type:     {5}\n" +
                    "Stream Position: {6}" +
                    "{7}",
                    tag, description, value, vr, vm, systemType, 
                    streamPosition, 
                    (s != "") ? "\nAnnotation:      " + s : "");
            }
            else if (isPersonName)
            {
                TreePath path = model.GetPath(node);
                if (path.Indices.Length > 0)
                    // row index in relation to lowest tree level
                    index = path.Indices[path.Indices.Length - 1];
                string[] s = { "Family Name:", "Given Name:", "Middle Name:",
                    "Name Prefix:", "Name Suffix:" };
                ContentTextView.Buffer.Text =  string.Format(
                    "Tag:             {0}\n" +
                    "Description:     {1}\n" +
                    "{2,-16} {3}\n" +
                    "VR:              {4}\n" +
                    "VM:              {5}\n" +
                    "System Type:     {6}\n" +
                    "Stream Position: {7}", 
                    tag, description, s[index], value, vr, vm, systemType, 
                    streamPosition);
            }
            else if (isGroup)
            {
                ContentTextView.Buffer.Text =  string.Format(
                    "Group:           {0}\n" +
                    "Description:     {1}\n" +
                    "Stream Position: {2}" +
                    "{3}", 
                    tag, description, streamPosition,
                    (description == "Unknown") ? 
                        "\nAnnotation:      This group is user defined." : "");
            }
            else
            {
                bool userDefined = (new Tag(tag)).IsUserDefined;
                bool retired = false;
                string s = "";
                if (userDefined)
                    s = "This tag is user defined.";
                else if ((new Tag(tag)).GetDictionaryEntry().IsRetired)
                    s = "This tag is retired.";
                ContentTextView.Buffer.Text =  string.Format(
                    "Tag:             {0}\n" +
                    "Description:     {1}\n" +
                    "Value:           {2}\n" +
                    "VR:              {3}\n" +
                    "VM:              {4}\n" +
                    "ValueLength:     {5}\n" +
                    "System Type:     {6}\n" +
                    "Stream Position: {7}" +
                    "{8}", 
                    tag, description, value, vr, vm, valueLength, systemType, 
                    streamPosition,
                    (s != "") ? "\nAnnotation:      " + s : "");
            }
        }
    }

    private void RefreshFileInfoTreeView(string fileName)
    {
        if (DicomFile != null)
        {
            TreeStore store = new TreeStore(typeof(string), typeof(string));
            MainFileInfoTreeView.Model = store;
            try
            {
                bool isDicomFile = 
                    openDicom.File.DicomFile.IsDicomFile(fileName);
                store.AppendValues("FilePath", fileName);
                store.AppendValues("FileType", 
                    isDicomFile ? "DICOM 3.0" : "ACR-NEMA 1.0 or 2.0");
                Tag modalityTag = new Tag("0008", "0060");
                store.AppendValues("FileModality",
                    DicomFile.DataSet.Contains(modalityTag) ? 
                        DicomFile.DataSet[modalityTag].Value[0].ToString() : 
                        "(not defined)");
                store.AppendValues("FilePreamble",
                    isDicomFile ? 
                        ((DicomFile) DicomFile).MetaInformation.FilePreamble :
                        "(not defined)");
                store.AppendValues("ImageResolution",
                    DicomFile.HasPixelData ? 
                        DicomFile.PixelData.Columns.ToString() + "x" + 
                        DicomFile.PixelData.Rows.ToString()  + "x" + 
                        (DicomFile.PixelData.SamplesPerPixel * 
                            DicomFile.PixelData.BitsStored).ToString() : 
                        "(not defined)");
                Tag numberOfFramesTag = new Tag("0028", "0008");
                store.AppendValues("NumberOfFrames",
                    DicomFile.DataSet.Contains(numberOfFramesTag) ?
                        DicomFile.DataSet[numberOfFramesTag].Value[0]
                            .ToString() : "(not defined)");
                store.AppendValues("CharacterEncoding",
                    DicomFile.DataSet.TransferSyntax.CharacterRepertoire
                        .Encoding.WebName.ToUpper());
                store.AppendValues("TransferSyntax",
                    DicomFile.DataSet.TransferSyntax.Uid.GetDictionaryEntry()
                        .Name);
                store.AppendValues("ValueRepresentation",
                    DicomFile.DataSet.TransferSyntax.IsImplicitVR ? 
                        "Implicit" : "Explicit");
                store.AppendValues("FileByteOrdering",
                    DicomFile.DataSet.TransferSyntax.IsLittleEndian ? 
                        "Little Endian" : "Big Endian");
                store.AppendValues("MachineByteOrdering",
                    DicomFile.DataSet.TransferSyntax.IsMachineLittleEndian ? 
                        "Little Endian" : "Big Endian");
            }
            catch (Exception e)
            {
                ExceptionDialog d = new ExceptionDialog(
                    "Unexpected problems refreshing file info view.", e);
                d.Self.Run();
            }
        }
    }
    
    private void MessageDialog(MessageType messageType, string message)
    {
        MessageDialog m = new MessageDialog(
                    Self,      
                    DialogFlags.Modal, 
                    messageType, 
                    ButtonsType.Ok,
                    message);
        m.Run();
        m.Destroy();
    }

    private void OnExportAsMenuItemActivate(object o, EventArgs args) 
    {
        ExportAsFileChooserDialog exportAsFileChooserDialog = 
            new ExportAsFileChooserDialog();
        int response = exportAsFileChooserDialog.Self.Run();
        if (response == -1)
        {
            string xmlFileName = exportAsFileChooserDialog.FileName;
            try
            {
                XmlFile xmlFile = new XmlFile(DicomFile, 
                    exportAsFileChooserDialog.ExcludePixelData);
                xmlFile.SaveTo(xmlFileName);
            }
            catch (Exception e)
            {
                ExceptionDialog d = new ExceptionDialog(
                  "Unexpected problems exporting as xml.", e);
                d.Self.Run();
            }
        }
    }

    private void OnPreferencesMenuItemActivate(object o, EventArgs args) 
    {
        new PreferencesDialog();
    }

    private void OnOpenToolButtonClicked(object o, EventArgs args)
    {
        OnOpenMenuItemActivate(o, args);
    }

    private void OnSaveImageToolButtonClicked(object o, EventArgs args) 
    {
        OnSaveImageMenuItemActivate(o, args);
    }

    private void OnExportAsToolButtonClicked(object o, EventArgs args) 
    {
        OnExportAsMenuItemActivate(o, args);
    }

    private void OnPreferencesToolButtonClicked(object o, EventArgs args) 
    {
        OnPreferencesMenuItemActivate(o, args);
    }

    private void OnQuitToolButtonClicked(object o, EventArgs args) 
    {
        OnQuitMenuItemActivate(o, args);
    }

    private void OnFirstSlideToolButtonClicked(object o, EventArgs args)
    {
        imageIndex = 0;
        ShowImage();
    }

    private void OnLastSlideToolButtonClicked(object o, EventArgs args)
    {
        imageIndex = images.Length - 1;
        ShowImage();
    }

    private void OnNextSlideToolButtonClicked(object o, EventArgs args)
    {
        imageIndex++;
        ShowImage();
    }

    private void OnPrevSlideToolButtonClicked(object o, EventArgs args)
    {
        imageIndex--;
        ShowImage();
    }

    private void OnCycleSlidesToolButtonClicked(object o, EventArgs args)
    {
        if ( ! isSlideCycling)
        {
            OpenMenuItem.Sensitive = false;
            SaveImageMenuItem.Sensitive = false;
            ExportAsMenuItem.Sensitive = false;
            PreferencesMenuItem.Sensitive = false;
            OpenToolButton.Sensitive = false;
            SaveImageToolButton.Sensitive = false;
            ExportAsToolButton.Sensitive = false;
            PreferencesToolButton.Sensitive = false;
            FirstSlideToolButton.Sensitive = false;
            PrevSlideToolButton.Sensitive = false;
            NextSlideToolButton.Sensitive = false;
            LastSlideToolButton.Sensitive = false;
            //BrightenToolButton.Sensitive = false;
            //DarkenToolButton.Sensitive = false;
            GimpToolButton.Sensitive = false;
            CycleSlidesToolButton.StockId = "gtk-media-stop";
            slideCyclingIdleHandler = 
                Idle.Add(new IdleHandler(CycleSlides));
            isSlideCycling = ! isSlideCycling;
        }
        else
        {
            GLib.Source.Remove(slideCyclingIdleHandler);
            OpenMenuItem.Sensitive = true;
            SaveImageMenuItem.Sensitive = true;
            ExportAsMenuItem.Sensitive = true;
            PreferencesMenuItem.Sensitive = true;
            OpenToolButton.Sensitive = true;
            SaveImageToolButton.Sensitive = true;
            ExportAsToolButton.Sensitive = true;
            PreferencesToolButton.Sensitive = true;
            FirstSlideToolButton.Sensitive = true;
            PrevSlideToolButton.Sensitive = true;
            NextSlideToolButton.Sensitive = true;
            LastSlideToolButton.Sensitive = true;
            //BrightenToolButton.Sensitive = true;
            //DarkenToolButton.Sensitive = true;
            GimpToolButton.Sensitive = true;
            CycleSlidesToolButton.StockId = "gtk-media-play";
            isSlideCycling = ! isSlideCycling;
        }
    }

    private bool CycleSlides()
    {
        imageIndex++;
        if (imageIndex >= images.Length)
            imageIndex = 0;
        ShowImage();
        System.Threading.Thread.Sleep((int) CyclingSpeedHScale.Value);
        // keeps running the idle routine
        return true;
    }

    private void OnZoomInToolButtonClicked(object o, EventArgs args)
    {
        scaleFactor += scaleStep;
        ShowImage();
    }

    private void OnZoomOutToolButtonClicked(object o, EventArgs args)
    {
        scaleFactor -= scaleStep;
        ShowImage();
    }

    private void OnOriginalSizeToolButtonClicked(object o, EventArgs args)
    {
        scaleFactor = 1.0;
        ShowImage();
    }

    private void OnFitWindowToolButtonClicked(object o, EventArgs args)
    {
        if (DicomFile.HasPixelData)
        {
            double dx = ImageViewViewport.Hadjustment.PageSize - 
                DicomFile.PixelData.Columns;
            double dy =  ImageViewViewport.Vadjustment.PageSize - 
                DicomFile.PixelData.Rows;
            if (dx <= dy)
            {
                scaleFactor = ImageViewViewport.Hadjustment.PageSize / 
                    (double) DicomFile.PixelData.Columns;
            }
            else
            {
                scaleFactor = ImageViewViewport.Vadjustment.PageSize / 
                    (double) DicomFile.PixelData.Rows;
            }
            ShowImage();
        }
    }

    private void OnBrightenToolButtonClicked(object o, EventArgs args)
    {
        Configuration.Global.ImageBrightnessFactor += brightnessStep;
        ShowImage();
    }

    private void OnDarkenToolButtonClicked(object o, EventArgs args)
    {
        Configuration.Global.ImageBrightnessFactor -= brightnessStep;
        ShowImage();
    }

    private void OnGimpToolButtonClicked(object o, EventArgs args)
    {
        string gimpRemoteExecutable = Configuration.Global.GimpRemoteExecutable;
        bool isUnix = 
            Regex.IsMatch(Environment.OSVersion.ToString().ToLower(), "unix");
        if ( ! File.Exists(gimpRemoteExecutable) && isUnix)
        {
            gimpRemoteExecutable = "/usr/bin/gimp-remote";
            if (File.Exists(gimpRemoteExecutable))
                Configuration.Global.GimpRemoteExecutable = 
                    gimpRemoteExecutable;
        }
        if (File.Exists(Configuration.Global.GimpRemoteExecutable))
        {
            string tempFileName = Path.GetTempFileName();
            bool saveTempProblem = false;
            try
            {
                Pixbuf pixbuf = new Pixbuf(images[imageIndex],
                    DicomFile.PixelData.Columns,
                    DicomFile.PixelData.Rows);
                pixbuf.Save(tempFileName, "png");
            }
            catch (Exception e1)
            {
                saveTempProblem = true;
                ExceptionDialog d = new ExceptionDialog(
                    "Unexpected problems preparing image for GIMP.", e1);
                d.Self.Run();
            }            
            if ( ! saveTempProblem)
            {
                try
                {
                    Process.Start(Configuration.Global.GimpRemoteExecutable,
                        tempFileName);
                }
                catch (Exception e2)
                {
                    ExceptionDialog d = new ExceptionDialog(
                        "Unexpected exception executing GIMP. Please make sure " +
                        "the right GIMP remote executable has been selected at " +
                        "preferences, e.g. \"gimp-win-remote.exe\" on a Windows " +
                        "machine or \"gimp-remote\" on GNU/Linux.", e2);
                    d.Self.Run();
                }
            }
        }
        else
            MessageDialog(MessageType.Info, "No GIMP remote executable is " +
                "registered. Please make sure you have installed GIMP and " +
                "correctly selected its remote executable at preferences, " +
                "e.g. \"gimp-win-remote.exe\" on a Windows machine or " +
                "\"gimp-remote\" on GNU/Linux.");
    }

    private void OnMainNotebookSwitchPage(object o, SwitchPageArgs args)
    {
        if (args.PageNum == 1 && treeViewLoaded)
        {
            ExpandTreeMenuItem.Sensitive = true;
            CollapseTreeMenuItem.Sensitive = true;
        }
        else
        {
            ExpandTreeMenuItem.Sensitive = false;
            CollapseTreeMenuItem.Sensitive = false;
        }
        if (args.PageNum == 2 && GimpToolButton.Sensitive)
        {
            // enable single image save option when changed to image view
            // page and image is processable
            SaveImageMenuItem.Sensitive = true;
            SaveImageToolButton.Sensitive = true;
        }
        else
        {
            SaveImageMenuItem.Sensitive = false;
            SaveImageToolButton.Sensitive = false;
        }
    }
}
