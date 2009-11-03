//
// CombinedDesignView.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using MonoDevelop.Ide.Gui;
using Emveepee.Widgets;

namespace Emveepee.Addin {

	public class ProfilerViewContent : AbstractViewContent {

		ProfileView view;
		
		public ProfilerViewContent ()
		{
			view = new ProfileView ();
			view.Show ();
		}
		
		public override void Dispose ()
		{
			view = null;
			base.Dispose ();
		}
		
		public override void Load (string filename)
		{
			Console.WriteLine ("loading filename " + filename);
			ContentName = filename;
			if (System.IO.File.Exists (filename))
				Gtk.Application.Invoke (delegate { view.LoadProfile (filename); });
		}
		
		public override Gtk.Widget Control {
			get { return view; }
		}
		
		public override string UntitledName {
			get { return "Profile Data"; }
			set {}
		}
		
		public override bool IsDirty {
			get { return false; }
			set {}
		}
		
		public override bool IsReadOnly {
			get { return true; }
		}
	}
}

