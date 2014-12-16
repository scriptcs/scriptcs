using System;
using System.Linq;
using System.Reflection;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs.Argument
{
    public class ArgumentHandler : IArgumentHandler
    {
        private readonly IArgumentParser _argumentParser;
        private readonly IConfigFileParser _configFileParser;
        private readonly IFileSystem _fileSystem;

        public ArgumentHandler(IArgumentParser argumentParser, IConfigFileParser configFileParser, IFileSystem fileSystem)
        {
            Guard.AgainstNullArgument("argumentParser", argumentParser);
            Guard.AgainstNullArgument("configFileParser", configFileParser);
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            _fileSystem = fileSystem;
            _argumentParser = argumentParser;
            _configFileParser = configFileParser;
        }

        public ArgumentParseResult Parse(string[] args)
        {
            var sr = SplitScriptArgs(args);

            var commandArgs = _argumentParser.Parse(sr.CommandArguments);
            var localConfigFile = commandArgs != null ? commandArgs.Config : Constants.ConfigFilename;
            var localConfigPath = string.Format("{0}\\{1}", _fileSystem.CurrentDirectory, localConfigFile);
            var localConfigArgs = _configFileParser.Parse(GetFileContent(localConfigPath));
            var globalConfigArgs = _configFileParser.Parse(GetFileContent(_fileSystem.GlobalOptsFile));
            var finalArguments = ReconcileArguments(globalConfigArgs, localConfigArgs, commandArgs, sr);

            if (finalArguments.LogLevel == null)
            {
                finalArguments.LogLevel = finalArguments.Debug ? LogLevel.Debug : LogLevel.Info;
            }

            return new ArgumentParseResult(args, finalArguments, sr.ScriptArguments);
        }

        private string GetFileContent(string filePath)
        {
            if (_fileSystem.FileExists(filePath))
            {
                return _fileSystem.ReadFile(filePath);
            }

            return null;
        }

        public static SplitResult SplitScriptArgs(string[] args)
        {
            Guard.AgainstNullArgument("args", args);

            var result = new SplitResult() { CommandArguments = args };

            // Split the arguments list on "--".
            // The arguments before the "--" (or all arguments if there is no "--") are
            // for ScriptCs.exe, and the arguments after that are for the csx script.
            int separatorLocation = Array.IndexOf(args, "--");
            int scriptArgsCount = separatorLocation == -1 ? 0 : args.Length - separatorLocation - 1;
            result.ScriptArguments = new string[scriptArgsCount];
            Array.Copy(args, separatorLocation + 1, result.ScriptArguments, 0, scriptArgsCount);

            if (separatorLocation != -1)
                result.CommandArguments = args.Take(separatorLocation).ToArray();

            return result;
        }

        private static ScriptCsArgs ReconcileArguments(ScriptCsArgs globalConfigArgs, ScriptCsArgs localConfigArgs, ScriptCsArgs commandArgs, SplitResult splitResult)
        {
            if (globalConfigArgs == null && localConfigArgs == null)
                return commandArgs;

            if (globalConfigArgs == null && commandArgs == null)
                return localConfigArgs;

            if (localConfigArgs == null && commandArgs == null)
                return globalConfigArgs;

            var reconciledArgs = new ScriptCsArgs();

            foreach (var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var defaultValue = GetPropertyDefaultValue(property);

                if (commandArgs != null)
                {
                    var commandValue = property.GetValue(commandArgs);
                    if (!Equals(commandValue, defaultValue))
                    {
                        property.SetValue(reconciledArgs, commandValue);
                        continue;
                    }
                    if (IsCommandLinePresent(splitResult.CommandArguments, property))
                    {
                        property.SetValue(reconciledArgs, commandValue);
                        continue;
                    }
                }

                if (localConfigArgs != null)
                {
                    var localConfigValue = property.GetValue(localConfigArgs);
                    if (!Equals(localConfigValue, defaultValue))
                    {
                        property.SetValue(reconciledArgs, localConfigValue);
                        continue;
                    }
                }

                if (globalConfigArgs != null)
                {
                    var globalConfigValue = property.GetValue(globalConfigArgs);
                    if (!Equals(globalConfigValue, defaultValue))
                    {
                        property.SetValue(reconciledArgs, globalConfigValue);
                    }
                }
            }

            return reconciledArgs;
        }

        private static bool IsCommandLinePresent(string[] args, PropertyInfo property)
        {
            bool attributeFound = false;

            var attribute = property.GetCustomAttribute<ArgShortcut>();

            if (attribute != null)
                attributeFound = args.Any(a => a.Contains("-" + (attribute as ArgShortcut).Shortcut));

            var result = args.Any(a => a.Contains("-" + property.Name)) || attributeFound;
            return result;
        }

        private static object GetPropertyDefaultValue(PropertyInfo property)
        {
            var defaultAttribute = property.GetCustomAttribute<DefaultValueAttribute>();

            return defaultAttribute != null
                       ? ((PowerArgs.DefaultValueAttribute)defaultAttribute).Value
                       : property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
        }

        public class SplitResult
        {
            public SplitResult()
            {
                CommandArguments = new string[0];
                ScriptArguments = new string[0];
            }

            public string[] CommandArguments { get; set; }
            public string[] ScriptArguments { get; set; }
        }
    }
}