namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class ScriptDirectory
    {
        private static readonly string rootDirectory = "scenarios";

        private readonly string _directory;
        private readonly string _log;

        [SuppressMessage(
            "Microsoft.Performance",
            "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "They are initialized inline. The constructor does other things.")]
        static ScriptDirectory()
        {
            FileSystem.EnsureDirectoryCreated(rootDirectory);
        }

        public ScriptDirectory(string scenario)
        {
            _directory = Path.Combine(rootDirectory, scenario);
            FileSystem.EnsureDirectoryDeleted(_directory);
            FileSystem.EnsureDirectoryCreated(_directory);

            _log = Path.Combine(_directory, string.Concat(scenario, ".log"));
        }

        public ScriptDirectory WriteLine(string fileName, string text)
        {
            using (var writer = new StreamWriter(Path.Combine(_directory, fileName), true))
            {
                writer.WriteLine(text);
                writer.Flush();
            }

            return this;
        }

        public string RunScript(string scriptName)
        {
            return RunScript(scriptName, Enumerable.Empty<string>(), Enumerable.Empty<string>());
        }

        public string RunScript(string scriptName, IEnumerable<string> args, IEnumerable<string> scriptArgs)
        {
            return RunScript(scriptName, true, args, scriptArgs);
        }

        public string RunScript(string scriptName, bool debug)
        {
            return RunScript(scriptName, debug, Enumerable.Empty<string>(), Enumerable.Empty<string>());
        }

        public string RunScript(string scriptName, bool debug, IEnumerable<string> args, IEnumerable<string> scriptArgs)
        {
            var debugArgs =
                    debug &&
                    !args.Select(arg => arg.Trim().ToUpperInvariant()).Contains("-DEBUG") &&
                    !args.Select(arg => arg.Trim().ToUpperInvariant()).Contains("-D")
                ? new[] { "-debug" }
                : new string[0];

            return ScriptCsExe.Execute(
                new[] { scriptName }.Concat(debugArgs).Concat(args), scriptArgs, _log, _directory);
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

            return ScriptCsExe.Execute(new[] { "-install", package, "-debug" }, _log, _directory);
        }

        public string Execute(string arg)
        {
            return ScriptCsExe.Execute(arg, _log, _directory);
        }
    }
}
