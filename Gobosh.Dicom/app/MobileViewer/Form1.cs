/*
 * Mobile Viewer Application
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
 * 
 * 
 */

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Gobosh;
using Gobosh.DICOM;

namespace MobileViewer
{

    public partial class Form1 : Form
    {
        Gobosh.DICOM.Document myDocument = null;
        string DataDictionaryToUse = @"\dd-ps3-2006.xml";
        string myPath = @".";

#region add cursor handling to .NET CF
        // since the .NET CF does not support the Screen.Cursor property
        // the Load/SetCursor functions get imported from the system DLLs.
        // 
        // this snipped was found at:
        // http://msdn2.microsoft.com/en-us/library/ms838263.aspx
        [DllImport("coredll.dll")]
        public static extern int LoadCursor(int zeroValue, int cursorID);
        [DllImport("coredll.dll")]
        public static extern int SetCursor(int cursorHandle);
        
        private static int hourGlassCursorID = 32514;

        private static void ShowWaitCursor(bool value)
        {
            SetCursor(value ? LoadCursor(0, hourGlassCursorID) : 0);
        }
#endregion
        
        public Form1()
        {
            InitializeComponent();
        }

        private void mnFileOpen_Click(object sender, EventArgs e)
        {
            // display open dialog
            string myFileName;
            OpenFileDialog fileSelector = new OpenFileDialog();

            // fileSelector.InitialDirectory = @"C:\Dokumente und Einstellungen\hedge\Eigene Dateien\Meine Projekte\studium\Diplomarbeit\dicom\TEST";// ; Environment.CurrentDirectory;
            fileSelector.Filter = "DICOM Files (*.DCM)|*.dcm|DICOM XML (*.XML)|*.xml|All files (*.*)|*.*";
            fileSelector.FilterIndex = 1;              // the file selector on Windows Mobile shows ALL files including Notes, Documents etc.pp.

            if (DialogResult.OK == fileSelector.ShowDialog())
            {
                try
                {
                    // prepare the directory

                    myFileName = fileSelector.FileName;
                    fileSelector = null;    // put the fileselector on top of the GC

                    label2.Text = "loading file:";
                    label1.Text = myFileName;
                    DocumentTree.BeginUpdate();
                    DocumentTree.Nodes.Clear();
                    DocumentTree.EndUpdate();

                    // allow repainting
                    Application.DoEvents();
                    ShowWaitCursor(true);
                    
                    string myEnding = myFileName.Substring(myFileName.Length - 4).ToLower();
                    if (myEnding == ".xml")
                    {
                        myDocument = null;
                        try
                        {
                            // try to load the Gobosh.DICOM XML
                            myDocument = Gobosh.DICOM.XMLReader.ReadFromFile(myFileName, myPath + DataDictionaryToUse);
                            PopulateTree(Path.GetFileName(myFileName));
                        }
                        catch
                        {
                            // just catch it and try the next
                            myDocument = null;
                        }
                        // load the Lb.DICOM XML variant
                        if (myDocument == null)
                        {
                            XmlDocument myXml = new XmlDocument();
                            myXml.Load(myFileName);
                            myDocument = null;
                            myDocument = new Document();
                            myDocument.SetDataDictionary(myPath + DataDictionaryToUse);
                            // use current culture

                            myDocument.LoadFromLbDicomXML(myXml, System.Globalization.CultureInfo.CurrentCulture.Name, Path.GetDirectoryName(myFileName));

                            myXml = null;
                            PopulateTree(Path.GetFileName(myFileName));
                        }
                    }
                    else
                    {
                        try
                        {                            
                            myDocument = null;
                            myDocument = new Document();
                            myDocument.SetDataDictionary(myPath + DataDictionaryToUse);
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
//                     Cursor = Cursors.Default;
                    ShowWaitCursor(false);
                }
            }            
        }

        private void PopulateTree(string rootName)
        {
			DocumentTree.Nodes.Clear();
			DataElement rootElement;
            label1.Text = "Preparing Document...";

            Application.DoEvents();

            rootElement = myDocument.GetRootNode();

			TreeNode newNode = new TreeNode(rootName);
			DocumentTree.Nodes.Add(newNode);
			newNode.Tag = rootElement;

			DocumentTree.BeginUpdate();
			TransferDataElementsToTreeView(newNode,rootElement,0);
			newNode.Expand();
			DocumentTree.EndUpdate();
			DocumentTree.SelectedNode = DocumentTree.Nodes[0];

			// Gobosh.DICOM.StringEncoding m = new Gobosh.DICOM.ISO_8859_1Encoding();
		}

        private void TransferDataElementsToTreeView(TreeNode node, DataElement dataElement, int level)
        {
            TreeNode newNode;
            foreach (DataElement elem in dataElement)
            {
                newNode = new TreeNode(elem.GetDescription());
                newNode.Tag = elem;
                node.Nodes.Add(newNode);
                if (elem.Count > 0)
                {
                    TransferDataElementsToTreeView(newNode, elem, level + 1);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            System.IO.Directory.SetCurrentDirectory(myPath);
        }

        private void mnFileQuit_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show(
                "Applikation beenden?", 
                "MobileViewer", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button2))
            {
                Application.Exit();
            }
        }

        private DataElement currentNode;

        private void DocumentTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DataElement k = (DataElement)e.Node.Tag;
            currentNode = k;

            if (k is Gobosh.DICOM.DataElements.Root)
            {
                //richTextBox1.Visible = true;
                //richTextBox1.Clear();

                StringBuilder p = new StringBuilder(100);
                foreach (string line in ((Gobosh.DICOM.DataElements.Root)k).Warnings)
                {

                    p.Append(line + "\n");
                }
                label1.Text = p.ToString();
            }
            else
            {
                label1.Text = k.GetHumanReadableString();
            }
            if (k.isImageData())
            {
                if (k.RawData != null)
                {
                    MemoryStream m = new MemoryStream(k.RawData, 0, k.ValueLength, false, true);
                    try
                    {
                        Bitmap tmpBmp = new Bitmap(m); // image doesn't know FromStream in .NET CF
                        pictureBox1.Height = tmpBmp.Height;
                        pictureBox1.Width = tmpBmp.Width;
                        pictureBox1.Image = tmpBmp;
                        // pictureBox1.Image = System.Drawing.Image Image.FromStream(m);
                        tabControl1.SelectedIndex = 1;
                    }
                    catch
                    {
                        pictureBox1.Image = null;
                        //tabPage2.Hide();
                        //tabPage1.Focus();
                        tabControl1.SelectedIndex = 0;
                    }
                    
                }
            }
            else
            {
                pictureBox1.Image = null;
                tabControl1.SelectedIndex = 0;
                //tabPage2.Hide();
                //tabPage1.Focus();
            }
            label2.Text = k.GetDescription();            
        }


        private int mMouseLastX, mMouseLastY;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                int deltaX = mMouseLastX - e.X;
                int deltaY = mMouseLastY - e.Y;

                Point newScrollPos = tabPage2.AutoScrollPosition;
                newScrollPos.X = deltaX;
                newScrollPos.Y = deltaY;
                tabPage2.AutoScrollPosition = newScrollPos;
                mMouseLastX = newScrollPos.X;
                mMouseLastY = newScrollPos.Y;
            }

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
        }
    }
}