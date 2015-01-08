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
        public static void SavingAPackage(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "When I install ScriptCs.Adder manually"
                .f(() =>
                {
                    ScriptCsExe.Install("ScriptCs.Adder.Local", directory = ScenarioDirectory.Create(scenario));
                    directory.DeleteFile(ScriptCsExe.PackagesFile);
                });

            "And I save packages"
                .f(() => ScriptCsExe.Save(directory));

            "Then ScriptCs.Adder is added to the packages file"
                .f(() => File.ReadAllText(directory.Map(ScriptCsExe.PackagesFile)).ShouldContain(
                    @"<package id=""ScriptCs.Adder.Local"" version=""0.1.1"" targetFramework=""net45"" />"));
        }
    }
}
