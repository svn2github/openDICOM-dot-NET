/* project created on 16.03.2007 at 10:52

	Applicaton stub for GtkDicomViewer
	
	(C) 2006,2007 Timo Kaluza (tk@gobosh.net)

*/
using System;
using Gtk;

namespace GtkDicomViewer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}