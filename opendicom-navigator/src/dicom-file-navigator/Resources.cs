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


public sealed class Resources
{
    // Glade XML file as embedded resource
    public static readonly string GladeXmlResource = 
        "dicom-file-navigator.glade";

    // Linux icon file as embedded resource
    public static readonly string LinuxIconResource = "sextant.png";

    // Windows icon file as embedded resource
    public static readonly string WindowsIconResource = "sextant.ico";

    public static readonly string IconResource = LinuxIconResource;

    // About logo file as embedded resource
    public static readonly string AboutLogoResource = "about.png";

    // Stock brighten file as embedded resource
    public static readonly string StockBrightenResource = "brighten.png";

    // Stock darken file as embedded resource
    public static readonly string StockDarkenResource = "darken.png";

    // GIMP icon file as embedded resource
    public static readonly string GimpIconResource = "gimp.png";

    // Configuration file as embedded resource
    public static readonly string ConfigResource =
        "dicom-file-navigator.config";

    // Local configuration file
    public static readonly string ConfigFileName =
        Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/" +
        ".dicom-file-navigator.config";
}
