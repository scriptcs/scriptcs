namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class ScriptPacks
    {
        [Scenario]
        public static void UsingAScriptPack(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses ScriptCs.Adder to print the sum of 1234 and 5678"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(Require<Adder>().Add(1234, 5678));"));

            "And ScriptCs.Adder is installed"
                .x(() => ScriptCsExe.Install("ScriptCs.Adder.Local", directory));

            "When execute the script"
                .x(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 6912"
                .x(() => output.ShouldContain("6912"));
        }
    }
}
