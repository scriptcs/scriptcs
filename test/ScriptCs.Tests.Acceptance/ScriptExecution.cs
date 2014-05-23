namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

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
    }
}
