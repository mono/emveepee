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

	public class UnloadedBlock : Block {

		bool is_appdomain;
		bool is_assembly;
		bool is_module;
		byte kind;
		uint id;
		string item_name;

		internal UnloadedBlock (Buffer raw, DirectivesBlock directives) : base (BlockCode.Unloaded)
		{
			kind = raw.ReadByte ();
			start_counter = raw.ReadUlong ();
			end_counter = raw.ReadUlong ();
			thread_id = raw.ReadUlong ();
			if (directives.LoadedElementsCarryId)
				id = raw.ReadUint ();
			item_name = raw.ReadString ();
					
			switch ((LoadedItemInfo) kind) {
			case LoadedItemInfo.APPDOMAIN:
				is_appdomain = true;
				break;
			case LoadedItemInfo.MODULE:
				is_module = true;
				break;
			case LoadedItemInfo.ASSEMBLY:
				is_assembly = true;
				break;
			default:
				throw new Exception (String.Format ("unknown load event kind {0}", kind));
			}
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public uint Id {
			get { return id; }
		}

		public bool IsAppDomain {
			get { return is_appdomain; }
		}

		public bool IsAssembly {
			get { return is_assembly; }
		}

		public bool IsModule {
			get { return is_module; }
		}

		public string ItemName {
			get { return item_name; }
		}
	}
}

