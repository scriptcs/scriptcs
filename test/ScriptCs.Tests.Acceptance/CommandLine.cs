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
                .x(() => exception = Record.Exception(() => ScriptCsExe.Run(
                    new[]
                    {
                        "-unknownoption"
                    },
                    ScenarioDirectory.Create(scenario))));

            "Then scriptcs errors"
                .x(() => exception.ShouldBeType<ScriptCsException>());

            "And I see an error message regarding the unknown option"
                .x(() =>
                {
                    Console.WriteLine(exception);
                    exception.Message.ShouldContain("Usage: scriptcs options");
                });
        }
    }
}
