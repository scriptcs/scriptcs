using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            
            "Given a script which uses ScriptCs.Calculator to print the sum of 40 and 2"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(new Calculator().Add(40, 2));"));

            "And ScriptCs.Calculator is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Calculator", directory));

            "When executing the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", false, Enumerable.Empty<string>(), args, directory));

            "Then I see 42"
                .f(() => output.ShouldContain("42"));

            "Then I see INFO outputted from the required Logger script pack"
                .f(() => output.ShouldContain("INFO"));
        }

        [Scenario]
        public static void UsingAMethodInAScriptLibraryInTheRepl(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();
            var args = new[] {"-r"};

            "Given a script which uses ScriptCs.Calculator"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Type:"" + new Calculator().GetType().Name);" + Environment.NewLine + "Environment.Exit(0);"));

            "And ScriptCs.Calculator is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Calculator", directory));

            "When executing the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", false, args, Enumerable.Empty<string>(), directory));

            "Then the ScriptCs.Calculator instance is created"
                .f(() => output.ShouldContain("Type:Calculator"));
        }

        [Scenario]
        public static void LoadingFromANonRootedScript(ScenarioDirectory directory, ScenarioDirectory scriptDirectory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();
            var scriptFolder = Path.Combine(scenario, "script");

            "Given a script which uses ScriptCs.Calculator and it is non-rooted"
                .f(() =>
                {
                    directory = ScenarioDirectory.Create(scenario);
                    scriptDirectory = ScenarioDirectory.Create(scriptFolder)
                        .WriteLine("foo.csx", @"Console.WriteLine(""Type:"" + new Calculator().GetType().Name);");
                });

            "And ScriptCs.Calculator is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Calculator", scriptDirectory));

            "When executing the script"
                .f(() => output = ScriptCsExe.Run(Path.Combine("script", "foo.csx"), directory));

            "Then the ScriptCs.Calculator instance is created"
                .f(() => output.ShouldContain("Type:Calculator"));
        }


        [Scenario]
        public static void UsingALoadedMethodInAScriptLibrary(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses ScriptCs.Calculator to print the product of 7 and 6"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(new Calculator().Multiply(7, 6));"));

            "And ScriptCs.Calculator is installed"
                .f(() => ScriptCsExe.Install("ScriptCs.Calculator", directory));

            "When executing the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 42"
                .f(() => output.ShouldContain("42"));
        }
    }
}
