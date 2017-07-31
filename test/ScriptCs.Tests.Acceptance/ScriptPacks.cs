namespace ScriptCs.Tests.Acceptance
{
    using System.Reflection;
    using ScriptCs.Tests.Acceptance.Support;
    using Should;
    using Xbehave;
    using System;
    using Should.Core.Assertions;
    using ScriptCs.Exceptions;

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

        [Scenario]
        public static void UsingAnUnavailableScriptPack(ScenarioDirectory directory, string output, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses a non-existing script pack 'Foo'"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"var fooContext = Require<Foo>();
                        class Foo : ScriptCs.Contracts.IScriptPackContext{}
                        Console.WriteLine(""hi"");"));

            "When I execute the script"
                .f(() => exception = Record.Exception(() => ScriptCsExe.Run("foo.csx", directory)));

            "Then scriptcs fails"
                .f(() => exception.ShouldBeType<ScriptCsException>());

            "With a script pack exception"
                .f(() =>
                {
                    exception.Message.ShouldContain("Tried to resolve a script pack 'Submission#0+Foo', but such script pack is not available in the current execution context.");
                });
        }
    }
}
