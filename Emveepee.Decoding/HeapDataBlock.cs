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

	public class ReferenceInfo {
		uint class_id;
		uint size;
		ulong object_id;
		List<ulong> references;

		public ReferenceInfo (uint class_id, uint size, ulong object_id)
		{
			this.class_id = class_id;
			this.size = size;
			this.object_id = object_id;
		}

		public uint ClassId {
			get { return class_id; }
		}

		public uint Size {
			get { return size; }
		}

		public ulong ObjectId {
			get { return object_id; }
		}

		public List<ulong> References {
			get {
				if (references == null)
					references = new List<ulong> ();
				return references;
			}
		}
	}

	public class HeapDataBlock : Block {

		DateTime job_start_time;
		DateTime job_end_time;
		ulong job_start_counter;
		ulong job_end_counter;
		uint collection;
		List<ReferenceInfo> refs = new List<ReferenceInfo> ();

                enum HeapSnapshotCode {
                        None = 0,
                        Object = 1,
                        FreeObjectClass = 2,
                }

		internal HeapDataBlock (Buffer raw) : base (BlockCode.HeapData)
		{
			job_start_counter = raw.ReadUlong ();
			job_start_time = raw.ReadTime ();
			job_end_counter = raw.ReadUlong ();
			job_end_time = raw.ReadTime ();
			collection = raw.ReadUint ();
			start_counter = raw.ReadUlong ();
			start_time = raw.ReadTime ();
				
			for (ulong item = raw.ReadUlong (); item != 0; item = raw.ReadUlong ()) {
				HeapSnapshotCode code = (HeapSnapshotCode) (item & 0x3);
				uint class_id;
				uint size;
				switch (code) {
				case HeapSnapshotCode.FreeObjectClass:
					class_id = (uint) (item >> 2);
					size = raw.ReadUint ();
					refs.Add (new ReferenceInfo (class_id, size, 0));
					break;
				case HeapSnapshotCode.Object:
					class_id = raw.ReadUint ();
					size = raw.ReadUint ();
					int ref_count = (int) raw.ReadUint ();
					// this works because the two low order bits of the object_id are always unset since it's a ptr
					ulong object_id = item & (~ (ulong) 0x3);
					ReferenceInfo info = new ReferenceInfo (class_id, size, object_id);
					for (int i = 0; i < ref_count; i++)
						info.References.Add (raw.ReadUlong ());
					refs.Add (info);
					break;
				default:
					throw new Exception ("unexpected item code " + code);
				}
			}
			end_counter = raw.ReadUlong ();
			end_time = raw.ReadTime ();
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public uint Collection {
			get { return collection; }
		}

		public ulong JobStartCounter {
			get { return job_start_counter; }
		}

		public ulong JobEndCounter {
			get { return job_end_counter; }
		}

		public DateTime JobStartTime {
			get { return job_start_time; }
		}

		public DateTime JobEndTime {
			get { return job_end_time; }
		}

		public List<ReferenceInfo> References {
			get { return refs; }
		}
	}
}

