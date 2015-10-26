namespace ScriptCs.Tests.Acceptance
{
    using System;
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;
    using Xunit;

    public static class ScriptExecution
    {
        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void HelloWorld(bool debug, ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Hello world!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => output = ScriptCsExe.Run("foo.csx", debug, directory));

            "Then I see 'Hello world!'"
                .f(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void ScriptThrowsAnException(bool debug, ScenarioDirectory directory, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"throw new Exception(""BOOM!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => exception = Record.Exception(() => ScriptCsExe.Run("foo.csx", debug, directory)));

            "Then scriptcs fails"
                .f(() => exception.ShouldBeType<ScriptCsException>());

            "And I see the exception message"
                .f(() => exception.Message.ShouldContain("BOOM!"));
        }

        [Scenario]
        public static void ScriptCanAccessEnv(ScenarioDirectory directory, string output )
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which access Env"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env)"));

            "When I execute the script"
                .f(()=> output = ScriptCsExe.Run("foo.csx", directory));

            "Then the Env object is displayed"
                .f(() => output.ShouldContain("ScriptCs.ScriptEnvironment"));
        }
        
        [Scenario]
        public static void ScriptCanUseDynamic(ScenarioDirectory directory, string output )
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses dynamic"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"dynamic obj = new ExpandoObject(); obj.foo = ""bar""; Console.WriteLine(obj.foo); ;"));

            "When I execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the dynamic value is properly returned "
                .f(() => output.ShouldContain("bar"));

        }
    }
}
