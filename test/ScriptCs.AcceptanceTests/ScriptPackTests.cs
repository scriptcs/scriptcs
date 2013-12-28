using System.IO;
using Xbehave;
using Xunit;
using Should;
using System;
using System.Diagnostics;

namespace ScriptCs.AcceptanceTests
{
    public class ScriptPackTests
    {
        [Scenario]
        [Trait("AcceptanceTest", "ScriptPackTests")]
        [Trait("Requires", "Internet Connection")]
        public void RequiringScriptPackAllowsAccessToContextObjectAndItsMethods(string originalWorkingDirectory, string localDirectory, int result)
        {
            var methodName = AcceptanceTestHelpers.GetCurrentMethodName();

            "Given a current working directory"._(() =>
            {
                originalWorkingDirectory = Environment.CurrentDirectory;
                localDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), methodName);
                Directory.CreateDirectory(localDirectory);

                Environment.CurrentDirectory = localDirectory;
            }).Teardown(() =>
            {
                Environment.CurrentDirectory = originalWorkingDirectory;
                Directory.Delete(localDirectory, true);
            });

            "And the ScriptCs.Adder script pack installed in it"._(() =>
            {
                var process =  AcceptanceTestHelpers.LaunchScriptCs("-install ScriptCs.Adder");
                process.WaitForExit();
            });

            "And a script file that uses it and throws an error if it is not working"._(() =>
            {
                var filesDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), "Files");
                var adderDirectory = Path.Combine(filesDirectory, "adder");

                File.Copy(Path.Combine(adderDirectory, "start.csx"), Path.Combine(localDirectory, "start.csx"));
            });

            "When the script is executed"._(() =>
            {
                var process =  AcceptanceTestHelpers.LaunchScriptCs("start.csx");
                process.WaitForExit();
                result = process.ExitCode;
            });

            "Then the program terminates correctly"._(() =>
            {
                result.ShouldEqual(0);
            });
        }
    }
}
