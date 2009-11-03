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
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Projects;
using Emveepee.Widgets;

namespace Emveepee.Addin {

	public class ProfilerNodeBuilderExtension: NodeBuilderExtension {

		public override bool CanBuildNode (Type type)
		{
			return typeof (DotNetProject).IsAssignableFrom (type);
		}
		
		public override Type CommandHandlerType {
			get { return typeof (ProfilerNodeCommandHandler); }
		}
	}

	public class ProfilerNodeCommandHandler: NodeCommandHandler {

		static ProfilerExecutionHandler handler = new ProfilerExecutionHandler ();

		[CommandUpdateHandler (ProfilerCommands.Profile)]
		public void UpdateProfileProject (CommandInfo cinfo)
		{
			cinfo.Enabled = IdeApp.ProjectOperations.CanExecute (CurrentNode.DataItem as IBuildTarget, handler);
		}

		[CommandHandler (ProfilerCommands.Profile)]
		public void OnProfileProject ()
		{
			DotNetProject project = CurrentNode.DataItem as DotNetProject;
			if (project == null)
				return;
			IdeApp.ProjectOperations.Execute (project, handler);
		}
	}
}
