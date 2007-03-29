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
using Gtk;
using Glade;


public sealed class ExportAsFileChooserDialog: GladeWidget
{
    public new FileChooserDialog Self
    {
        get { return (FileChooserDialog) base.Self; }
    }

    private string fileName = null;
    public string FileName
    {
        get { return fileName; }
    }

    private bool excludePixelData = true;
    public bool ExcludePixelData
    {
        get { return excludePixelData; }
    }

    [WidgetAttribute]
    RadioButton ExcludePixelDataRadioButton;

    [WidgetAttribute]
    RadioButton IncludePixelDataRadioButton;


    public ExportAsFileChooserDialog(): base("ExportAsFileChooserDialog")
    {
        Self.SetCurrentFolder(Configuration.Global.LastSaveFolder);
    }

    private void OnCancelButtonClicked(object o, EventArgs args)
    {
        Self.Destroy();
    }

    private void OnSaveButtonClicked(object o, EventArgs args)
    {
        Configuration.Global.LastSaveFolder = Self.CurrentFolder;
        fileName = Self.Filename;
        if (Path.GetExtension(fileName) == "")
            fileName += ".xml";
        Self.Destroy();
    }

    private void OnExcludePixelDataRadioButtonToggled(object o, EventArgs args)
    {
        excludePixelData = ExcludePixelDataRadioButton.Active;
    }

    private void OnIncludePixelDataRadioButtonToggled(object o, EventArgs args)
    {
        excludePixelData = ! IncludePixelDataRadioButton.Active;
    }
}
