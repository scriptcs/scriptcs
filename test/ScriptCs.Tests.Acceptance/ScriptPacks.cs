namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class ScriptPacks
    {
        [Scenario]
        public static void UsingAScriptPack(ScriptFile script, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses ScriptCs.Adder to print the sum of 1234 and 5678"
                .f(() => script = ScriptFile.Create(scenario, true).WriteLine(
                    @"Console.WriteLine(Require<Adder>().Add(1234, 5678));"));

            "When I install ScriptCs.Adder"
                .f(() => script.Install("ScriptCs.Adder.Local"));

            "And execute the script"
                .f(() => output = script.Execute());

            "Then I see 6912"
                .f(() => output.ShouldContain("6912"));
        }
    }
}
