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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace  Emveepee.Decoding {	

	public enum BlockCode {
		Intro = 1,
		End = 2,
		Mapping = 3,
		Loaded = 4,
		Unloaded = 5,
		Events = 6,
		Statistical = 7,
		HeapData = 8,
		HeapSummary = 9,
		Directives = 10
	}
	
	public abstract class Block {

		BlockCode code;
		protected DateTime end_time;
		protected DateTime start_time;
		protected ulong end_counter;
		protected ulong start_counter;
		protected ulong thread_id;

		protected Block (BlockCode code)
		{
			this.code = code;
		}

		public BlockCode Code {
			get { return code; }
		}

		public ulong EndCounter {
			get { return end_counter; }
		}

		public DateTime EndTime {
			get { return end_time; }
		}

		public ulong StartCounter {
			get { return start_counter; }
		}

		public DateTime StartTime {
			get { return start_time; }
		}

		public ulong ThreadId {
			get { return thread_id; }
		}
	}
}

