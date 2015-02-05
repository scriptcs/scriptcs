namespace ScriptCs.Tests.Acceptance
{
    using System.IO;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class ImplicitInstallation
    {
        [Scenario]
        public static void LegacyFileSystem(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses ScriptCs.Adder to print the sum of 1234 and 5678"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(Require<Adder>().Add(1234, 5678));"));

            "And a legacy packages file declaring the ScriptCs.Adder dependency"
                .f(() =>
                {
                    var nugetConfig =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""Local"" value=""" + Path.GetFullPath(Path.Combine("Support", "Gallery")) + @""" />
  </packageSources>
  <activePackageSource>
    <add key=""All"" value=""(Aggregate source)"" />
  </activePackageSource>
</configuration>
";
                    var packagesConfig =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""ScriptCs.Adder.Local"" version=""0.1.1"" targetFramework=""net45"" />
</packages>
";

                    directory.WriteLine("nuget.config", nugetConfig);
                    directory.WriteLine("packages.config", packagesConfig);
                });

            "When execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 6912"
                .f(() => output.ShouldContain("6912"));
        }
    }
}
