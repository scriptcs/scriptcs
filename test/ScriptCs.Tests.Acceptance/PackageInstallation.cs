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
        public static void InstallingAPackage(ScriptDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script directory"
                .f(() => directory = new ScriptDirectory(scenario));

            "When I install ScriptCs.Adder.Local"
                .f(() => directory.Install("ScriptCs.Adder.Local"));

            "Then the ScriptCs.Adder.Local NuGet package should be in the packages folder"
                .f(() => File.Exists(
                        Path.Combine(
                            directory.PackagesFolder,
                            "ScriptCs.Adder.Local.0.1.1/ScriptCs.Adder.Local.0.1.1.nupkg"))
                    .ShouldBeTrue());

            "And the ScriptCs.Adder.Local assembly should be in the packages folder"
                .f(() => File.Exists(
                        Path.Combine(
                            directory.PackagesFolder,
                            "ScriptCs.Adder.Local.0.1.1/lib/net45/ScriptCs.Adder.dll"))
                    .ShouldBeTrue());

            "And the packages file should contain an entry for ScriptCs.Adder.Local"
                .f(() => File.ReadAllText(directory.PackagesFile).ShouldContain(
                        @"<package id=""ScriptCs.Adder.Local"" version=""0.1.1"" targetFramework=""net45"" />"));
        }
    }
}
