using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using Xbehave;
using Xunit;
using System.IO;
using System.Net.Http;
using System.Diagnostics;

namespace ScriptCs.AcceptanceTests
{
    public class ScriptExecutionTests
    {
        [Scenario]
        [Trait("AcceptanceTest", "RunningScriptsTests")]
        public void RunningWebApiHostSampleEnablesWebAccessInLocalhost(
            string originalWorkingDirectory, 
            string localDirectory,
            int scriptResult)
        {
            string methodName = AcceptanceTestHelpers.GetCurrentMethodName();

            "Given an existing set of scripts"
            ._(() =>
            {
                originalWorkingDirectory = Environment.CurrentDirectory;
                localDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), methodName);
                
                var scriptsDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(),  @"Files\scripts");
                Directory.CreateDirectory(localDirectory);
                AcceptanceTestHelpers.CopyDirectory(scriptsDirectory, localDirectory, true);

                Environment.CurrentDirectory = localDirectory;

                var process =  AcceptanceTestHelpers.LaunchScriptCs("-install");
                process.WaitForExit();
            }).Teardown(() =>
            {
                Environment.CurrentDirectory = originalWorkingDirectory;
                Directory.Delete(localDirectory, true);
            });

            "When the sample is executed"._(() =>
            {
                if (File.Exists("result.txt"))
                {
                    File.Delete("result.txt");
                }

                var process =  AcceptanceTestHelpers.LaunchScriptCs("start.csx");
                process.WaitForExit();
                scriptResult = process.ExitCode;
            });

            "Then the result is stored in a file"._(() =>
            {
                var result = Convert.ToInt32(File.ReadAllText("result.txt"));
                result.ShouldEqual(42);
            });

            "And the program terminates correctly"._(() =>
            {
                scriptResult.ShouldEqual(0);
            });
        }
    }
}
