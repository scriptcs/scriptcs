using System.IO;

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
        public static void HelloWorld(bool debug, ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a hello world script"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Hello world!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => output = ScriptCsExe.Run("foo.csx", debug, directory));

            "Then I see 'Hello world!'"
                .f(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void ScriptThrowsAnException(bool debug, ScenarioDirectory directory, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"throw new Exception(""BOOM!"");"));

            "When I execute the script with debug set to {0}"
                .f(() => exception = Record.Exception(() => ScriptCsExe.Run("foo.csx", debug, directory)));

            "Then scriptcs fails"
                .f(() => exception.ShouldBeType<ScriptCsException>());

            "And I see the exception message"
                .f(() => exception.Message.ShouldContain("BOOM!"));
        }

        [Scenario]
        public static void ScriptCanWorkWithUsingStatic(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which defined a static import"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "using static System.Console;" + Environment.NewLine + @"WriteLine(""Hello world!"");"));

            "When I execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 'Hello world!'"
                .f(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        public static void ScriptCanAccessEnv(ScenarioDirectory directory, string output )
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which accesses Env"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env)"));

            "When I execute the script"
                .f(()=> output = ScriptCsExe.Run("foo.csx", directory));

            "Then the Env object is displayed"
                .f(() => output.ShouldContain("ScriptCs.ScriptEnvironment"));
        }
        
        [Scenario]
        public static void ScriptCanUseDynamic(ScenarioDirectory directory, string output )
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses dynamic"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"dynamic obj = new ExpandoObject(); obj.foo = ""bar""; Console.WriteLine(obj.foo); ;"));

            "When I execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the dynamic value is properly returned "
                .f(() => output.ShouldContain("bar"));

        }

        [Scenario]
        public static void ScriptAssemblyIsSet(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which accesses Env.ScriptAssembly"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env.ScriptAssembly)"));

            "When I execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the Assembly is displayed"
                .f(() => output.ShouldContain("Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));

        }

        [Scenario]
        public static void ScriptPathIsSet(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which accesses Env.ScriptPath"
                .f(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env.ScriptPath)"));

            "When I execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the ScriptPath is displayed"
                .f(() => output.ShouldContain("foo.csx"));
        }

        [Scenario]
        public static void LoadedScriptsIsSet(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which loads another script and accesses Env.LoadedScripts"
                .f(() =>
                {
                    directory = ScenarioDirectory.Create(scenario)
                        .WriteLine(
                            "foo.csx", "#load bar.csx;" + Environment.NewLine +
                                       "Console.WriteLine(Env.LoadedScripts.First());"
                        );
                    directory.WriteLine("bar.csx", "");
                });
                    

            "When I execute the script"
                .f(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the loaded script path is displayed"
                .f(() => output.ShouldContain("bar.csx"));
        }


    }
}
