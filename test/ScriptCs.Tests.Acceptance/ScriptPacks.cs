namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;

    public static class ScriptPacks
    {
        [Scenario]
        public static void UsingAScriptPack(ScriptDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses ScriptCs.Adder to print the sum of 1234 and 5678"
                .f(() => directory = new ScriptDirectory(scenario).WriteLine(
                    "foo.csx", @"Console.WriteLine(Require<Adder>().Add(1234, 5678));"));

            "When I install ScriptCs.Adder"
                .f(() => directory.Install("ScriptCs.Adder.Local"));

            "And execute the script"
                .f(() => output = directory.Execute("foo.csx"));

            "Then I see 6912"
                .f(() => output.ShouldContain("6912"));
        }
    }
}
