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

	public class StackItem {

		public static StackItem Unknown = new StackItem ("[Unknown]", "[Unknown]");

                public static Comparison<StackItem> DescendingCost = delegate (StackItem a, StackItem b) {
                        int result = b.TotalCost.CompareTo (a.TotalCost);
                        if (result == 0)
                                result = a.Name.CompareTo (b.Name);
                        return result;
                };

		List<StackNode> nodes = new List<StackNode> ();
		bool is_wrapper;
		string name;
		string provider;
		ulong total_cost;

		internal StackItem (string name, string provider) : this (name, provider, false) {}

		internal StackItem (string name, string provider, bool is_wrapper)
		{
			this.name = name;
			this.provider = provider;
			this.is_wrapper = is_wrapper;
		}

		internal List<StackNode> Nodes {
			get { return nodes; }
		}

		public List<StackItem> Callers {
			get {
				List<StackItem> callers = new List<StackItem> ();
				foreach (StackNode node in nodes) {
					if (node.Parent == null)
						continue;
					else if (!callers.Contains (node.Parent.StackItem))
						callers.Add (node.Parent.StackItem);
				}
				callers.Sort (DescendingCost);
				return callers;
			}
		}

		public List<StackItem> Calls {
			get {
				List<StackItem> calls = new List<StackItem> ();
				foreach (StackNode node in nodes)
					foreach (StackNode child in node.Children)
						if (!calls.Contains (child.StackItem))
							calls.Add (child.StackItem);
				calls.Sort (DescendingCost);
				return calls;
			}
		}

		public bool IsWrapper {
			get { return is_wrapper; }
		}

		public string Name {
			get { return name; }
		}

		public string Provider {
			get { return provider; }
		}

		public ulong TotalCost {
			get { return total_cost; }
			set { total_cost = value; }
		}
	}
}

