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

namespace  Emveepee.Decoding {	

	public enum StatCode {
		End = 0,
		Method = 1,
		UnmanagedFunctionId = 2,
		UnmanagedFunctionNewId = 3,
		UnmanagedFunctionOffsetInRegion = 4,
		CallChain = 5,
		Regions = 7
	}

	public class RegionInfo {
		public uint Id;
		public ulong Start;
		public uint Size;
		public uint Offset;
		public string Filename;

		public RegionInfo (uint id, ulong start, uint size, uint offset, string filename)
		{
			Id = id;
			Start = start;
			Size = size;
			Offset = offset;
			Filename = filename;
		}
	}

	public class StatInfo {

		public StatCode Code;
		public ulong Address;
		public List<StatInfo> Chain;
		public uint Id;
		public string Name;
		public uint Offset;
		public uint Region;
		public List<uint> InvalidRegions;
		public List<RegionInfo> Regions;

		public StatInfo (StatCode code)
		{
			Code = code;
			Address = 0;
			Chain = null;
			Id = 0;
			Name = null;
			Offset = 0;
			Region = 0;
			InvalidRegions = new List<uint> ();
			Regions = new List<RegionInfo> ();
		}
	}

	public class StatBlock : Block {

		List<StatInfo> items = new List<StatInfo> ();

		internal StatBlock (Buffer raw) : base (BlockCode.Statistical)
		{
			start_counter = raw.ReadUlong ();
			start_time = raw.ReadTime ();
					
			for (uint id = raw.ReadUint (); id != 0; id = raw.ReadUint ()) {
				StatInfo info = new StatInfo ((StatCode) (id & 0x7));
				uint data = id >> 3;

				switch (info.Code) {
				case StatCode.Method:
				case StatCode.UnmanagedFunctionId:
					info.Id = data;
					break;
				case StatCode.UnmanagedFunctionNewId:
					info.Region = data;
					info.Id = raw.ReadUint ();
					info.Name = raw.ReadString ();
					break;
				case StatCode.UnmanagedFunctionOffsetInRegion:
					info.Region = data;
					if (data != 0)
						info.Offset = raw.ReadUint ();
					else
						info.Address = raw.ReadUlong ();
					break;
				case StatCode.CallChain:
					info.Chain = new List<StatInfo> ();
					for (int i = 0; i < data; i++) {
						uint nid = raw.ReadUint ();
						StatInfo ninfo = new StatInfo ((StatCode) (nid & 0x7));
						uint ndata = nid >> 3;
						switch (ninfo.Code) {
						case StatCode.Method:
						case StatCode.UnmanagedFunctionId:
							ninfo.Id = ndata;
							break;
						case StatCode.UnmanagedFunctionNewId:
							ninfo.Region = ndata;
							ninfo.Id = raw.ReadUint ();
							ninfo.Name = raw.ReadString ();
							break;
						case StatCode.UnmanagedFunctionOffsetInRegion:
							ninfo.Region = ndata;
							if (ndata != 0)
								ninfo.Offset = raw.ReadUint ();
							else
								ninfo.Address = raw.ReadUlong ();
							break;
						default:
							throw new Exception ("unexpected code in call chain " + ninfo.Code);
						}
						info.Chain.Insert (0, ninfo);
					}
					break;
				case StatCode.Regions:
					for (uint region = raw.ReadUint (); region != 0; region = raw.ReadUint ())
						info.InvalidRegions.Add (region);
					for (uint region = raw.ReadUint (); region != 0; region = raw.ReadUint ())
						info.Regions.Add (new RegionInfo (region, raw.ReadUlong (), raw.ReadUint (), raw.ReadUint (), raw.ReadString ()));
					break;
				default:
					throw new Exception ("Unexpected code " + info.Code);
				}
				items.Add (info);
			}
					
			end_counter = raw.ReadUlong ();
			end_time = raw.ReadTime ();
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public List<StatInfo> Items {
			get { return items; }
		}
	}
}

