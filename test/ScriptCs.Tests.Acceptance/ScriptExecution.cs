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
        public static void HelloWorld(ScriptFile script, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .f(() => script = ScriptFile.Create(scenario).WriteLine(@"Console.WriteLine(""Hello world!"");"));

            "When I execute the script"
                .f(() => output = script.Execute());

            "Then I see 'Hello world!'"
                .f(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        public static void ScriptThrowsAnException(ScriptFile script, Exception ex)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .f(() => script = ScriptFile.Create(scenario).WriteLine(@"throw new Exception(""BOOM!"");"));

            "When I execute the script"
                .f(() => ex = Record.Exception(() => script.Execute()));

            "Then the script fails"
                .f(() => ex.ShouldNotBeNull());

            "And I see the exception message"
                .f(() => ex.Message.ShouldContain("BOOM!"));
        }
    }
}
