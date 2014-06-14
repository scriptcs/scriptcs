namespace ScriptCs.Tests.Acceptance.Support
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

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

        public string Execute(params string[] scriptArgs)
        {
            return ScriptCsExe.Execute(new[] { this.name, "-debug" }, scriptArgs, this.log, this.directory);
        }

        public string Install(string package)
        {
            var packagesDirectory = Path.Combine(this.directory, "packages");
            FileSystem.EnsureDirectoryCreated(packagesDirectory);

            var nugetConfig = Path.Combine(packagesDirectory, "nuget.config");
            File.Delete(nugetConfig);
            using (var writer = new StreamWriter(nugetConfig))
            {
                writer.Write(
@"
<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <activePackageSource>
    <add key=""Local"" value=""" + Path.GetFullPath(Path.Combine("Support", "Gallery")) + @""" />
  </activePackageSource>
</configuration>"
                    );

                writer.Flush();
            }

            return ScriptCsExe.Execute(new[] { "-install", package }, new string[0], this.log, this.directory);
        }
    }
}
