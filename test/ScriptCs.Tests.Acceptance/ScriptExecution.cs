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
        public static void HelloWorld(bool debug, ScriptFile script, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .f(() => script = new ScriptFile(scenario).WriteLine(@"Console.WriteLine(""Hello world!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => output = script.Execute(debug));

            "Then I see 'Hello world!'"
                .f(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void ScriptThrowsAnException(bool debug, ScriptFile script, Exception ex)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .f(() => script = new ScriptFile(scenario).WriteLine(@"throw new Exception(""BOOM!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => ex = Record.Exception(() => script.Execute(debug)));

            "Then the script fails"
                .f(() => ex.ShouldNotBeNull());

            "And I see the exception message"
                .f(() => ex.Message.ShouldContain("BOOM!"));
        }
    }
}
