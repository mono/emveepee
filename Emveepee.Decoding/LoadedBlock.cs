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

	public struct AssemblyInfo {

		public string BaseName;
		public uint Major;
		public uint Minor;
		public uint Build;
		public uint Revision;
		public string Culture;
		public string PublicKeyToken;
		public bool Retargetable;

		public AssemblyInfo (string base_name) : this (base_name, 0, 0, 0, 0, "neutral", "null", false) {}

		public AssemblyInfo (string base_name, uint major, uint minor, uint build, uint revision, string culture, string key, bool retargetable)
		{
			BaseName = base_name;
			Major = major;
			Minor = minor;
			Build = build;
			Revision = revision;
			Culture = culture;
			PublicKeyToken = key;
			Retargetable = retargetable;
		}
	}

	public enum LoadedItemInfo {
		MODULE = 1,
		ASSEMBLY = 2,
		APPDOMAIN = 4,
		SUCCESS = 8,
		FAILURE = 16
	}

	public class LoadedBlock : Block {

		bool is_appdomain;
		bool is_assembly;
		bool is_module;
		bool success;
		byte kind;
		string item_name;
		uint id;
		AssemblyInfo assembly_info;

		internal LoadedBlock (Buffer raw, DirectivesBlock directives) : base (BlockCode.Loaded)
		{
			kind = raw.ReadByte ();
			start_counter = raw.ReadUlong ();
			end_counter = raw.ReadUlong ();
			thread_id = raw.ReadUlong ();
			if (directives.LoadedElementsCarryId)
				id = raw.ReadUint ();
			item_name = raw.ReadString ();
					
			success = ((kind & (byte)LoadedItemInfo.SUCCESS) != 0);
			kind &= (byte) (LoadedItemInfo.APPDOMAIN|LoadedItemInfo.ASSEMBLY|LoadedItemInfo.MODULE);

			switch ((LoadedItemInfo) kind) {
			case LoadedItemInfo.APPDOMAIN:
				is_appdomain = true;
				break;
			case LoadedItemInfo.MODULE:
				is_module = true;
				break;
			case LoadedItemInfo.ASSEMBLY:
				is_assembly = true;
				if (directives.ClassesCarryAssemblyId)
					assembly_info = new AssemblyInfo (raw.ReadString (), raw.ReadUint (), raw.ReadUint (), raw.ReadUint (), raw.ReadUint (), raw.ReadString (), raw.ReadString (), raw.ReadUint () != 0);
				else {
					int commaPosition = item_name.IndexOf (',');
					assembly_info = new AssemblyInfo (commaPosition > 0 ? item_name.Substring (0, commaPosition) : "UNKNOWN");
				}
				break;
			default:
				throw new Exception (String.Format ("unknown load event kind {0}", kind));
			}
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public AssemblyInfo Assembly {
			get { return assembly_info; }
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

		public bool Success {
			get { return success; }
		}
	}
}

