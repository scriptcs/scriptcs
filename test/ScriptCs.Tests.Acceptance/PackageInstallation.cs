namespace ScriptCs.Tests.Acceptance
{
    using System.IO;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class PackageInstallation
    {
        [Scenario]
        public static void InstallingAPackage(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "When I install ScriptCs.Adder"
                .x(() => ScriptCsExe.Install("ScriptCs.Adder.Local", directory = ScenarioDirectory.Create(scenario)));

            "Then the ScriptCs.Adder NuGet package is added to the packages folder"
                .x(() => File.Exists(
                        Path.Combine(
                            directory.Map(ScriptCsExe.PackagesFolder),
                            "ScriptCs.Adder.Local.0.1.1/ScriptCs.Adder.Local.0.1.1.nupkg"))
                    .ShouldBeTrue());

            "And the ScriptCs.Adder assembly is extracted"
                .x(() => File.Exists(
                        Path.Combine(
                            directory.Map(ScriptCsExe.PackagesFolder),
                            "ScriptCs.Adder.Local.0.1.1/lib/net45/ScriptCs.Adder.dll"))
                    .ShouldBeTrue());

            "And ScriptCs.Adder is added to the packages file"
                .x(() => File.ReadAllText(directory.Map(ScriptCsExe.PackagesFile)).ShouldContain(
                    @"<package id=""ScriptCs.Adder.Local"" version=""0.1.1"" targetFramework=""net45"" />"));
        }
    }
}
