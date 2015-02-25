using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Should;
using ScriptCs.Tests.Acceptance.Support;
using Xbehave;

namespace ScriptCs.Tests.Acceptance
{
    public class ScriptLibraries
    {
        [Scenario]
        public static void UsingAMethodInAScriptLibrary(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();
            var args = new string[] {"-loglevel","info"};
            
            "Given a script which uses ScriptCs.Calculator to print the sum of 5 and 2"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(new Calculator().Add(5, 2));"));

            "And ScriptCs.Calculator is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Calculator", directory));

            "When execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", false, Enumerable.Empty<string>(), args, directory));

            "Then I see 7"
                .f(() => output.ShouldContain("7"));

            "Then I see INFO outputted from the required Logger script pack"
                .f(() => output.ShouldContain("INFO"));
        }

        [Scenario]
        public static void UsingALoadedMethodInAScriptLibrary(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses ScriptCs.Calculator to print the product of 5 and 2"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(new Calculator().Multiply(5, 2));"));

            "And ScriptCs.Calculator is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Calculator", directory));

            "When execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 10"
                .f(() => output.ShouldContain("10"));
        }
    }
}
