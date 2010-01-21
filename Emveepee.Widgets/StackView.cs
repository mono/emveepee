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
using System.Collections.Generic;
using Gdk;
using Gtk;

using Emveepee.Decoding;

namespace Emveepee.Widgets {

	internal class StackView : VPaned {

		StackList list;
		StackDetail detail;

		public StackView (ProfileData data, DisplayOptions options)
		{
			list = new StackList (data, options);
			ScrolledWindow sw = new ScrolledWindow ();
			sw.Add (list);
			sw.ShowAll ();
			Add1 (sw);
			detail = new StackDetail (data, options);
			detail.CurrentItem = list.SelectedItem;
			list.Selection.Changed += delegate { detail.CurrentItem = list.SelectedItem; };
			Add2 (detail);
			Position = 200;
		}
	
		internal class StackNode : Node {
			
			static List<Node> children = new List<Node> ();
			StackItem item;
				
			public StackNode (ProfileStore store, Node parent, StackItem item) : base (store, parent)
			{
				this.item = item;
			}
			
			public override List<Node> Children {
				get { return children; }
			}
				
			public StackItem StackItem {
				get { return item; }
			}

			public override string Name {
				get { return item.Name; }
			}
			
			public override ulong Value {
				get { return item.TotalCost; }
			}
		}
		
		class StackList : TreeView {

			class Store : ProfileStore {

				ulong total;

				public Store (ProfileData data, DisplayOptions options) : base (data, options)
				{
					nodes = new List<Node> ();
					foreach (StackItem item in data.StackItems) {
						if (item.TotalCost <= 0)
							continue;
						if (options.ShowWrappers || !item.IsWrapper)
							nodes.Add (new StackNode (this, null, item));
					}
					total = data.TotalStackCost;
				}

				public StackItem this [int index] {
					get { return (nodes [index] as StackNode).StackItem; }
				}
	
				public override void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value val)
				{
					Node node = (Node) iter;
					if (column == 0)
						val = new GLib.Value (node.Name);
					else if (column == 1) {
						double percent = (double) node.Value / (double) total * 100.0;
						val = new GLib.Value (String.Format ("{0,5:F2}%", percent));
					}
				}		
			}

			Store store;

			public StackList (ProfileData data, DisplayOptions options) : base ()
			{
				store = new Store (data, options);
				Model = new TreeModelAdapter (store);
				Selection.SelectPath (new TreePath ("0"));
				AppendColumn ("Percent", new CellRendererText (), "text", 1);
				TreeViewColumn col = new TreeViewColumn ("Method", new CellRendererText (), "text", 0);
				AppendColumn (col);
				ExpanderColumn = col;
				options.Changed += delegate { 
					data.FilteredAssemblies = options.Filters.ToArray ();
					store = new Store (data, options);
					Model = new TreeModelAdapter (store);
					Selection.SelectPath (new TreePath ("0"));
				};
			}

			public StackItem SelectedItem {
				get { 
					TreeModel model;
					TreePath[] paths = Selection.GetSelectedRows (out model);
					return paths.Length > 0 ? store [paths [0].Indices [0]] : null;
				}
				set { throw new NotImplementedException (); }
			}
	
		}

		class StackDetail : Notebook {

			class PageView : ScrolledWindow {
	
				TreeView view;

				public PageView () : base ()
				{
					view = new TreeView ();
					view.AppendColumn ("Count", new CellRendererText (), "text", 1);
					TreeViewColumn col = new TreeViewColumn ("Method", new CellRendererText (), "text", 0);
					view.AppendColumn (col);
					view.ExpanderColumn = col;
					view.Show ();
					Add (view);
				}

				public Store Store {
					set { view.Model = value == null ? null : new TreeModelAdapter (value); }
				}
			}

			class Store : ProfileStore {

				ulong total;

				public Store (ProfileData data, DisplayOptions options, List<StackItem> items) : base (data, options)
				{
					nodes = new List<Node> ();
					foreach (StackItem item in items)
						nodes.Add (new StackNode (this, null, item));
					nodes.Sort (Node.DescendingValue);
					total = data.TotalStackCost;
				}

				public override void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value val)
				{
					Node node = (Node) iter;
					if (column == 0)
						val = new GLib.Value (node.Name);
					else if (column == 1) {
						double percent = (double) node.Value / (double) total * 100.0;
						val = new GLib.Value (String.Format ("{0,5:F2}%", percent));
					}
				}		
			}

			DisplayOptions options;
			PageView callers;
			PageView calls;
			ProfileData data;
			StackItem item;
		
			public StackDetail (ProfileData data, DisplayOptions options)
			{
				this.data = data;
				this.options = options;
				Label callers_lbl = new Label (Mono.Unix.Catalog.GetString ("Callers"));
				callers_lbl.Show ();
				Label calls_lbl = new Label (Mono.Unix.Catalog.GetString ("Calls"));
				calls_lbl.Show ();
				callers = new PageView ();
				callers.Show ();
				calls = new PageView ();
				calls.Show ();
				AppendPage (calls, calls_lbl);
				AppendPage (callers, callers_lbl);
			}

			void Refresh ()
			{
				calls.Store = new Store (data, options, item == null ? new List<StackItem> () : item.Calls);
				callers.Store = new Store (data, options, item == null ? new List<StackItem> () : item.Callers);
			}

			public StackItem CurrentItem {
				get { return item; }
				set {
					if (item == value)
						return;
	
					item = value;
					Refresh ();
				}
			}
		}
	}
}
