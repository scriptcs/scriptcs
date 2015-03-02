using System;
using System.Linq;
using System.Reflection;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs.Argument
{
    public class ArgumentHandler : IArgumentHandler
    {
        private readonly IConfigFileParser _configFileParser;
        private readonly IFileSystem _fileSystem;

        public ArgumentHandler(IConfigFileParser configFileParser, IFileSystem fileSystem)
        {
            Guard.AgainstNullArgument("configFileParser", configFileParser);
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            _fileSystem = fileSystem;
            _configFileParser = configFileParser;
        }

        public ScriptCsArgs Parse(ScriptCsArgs scriptCsArgs, string[] args)
        {
            var localConfigFile = scriptCsArgs.Config;
            var localConfigPath = string.Format("{0}\\{1}", _fileSystem.CurrentDirectory, localConfigFile);
            var localConfigArgs = _configFileParser.Parse(GetFileContent(localConfigPath));
            var globalConfigArgs = _configFileParser.Parse(GetFileContent(_fileSystem.GlobalOptsFile));
            var finalArguments = ReconcileArguments(globalConfigArgs, localConfigArgs, scriptCsArgs, args);

            if (finalArguments.LogLevel == null)
            {
                finalArguments.LogLevel = finalArguments.Debug ? LogLevel.Debug : LogLevel.Info;
            }

            return finalArguments;
        }

        private string GetFileContent(string filePath)
        {
            if (_fileSystem.FileExists(filePath))
            {
                return _fileSystem.ReadFile(filePath);
            }

            return null;
        }

        private static ScriptCsArgs ReconcileArguments(ScriptCsArgs globalConfigArgs, ScriptCsArgs localConfigArgs, ScriptCsArgs commandArgs, string[] args)
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
                    if (IsCommandLinePresent(args, property))
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
    }
}