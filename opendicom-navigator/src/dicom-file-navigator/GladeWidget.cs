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
using Glade;
using Gtk;


public class GladeWidget
{
    public static string DefaultResource = string.Empty;

    protected string name = string.Empty;
    public string Name
    {
        get { return name; }
    }

    protected XML xml = null;
    public XML Xml
    {
        get { return xml; }
    }

    public Widget Self
    {
        get { return Xml.GetWidget(Name); }
    }

    public GladeWidget(string name):
        this(GladeWidget.DefaultResource, name) {}

    public GladeWidget(string resource, string name)
    {
        this.name = name;
        xml = XML.FromAssembly(resource, name, string.Empty);
        xml.Autoconnect(this);
    }
}
