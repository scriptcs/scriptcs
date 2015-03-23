namespace ScriptCs.Tests.Acceptance
{
    using System;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Should.Core.Assertions;
    using Xbehave;

    public static class Packages
    {
        [Scenario]
        public static void PackageContainsAFrameworkAssemblyReference(ScenarioDirectory directory, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a simple script"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine();"));

            "When I install a package which contains a framework assembly reference"
                .f(() => ScriptCsExe.Install("FrameworkAssemblyReferencer", directory));

            "And I execute the script"
                .f(() => exception = Record.Exception(() => ScriptCsExe.Run("foo.csx", directory)));

            "Then there should be no errors"
                .f(() => exception.ShouldBeNull());
        }

        [Scenario]
        public static void PackagesWithDuplicateAssemblies(ScenarioDirectory directory, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a simple script"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine();"));

            "When I install a package"
                .f(() => ScriptCsExe.Install("Duplicate.A", directory));

            "And I install another package containing the same assembly"
                .f(() => ScriptCsExe.Install("Duplicate.B", directory));

            "And I execute the script"
                .f(() => exception = Record.Exception(() => ScriptCsExe.Run("foo.csx", directory)));

            "Then there should be no errors"
                .f(() => exception.ShouldBeNull());
        }
    }
}
