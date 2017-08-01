namespace ScriptCs.Tests.Acceptance
{
    using System;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;
    using Xunit;

    public static class LooseScriptExecution
    {
        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void HelloWorld(bool debug, ScenarioDirectory directory, string output, string[] args, string script)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .x(() =>
                {
                    directory = ScenarioDirectory.Create(scenario);
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        script = @"Console.WriteLine(""""""Hello World!"""""");";
                    }
                    else
                    {
                        script = @"'Console.WriteLine(""Hello World!"");'";
                    }
                    args = new[] {"-e", script};
                });

            "When I execute the script with debug set to {0}"
                .x(() => output = ScriptCsExe.Run(args, debug, directory));

            "Then I see 'Hello World!'"
                .x(() => output.ShouldContain("Hello World!"));
        }

        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void ScriptThrowsAnException(bool debug, ScenarioDirectory directory, Exception exception, string[] args, string script)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .x(() =>
                {
                    directory = ScenarioDirectory.Create(scenario);
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        script = @"""throw new Exception(""""""BOOM!"""""");""";
                    }
                    else
                    {
                        script = @"'throw new Exception(""BOOM!"");'";
                    }
                            
                    args = new[] {"-e", script};
                });

            "When I execute the script with debug set to {0}"
                .x(() => exception = Record.Exception(() => ScriptCsExe.Run(args, debug, directory)));

            "Then scriptcs fails"
                .x(() => exception.ShouldBeType<ScriptCsException>());

            "And I see the exception message"
                .x(() =>
                {
                    exception.Message.ShouldContain("BOOM!");
                });
        }

        [Scenario]
        public static void ScriptCanAccessEnv(ScenarioDirectory directory, string output, string[] args, string script)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which access Env"
                .x(() =>
                {
                    directory = ScenarioDirectory.Create(scenario);
                    script = "Console.WriteLine(Env)";
                    args = new[] {"-e", script};
                });
            "When I execute the script"
                .x(() => output = ScriptCsExe.Run(args, directory));

            "Then the Env object is displayed"
                .x(() => output.ShouldContain("ScriptCs.ScriptEnvironment"));
        }
    }
}
