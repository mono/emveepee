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
using System.Collections.Generic;

namespace Emveepee.Decoding {	

	public struct ClassAllocationSummary {

		public uint Id;
		public uint ReachableInstances;
		public uint ReachableBytes;
		public uint UnreachableInstances;
		public uint UnreachableBytes;

		public ClassAllocationSummary (uint id, uint reachable_instances, uint reachable_bytes, uint unreachable_instances, uint unreachable_bytes)
		{
			Id = id;
			ReachableInstances = reachable_instances;
			ReachableBytes = reachable_bytes;
			UnreachableInstances = unreachable_instances;
			UnreachableBytes = unreachable_bytes;
		}
	}

	public class HeapSummaryBlock : Block {

		uint collection;
		List<ClassAllocationSummary> classes = new List<ClassAllocationSummary> ();

		internal HeapSummaryBlock (Buffer raw) : base (BlockCode.HeapSummary)
		{
			start_counter = raw.ReadUlong ();
			start_time = raw.ReadTime ();
			collection = raw.ReadUint ();
			for (uint class_id = raw.ReadUint (); class_id != 0; class_id = raw.ReadUint ())
				classes.Add (new ClassAllocationSummary (class_id, raw.ReadUint (), raw.ReadUint (), raw.ReadUint (), raw.ReadUint ()));
			end_counter = raw.ReadUlong ();
			end_time = raw.ReadTime ();
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public List<ClassAllocationSummary> Classes {
			get { return classes; }
		}

		public uint Collection {
			get { return collection; }
		}
	}
}

