// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Emveepee.Decoding;

namespace Emveepee.Widgets {

	public class ProfilerProcess {	

		Process proc;
		ProfilerSocket socket;
		string log_file;

		public ProfilerProcess (ProfileConfiguration config) : this (config, new Process ()) {}

		public ProfilerProcess (ProfileConfiguration config, Process proc)
		{
			log_file = System.IO.Path.GetTempFileName () + ".mprof";
			this.proc = proc;
			socket = new ProfilerSocket ();
			socket.Paused += delegate { OnPaused (); };
			proc.StartInfo.FileName = "mono";
			proc.StartInfo.Arguments = "--profile=logging:" + config.ToArgs () + ",o=" + log_file + ",cp=" + socket.Port.ToString () + " " + config.AssemblyPath;
			proc.EnableRaisingEvents = true;
			proc.Exited += delegate { OnExited (); };
		}

		public event EventHandler Exited;

		void OnExited ()
		{
			if (Exited == null)
				return;
			Exited (this, EventArgs.Empty);
		}

		public event EventHandler Paused;

		void OnPaused ()
		{
			if (Paused == null)
				return;
			Paused (this, EventArgs.Empty);
		}

		public string LogFile {
			get { return log_file; }
		}

		public void Pause ()
		{
			socket.Pause ();
		}

		public void Resume ()
		{
			socket.Resume ();
		}

		public void Start ()
		{
			proc.Start ();
		}
	}
}
