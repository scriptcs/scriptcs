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
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"Console.WriteLine(""Hello world!"");"));

            "When I execute the script with debug set to {0}"
                .x(() => output = ScriptCsExe.Run("foo.csx", debug, directory));

            "Then I see 'Hello world!'"
                .x(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        [Example(true)]
        [Example(false)]
        public static void ScriptThrowsAnException(bool debug, ScenarioDirectory directory, Exception exception)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which throws an exception"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"throw new Exception(""BOOM!"");"));

            "When I execute the script with debug set to {0}"
                .x(() => exception = Record.Exception(() => ScriptCsExe.Run("foo.csx", debug, directory)));

            "Then scriptcs fails"
                .x(() => exception.ShouldBeType<ScriptCsException>());

            "And I see the exception message"
                .x(() => exception.Message.ShouldContain("BOOM!"));
        }

        [Scenario]
        public static void ScriptCanWorkWithUsingStatic(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which defined a static import"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "using static System.Console;" + Environment.NewLine + @"WriteLine(""Hello world!"");"));

            "When I execute the script"
                .x(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then I see 'Hello world!'"
                .x(() => output.ShouldContain("Hello world!"));
        }

        [Scenario]
        public static void ScriptCanAccessEnv(ScenarioDirectory directory, string output )
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which accesses Env"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env)"));

            "When I execute the script"
                .x(()=> output = ScriptCsExe.Run("foo.csx", directory));

            "Then the Env object is displayed"
                .x(() => output.ShouldContain("ScriptCs.ScriptEnvironment"));
        }
        
        [Scenario]
        public static void ScriptCanUseDynamic(ScenarioDirectory directory, string output )
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which uses dynamic"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", @"dynamic obj = new ExpandoObject(); obj.foo = ""bar""; Console.WriteLine(obj.foo); ;"));

            "When I execute the script"
                .x(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the dynamic value is properly returned "
                .x(() => output.ShouldContain("bar"));

        }

        [Scenario]
        public static void ScriptAssemblyIsSet(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which accesses Env.ScriptAssembly"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env.ScriptAssembly)"));

            "When I execute the script"
                .x(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the Assembly is displayed"
                .x(() => output.ShouldContain("Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));

        }

        [Scenario]
        public static void ScriptPathIsSet(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which accesses Env.ScriptPath"
                .x(() => directory = ScenarioDirectory.Create(scenario)
                    .WriteLine("foo.csx", "Console.WriteLine(Env.ScriptPath)"));

            "When I execute the script"
                .x(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the ScriptPath is displayed"
                .x(() => output.ShouldContain("foo.csx"));
        }

        [Scenario]
        public static void LoadedScriptsIsSet(ScenarioDirectory directory, string output)
        {
            var scenario = MethodBase.GetCurrentMethod().GetFullName();

            "Given a script which loads another script and accesses Env.LoadedScripts"
                .x(() =>
                {
                    directory = ScenarioDirectory.Create(scenario)
                        .WriteLine(
                            "foo.csx", "#load bar.csx;" + Environment.NewLine +
                                       "Console.WriteLine(Env.LoadedScripts.First());"
                        );
                    directory.WriteLine("bar.csx", "");
                });
                    

            "When I execute the script"
                .x(() => output = ScriptCsExe.Run("foo.csx", directory));

            "Then the loaded script path is displayed"
                .x(() => output.ShouldContain("bar.csx"));
        }


    }
}
