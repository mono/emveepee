// Copyright (c) 2009  Novell, Inc  http://www.novell.com
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

namespace Emveepee.Decoding {

	internal class Buffer {

		byte[] data;
		uint length;
		uint offset;

		public Buffer (byte[] data, uint length)
		{
			this.data = data;
			this.length = length;
		}

		public bool IsEmpty {
			get { return offset == length; }
		}

		public uint Offset {
			get { return offset; }
		}

		const byte SEVEN_BITS_MASK = 0x7f;
		const byte EIGHT_BIT_MASK = 0x80;
		
		public byte ReadByte ()
		{
			byte result = data [offset];
			offset++;
			return result;
		}
		
		static readonly DateTime epoch = new DateTime (1970, 1, 1, 0, 0, 0, 0);

		public DateTime ReadTime ()
		{
			ulong usecs = ReadUlong ();
			return epoch + TimeSpan.FromTicks ((long) usecs * 10);
		}
		
		public uint ReadUint ()
		{
			int factor = 0;
			uint r = 0;
			byte v;
			do {
				v = data [offset];
				r |= (((uint)(v & SEVEN_BITS_MASK)) << factor);
				offset++;
				factor += 7;
			} while ((v & EIGHT_BIT_MASK) == 0);
			return r;
		}
		
		public ulong ReadUlong ()
		{
			int factor = 0;
			ulong r = 0;
			byte v;
			do {
				v = data [offset];
				r |= (((ulong)(v & SEVEN_BITS_MASK)) << factor);
				offset++;
				factor += 7;
			} while ((v & EIGHT_BIT_MASK) == 0);
			return r;
		}
		
		public string ReadString ()
		{
			int count = 0;
			while (data [offset + count] != 0) {
				//LogLine ("Read string: data [offset+ count] is {0}", (char) data [offset+ count]);
				count++;
			}
			
			System.Text.StringBuilder builder = new System.Text.StringBuilder ();
			for (int i = 0; i < count; i++) {
				builder.Append ((char) data [offset + i]);
			}
			offset += (uint) (count + 1);
			return builder.ToString ();
		}
	}

	internal class LogFileStream : IDisposable {

		const int HeaderSize = 10;

		struct Header {

			public BlockCode Code;
			public int Length;
			public uint CounterDelta;

			public Header (byte[] data)
			{
				Code = (BlockCode) (data [0] | (data [1] << 8));
				Length = data [2] | (data [3] << 8) | (data [4] << 16) | (data [5] << 24); 
				CounterDelta = (uint) (data [6] | (data [7] << 8) | (data [8] << 16) | (data [9] << 24));
			}
		}

		uint offset;
		ulong count;
		Block current_block;
		DirectivesBlock directives;
		Stream stream;
		
		public LogFileStream (string filename)
		{
			stream = File.Open (filename, FileMode.Open);
		}
		
		public Block CurrentBlock {
			get { return current_block; }
		}

		public void Dispose () 
		{
			if (stream != null)
				stream.Close ();
			stream = null;
		}

		Block DecodeBlock (BlockCode code, uint length, byte[] data)
		{
			switch (code) {
			case BlockCode.Directives:
				directives = new DirectivesBlock (new Buffer (data, length));
				return directives;
			case BlockCode.End:
				return new EndBlock (new Buffer (data, length));
			case BlockCode.Events:
				return new EventsBlock (new Buffer (data, length), directives);
			case BlockCode.Intro:
				return new IntroBlock (new Buffer (data, length));
			case BlockCode.Loaded:
				return new LoadedBlock (new Buffer (data, length), directives);
			case BlockCode.Mapping:
				return new MappingBlock (new Buffer (data, length), directives);
			case BlockCode.Statistical:
				return new StatBlock (new Buffer (data, length));
			case BlockCode.Unloaded:
				return new UnloadedBlock (new Buffer (data, length), directives);
			default:
				Console.WriteLine ("Unsupported block code: " + code);
				break;
			}
			return null;
		}

		public bool ReadBlock ()
		{
			current_block = null;
			if (stream == null)
				return false;

			byte[] buffer = new byte [HeaderSize];
			stream.Read (buffer, 0, HeaderSize);
			offset += (uint) HeaderSize;
			Header hdr = new Header (buffer);
				
			count += hdr.CounterDelta;
			buffer = new byte [hdr.Length];
			stream.Read (buffer, 0, hdr.Length);
			current_block =  DecodeBlock (hdr.Code, (uint) hdr.Length, buffer);
			offset += (uint) hdr.Length;
			if (hdr.Code == BlockCode.End) {
				stream.Close ();
				stream = null;
			}

			return current_block != null;
		}
	}
}
