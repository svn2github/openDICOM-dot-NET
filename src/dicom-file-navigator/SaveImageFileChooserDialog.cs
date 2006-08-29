/*
    openDICOM.NET Navigator 0.1.0

    Simple GTK ACR-NEMA and DICOM Viewer for Mono / .NET based on the 
    openDICOM.NET library.

    Copyright (C) 2006  Albert Gnandt

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

*/
using System;
using System.IO;
using Gtk;
using Glade;
using Gdk;


public sealed class SaveImageFileChooserDialog: GladeWidget
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

    private string fileType = "png";
    public string FileType
    {
        get { return fileType; }
    }

    [WidgetAttribute]
    TreeView FileTypeTreeView;


    public SaveImageFileChooserDialog(): base("SaveImageFileChooserDialog")
    {
        Self.SetCurrentFolder(Configuration.Global.LastSaveFolder);
        FileTypeTreeView.AppendColumn("Name", new CellRendererText(),
            "text", 0);
        FileTypeTreeView.AppendColumn("Description", new CellRendererText(),
            "text", 1);
        TreeStore store = new TreeStore(typeof(string), typeof(string));
        FileTypeTreeView.Model = store;
        foreach (PixbufFormat f in Pixbuf.Formats)
        {
            if (f.IsWritable)
            {
                TreeIter iter = store.AppendValues(f.Name, f.Description);
                if (f.Name == fileType)
                    FileTypeTreeView.Selection.SelectIter(iter);
            }
        }
    }

    private void OnCancelButtonClicked(object o, EventArgs args)
    {
        Self.Destroy();
    }

    private void OnSaveButtonClicked(object o, EventArgs args)
    {
        Configuration.Global.LastSaveFolder = Self.CurrentFolder;
        fileName = Self.Filename;
        TreeIter iter;
        TreeModel model;
        if (FileTypeTreeView.Selection.GetSelected(out model, out iter))
            fileType = (string) model.GetValue(iter, 0);
        if (Path.GetExtension(fileName) == "")
            fileName += "." + fileType;
        Self.Destroy();
    }
}
