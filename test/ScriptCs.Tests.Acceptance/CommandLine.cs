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
        public static void UnexpectedOption(ScriptDirectory directory, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script directory"
                .f(() => directory = new ScriptDirectory(scenario));

            "When I execute scriptcs.exe with an argument of '-unknownoption'"
                .f(() => exception = Record.Exception(() => directory.ExecuteScriptCsExe("-unknownoption")));

            "Then the process errors"
                .f(() => exception.ShouldNotBeNull());

            "And an error message is shown regarding 'unknownoption'"
                .f(() => exception.Message.ShouldContain("unknownoption"));

            "And usage is shown"
                .f(() => exception.Message.ShouldContain("Usage:"));
        }
    }
}
