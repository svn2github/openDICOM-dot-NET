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


    $Id$
*/
using System;
using System.IO;
using Gtk;
using Glade;
using Gdk;


public sealed class ExceptionDialog: GladeWidget
{
    public new Dialog Self
    {
        get { return (Dialog) base.Self; }
    }

    [WidgetAttribute]
    Label ExceptionLabel;

    [WidgetAttribute]
    TextView ExceptionDetailsTextView;


    public ExceptionDialog(string message): this(message, null) {}

    public ExceptionDialog(Exception exception): this(null, exception) {}

    public ExceptionDialog(string message, Exception exception):
        base("ExceptionDialog")
    {
        if (message != null)
            ExceptionLabel.Text = message;
        if (exception != null)
            ExceptionDetailsTextView.Buffer.Text = exception.ToString();
    }

    private void OnOkButtonClicked(object o, EventArgs args)
    {
        Self.Destroy();
    }
}
