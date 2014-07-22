namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class ScriptFile
    {
        private static readonly string directory = "scenarios";

        private readonly string path;
        private readonly string name;
        private readonly string log;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "They are initialized inline. The constructor does other things.")]
        static ScriptFile()
        {
            FileSystem.EnsureDirectoryCreated(directory);
        }

        private ScriptFile(string scenario, bool createWorkingDirectory)
        {
            File.Delete(this.path = Path.Combine(directory, this.name = string.Concat(scenario, ".csx")));
            File.Delete(this.log = Path.Combine(directory, string.Concat(scenario, ".log")));

            if (createWorkingDirectory)
            {
                FileSystem.EnsureDirectoryDeleted(Path.Combine(directory, scenario));
                FileSystem.EnsureDirectoryCreated(Path.Combine(directory, scenario));
            }
        }

        public static ScriptFile Create(string scenario, bool createWorkingDirectory = false)
        {
            return new ScriptFile(scenario, createWorkingDirectory);
        }

        public ScriptFile WriteLine(string code)
        {
            using (var writer = new StreamWriter(this.path, true))
            {
                writer.WriteLine(code);
                writer.Flush();
            }

            return this;
        }

        public string Execute()
        {
            return Execute(Enumerable.Empty<string>(), Enumerable.Empty<string>());
        }

        public string Execute(IEnumerable<string> args, IEnumerable<string> scriptArgs)
        {
            return Execute(true, args, scriptArgs);
        }

        public string ExecuteWithoutDebug()
        {
            return Execute(false, Enumerable.Empty<string>(), Enumerable.Empty<string>());
        }

        private string Execute(bool debug, IEnumerable<string> args, IEnumerable<string> scriptArgs)
        {
            var debugArgs = debug && !args.Select(arg => arg.ToUpperInvariant()).Contains("-DEBUG")
                ? new[] { "-debug" }
                : new string[0];

            return ScriptCsExe.Execute(
                new[] { this.name }.Concat(debugArgs).Concat(args), scriptArgs, this.log, directory);
        }
    }
}
