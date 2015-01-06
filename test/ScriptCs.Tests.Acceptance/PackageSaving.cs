namespace ScriptCs.Tests.Acceptance
{
    using System.IO;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class PackageSaving
    {
        [Scenario]
        public static void SavingAPackage(ScriptDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script directory"
                .f(() => directory = new ScriptDirectory(scenario));

            "When I install ScriptCs.Adder.Local manually"
                .f(() => directory.InstallManually("ScriptCs.Adder.Local"));

            "When I save the package"
                .f(() => directory.Save());

            "Then packages file should contain an entry for ScriptCs.Adder.Local"
                .f(() => File.ReadAllText(directory.PackagesFile).ShouldContain(
                        @"<package id=""ScriptCs.Adder.Local"" version=""0.1.1"" targetFramework=""net45"" />"));
        }
    }
}
