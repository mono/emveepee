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

	public class ClassInfo {

		public uint Id;
		public uint AssemblyId;
		public string Name;

		public ClassInfo (uint id, uint assembly, string name)
		{
			Id = id;
			AssemblyId = assembly;
			Name = name;
		}
	}

	public class MethodInfo {

		uint id;
		public uint Id {
			get { return id; }
		}

		uint class_id;
		public uint ClassId {
			get { return class_id; }
		}

		bool is_wrapper;
		public bool IsWrapper {
			get { return is_wrapper; }
		}

		string name;
		public string Name {
			get { return name; }
		}

		public MethodInfo (uint id, uint class_id, bool is_wrapper, string name)
		{
			this.id = id;
			this.class_id = class_id;
			this.is_wrapper = is_wrapper;
			this.name = name;
		}
	}

	public class MappingBlock : Block {

		List<ClassInfo> classes = new List<ClassInfo> ();
		List<MethodInfo> methods = new List<MethodInfo> ();

		internal MappingBlock (Buffer raw, DirectivesBlock directives) : base (BlockCode.Mapping)
		{
			start_counter = raw.ReadUlong ();
			start_time = raw.ReadTime ();
			thread_id = raw.ReadUlong ();
					
			for (uint item_id = raw.ReadUint (); item_id != 0; item_id = raw.ReadUint ()) {
				uint assembly = 0;
				if (directives.ClassesCarryAssemblyId)
					assembly = raw.ReadUint ();
				string name = raw.ReadString ();
				classes.Add (new ClassInfo (item_id, assembly, name));
			}
					
			for (uint item_id = raw.ReadUint (); item_id != 0; item_id = raw.ReadUint ()) {
				uint class_id = raw.ReadUint ();
				bool is_wrapper = false;
				if (directives.MethodsCarryWrapperFlag)
					is_wrapper = raw.ReadUint () != 0;
				string name = raw.ReadString ();
				methods.Add (new MethodInfo (item_id, class_id, is_wrapper, name));
			}
					
			end_counter = raw.ReadUlong ();
			end_time = raw.ReadTime ();
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public List<ClassInfo> Classes {
			get { return classes; }
		}

		public List<MethodInfo> Methods {
			get { return methods; }
		}
	}
}

