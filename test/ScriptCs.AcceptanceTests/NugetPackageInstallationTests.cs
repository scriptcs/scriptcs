using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Should;
using Xbehave;
using Xunit;
using System.Diagnostics;

namespace ScriptCs.AcceptanceTests
{
    public class NugetPackageInstallationTests
    {
        [Scenario]
        [Trait("AcceptanceTest", "NugetPackageInstallationTests")]
        [Trait("Requires", "Internet Connection")]
        public void InstallPackagesFromPackagesConfigFile(string localDirectory, int result, string packagesPath, string originalWorkingDirectory)
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

            "And a packages.config file located in the current directory"._(() =>
            {
                var filesDirectory = Path.Combine(AcceptanceTestHelpers.GetExecutingAssemblyDirectory(), "Files");
                var packageConfigName = string.Format("{0}.packages.config", methodName);
                Directory.CreateDirectory(localDirectory);
                File.Copy(Path.Combine(filesDirectory, packageConfigName), Path.Combine(localDirectory, "packages.config"));
            });

            "When the packages are installed"._(() =>
            {
                var process =  AcceptanceTestHelpers.LaunchScriptCs("-install");
                process.WaitForExit();

                result = process.ExitCode;
            });

            "Then the program executes successfully"._(() => result.ShouldEqual(0));

            "And a packages directory is created inside the working directory"._(() =>
            {
                packagesPath = Path.Combine(localDirectory, "packages");

                Directory.Exists(packagesPath).ShouldBeTrue();
            });

            "And a directory for the package and each of its dependencies is created"._(() =>
            {
                var packagesDir = new DirectoryInfo(packagesPath);

                var packagesDirectoriesNames = packagesDir.EnumerateDirectories().Select(pd => pd.Name).ToArray();

                packagesDirectoriesNames.Length.ShouldEqual(4);

                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"Common\.Logging\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"ScriptCs\.Contracts\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"ScriptCs\.Core\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"ServiceStack\.Text\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
            });
        }

        [Scenario]
        [Trait("AcceptanceTest", "NugetPackageInstallationTests")]
        [Trait("Requires", "Internet Connection")]
        public void InstallingAPackageByName(string originalWorkingDirectory, string packageName, string localDirectory, int result, string packagesPath)
        {
            var methodName = AcceptanceTestHelpers.GetCurrentMethodName();
            
            "Given a package name"._(() => packageName = "ScriptCs.Core");

            "And a current working directory "._(() =>
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

            "When that package is installed"._(() =>
            {
                string args = string.Format("-install {0}", packageName);

                var process = AcceptanceTestHelpers.LaunchScriptCs(args);
                process.WaitForExit();

                result = process.ExitCode;
            });

            "Then the program executes successfully"._(() => result.ShouldEqual(0));

            "And a packages directory is created inside the working directory"._(() =>
            {
                packagesPath = Path.Combine(localDirectory, "packages");

                Directory.Exists(packagesPath).ShouldBeTrue();
            });

            "And a directory for the package and each of its dependencies is created"._(() =>
            {
                var packagesDir = new DirectoryInfo(packagesPath);

                var packagesDirectoriesNames = packagesDir.EnumerateDirectories().Select(pd => pd.Name).ToArray();

                packagesDirectoriesNames.Length.ShouldEqual(4);

                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"Common\.Logging\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"ScriptCs\.Contracts\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"ScriptCs\.Core\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
                packagesDirectoriesNames.Count(n => Regex.IsMatch(n, @"ServiceStack\.Text\.[0-9]+\.[0-9]+\.[0-9]+")).ShouldEqual(1);
            });
        }
    }
}
