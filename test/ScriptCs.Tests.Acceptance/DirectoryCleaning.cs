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
                .x(() => directory = ScenarioDirectory.Create(scenario));

            "And the directory has an installed package"
                .x(() => ScriptCsExe.Install("ScriptCs.Adder.Local", directory));

            "And the directory has an assembly cache"
                .x(() => directory.WriteLine(Path.Combine(directory.Map(ScriptCsExe.DllCacheFolder), "foo.txt"), null));

            "When I clean the directory"
                .x(() => ScriptCsExe.Clean(directory));

            "Then the packages folder is removed"
                .x(() => Directory.Exists(directory.Map(ScriptCsExe.PackagesFolder)).ShouldBeFalse());

            "And the assembly cache folder is removed"
                .x(() => Directory.Exists(directory.Map(ScriptCsExe.DllCacheFolder)).ShouldBeFalse());
        }
    }
}
