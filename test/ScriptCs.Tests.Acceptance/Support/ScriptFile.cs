namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class ScriptFile
    {
        private static readonly string rootDirectory = "scenarios";

        private readonly string path;
        private readonly string name;
        private readonly string log;
        private readonly string directory;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "They are initialized inline. The constructor does other things.")]
        static ScriptFile()
        {
            FileSystem.EnsureDirectoryCreated(rootDirectory);
        }

        private ScriptFile(string scenario, bool createDirectory)
        {
            if (createDirectory)
            {
                this.directory = Path.Combine(rootDirectory, scenario);
                FileSystem.EnsureDirectoryDeleted(directory);
                FileSystem.EnsureDirectoryCreated(directory);
            }
            else
            {
                this.directory = rootDirectory;

            }

            File.Delete(this.path = Path.Combine(directory, this.name = string.Concat(scenario, ".csx")));
            File.Delete(this.log = Path.Combine(directory, string.Concat(scenario, ".log")));
        }

        public static ScriptFile Create(string scenario, bool createDirectory = false)
        {
            return new ScriptFile(scenario, createDirectory);
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

        public string Execute(bool debug)
        {
            return Execute(debug, Enumerable.Empty<string>(), Enumerable.Empty<string>());
        }

        public string Execute(bool debug, IEnumerable<string> args, IEnumerable<string> scriptArgs)
        {
            var debugArgs =
                    debug &&
                    !args.Select(arg => arg.Trim().ToUpperInvariant()).Contains("-DEBUG") &&
                    !args.Select(arg => arg.Trim().ToUpperInvariant()).Contains("-D")
                ? new[] { "-debug" }
                : new string[0];

            return ScriptCsExe.Execute(
                new[] { this.name }.Concat(debugArgs).Concat(args), scriptArgs, this.log, directory);
        }

        public string Install(string package)
        {
            var nugetConfig = Path.Combine(this.directory, "scriptcs_nuget.config");
            File.Delete(nugetConfig);
            using (var writer = new StreamWriter(nugetConfig))
            {
                writer.Write(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""Local"" value=""" + Path.GetFullPath(Path.Combine("Support", "Gallery")) + @""" />
  </packageSources>
  <activePackageSource>
    <add key=""All"" value=""(Aggregate source)"" />
  </activePackageSource>
</configuration>"
                    );

                writer.Flush();
            }

            return ScriptCsExe.Execute(new[] { "-install", package, "-debug" }, new string[0], this.log, this.directory);
        }
    }
}
