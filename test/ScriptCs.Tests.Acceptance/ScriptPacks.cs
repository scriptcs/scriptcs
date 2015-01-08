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
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(Require<Adder>().Add(1234, 5678));"));

            "And ScriptCs.Adder is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Adder.Local", directory));

            "When execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 6912"
                .f(() => output.ShouldContain("6912"));
        }
    }
}
