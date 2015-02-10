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
    }
}
