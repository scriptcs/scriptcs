namespace ScriptCs.Tests.Acceptance
{
    using System.IO;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class Migration
    {
        [Scenario]
        public static void Migrating(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script directory with a full population of legacy artifacts including a hello world script"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                .WriteLine("bin/foo.txt", null)
                .WriteLine(".cache/foo.txt", null)
                .WriteLine("packages/foo.txt", null)
                .WriteLine("packages.config", @"<?xml version=""1.0"" encoding=""utf-8""?><packages></packages>")
                .WriteLine("nuget.config", @"<?xml version=""1.0"" encoding=""utf-8""?><configuration></configuration>")
                .WriteLine("hello.csx", @"Console.WriteLine(""Hello, World!"");"));

            "When I execute the script"
                .f(() => ScriptCsExe.Run("hello.csx", directory));

            "Then the artifacts are migrated"
                .f(() =>
                {
                    File.Exists(directory.Map("bin/foo.txt")).ShouldBeTrue("bin/ is unchanged");
                    File.Exists(directory.Map(".cache/foo.txt")).ShouldBeFalse(".cache/ is renamed to .scriptcs_cache/");
                    File.Exists(directory.Map("packages/foo.txt")).ShouldBeTrue("packages/ is unchanged");
                    File.Exists(directory.Map("packages.config")).ShouldBeTrue("packages.config is unchanged");
                    File.Exists(directory.Map("nuget.config")).ShouldBeTrue("nuget.config is unchanged");

                    File.Exists(directory.Map("scriptcs_bin/foo.txt")).ShouldBeTrue("bin/ is copied to scriptcs_bin/");
                    File.Exists(directory.Map(".scriptcs_cache/foo.txt")).ShouldBeTrue(".scriptcs_cache/ is renamed from .cache/");
                    File.Exists(directory.Map("scriptcs_packages/foo.txt")).ShouldBeTrue("packages/ is copied to scriptcs_packages/");
                    File.Exists(directory.Map("scriptcs_packages.config")).ShouldBeTrue("packages.config is copied to scriptcs_packages.config");
                    File.Exists(directory.Map("scriptcs_nuget.config")).ShouldBeTrue("nuget.config is copied to scriptcs_nuget.config");
                });
        }
    }
}
