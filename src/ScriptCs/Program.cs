using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ScriptCs.Command;

namespace ScriptCs
{
    internal static class Program
    {
        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomain)]
        private static int Main(string[] args)
        {
            SetProfile();

            var nonScriptArgs = args.TakeWhile(arg => arg != "--").ToArray();
            var scriptArgs = args.Skip(nonScriptArgs.Length + 1).ToArray();

            ScriptCsArgs commandArgs;
            try
            {
                commandArgs = ScriptCsArgs.Parse(nonScriptArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ScriptCsArgs.GetUsage());
                return 1;
            }

            if (commandArgs.Help)
            {
                Console.WriteLine(ScriptCsArgs.GetUsage());
                return 0;
            }

            if (commandArgs.Version)
            {
                VersionWriter.Write(FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion);
                return 0;
            }

            if (commandArgs.Config != null && !File.Exists(commandArgs.Config))
            {
                Console.WriteLine("The specified config file does not exist.");
                return 1;
            }

            var config = new Config()
                .Apply(ConfigMask.ReadOrDefault(new FileSystem().GlobalOptsFile))
                .Apply(ConfigMask.ReadOrDefault(commandArgs.Config ?? Constants.ConfigFilename))
                .Apply(ConfigMask.Create(commandArgs));

            var scriptServicesBuilder = ScriptServicesBuilderFactory.Create(config, scriptArgs);
            var factory = new CommandFactory(scriptServicesBuilder);
            var command = factory.CreateCommand(config, scriptArgs);
            return (int)command.Execute();
        }

        private static void SetProfile()
        {
            var profileOptimizationType = Type.GetType("System.Runtime.ProfileOptimization");
            if (profileOptimizationType != null)
            {
                var setProfileRoot = profileOptimizationType.GetMethod("SetProfileRoot", BindingFlags.Public | BindingFlags.Static);
                setProfileRoot.Invoke(null, new object[] { typeof(Program).Assembly.Location });

                var startProfile = profileOptimizationType.GetMethod("StartProfile", BindingFlags.Public | BindingFlags.Static);
                startProfile.Invoke(null, new object[] { typeof(Program).Assembly.GetName().Name + ".profile" });
            }
        }
    }
}
