using ScriptCs.Contracts;
using System;

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
            string arg = args.Length > 0 ? (string)args[0] : null;
            var launcher = _writer.WriteSolution(fs, arg, new VisualStudioSolution());
            _console.WriteLine("Opening Visual Studio");
            LaunchSolution(launcher);
            return null;
        }

        protected internal virtual void LaunchSolution(string launcher)
        {
            System.Diagnostics.Process.Start(launcher);
        }

        protected internal virtual PlatformID PlatformID => Environment.OSVersion.Platform;

        public string Description => "Opens a script to edit/debug in Visual Studio";

        public string CommandName => "openvs";
    }
}
