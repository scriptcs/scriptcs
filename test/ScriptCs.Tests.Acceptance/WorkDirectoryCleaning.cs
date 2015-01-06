namespace ScriptCs.Tests.Acceptance
{
    using System.IO;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class WorkDirectoryCleaning
    {
        [Scenario]
        public static void CleaningTheWorkingDirectory(ScriptDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script directory with an installed packaged"
                .f(() => directory = new ScriptDirectory(scenario));

            "And the script directory has an installed package"
                .f(() => directory.Install("ScriptCs.Adder.Local"));

            "And the script directory has an assembly cache"
                .f(() => directory.WriteLine(Path.Combine(directory.DllCacheFolder, "foo.txt"), null));

            "When I clean the working directory"
                .f(() => directory.Clean());

            "Then the packages folder should be removed"
                .f(() => Directory.Exists(directory.PackagesFolder).ShouldBeFalse());

            "And the assembly cache folder should be removed"
                .f(() => Directory.Exists(directory.DllCacheFolder).ShouldBeFalse());
        }
    }
}
