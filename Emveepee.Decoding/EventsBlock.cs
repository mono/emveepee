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

	public enum EventInfoType {
		Enter = 1,
		ExitImplicit = 2,
		ExitExplicit = 3,
		Allocation = 4,
		Method = 5,
		Class = 6,
		Other = 7
	}
			
	public class EventInfo {

		EventInfoType type;

		protected EventInfo (EventInfoType type)
		{
			this.type = type;
		}

		public EventInfoType Type {
			get { return type; }
		}
	}

	public class AllocationInfo : EventInfo {

		public uint Id;
		public uint Size;
		public uint Caller;
		public ulong ObjectId;

		public AllocationInfo (uint id, uint size, uint caller, ulong object_id) : base (EventInfoType.Allocation)
		{
			Id = id;
			Size = size;
			Caller = caller;
			ObjectId = object_id;
		}
	}

	public class ClassEventInfo : EventInfo {

		enum ClassEventCode {
			Load = 0,
			Unload = 1,
			Exception = 2,
			Lock = 3,
		}

		uint id;
		ulong counter;
		ClassEventCode code;
		bool is_start;

		public ClassEventInfo (byte code, uint id, ulong counter) : base (EventInfoType.Class)
		{
			this.id = id;
			this.counter = counter;
			this.code = (ClassEventCode) (code & 0x3);
			is_start = (code & 0x10) == 0;
		}

		public ulong Counter {
			get { return counter; }
		}

		public uint Id {
			get { return id; }
		}

		public bool IsException {
			get { return code == ClassEventCode.Exception; }
		}

		public bool IsLoad {
			get { return code == ClassEventCode.Load; }
		}

		public bool IsStart {
			get { return is_start; }
		}

		public bool IsUnload {
			get { return code == ClassEventCode.Unload; }
		}
	}

	public enum GCEventType {
		Collection = 2,
		Mark = 3,
		Sweep = 4,
		Resize = 5,
		StopWorld = 6,
		StartWorld = 7,
	}
		
	public class GCEventInfo : EventInfo {

		byte event_data;
		uint collection_raw;
		ulong counter;
		GCEventType type;

		public GCEventInfo (byte type, uint collection_raw, ulong counter, byte event_data) : base (EventInfoType.Other)
		{
			this.type = (GCEventType) type;
			this.collection_raw = collection_raw;
			this.counter = counter;
			this.event_data = event_data;
		}

		public uint Collection {
			get { return type == GCEventType.Resize ? collection_raw : collection_raw >> 8; }
		}

		public ulong Counter {
			get { return counter; }
		}

		public uint Generation {
			get { return collection_raw & 0xff; }
		}

		public bool IsStart {
			get { return (event_data & 0x10) == 0; }
		}

		public GCEventType GCEventType {
			get { return type; }
		}
	}

	public class LockEventInfo : ClassEventInfo {

		ulong object_id;
		uint lock_event;

		public LockEventInfo (uint id, ulong counter, uint lock_event, ulong object_id) : base (3, id, counter)
		{
			this.lock_event = lock_event;
			this.object_id = object_id;
		}

		public uint LockEvent {
			get { return lock_event; }
		}

		public ulong ObjectId {
			get { return object_id; }
		}
	}

	public class MethodEventInfo : EventInfo {

		byte event_data;
		uint method_id;
		ulong counter;

		public MethodEventInfo (EventInfoType type, uint method_id, ulong counter, byte event_data) : base (type)
		{
			this.method_id = method_id;
			this.counter = counter;
			this.event_data = event_data;
		}

		public ulong Counter {
			get { return counter; }
		}

		public bool IsFreed {
			get { return (event_data & 0x1) != 0; }
		}

		public bool IsJit {
			get { return (event_data & 0x1) == 0; }
		}

		public bool IsStart {
			get { return (event_data & 0x10) == 0; }
		}

		public uint MethodId {
			get { return method_id; }
		}
	}

	public class StackEventInfo : EventInfo {

		public struct Item {
			public uint Id;
			public bool IsJitting;
			public Item (uint id, bool is_jitting)
			{
				Id = id;
				IsJitting = is_jitting;
			}
		}

		public uint LastValid;
		public List<Item> StackItems = new List<Item> ();

		public StackEventInfo (uint last_valid) : base (EventInfoType.Other)
		{
			LastValid = last_valid;
		}
	}

	public class ThreadEventInfo : EventInfo {

		byte event_data;
		ulong thread_id;
		ulong counter;

		public ThreadEventInfo (ulong thread_id, ulong counter, byte event_data) : base (EventInfoType.Other)
		{
			this.thread_id = thread_id;
			this.counter = counter;
			this.event_data = event_data;
		}

		public ulong Counter {
			get { return counter; }
		}

		public bool IsStart {
			get { return (event_data & 0x10) == 0; }
		}

		public ulong ThreadId {
			get { return thread_id; }
		}
	}

	public class EventsBlock : Block {

		List<EventInfo> events = new List<EventInfo> ();

		internal EventsBlock (Buffer raw, DirectivesBlock directives) : base (BlockCode.Events)
		{
			start_counter = raw.ReadUlong ();
			start_time = raw.ReadTime ();
			thread_id = raw.ReadUlong ();
					
			ulong base_counter = raw.ReadUlong ();

			for (byte event_code = raw.ReadByte (); event_code != 0; event_code = raw.ReadByte ()) {
				EventInfoType event_type = (EventInfoType) (event_code & 0x7);
				byte event_data = (byte) (event_code >> 3);

				switch (event_type) {
				case EventInfoType.Allocation:
					uint id = (raw.ReadUint () << 5) | (uint) event_data;
					uint size = raw.ReadUint ();
					uint caller = directives.AllocationsCarryCallerMethod ? raw.ReadUint () : 0;
					ulong object_id = directives.AllocationsCarryId ? raw.ReadUlong () : 0;
					events.Add (new AllocationInfo (id, size, caller, object_id));
					break;
				case EventInfoType.Class:
					uint class_id = raw.ReadUint ();
					base_counter += raw.ReadUlong ();
					if ((event_data & 0x3) == 3) {
						uint lock_event = raw.ReadUint ();
						ulong oid = raw.ReadUlong ();
						events.Add (new LockEventInfo (class_id, base_counter, lock_event, oid));
					} else
						events.Add (new ClassEventInfo (event_data, class_id, base_counter));
					break;
				case EventInfoType.Enter:
				case EventInfoType.ExitExplicit:
					uint method_id = raw.ReadUint () << 5 | (uint) event_data;
					base_counter += raw.ReadUlong ();
					events.Add (new MethodEventInfo (event_type, method_id, base_counter, 0));
					break;
				case EventInfoType.ExitImplicit:
					throw new Exception ("Implicit method exit events are unsupported.");
				case EventInfoType.Method:
					uint mid = raw.ReadUint ();
					base_counter += raw.ReadUlong ();
					events.Add (new MethodEventInfo (EventInfoType.Method, mid, base_counter, event_data));
					break;
				case EventInfoType.Other:
					byte generic_code = (byte) (event_data & 0xf);
					switch (generic_code) {
					case 1: // Thread
						ulong tid = raw.ReadUlong ();
						base_counter += raw.ReadUlong ();
						events.Add (new ThreadEventInfo (tid, base_counter, event_data));
						break; 
					case 2: // GC Collection	
					case 3: // GC Mark	
					case 4: // GC Sweep	
					case 6: // GC StopWorld	
					case 7: // GC StartWorld	
						uint collection = raw.ReadUint ();
						base_counter += raw.ReadUlong ();
						events.Add (new GCEventInfo (generic_code, collection, base_counter, event_data));
						break;
					case 5: // GC Resize	
						ulong new_size = raw.ReadUlong ();
						uint coll = raw.ReadUint ();
						events.Add (new GCEventInfo (generic_code, coll, new_size, event_data));
						break;
					case 8: // Jit Time Allocation	
						events.Add (new AllocationInfo (raw.ReadUint (), raw.ReadUint (), directives.AllocationsCarryCallerMethod ? raw.ReadUint () : 0, directives.AllocationsCarryId ? raw.ReadUlong () : 0));
						break;
					case 9: // Stack Section
						//FIXME: for now, we are just reading the data to get to the end
						StackEventInfo stack_info = new StackEventInfo (raw.ReadUint ());
						uint top_section_size = raw.ReadUint ();
								
						for (int i = 0; i < top_section_size; i++) {
							uint meth_id = raw.ReadUint ();
							bool is_jitting = (meth_id & 0x1) != 0;
							meth_id >>= 1;
							stack_info.StackItems.Add (new StackEventInfo.Item (meth_id, is_jitting));
						}
						events.Add (stack_info);
						break;
					default:
						throw new Exception ("unknown 'Other' event code " + generic_code);
					}
					break;
				default:
					throw new Exception ("Unexpected event type " + event_type);
				}
			}
					
			end_counter = raw.ReadUlong ();
			end_time = raw.ReadTime ();
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public List<EventInfo> Events {
			get { return events; }
		}
	}
}

