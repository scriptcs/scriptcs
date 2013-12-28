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
        [Trait("Requires", "Internet Connection")]
        [Trait("Requires", "Administrator Privileges")]
        public void RunningWebApiHostSampleEnablesWebAccessInLocalhost(string originalWorkingDirectory, string localDirectory, Task<int> scriptTask)
        {
            string methodName = AcceptanceTestHelpers.GetCurrentMethodName();

            "Given an existing Web API host sample (similar to https://github.com/scriptcs/scriptcs-samples/tree/master/webapihost)"
            ._(() =>
            {
                originalWorkingDirectory = Environment.CurrentDirectory;
                localDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), methodName);
                
                var webApiHostDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(),  @"Files\webapihost");
                Directory.CreateDirectory(localDirectory);
                AcceptanceTestHelpers.CopyDirectory(webApiHostDirectory, localDirectory, true);

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
                scriptTask = Task.Run(() =>
                {
                    var process =  AcceptanceTestHelpers.LaunchScriptCs("start.csx");
                    process.WaitForExit();
                    return process.ExitCode;
                });
            });

            "Then the endpoint can be hit"._(() =>
            {
                Task.Delay(10000).Wait();
                var client = new HttpClient();
                var getResult = client.GetStringAsync("http://localhost:8080/api/test").Result;

                getResult.ShouldContain("Hello world!");
            });

            "And the program terminates correctly"._(() =>
            {
                var result = scriptTask.Result;
                result.ShouldEqual(0);
            });
        }
    }
}
