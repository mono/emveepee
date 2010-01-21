// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Ide.Execution;
using MonoDevelop.Ide.Gui;
using Gtk;
using Emveepee.Widgets;

namespace Emveepee.Addin {

	public class ProfilerExecutionOptions {

		[ItemProperty (DefaultValue=true)]
		bool start_enabled = true;

		[ItemProperty (DefaultValue=ProfileMode.Statistical)]
		ProfileMode mode = ProfileMode.Statistical;

		public ProfileMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		public bool StartEnabled {
			get { return start_enabled; }
			set { start_enabled = value; }
		}

		public ProfilerExecutionOptions Clone ()
		{
			ProfilerExecutionOptions result = new ProfilerExecutionOptions ();
			result.start_enabled = start_enabled;
			result.mode = mode;
			return result;
		}
	}

	public class ProfilerExecutionHandler : ParameterizedExecutionHandler {

		public override bool CanExecute (ExecutionCommand command)
		{
			return command is DotNetExecutionCommand && (command as DotNetExecutionCommand).TargetRuntime is MonoTargetRuntime;
		}

		public override IProcessAsyncOperation Execute (ExecutionCommand command, IConsole console, CommandExecutionContext ctx, object config_data)
		{
			DotNetExecutionCommand dnec = command as DotNetExecutionCommand;

			ProfileConfiguration config = new ProfileConfiguration ();
			config.TargetPath = command.CommandString;
			ProfilerExecutionOptions options = config_data as ProfilerExecutionOptions;
			if (options != null) {
				config.StartEnabled = options.StartEnabled;
				config.Mode = options.Mode;
			}

			string logfile = System.IO.Path.GetTempFileName () + ".mprof";
			ProfilerSocket socket = new ProfilerSocket ();
			ProfilerViewContent view = new ProfilerViewContent ();
			socket.Paused += delegate { view.Load (logfile); };
			dnec.RuntimeArguments += String.Format (" --profile=logging:{0},o={1},cp={2}", config.ToArgs (), logfile, socket.Port);
			IExecutionHandler h = Runtime.ProcessService.GetDefaultExecutionHandler (command);
			IProcessAsyncOperation result = h.Execute (command, console);
			result.Completed += delegate { view.Load (logfile); };
			Gtk.Application.Invoke (delegate { IdeApp.Workbench.OpenDocument (view, true); });
			return result;
		}

		public override IExecutionConfigurationEditor CreateEditor ()
		{
			Console.WriteLine ("Creating editor");
			return new Editor ();
		}

		class Editor : IExecutionConfigurationEditor {

			ProfilerExecutionOptions options;

			public Gtk.Widget Load (CommandExecutionContext ctx, object config_data)
			{
				options = config_data as ProfilerExecutionOptions;
				if (options == null)
					options = new ProfilerExecutionOptions ();
				else
					options = options.Clone ();

				VBox result = new VBox (false, 0);
				HBox box = new HBox (false, 6);
				box = new HBox (false, 6);
				box.PackStart (new Label ("Mode:"), false, false, 0);
				ComboBox type_combo = ComboBox.NewText ();
				type_combo.AppendText ("Allocations");
				type_combo.AppendText ("Calls/Instrumented");
				type_combo.AppendText ("Statistical");
				type_combo.Active = 2;
				type_combo.Changed += delegate { options.Mode = (ProfileMode) (1 << type_combo.Active); };
				box.PackStart (type_combo, false, false, 0);
				box.ShowAll ();
				result.PackStart (box, false, false, 3);
				box = new HBox (false, 6);
				CheckButton start_enabled_chkbtn = new CheckButton ("Enable Logging at Startup");
				start_enabled_chkbtn.Active = true;
				start_enabled_chkbtn.Toggled += delegate { options.StartEnabled = start_enabled_chkbtn.Active; };
				box.PackStart (start_enabled_chkbtn, false, false, 0);
				box.ShowAll ();
				result.PackStart (box, false, false, 3);
				return result;
			}

			public object Save ()
			{
				return options;
			}
		}
	}
}

