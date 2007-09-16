/*
    openDICOM.NET Beagle Filter Plugin 0.1.1

    openDICOM.NET Beagle Filter Plugin makes DICOM file content available
    for the Beagle Desktop Search
    Copyright (C) 2007  Albert Gnandt

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


    $Id: FilterDicom.cs 23 2007-03-27 22:51:24Z agnandt $
*/
using System.Reflection;
using System.Runtime.CompilerServices;

using Beagle.Filters;

// Registration as Beagle Filter Plugin
[assembly: Beagle.Daemon.FilterTypes(typeof (FilterDicom))]

// Information about this assembly is defined by the following
// attributes.
//
// change them to the information which is associated with the assembly
// you compile.

[assembly: AssemblyTitle("openDICOM.NET Beagle Filter Plugin")]
[assembly: AssemblyDescription("The openDICOM.NET Beagle Filter Plugin makes DICOM file content available for the Beagle Desktop Search")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("FilterDicom.dll")]
[assembly: AssemblyCopyright("Copyright (C) 2007  Albert Gnandt")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all values by your own or you can build default build and revision
// numbers with the '*' character (the default):

[assembly: AssemblyVersion("0.1.1")]

// The following attributes specify the key for the sign of your assembly. See the
// .NET Framework documentation for more information about signing.
// This is not required, if you don't want signing let these attributes like they're.
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
