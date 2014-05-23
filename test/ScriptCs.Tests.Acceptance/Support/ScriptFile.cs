namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

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

        public string Execute(params string[] scriptArgs)
        {
            return ScriptCsExe.Execute(new[] { this.name, "-debug" }, scriptArgs, this.log, directory);
        }
    }
}
