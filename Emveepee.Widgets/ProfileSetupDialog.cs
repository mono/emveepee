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
using System.Text;
using Gtk;

namespace Emveepee.Widgets {
	
	public class ProfileSetupDialog : Dialog {
		
		ProfileConfiguration config;
		
		public ProfileSetupDialog (Gtk.Window parent) : base ("Profile Options", parent, DialogFlags.DestroyWithParent, Stock.Cancel, ResponseType.Cancel, Stock.Execute, ResponseType.Accept)
		{
			config = new ProfileConfiguration ();
			HBox box = new HBox (false, 6);
			box.PackStart (new Label ("Application:"), false, false, 0);
			FileChooserButton target_button = new FileChooserButton ("Select Application", FileChooserAction.Open);
			FileFilter filter = new FileFilter ();
			filter.AddPattern ("*.exe");
			filter.AddPattern ("*.aspx");
			target_button.Filter = filter;
			target_button.SelectionChanged += delegate { 
				config.TargetPath = target_button.Filename;
				SetResponseSensitive (ResponseType.Accept, !String.IsNullOrEmpty (target_button.Filename));
			};
			box.PackStart (target_button, true, true, 0);
			box.ShowAll ();
			VBox.PackStart (box, false, false, 3);
			Widget editor = new ProfileOptionsEditor (config);
			editor.ShowAll ();
			VBox.PackStart (editor, false, false, 3);
			SetResponseSensitive (ResponseType.Accept, false);
		}

		public ProfileConfiguration Config {
			get { return config; }
		}
	}
}
