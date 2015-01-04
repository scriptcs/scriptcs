namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class Migration
    {
        [Scenario]
        public static void Migrating(ScriptDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script directory with a full population of artifacts"
                .f(() => directory = new ScriptDirectory(scenario)
                .AddDirectory("bin")
                .AddDirectory(".cache")
                .AddDirectory("packages")
                .WriteLine("packages.config",
@"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
</packages>")
                .WriteLine("nuget.config",
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
</configuration>"));

            "When I migrate"
                .f(() => directory.Migrate());

            "Then the artifacts are migrated"
                .f(() =>
                {
                    directory.DirectoryExists("bin").ShouldBeFalse("the bin directory should not be present");
                    directory.DirectoryExists(".cache").ShouldBeFalse("the .cache directory should not be present");
                    directory.DirectoryExists("packages").ShouldBeFalse("the packages directory should not be present");
                    directory.FileExists("packages.config").ShouldBeFalse("the packages.config file not be present");
                    directory.FileExists("nuget.config").ShouldBeFalse("the nuget.config file not be present");
                    
                    directory.DirectoryExists("scriptcs_bin").ShouldBeTrue("the scriptcs_bin directory should be present");
                    directory.DirectoryExists(".scriptcs_cache").ShouldBeTrue("the .scriptcs_cache directory should be present");
                    directory.DirectoryExists("scriptcs_packages").ShouldBeTrue("the scriptcs_packages directory should be present");
                    directory.FileExists("scriptcs_packages.config").ShouldBeTrue("the scriptcs_packages.config file should be present");
                    directory.FileExists("scriptcs_nuget.config").ShouldBeTrue("the scriptcs_nuget.config file should be present");
                });
        }
    }
}
