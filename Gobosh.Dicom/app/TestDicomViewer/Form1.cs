/*  DicomViewer
 *  v.0.5
 *  
 * (C) 2006,2007 Timo Kaluza
 * tk@gobosh.net
 * 
 * This program is free software; you can redistribute it and/or modify   
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 *     You should have received a copy of the GNU General Public License
 *     along with this program; if not, write to the Free Software
 * 
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
 *
 *  ******************************************************************************************
 *  This is an application to test and debug the Gobosh.DICOM namespace and its functionality.
 *  Especially decoding the actual pixel data is incomplete and does not suit any professional
 *  accuracy or performance needs!
 *  ******************************************************************************************
 * 
 *  Feel free to participate.
 * 
 * 
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using Gobosh;
using Gobosh.DICOM;

namespace DicomViewer
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel1;
        private IContainer components;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.MenuItem FileMenuSave;
        private MenuItem menuItem5;
        private MenuItem menuItem7;
        private MenuItem SelectDD2006;

        /// <summary>
        /// Reference to the DICOM Document, if present
        /// </summary>
        private Document myDocument = null;
        private MenuItem menuItem6;
        private MenuItem menuItem8;
        private MenuItem ActionsAnonymize;

        private string DataDictionaryToUse = "dd-ps3-2006.xml";

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.FileMenuSave = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.ActionsAnonymize = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.SelectDD2006 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem8,
            this.menuItem7,
            this.menuItem3});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.FileMenuSave,
            this.menuItem6});
            this.menuItem1.Text = "File";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuItem2.Text = "Open..";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // FileMenuSave
            // 
            this.FileMenuSave.Index = 1;
            this.FileMenuSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.FileMenuSave.Text = "Save..";
            this.FileMenuSave.Click += new System.EventHandler(this.FileMenuSave_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 2;
            this.menuItem6.Shortcut = System.Windows.Forms.Shortcut.CtrlQ;
            this.menuItem6.Text = "Quit";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click_1);
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 1;
            this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.ActionsAnonymize});
            this.menuItem8.Text = "Actions";
            // 
            // ActionsAnonymize
            // 
            this.ActionsAnonymize.Index = 0;
            this.ActionsAnonymize.Text = "Anonymize";
            this.ActionsAnonymize.Click += new System.EventHandler(this.ActionsAnonymize_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 2;
            this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.SelectDD2006});
            this.menuItem7.Text = "DataDictionary";
            // 
            // SelectDD2006
            // 
            this.SelectDD2006.Checked = true;
            this.SelectDD2006.Index = 0;
            this.SelectDD2006.Tag = "dd-ps3-2006.xml";
            this.SelectDD2006.Text = "DICOM 2006";
            this.SelectDD2006.Click += new System.EventHandler(this.SelectDD2006_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 3;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4,
            this.menuItem5});
            this.menuItem3.Text = "Help";
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            this.menuItem4.Text = "About";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 1;
            this.menuItem5.Text = "TEST";
            this.menuItem5.Visible = false;
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(260, 425);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(260, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 425);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.richTextBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(263, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(520, 425);
            this.panel1.TabIndex = 2;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(16, 40);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(480, 360);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "richTextBox1";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(475, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a node from the tree";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(16, 40);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(491, 368);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // Form1
            // 
            this.AccessibleName = "";
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(783, 425);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.treeView1);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Dicom Test Viewer 0.5";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			// display open dialog
			string myFileName;
			OpenFileDialog fileSelector = new OpenFileDialog();

			// fileSelector.InitialDirectory = @"C:\Dokumente und Einstellungen\hedge\Eigene Dateien\Meine Projekte\studium\Diplomarbeit\dicom\TEST";// ; Environment.CurrentDirectory;
			fileSelector.Filter = "DICOM Files (*.DCM)|*.dcm|DICOM XML (*.XML)|*.xml|All files (*.*)|*.*";
			fileSelector.FilterIndex = 3;
			fileSelector.RestoreDirectory = true;

			if ( DialogResult.OK == fileSelector.ShowDialog() )
			{
                try
                {
                    Cursor = Cursors.WaitCursor;

                    myFileName = fileSelector.FileName;
                    fileSelector = null;    // put the fileselector on top of the GC

                    string myEnding = myFileName.Substring(myFileName.Length - 4).ToLower();
                    if (myEnding == ".xml")
                    {
                        myDocument = null;
                        try
                        {
                            // try to load the Gobosh.DICOM XML
                            myDocument = Gobosh.DICOM.XMLReader.ReadFromFile(myFileName, DataDictionaryToUse);
                            PopulateTree(Path.GetFileName(myFileName));
                        }
                        catch
                        {
                            // just catch it and try the next
                            myDocument = null;
                        }
                        // load alternative XML variant
                        if (myDocument == null)
                        {
                            // TODO: if need support for other XML dialects, you can implement
                            // the reading here and create a Document. Call PopulateTree() afterwards
                            // to view the content
                        }
                    }
                    else
                    {
                        try
                        {
                            myDocument = null;
                            myDocument = new Document();
                            myDocument.SetDataDictionary(DataDictionaryToUse);
                            myDocument.LoadFromFile(myFileName);
                            PopulateTree(Path.GetFileName(myFileName));
                        }
                        catch (Exception er)
                        {
                            System.Windows.Forms.MessageBox.Show(er.Message);
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
		}

		private void PopulateTree(string rootName)
		{
			treeView1.Nodes.Clear();
			DataElement rootElement;

            rootElement = myDocument.GetRootNode();

			TreeNode newNode = new TreeNode(rootName);
			treeView1.Nodes.Add(newNode);
			newNode.Tag = rootElement;

			treeView1.BeginUpdate();
			TransferDataElementsToTreeView(newNode,rootElement,0);
			newNode.Expand();
			treeView1.EndUpdate();
			treeView1.SelectedNode = treeView1.Nodes[0];

			// Gobosh.DICOM.StringEncoding m = new Gobosh.DICOM.ISO_8859_1Encoding();
		}

		private void TransferDataElementsToTreeView(TreeNode node,DataElement dataElement,int level)
		{
			TreeNode newNode;
			foreach (DataElement elem in dataElement)
			{
				newNode = new TreeNode(elem.GetDescription());
				newNode.Tag = elem;
				node.Nodes.Add(newNode);
				if ( elem.Count > 0 )
				{
					TransferDataElementsToTreeView(newNode,elem,level+1);
				}
			}
		}

        private DataElement currentNode;

		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			DataElement k = (DataElement) e.Node.Tag;
            currentNode = k;

			if ( k is Gobosh.DICOM.DataElements.Root )
			{
				pictureBox1.Visible = false;
				richTextBox1.Visible = true;
				richTextBox1.Clear();
								
				foreach ( string line in ((Gobosh.DICOM.DataElements.Root)k).Warnings )
				{
					richTextBox1.AppendText(line+"\n");
				}
							
			}
			else
			{
				pictureBox1.Visible = true;
				richTextBox1.Visible = false;
			}
			if ( k.isImageData() )
			{
                if (k.RawData != null)
                {
                    if (pictureBox1.Image == null)
                    {
                        pictureBox1.Image = null;
                    }
                    MemoryStream m = new MemoryStream(k.RawData, 0, k.ValueLength, false, true);
                    try
                    {
                        pictureBox1.Image = System.Drawing.Image.FromStream(m);
                    }
                    catch
                    {
                        // pictureBox1.Image = null;
                    }
                    if (pictureBox1.Image == null)
                    {
                        try
                        {
                            DecodeStandardPixeldata();
                        }
                        catch
                        {
                        }
                    }
                    m.Dispose();
                }
			}
			else
			{
				pictureBox1.Image = null;
			}
            label1.Text = k.GetHumanReadableString();
		}

		/// <summary>
		/// Opens a File Save Dialog and saves the DICOM file
		/// </summary>
		/// <param name="sender">The menu entry object</param>
		/// <param name="e">Event arguments</param>
		private void FileMenuSave_Click(object sender, System.EventArgs e)
		{                      
			// write only if a document was loaded
            if (myDocument != null)
            {
                // display open dialog
                string myFileName;
                SaveFileDialog fileSelector = new SaveFileDialog();

                // fileSelector.InitialDirectory = @"C:\Dokumente und Einstellungen\hedge\Eigene Dateien\Meine Projekte\studium\Diplomarbeit\dicom\TEST";// ; Environment.CurrentDirectory;
                fileSelector.Filter = "DICOM Files (*.DCM)|*.dcm|DICOM XML (*.XML)|*.xml|All files (*.*)|*.*";
                fileSelector.FilterIndex = 3;
                fileSelector.RestoreDirectory = true;

                if (DialogResult.OK == fileSelector.ShowDialog())
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        myDocument.SetDataDictionary(DataDictionaryToUse);
                        myFileName = fileSelector.FileName;
                        int myIndex = fileSelector.FilterIndex;
                        fileSelector = null;    // put the fileselector on top of the GC

                        string myEnding = myFileName.Substring(myFileName.Length - 4).ToLower();
                        if (myEnding == ".xml")
                        {
                            if (myIndex == 2 || myIndex == 3){
                                // write XML
                                Gobosh.DICOM.XMLWriter.WriteToFile(myFileName, myDocument.GetRootNode());
                            }
                        }
                        else
                        {
                            myDocument.SaveToFile(myFileName);
                        }
                    }
                    finally
                    {
                        Cursor = Cursors.Default;            
                    }

                    
                }
            }
		}

        private void menuItem5_Click(object sender, EventArgs e)
        {

        }

        private void DecodeStandardPixeldata()
        {
            // *******************************************************************************
            // this is just a quick hack implementation and not suitable for production:
            // it solely demonstrates the access to several DataElements and the possible
            // use in the decoding.
            // Actual decoding is planned to be an on-top layer for the current implementation
            // *******************************************************************************

            int Rows, Cols, BitsAllocated, BitsStored, HighBit;
            int smallestGrey, highestGray;

            // get the root node
            Gobosh.DICOM.DataElement myRoot = myDocument.GetRootNode();
            Gobosh.DICOM.DataElement aNode;

            // get the image dimensions
            Rows = ((Gobosh.DICOM.DataElementIntegerValue)myRoot.GetChildByTag(0x0028, 0x0010).GetValue()).getValueAsInteger();
            Cols = ((Gobosh.DICOM.DataElementIntegerValue)myRoot.GetChildByTag(0x0028, 0x0011).GetValue()).getValueAsInteger();
            BitsAllocated = ((Gobosh.DICOM.DataElementIntegerValue)myRoot.GetChildByTag(0x0028, 0x0100).GetValue()).getValueAsInteger();
            BitsStored = ((Gobosh.DICOM.DataElementIntegerValue)myRoot.GetChildByTag(0x0028, 0x0101).GetValue()).getValueAsInteger();
            HighBit = ((Gobosh.DICOM.DataElementIntegerValue)myRoot.GetChildByTag(0x0028, 0x0102).GetValue()).getValueAsInteger();

            // grey values
            aNode = myRoot.GetChildByTag(0x0028, 0x0106);
            if (aNode != null)
            {
                smallestGrey = ((Gobosh.DICOM.DataElementIntegerValue)aNode.GetValue()).getValueAsInteger();
            }
            else
            {
                smallestGrey = 0;
            }

            aNode = myRoot.GetChildByTag(0x0028, 0x0107);
            if (aNode != null)
            {
                highestGray = ((Gobosh.DICOM.DataElementIntegerValue)aNode.GetValue()).getValueAsInteger();
            }
            else
            {
                highestGray = 0;    // need to determine
                aNode = myRoot.GetChildByTag(0x7fe0, 0x0010); 
                int gxx;
                bool endian = aNode.IsLittleEndian();
                for (int i = 0; i < Rows * Cols; i++)
                {
                    if (BitsAllocated > 8)
                    {
                        gxx = Utils.ReadUInt16(aNode.RawData, i * 2, endian);
                    }
                    else
                    {
                        gxx = aNode.RawData[i];
                    }
                    if (gxx > highestGray)
                    {
                        highestGray = gxx;
                    }
                }
                
            }

            pictureBox1.Visible = true;
            richTextBox1.Visible = false;

            aNode = myRoot.GetChildByTag(0x7fe0, 0x0010); 

            // make me a bitmap
            // System.Drawing.Bitmap img
                this.pictureBox1.Image = new System.Drawing.Bitmap(Cols, Rows, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(pictureBox1.Image);

            int BytesAllocated = 1;

            if (BitsAllocated > 8)
            {
                BytesAllocated = 2;
            }           
            int offset = 0;
            bool isLittleEndian = aNode.IsLittleEndian();
            int myMask = (int)((Math.Pow(2, BitsStored)) - 1);

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols; x++)
                {
                    // ** experimental **
                    // the inner loop of this is not very fast, especially
                    // plotting the pixels via DrawLine() is bad style.
                    int greyvalue;
                    if (BytesAllocated > 1)
                    {
                        greyvalue = Utils.ReadUInt16(aNode.RawData, offset, isLittleEndian);
                    }
                    else
                    {
                        greyvalue = aNode.RawData[offset];
                    }
                    offset += BytesAllocated;       // increase offset to the next pixel

                    // unmask and shift the value (this is possible to be wrong)
                    greyvalue = greyvalue >> (BitsStored - (HighBit + 1));
                    greyvalue = greyvalue & myMask;
                    if (greyvalue > highestGray)
                    {
                        greyvalue = highestGray;    // partially this is exceeded
                    }
                    greyvalue = (greyvalue * 255) / (highestGray /* - smallestGrey */);
                    if (greyvalue > 255)
                    {
                        greyvalue = 255;
                    }
                    Pen p = new Pen(Color.FromArgb(greyvalue, greyvalue, greyvalue));
                    g.DrawLine(p, x, y, x+1, y);    // +1 because of the GDI+ behaviour not to draw lines from and to the same point
                    p.Dispose();
                }
            }

            g.Flush();
            g.Dispose();
        }

        private void SelectDD2006_Click(object sender, EventArgs e)
        {
            DataDictionaryToUse = ((MenuItem)sender).Tag.ToString();
        }

        private void menuItem6_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuItem4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("(C) 2006,2007 Timo Kaluza","TestDicomViewer 0.4",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void ReadFromTCP()
        {
            // TODO: implement TCP usage
            //Gobosh.DICOM.Document myDocument = new Document();
            //myDocument.PrepareToLoad(DataDictionaryToUse, false, true, true);
            
            //myDocument.PartialLoad(Buffer,sizeof);
            //myDocument.FinishLoad();
        }

        private void ActionsAnonymize_Click(object sender, EventArgs e)
        {
            Anonymize(myDocument.GetRootNode());
        }

        private void Anonymize(DataElement node)
        {
            foreach (DataElement n in node)
            {
                if (n is Gobosh.DICOM.DataElements.DA)
                {
                    n.SetValue("1900.01.01");
                }
                if (n is Gobosh.DICOM.DataElements.TM)
                {
                    n.SetValue("00:00");
                }
                if (n is Gobosh.DICOM.DataElements.PN)
                {
                    n.SetValue("Onymous^A^N^^");
                }
                if (n is Gobosh.DICOM.DataElements.DT)
                {
                    n.SetValue("1900.01.01");
                }
                if (n.Count > 0)
                {
                    Anonymize(n);
                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete )
            {
                TreeView my = (TreeView)(sender);
                TreeNode newNode = my.SelectedNode.NextNode;
                if (newNode == null)
                {
                    newNode = my.SelectedNode.Parent;
                    if (newNode == null)
                    {
                        newNode = my.Nodes[0];
                    }
                }
                DataElement k = (DataElement)(my.SelectedNode.Tag);
                currentNode = k;
                if (my.SelectedNode.Parent != null)
                {
                    DataElement z = (DataElement)(my.SelectedNode.Parent.Tag);
                    my.SelectedNode.Remove();
                    z.Remove(k);
                    
                }
                my.SelectedNode = newNode;
            }
        }

    }
}
