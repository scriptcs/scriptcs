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
        public void LoadingASpecificModule(
            string moduleName,
            string modulesFolderPath,
            string modulesPackagesFolderPath,
            string moduleFolderPath,
            string modulesPackagesConfig,
            string originalWorkingDirectory,
            string localDirectory,
            string output,
            int result,
            bool otherModulesExisted)
        {
            var methodName = AcceptanceTestHelpers.GetCurrentMethodName();
            otherModulesExisted = false;
            var filesDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), "Files");

            "Given an installed module"._(() =>
            {
                moduleName = "ScriptCs.TestModule";
                modulesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "scriptcs");
                modulesPackagesConfig = Path.Combine(modulesFolderPath, "packages.config");
                otherModulesExisted = Directory.Exists(modulesPackagesConfig);
                modulesPackagesFolderPath = Path.Combine(modulesFolderPath, "packages");

                string args = string.Format("-install {0} -g", moduleName);

                var process = AcceptanceTestHelpers.LaunchScriptCs(args);
                process.WaitForExit();
            })
            .Teardown(() =>
            {
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

            "And a current working directory"._(() =>
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

            "And a script file in the current working directory"._(() =>
            {
                filesDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), "Files\\modules");
                File.Copy(Path.Combine(filesDirectory, "test.csx"), Path.Combine(localDirectory, "test.csx"));
            });

            "When a script is executed while forcing a module to load"._(() => { 
                string args = "test.csx -modules test";

                var process = AcceptanceTestHelpers.LaunchScriptCs(args);
                process.WaitForExit();

                output = process.StandardOutput.ReadToEnd();

                result = process.ExitCode;
            });

            "Then the program executes successfully"._(() => result.ShouldEqual(0));

            "And the output includes text written by the module"._(() =>
            {
                output.ShouldContain("Test module initialized");
            });

            "And text written by the script"._(() =>
            {
                output.ShouldContain("Hello world");
            });

            "And the module's output is before the script's output"._(() =>
            {
                var moduleOutputIndex = output.IndexOf("Test module initialized");
                var scriptOutputIndex = output.IndexOf("Hello world");

                moduleOutputIndex.ShouldBeLessThan(scriptOutputIndex);
            });
        }

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
            }).Teardown(() =>
            {
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
