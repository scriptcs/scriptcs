namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class Configuration
    {
        [Scenario]
        public static void LocalConfiguration(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Hello world!"");"));

            "And a local config file specfying the log level as debug"
                .x(() => directory.WriteLine("scriptcs.opts", @"{ logLevel: ""debug"" }"));

            "When I execute the script without the log level option"
                .x(() => output = ScriptCsExe.Run("foo.csx", false, directory));

            "Then I see debug messages"
                .x(() => output.ShouldContain("DEBUG:"));
        }

        [Scenario]
        public static void CustomConfiguration(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Hello world!"");"));

            "And a local config file specfying to run as debug"
                .x(() => directory.WriteLine("custom.opts", @"{ logLevel: ""debug"" }"));

            "When I execute the script without the log level option but specifying the custom config"
                .x(() =>
                {
                    var args = new[] { "--config", "custom.opts", };
                    output = ScriptCsExe.Run("foo.csx", false, args, directory);
                });

            "Then I see debug messages"
                .x(() => output.ShouldContain("DEBUG:"));
        }
    }
}
