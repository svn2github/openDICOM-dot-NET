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
using openDicom.Registry;


public sealed class PreferencesDialog: GladeWidget
{
    public new Dialog Self
    {
        get { return (Dialog) base.Self; }
    }

    private Configuration tempConfig = new Configuration();

    private Tooltips tooltips = new Tooltips();

    [WidgetAttribute]
    ComboBox DataElementDictionaryFileFormatComboBox;

    [WidgetAttribute]
    ComboBox UidDictionaryFileFormatComboBox;

    [WidgetAttribute]
    Button DataElementDictionaryFileButton;

    [WidgetAttribute]
    Button UidDictionaryFileButton;

    [WidgetAttribute]
    RadioButton DecodeStrictRadioButton;

    [WidgetAttribute]
    Button GimpButton;


    public PreferencesDialog(): base("PreferencesDialog")
    {
        Configuration.Global.CopyTo(tempConfig);
        DataElementDictionaryFileFormatComboBox.Active = 
            (int) tempConfig.DataElementDictionaryFileFormat;
        UidDictionaryFileFormatComboBox.Active =
            (int) tempConfig.UidDictionaryFileFormat;
        if (tempConfig.DataElementDictionaryFileName != null)
        {
            DataElementDictionaryFileButton.Label =
                Path.GetFileName(tempConfig.DataElementDictionaryFileName);
            tooltips.SetTip(DataElementDictionaryFileButton, 
                tempConfig.DataElementDictionaryFileName, null);
        }
        if (tempConfig.UidDictionaryFileName != null)
        {
            UidDictionaryFileButton.Label =
                Path.GetFileName(tempConfig.UidDictionaryFileName);
            tooltips.SetTip(UidDictionaryFileButton, 
                tempConfig.UidDictionaryFileName, null);
        }
        DecodeStrictRadioButton.Active = tempConfig.UseStrictDecoding;
        if (tempConfig.GimpRemoteExecutable != null)
        {
            GimpButton.Label = Path.GetFileName(
                tempConfig.GimpRemoteExecutable);
            tooltips.SetTip(GimpButton, tempConfig.GimpRemoteExecutable, null);
        }
    }

    private void OnDataElementDictionaryFileButtonClicked(
        object o, EventArgs args)
    {
        GenericOpenFileChooserDialog dialog = 
            new GenericOpenFileChooserDialog("Select Data Element Dictionary");
        int response = dialog.Self.Run();
        if (response == (int) ResponseType.Ok)
        {            
            if (dialog.FileName != null)
            {
                tempConfig.DataElementDictionaryFileName = dialog.FileName;
                DataElementDictionaryFileButton.Label = 
                    Path.GetFileName(tempConfig.DataElementDictionaryFileName);
                tooltips.SetTip(DataElementDictionaryFileButton, 
                    tempConfig.DataElementDictionaryFileName, null);
            }
        }
    }

    private void OnDataElementDictionaryFileFormatComboBoxChanged(
        object o, EventArgs args)
    {
        tempConfig.DataElementDictionaryFileFormat = 
            (DictionaryFileFormat) Enum.ToObject(
                typeof(DictionaryFileFormat), 
                DataElementDictionaryFileFormatComboBox.Active);
    }

    private void OnUidDictionaryFileButtonClicked(
        object o, EventArgs args)
    {
        GenericOpenFileChooserDialog dialog = 
            new GenericOpenFileChooserDialog("Select UID Dictionary");
        int response = dialog.Self.Run();
        if (response == (int) ResponseType.Ok)
        {            
            if (dialog.FileName != null)
            {
                tempConfig.UidDictionaryFileName = dialog.FileName;
                UidDictionaryFileButton.Label = 
                    Path.GetFileName(tempConfig.UidDictionaryFileName);
                tooltips.SetTip(UidDictionaryFileButton, 
                    tempConfig.UidDictionaryFileName, null);
            }
        }
    }

    private void OnUidDictionaryFileFormatComboBoxChanged(
        object o, EventArgs args)
    {
        tempConfig.UidDictionaryFileFormat = 
            (DictionaryFileFormat) Enum.ToObject(
                typeof(DictionaryFileFormat), 
                UidDictionaryFileFormatComboBox.Active);
    }

    private void OnDecodeStrictRadioButtonToggled(object o, EventArgs args)
    {
        tempConfig.UseStrictDecoding = DecodeStrictRadioButton.Active;
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

    private void OnOkButtonClicked(object o, EventArgs args)
    {
        if (tempConfig.DataElementDictionaryFileName == null ||
            tempConfig.UidDictionaryFileName == null)
            MessageDialog(MessageType.Error, "Please choose files for both " +
                "dictionaries.");
        else
        {
            try
            {
                tempConfig.LoadDictionaries();
                tempConfig.CopyTo(Configuration.Global);
                Self.Destroy();
            }
            catch (Exception e)
            {
                ExceptionDialog d = new ExceptionDialog(
                    "Cannot load specified dictionaries. " +
                    "Specified file format probably does not match real file " +
                    "format or specified files are no openDICOM.NET " + 
                    "dictionary files.", e);
                d.Self.Run();
            }
        }
    }

    private void OnCancelButtonClicked(object o, EventArgs args)
    {
        Self.Destroy();
    }

    private void OnGimpButtonClicked(object o, EventArgs args)
    {
        GenericOpenFileChooserDialog dialog = 
            new GenericOpenFileChooserDialog("Select GIMP remote executable");
        int response = dialog.Self.Run();
        if (response == (int) ResponseType.Ok)
        {            
            if (dialog.FileName != null)
            {
                tempConfig.GimpRemoteExecutable = dialog.FileName;
                GimpButton.Label = 
                    Path.GetFileName(tempConfig.GimpRemoteExecutable);
                tooltips.SetTip(GimpButton, 
                    tempConfig.GimpRemoteExecutable, null);
            }
        }
    }
}
