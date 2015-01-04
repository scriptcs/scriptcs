namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class ScriptFile
    {
        private static readonly string rootDirectory = "scenarios";

        private readonly string _path;
        private readonly string _name;
        private readonly string _log;
        private readonly string _directory;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "They are initialized inline. The constructor does other things.")]
        static ScriptFile()
        {
            FileSystem.EnsureDirectoryCreated(rootDirectory);
        }

        public ScriptFile(string scenario)
        {
            _directory = Path.Combine(rootDirectory, scenario);
            FileSystem.EnsureDirectoryDeleted(_directory);
            FileSystem.EnsureDirectoryCreated(_directory);

            File.Delete(_path = Path.Combine(_directory, _name = string.Concat(scenario, ".csx")));
            File.Delete(_log = Path.Combine(_directory, string.Concat(scenario, ".log")));
        }

        public ScriptFile WriteLine(string code)
        {
            using (var writer = new StreamWriter(_path, true))
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
                new[] { _name }.Concat(debugArgs).Concat(args), scriptArgs, _log, _directory);
        }

        public string Install(string package)
        {
            var nugetConfig = Path.Combine(_directory, "scriptcs_nuget.config");
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

            return ScriptCsExe.Execute(new[] { "-install", package, "-debug" }, new string[0], _log, _directory);
        }
    }
}
