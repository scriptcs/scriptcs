namespace ScriptCs.Tests.Acceptance
{
    using System;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;
    using Xunit;

    public class CommandLine
    {
        [Scenario]
        public static void UnexpectedOption(Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "When I execute scriptcs with an unknown option"
                .f(() => exception = Record.Exception(() => ScriptCsExe.Run(
                    new[]
                    {
                        "-unknownoption"
                    },
                    ScenarioDirectory.Create(scenario))));

            "Then scriptcs errors"
                .f(() => exception.ShouldBeType<ScriptCsException>());

            "And I see an error message regarding the unknown option"
                .f(() =>
                {
                    exception.Message.ShouldContain("unknownoption");
                });

            "And I see scriptcs usage details"
                .f(() => exception.Message.ShouldContain("Usage:"));
        }
    }
}
