// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
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


using System;
using Emveepee.Decoding;

namespace Emveepee.Widgets {
	
	[System.ComponentModel.ToolboxItem (true)]
	public class ProfileView : Gtk.EventBox {

		bool supports_filtering = false;
		DisplayOptions options;
		string path;
		
		public string LogFile {
			get { return path; }
		}
		
		public bool LoadProfile (string path)
		{
			this.path = path;
			ProfileData data = new ProfileData (path);
			data.ShowWrappers = Options.ShowWrappers;
			data.FilteredAssemblies = Options.Filters.ToArray ();
			Gtk.Widget view = null;
			if (data.HasAllocationData)
				view = new AllocationsView (data, Options);
			else if (data.HasStackData)
				view = new StackView (data, Options);
			view.ShowAll ();
			View = view;
			return true;
		}
		
		public DisplayOptions Options {
			get { 
				if (options == null)
					options = new DisplayOptions ();
				return options;
			}
		}
		
		public bool SupportsFiltering {
			get { return supports_filtering; }
		}

		Gtk.Widget View {
			get { return Child; }
			set {
				if (Child != null)
					Remove (Child);
				if (value != null)
					Add (value);
			}
		}
	}
}
