namespace ScriptCs.Tests.Acceptance
{
    using System.IO;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class DirectoryCleaning
    {
        [Scenario]
        public static void CleaningADirectory(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a directory"
                .f(() => directory = ScenarioDirectory.Create(scenario));

            "And the directory has an installed package"
                .f(() => ScriptCsExe.Install("ScriptCs.Adder.Local", directory));

            "And the directory has an assembly cache"
                .f(() => directory.WriteLine(Path.Combine(directory.Map(ScriptCsExe.DllCacheFolder), "foo.txt"), null));

            "When I clean the directory"
                .f(() => ScriptCsExe.Clean(directory));

            "Then the packages folder is removed"
                .f(() => Directory.Exists(directory.Map(ScriptCsExe.PackagesFolder)).ShouldBeFalse());

            "And the assembly cache folder is removed"
                .f(() => Directory.Exists(directory.Map(ScriptCsExe.DllCacheFolder)).ShouldBeFalse());
        }
    }
}
