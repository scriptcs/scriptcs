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
        public static void HelloWorld(bool debug, ScriptDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .f(() => directory = new ScriptDirectory(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Hello world!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => output = directory.RunScript("foo.csx", debug));

            "Then I see 'Hello world!'"
                .f(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void ScriptThrowsAnException(bool debug, ScriptDirectory directory, Exception ex)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .f(() => directory = new ScriptDirectory(scenario).WriteLine("foo.csx", @"throw new Exception(""BOOM!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => ex = Record.Exception(() => directory.RunScript("foo.csx", debug)));

            "Then the script fails"
                .f(() => ex.ShouldNotBeNull());

            "And I see the exception message"
                .f(() => ex.Message.ShouldContain("BOOM!"));
        }
    }
}
