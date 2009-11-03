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

	public class ProfileData {

		public class ClassItem {

                	public static Comparison<ClassItem> DescendingBytes = delegate (ClassItem a, ClassItem b) {
                        	int result = b.AllocatedBytes.CompareTo (a.AllocatedBytes);
                        	if (result == 0)
                                	result = a.Name.CompareTo (b.Name);
                        	return result;
                	};

			AssemblyInfo assembly;
			uint bytes;
			uint id;
			string name;
			Dictionary<StackItem, uint> allocations;
			Dictionary<StackNode, uint> allocations_by_stack;

			public ClassItem (uint id, AssemblyInfo assembly, string name)
			{
				this.id = id;
				this.assembly = assembly;
				this.name = name;
			}

			public uint AllocatedBytes {
				get { return bytes; }
			}

			public Dictionary<StackItem, uint> Allocations {
				get {
					if (allocations == null)
						allocations = new Dictionary<StackItem, uint> ();
					return allocations;
				}
			}

			public Dictionary<StackNode, uint> AllocationsByStack {
				get {
					if (allocations_by_stack == null)
						allocations_by_stack = new Dictionary<StackNode, uint> ();
					return allocations_by_stack;
				}
			}

			public AssemblyInfo Assembly {
				get { return assembly; }
			}
			
			public uint Id {
				get { return id; }
			}
			
			public string Name {
				get { return name; }
			}

			public void InstanceAllocated (uint size, StackItem allocator, StackNode stack)
			{
				bytes += size;
				if (allocator != null) {
					if (Allocations.ContainsKey (allocator))
						Allocations [allocator]++;
					else
						Allocations [allocator] = 1;
				}
				if (stack != null) {
					if (AllocationsByStack.ContainsKey (stack))
						AllocationsByStack [stack]++;
					else
						AllocationsByStack [stack] = 1;
				}
			}
		}

		AssemblyInfo[] assemblies = new AssemblyInfo [10];
		ClassItem[] classes = new ClassItem [10];
		DateTime current_time;
		DateTime end_time;
		DateTime start_time;
		FilteredCallStack stack = new FilteredCallStack ();
		DirectivesBlock directives;
		RegionInfo[] regions = new RegionInfo [10];
		StackItem[] methods = new StackItem [10];
		StackItem[] functions = new StackItem [10];
		bool has_allocation_data = false;
		bool has_stack_data = false;
		double ticks_per_counter_unit;
		string runtime_file;
		uint version;
		ulong current_counter;
		ulong end_counter;
		ulong start_counter;
		
		public ProfileData (string filename)
		{
			using (LogFileStream stream = new LogFileStream (filename)) {
				while (stream.ReadBlock ())
					AddBlock (stream.CurrentBlock);
			}
		}

		StackNode CurrentStack {
			get { return stack.CurrentNode; }
		}

		void AddBlock (Block block)
		{
			switch (block.Code) {
			case BlockCode.Directives:
				AddDirectivesBlock (block as DirectivesBlock);
				break;
			case BlockCode.End:
				AddEndBlock (block as EndBlock);
				break;
			case BlockCode.Events:
				AddEventsBlock (block as EventsBlock);
				break;
			case BlockCode.Intro:
				AddIntroBlock (block as IntroBlock);
				break;
			case BlockCode.Loaded:
				AddLoadedBlock (block as LoadedBlock);
				break;
			case BlockCode.Mapping:
				AddMappingBlock (block as MappingBlock);
				break;
			case BlockCode.Statistical:
				AddStatBlock (block as StatBlock);
				break;
			case BlockCode.Unloaded:
			default:
				break;
			}
		}

		void AddAllocation (AllocationInfo info)
		{
			has_allocation_data = true;
			if (info.Id < 0 || info.Id >= classes.Length || classes [info.Id] == null)
				throw new Exception ("Allocation info received for unknown class");
			if (info.Caller < 0 || info.Caller >= methods.Length)
				throw new Exception ("Allocation info received for unknown class");
			ClassItem klass = classes [info.Id];
			klass.InstanceAllocated (info.Size, methods [info.Caller], directives.AllocationsHaveStackTrace ? CurrentStack : null);
		}

		void AddClass (ClassInfo class_info)
		{
			if (class_info.Id >= classes.Length) {
				ClassItem[] grow = new ClassItem [((class_info.Id / 10) + 1) * 10];
				classes.CopyTo (grow, 0);
				classes = grow;
			}
			classes [class_info.Id] = new ClassItem (class_info.Id, assemblies [class_info.AssemblyId], class_info.Name);
		}

		void AddDirectivesBlock (DirectivesBlock block)
		{
			directives = block;
			UpdateCounterAndTime (block.EndCounter, block.EndTime);
		}

		void AddEndBlock (EndBlock block)
		{
			if (this.version != block.Version) {
				throw new Exception (String.Format ("Version {0} specified at start is inconsistent witn {1} specified at end", this.version, version));
			}
			end_counter = block.EndCounter;
			end_time = block.EndTime;
			UpdateCounterAndTime (end_counter, end_time);
		}

		void AddEventsBlock (EventsBlock block)
		{
			UpdateCounterAndTime (block.StartCounter, block.StartTime);
			stack.CurrentThread = block.ThreadId;
			foreach (EventInfo info in block.Events) {
				switch (info.Type) {
				case EventInfoType.Allocation:
					AddAllocation (info as AllocationInfo);
					break;
				case EventInfoType.Enter:
					EnterMethod (info as MethodEventInfo);
					break;
				case EventInfoType.ExitExplicit:
					ExitMethod (info as MethodEventInfo);
					break;
				default:
					break;
					//throw new Exception ("Unsupported event code " + info.Type);
				}
			}
			UpdateCounterAndTime (block.EndCounter, block.EndTime);
		}

		void AddFunction (uint id, string name, string provider)
		{
			if (id >= functions.Length) {
				StackItem[] grow = new StackItem [((id / 10) + 1) * 10];
				functions.CopyTo (grow, 0);
				functions = grow;
			}
			functions [id] = new StackItem (name, provider);
		}

		void AddIntroBlock (IntroBlock block)
		{
			version = block.Version;
			runtime_file = block.RuntimeFile;
			start_counter = block.StartCounter;
			start_time = block.StartTime;
		}
		
		void AddLoadedBlock (LoadedBlock block)
		{
			if (block.IsAssembly) {
				if (block.Id >= assemblies.Length) {
					AssemblyInfo[] grow = new AssemblyInfo [((block.Id / 10) + 1) * 10];
					assemblies.CopyTo (grow, 0);
					assemblies = grow;
				}
				assemblies [block.Id] = block.Assembly;
			}
		}
		
		void AddMappingBlock (MappingBlock block)
		{
			UpdateCounterAndTime (block.StartCounter, block.StartTime);
			stack.CurrentThread = block.ThreadId;
			foreach (ClassInfo klass in block.Classes)
				AddClass (klass);
			foreach (MethodInfo method in block.Methods)
				AddMethod (method);
			UpdateCounterAndTime (block.EndCounter, block.EndTime);
		}

		void AddMethod (MethodInfo method_info)
		{
			if (method_info.Id >= methods.Length) {
				StackItem[] grow = new StackItem [((method_info.Id / 10) + 1) * 10];
				methods.CopyTo (grow, 0);
				methods = grow;
			}
			ClassItem class_item = classes [method_info.ClassId];
			string name = class_item.Name + "." + method_info.Name;
			string provider = class_item.Assembly.BaseName;
			methods [method_info.Id] = new StackItem (name, provider, method_info.IsWrapper);
		}

		void AddRegion (RegionInfo info)
		{
			if (info.Id >= regions.Length) {
				RegionInfo[] grow = new RegionInfo [((info.Id / 10) + 1) * 10];
				regions.CopyTo (grow, 0);
				regions = grow;
			}
			regions [info.Id] = info;
		}

		StackItem GetStackItem (StatInfo info)
		{
			switch (info.Code) {
			case StatCode.Method:
				if (info.Id == 0 || info.Id > methods.Length)
					return StackItem.Unknown;
				return methods [info.Id];
			case StatCode.UnmanagedFunctionId:
				if (info.Id == 0 || info.Id > functions.Length)
					return StackItem.Unknown;
				return functions [info.Id];
			case StatCode.UnmanagedFunctionNewId:
				AddFunction (info.Id, info.Name, regions [info.Region].Filename);
				return functions [info.Id];
			case StatCode.UnmanagedFunctionOffsetInRegion:
				return StackItem.Unknown;
			default:
				throw new Exception ("Unexpected stat code " + info.Code);
			}
		}

		void EnterMethod (MethodEventInfo info)
		{
			if (info.MethodId < 0 || info.MethodId >= methods.Length)
				throw new Exception ("unknown method id");
			has_stack_data = true;
			StackItem method = methods [info.MethodId];
			StackNode node = new StackNode (method, stack.CurrentNode);
			node.start_counter = info.Counter;
			stack.PushNode (node);
		}

		void ExitMethod (MethodEventInfo info)
		{
			if (info.MethodId < 0 || info.MethodId >= methods.Length)
				throw new Exception ("unknown method id");
			StackItem method = methods [info.MethodId];
			stack.PopNode (method, info.Counter);
		}

		void AddChain (StatInfo chain, StackItem last_hit)
		{
			if (last_hit == null)
				throw new Exception ("Unexpected call chain");

			List<StackItem> items = new List<StackItem> ();
			foreach (StatInfo caller in chain.Chain)
				items.Insert (0, GetStackItem (caller));
			items.Add (last_hit);
			stack.AddTrace (items);
		}

		void AddStatBlock (StatBlock block)
		{
			has_stack_data = true;
			UpdateCounterAndTime (block.StartCounter, block.StartTime);
			StackItem item = null;
			foreach (StatInfo info in block.Items) {
				switch (info.Code) {
				case StatCode.CallChain:
					AddChain (info, item);
					item = null;
					Console.WriteLine ("Stat call chain of {0} items", info.Chain.Count);
					break;
				case StatCode.Method:
				case StatCode.UnmanagedFunctionId:
				case StatCode.UnmanagedFunctionNewId:
				case StatCode.UnmanagedFunctionOffsetInRegion:
					if (item != null)
						stack.AddNode (new StackNode (item, null));
					item = GetStackItem (info);
					if (item != null)
						item.TotalCost++;
					break;
				case StatCode.Regions:
					foreach (uint id in info.InvalidRegions)
						if (id >= 0 && id < regions.Length)
							regions [id] = null;
					foreach (RegionInfo r in info.Regions)
						AddRegion (r);
					break;
				default:
					throw new Exception ("Unexpected stat code " + info.Code);
				}
			}
			if (item != null)
				stack.AddNode (new StackNode (item, null));
			UpdateCounterAndTime (block.EndCounter, block.EndTime);
		}

		void UpdateCounterAndTime (ulong current_counter, DateTime current_time)
		{
			this.current_counter = current_counter;
			this.current_time = current_time;
			UpdateTicksPerCounterUnit ();
		}

		void UpdateTicksPerCounterUnit ()
		{
			if (current_counter > start_counter) {
				ulong counter_span = current_counter - start_counter;
				TimeSpan time_span = current_time - start_time;
				ticks_per_counter_unit = ((double)time_span.Ticks) / ((double)counter_span);
			}
		}

		public List<StackItem> StackItems {
			get { return stack.StackItems; }
		}

		public List<StackNode> CallTree {
			get { return stack.CallTree; }
		}

		public List<ClassItem> Classes {
			get {
				List<ClassItem> result = new List<ClassItem> ();
				foreach (ClassItem item in classes)
					if (item != null)
						result.Add (item);
				result.Sort (ClassItem.DescendingBytes);
				return result;
			}
		}

		public string[] FilteredAssemblies {
			get { return stack.FilteredAssemblies; }
			set { stack.FilteredAssemblies = value; }
		}

		public bool HasAllocationData {
			get { return has_allocation_data; }
		}

		public bool HasStackData {
			get { return has_stack_data; }
		}

		public string RuntimeFile {
			get { return runtime_file; }
		}

		public bool ShowWrappers {
			get { return stack.ShowWrappers; }
			set { stack.ShowWrappers = value; }
		}

		public ulong TotalStackCost {
			get { return stack.TotalCost; }
		}
	}
}
		
