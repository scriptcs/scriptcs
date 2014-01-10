using System;
using System.IO;
using Should;
using Xbehave;
using Xunit;

namespace ScriptCs.AcceptanceTests
{
    public class ModulesTests
    {
        [Scenario]
        [Trait("AcceptanceTest", "NugetPackageInstallationTests")]
        [Trait("Requires", "Internet Connection")]
        public void InstallingAModuleByName(
            string moduleName, 
            string modulesFolderPath,
            string modulesPackagesFolderPath, 
            string moduleFolderPath, 
            string modulesPackagesConfig,
            int result, 
            bool otherModulesExisted)
        {
            var methodName = AcceptanceTestHelpers.GetCurrentMethodName();
            otherModulesExisted = false;

            "Given a module name"._(() => moduleName = "ScriptCs.TestModule");

            "And the folder where global modules are installed"._(() =>
            {
                modulesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "scriptcs");
                modulesPackagesConfig = Path.Combine(modulesFolderPath, "packages.config");
                otherModulesExisted = Directory.Exists(modulesPackagesConfig);
            });

            "And the folder where module packages are installed"._(() =>
            {
                modulesPackagesFolderPath = Path.Combine(modulesFolderPath, "packages");
            });

            "When that package is installed"._(() =>
            {
                string args = string.Format("-install {0} -g", moduleName);

                var process = AcceptanceTestHelpers.LaunchScriptCs(args);
                process.WaitForExit();

                result = process.ExitCode;
            }).Teardown(() => {
                if (!otherModulesExisted)
                {
                    Directory.Delete(modulesFolderPath, true);
                }
                else
                {
                    if (Directory.Exists(moduleFolderPath))
                    {
                        Directory.Delete(moduleFolderPath, true);
                    }

                    var text = File.ReadAllText(modulesPackagesConfig);
                    var updatedText = text.Replace("<package id=\"ScriptCs.TestModule\" version=\"0.1.0\" targetFramework=\"net45\" />", string.Empty);
                    File.WriteAllText(modulesPackagesConfig, updatedText);
                }
            });

            "Then the program executes successfully"._(() => result.ShouldEqual(0));

            "And a module directory is created inside the modules packages directory"._(() =>
            {
                moduleFolderPath = Path.Combine(modulesPackagesFolderPath, "ScriptCs.TestModule.0.1.0");

                Directory.Exists(moduleFolderPath).ShouldBeTrue();
            });
        }
    }
}
