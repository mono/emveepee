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

	public class DirectivesBlock : Block {

		bool allocationsCarryCallerMethod;
		bool allocationsCarryId;
		bool allocationsHaveStackTrace;
		bool classesCarryAssemblyId;
		bool loadedElementsCarryId;
		bool methodsCarryWrapperFlag;

		enum DirectiveCodes {
			End = 0,
			AllocationsCarryCaller = 1,
			AllocationsHaveStack = 2,
			AllocationsCarryId = 3,
			LoadedElementsCarryId = 4,
			ClassesCarryAssemblyId = 5,
			MethodsCarryWrapperFlag = 6,
			LAST
		}

		internal DirectivesBlock (Buffer raw) : base (BlockCode.Directives)
		{
			start_counter = raw.ReadUint ();
			start_time = raw.ReadTime ();
					
			DirectiveCodes directive = (DirectiveCodes) raw.ReadUint ();
			while (directive != DirectiveCodes.End) {
				switch (directive) {
				case DirectiveCodes.AllocationsCarryCaller:
					allocationsCarryCallerMethod = true;
					break;
				case DirectiveCodes.AllocationsHaveStack:
					allocationsHaveStackTrace = true;
					break;
				case DirectiveCodes.AllocationsCarryId:
					allocationsCarryId = true;
					break;
				case DirectiveCodes.LoadedElementsCarryId:
					loadedElementsCarryId = true;
					break;
				case DirectiveCodes.ClassesCarryAssemblyId:
					classesCarryAssemblyId = true;
					break;
				case DirectiveCodes.MethodsCarryWrapperFlag:
					methodsCarryWrapperFlag = true;
					break;
				default:
					throw new Exception (String.Format ("unknown directive {0} at offset {1}", directive, raw.Offset));
				}
					
				directive = (DirectiveCodes) raw.ReadUint ();
			}

			end_counter = raw.ReadUint ();
			end_time = raw.ReadTime ();
			if (!raw.IsEmpty)
				throw new Exception ("Unexpected data remaining in block");
		}

		public bool AllocationsCarryCallerMethod {
			get { return allocationsCarryCallerMethod; }
		}
		
		public bool AllocationsCarryId {
			get { return allocationsCarryId; }
		}

		public bool AllocationsHaveStackTrace {
			get { return allocationsHaveStackTrace; }
		}
		
		public bool ClassesCarryAssemblyId {
			get { return classesCarryAssemblyId; }
		}
		
		public bool LoadedElementsCarryId {
			get { return loadedElementsCarryId; }
		}

		public bool MethodsCarryWrapperFlag {
			get { return methodsCarryWrapperFlag; }
		}
	}
}

