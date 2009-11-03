// Copyright (c) 2009 Novell, Inc  http://www.novell.com
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
using System.IO;
using System.Collections.Generic;

namespace Emveepee.Decoding {

	internal class FilteredCallStack {

		bool refilter_needed = true;
		bool show_wrappers = true;
		string[] filtered_assemblies = new string [0];
		ulong current_thread_id;
		List<StackItem> items = new List<StackItem> ();
		List<StackNode> filtered_nodes = new List<StackNode> ();
		List<StackNode> nodes = new List<StackNode> ();
		Dictionary<ulong, StackNode> current_by_thread = new Dictionary<ulong, StackNode> ();

		void AddNodeToItem (StackNode node)
		{
			StackItem item = node.StackItem;
			if (!items.Contains (item)) {
				item.Nodes.Clear ();
				item.TotalCost = 0;
				items.Add (item);
			}
			StackNode recursive_parent = node.Parent;
			while (recursive_parent != null && recursive_parent.StackItem != item)
				recursive_parent = recursive_parent.Parent;
			if (recursive_parent == null)
				item.TotalCost += node.Cost;

			item.Nodes.Add (node);
		}

		StackNode FindItemInList (List<StackNode> nodes, StackItem item)
		{
			foreach (StackNode node in nodes)
				if (node.StackItem == item)
					return node;
			return null;
		}

		void FilterChildren (StackNode node, StackNode filtered_parent, List<StackNode> result, bool filtering)
		{
			foreach (StackNode child in node.Children) {
				if ((child.StackItem.IsWrapper && !ShowWrappers) || (filtering && Array.IndexOf (filtered_assemblies, child.StackItem.Provider) >= 0)) {
					FilterChildren (child, filtered_parent, result, filtering);
				} else {
					StackNode filtered = new StackNode (child.StackItem, filtered_parent);
					filtered.Cost = child.Cost;
					result.Add (filtered);
					AddNodeToItem (filtered);
					FilterChildren (child, filtered, filtered.Children, Array.IndexOf (filtered_assemblies, child.StackItem.Provider) >= 0);
				}
			}
		}

		void FilterNodes ()
		{
			filtered_nodes.Clear ();
			items.Clear ();
			foreach (StackNode node in nodes) {
				if (node.StackItem.IsWrapper && !ShowWrappers) {
					FilterChildren (node, null, filtered_nodes, false);
				} else {
					StackNode filtered = new StackNode (node.StackItem);
					filtered.Cost = node.Cost;
					filtered_nodes.Add (filtered);
					AddNodeToItem (filtered);
					FilterChildren (node, filtered, filtered.Children, Array.IndexOf (filtered_assemblies, node.StackItem.Provider) >= 0);
				}
			}
			items.Sort (StackItem.DescendingCost);
			refilter_needed = false;
		}

		public List<StackNode> CallTree {
			get {
				if (refilter_needed)
					FilterNodes ();
				return filtered_nodes;
			}
		}

		public StackNode CurrentNode {
			get { return current_by_thread.ContainsKey (CurrentThread) ? current_by_thread [CurrentThread] : null; }
			internal set { current_by_thread [CurrentThread] = value; }
		}
				
		public ulong CurrentThread {
			get { return current_thread_id; }
			set { current_thread_id = value; }
		}

		public string[] FilteredAssemblies {
			get { return filtered_assemblies; }
			set { 
				if (filtered_assemblies == value)
					return;
				filtered_assemblies = value;
				refilter_needed = true;
			}
		}

		public bool ShowWrappers {
			get { return show_wrappers; }
			set {
				if (show_wrappers == value)
					return;
				show_wrappers = value;
				refilter_needed = true;
			}
		}

		public List<StackItem> StackItems {
			get {
				if (refilter_needed)
					FilterNodes ();
				return items;
			}
		}

		public ulong TotalCost {
			get {
				if (refilter_needed)
					FilterNodes ();
				ulong result = 0;
				foreach (StackNode node in filtered_nodes)
					result += node.Cost;
				return result;
			}
		}

		public void AddNode (StackNode node)
		{
			nodes.Add (node);
			refilter_needed = true;
		}

		public void AddTrace (List<StackItem> items)
		{
			StackNode parent = null;
			List<StackNode> node_list = nodes;
			for (int i = 0; i < items.Count; i++) {
				StackItem curr = items [i];
				StackNode node = FindItemInList (node_list, curr);
				if (node == null) {
					node = new StackNode (curr, parent);
					node_list.Add (node);
				}
				node.Cost++;
				node.StackItem.TotalCost++;
				node_list = node.Children;
				parent = node;
			}
			refilter_needed = true;
		}

		public void PopNode (StackItem item, ulong counter)
		{
			StackNode curr = CurrentNode;
			while (curr != null && curr.StackItem != item)
				curr = curr.Parent;
			if (curr == null)
				throw new Exception ("Exiting method " + item.Name + " was not on the stack");
			curr.Cost = counter - curr.start_counter;
			CurrentNode = curr.Parent;
		}

		public void PushNode (StackNode node)
		{
			StackNode current = CurrentNode;
			if (current == null)
				nodes.Add (node);
			else
				current.Children.Add (node);
			CurrentNode = node;
			refilter_needed = true;
		}
	}
}

