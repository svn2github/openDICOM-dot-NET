/*

	GTK implementation of DICOMViewer
	using the Gobosh.DICOM namespace
	
	(C) 2006,2007 Timo Kaluza (tk@gobosh.net)

	this window provides the possibility to load and save native DICOM3.0 data streams as well
	as their XML equivalent.

*/

using System;
using Gtk;

public class MainWindow: Gtk.Window
{	

	// the DICOM document
	protected Gobosh.DICOM.Document mDocument = null;
	
	// the treeview to display our document structure
	protected Gtk.TreeView treeview1;
	
	// the treeview to display the value (split up into values)
	protected Gtk.TreeView treeview2;

	public MainWindow (): base ("")
	{
		Stetic.Gui.Build (this, typeof(MainWindow));
		
        treeview1.AppendColumn("Group", new CellRendererText(), "text", 0);
        treeview1.AppendColumn("Element", new CellRendererText(), 
            "text", 1);
        treeview1.AppendColumn("Object", new CellRendererText(), "text", 
            2);
            
        treeview2.AppendColumn("Type",new CellRendererText(),"text",0);
        treeview2.AppendColumn("Value",new CellRendererText(),"text",1);

	}

	private void MessageBox(string s)
	{
		MessageDialog v = new MessageDialog(this,
			DialogFlags.DestroyWithParent,
			MessageType.Info,
			ButtonsType.Close,s);
		v.Run();
		v.Destroy();

	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	
	protected virtual void OnOpenActivated(object sender, System.EventArgs e)
	{
		
		FileChooserDialog fc = new FileChooserDialog(
			"Choose a DICOM or an XML file to open",
			this,
			FileChooserAction.Open,
			"Cancel", ResponseType.Cancel,
			"Open",ResponseType.Accept);
		if ( fc.Run() == (int) ResponseType.Accept )
		{
			if ( fc.Filename.EndsWith(".xml") )
			{
				mDocument = null;

				mDocument = Gobosh.DICOM.XMLReader.ReadFromFile(fc.Filename,"dd-ps3-2006.xml");
			}
			else
			{
				mDocument = null;
				mDocument = new Gobosh.DICOM.Document();
				// set the data dictionary to use
				mDocument.SetDataDictionary("dd-ps3-2006.xml");
				// open the document
				mDocument.LoadFromFile(fc.Filename);
				}			
				// populate the tree
				PopulateTree(treeview1);
		}
		fc.Destroy();
	}
	
	protected void PopulateTree(TreeView view)
	{

		// get the root document tree element of the Gobosh.DICOM.Document 
		Gobosh.DICOM.DataElement root = mDocument.GetRootNode();

		// create a new treestore
		TreeStore store = new TreeStore(
			// 0008			0010			"blabla"		reference
			typeof(string), typeof(string), typeof(string), typeof(Gobosh.DICOM.DataElement));
		view.Model = store;

		// creating the root node of the tree including the reference to the
		// Root Node of the Gobosh.DICOM.Document
		TreeIter myRootNode = store.AppendValues("0000","0000","Rootnode",root);

		// populate the tree with all sub nodes
		PopulateNode(store,myRootNode,root);
		
		// Select the first node
	}
	
	protected void PopulateNode(TreeStore store, TreeIter node, Gobosh.DICOM.DataElement elem)
	{
		foreach(Gobosh.DICOM.DataElement n in elem)
		{
			TreeIter newNode = store.AppendValues(
				node,n.GroupTag.ToString("x4"),
				n.ElementTag.ToString("x4"),
				n.GetDescription(),
				n
			);
			// store.SetValue(newNode,0,n.GetHumanReadableString());
			if (elem.Count > 0 )
			{
				PopulateNode(store,newNode,n);
			}
			
		}
	}

	// react on cursor change on the left treeview displaying the document structure
	protected virtual void OnTreeview1CursorChanged(object sender, System.EventArgs e)
	{
		// o is the treeview?
		Gtk.TreeView m = sender as Gtk.TreeView;
		
		Gtk.TreePath myPath = new Gtk.TreePath();
		Gtk.TreeViewColumn myColumn = new Gtk.TreeViewColumn();
		m.GetCursor(out myPath,out myColumn);
		Gtk.TreeIter myNode;
		if ( m.Model.GetIter(out myNode,myPath) )
		{
			Gobosh.DICOM.DataElement myDataElement;
			myDataElement = m.Model.GetValue(myNode,3) as Gobosh.DICOM.DataElement;			
			if ( myDataElement!= null )
			{
				PopulateValueTree(treeview2,myDataElement);
				// MessageBox(myDataElement.GetHumanReadableString());
			}
		}
	}
		
	protected virtual void PopulateValueTree(Gtk.TreeView view, Gobosh.DICOM.DataElement elem)
	{
		// create a new treestore
		TreeStore store = new TreeStore(
			// Type			Value 
			typeof(string), typeof(string));
		view.Model = store;
		string vr = elem.GetValueRepresentationAsString();
		
		TreeIter myRootNode = store.AppendValues(
			elem.GetValueRepresentationAsString(),
			elem.GetHumanReadableString()
			);
		
		if ( ( vr == "OB" ) || (vr == "OW") || (vr == "OF") )
		{
			// do nothing
			store.AppendValues(
				myRootNode,
				vr,
				"(binary data!)");
		}
		else
		{
			int i;
			// the Values do not have a Collection (unfortunately)
			for (i = 0; i < elem.GetValueCount() ; i++)
			{
				Gobosh.DICOM.DataElementValue val = elem.GetValue(i);
				TreeIter myNode = store.AppendValues(
					myRootNode,
					i.ToString(),
					val.GetValueAsString());
				// now append the scalar subnodes
				foreach (string key in val.Properties.Keys)
				{
					Gobosh.DICOM.VarProperty prop = val.GetProperty(key);
					store.AppendValues(
						myNode,
						key,prop.AsString());
				}
			}
		}
		
//		TreeIter myRootNode = store.AppendValues("0000","0000","Rootnode",root);

		// populate the tree with all sub nodes
//		PopulateNode(store,myRootNode,root);
	}

	protected virtual void OnAboutActivated(object sender, System.EventArgs e)
	{
		MessageBox("Gtk DICOM Viewer\n\n(C) 2007 Timo Kaluza\ntk@gobosh.net");
	}

	protected virtual void OnQuitActivated(object sender, System.EventArgs e)
	{
		Application.Quit();
	}

	protected virtual void OnSaveActivated(object sender, System.EventArgs e)
	{
		// save
		FileChooserDialog fc = new FileChooserDialog(
			"Choose a filename for the document",
			this,
			FileChooserAction.Save,
			"Cancel", ResponseType.Cancel,
			"Save",ResponseType.Accept);
		if ( fc.Run() == (int) ResponseType.Accept )
		{
			if ( fc.Filename.EndsWith(".xml") )
			{
				Gobosh.DICOM.XMLWriter.WriteToFile(fc.Filename,mDocument.GetRootNode());
			}
			else
			{
				mDocument.SaveToFile(fc.Filename);
			}
		}
		fc.Destroy();
	}
}