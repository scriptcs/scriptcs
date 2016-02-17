using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.ReplCommands
{
    public class OpenVsCommand : IReplCommand
    {
        private readonly IConsole _console;
        private IVisualStudioSolutionWriter _writer;

        public OpenVsCommand(IConsole console, IVisualStudioSolutionWriter writer)
        {
            _console = console;
            _writer = writer;
        }

        public object Execute(IRepl repl, object[] args)
        {
            if (PlatformID != PlatformID.Win32NT)
            {
                _console.WriteLine("Requires Windows 8 or later to run");
                return null;
            }
            var fs = repl.FileSystem;
            var launcher = _writer.WriteSolution(fs, (string) args[0], new VisualStudioSolution());
            _console.WriteLine("Opening Visual Studio");
            LaunchSolution(launcher);
            return null;
        }

        protected internal virtual void LaunchSolution(string launcher)
        {
            System.Diagnostics.Process.Start(launcher);
        }

        protected internal virtual PlatformID PlatformID
        {
            get { return Environment.OSVersion.Platform; }
        }

        public string Description
        {
            get { return "Opens a script to edit/debug in Visual Studio"; }
        }

        public string CommandName
        {
            get { return "openvs"; }
        }
    }
}
