using System;
using System.Linq;
using System.Reflection;
using PowerArgs;
using ServiceStack.Text;

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
            Guard.AgainstNullArgument("configFileParser", fileSystem);

            _fileSystem = fileSystem;
            _argumentParser = argumentParser;
            _configFileParser = configFileParser;
        }

        public ArgumentParseResult Parse(string[] args)
        {
            var sr = SplitScriptArgs(args);

            var commandArgs = _argumentParser.Parse(sr.CommandArguments);
            var configArgs = _configFileParser.Parse(GetFileContent(commandArgs.Config));
            var finalArguments = ReconcileArguments(configArgs, commandArgs, sr);

            return new ArgumentParseResult(args, finalArguments, sr.ScriptArguments);
        }

        private string GetFileContent(string fileName)
        {
            string filePath = _fileSystem.CurrentDirectory + '\\' + fileName;
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

            if(separatorLocation != -1)
                result.CommandArguments = args.Take(separatorLocation).ToArray();

            return result;
        }

        private static ScriptCsArgs ReconcileArguments(ScriptCsArgs configArgs, ScriptCsArgs commandArgs, SplitResult splitResult)
        {
            if (configArgs == null)
                return commandArgs;

            if (commandArgs == null)
                return configArgs;

            foreach (var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var configValue = property.GetValue(configArgs);
                var commandValue = property.GetValue(commandArgs);
                var defaultValue = GetPropertyDefaultValue(property);

                if(!object.Equals(configValue, commandValue))
                {
                    if(!object.Equals(commandValue, defaultValue))
                    {
                        property.SetValue(configArgs, commandValue);
                    }
                    else
                    {
                        if(IsCommandLinePresent(splitResult.CommandArguments, property))
                            property.SetValue(configArgs, commandValue);
                    }
                }
            }

            return configArgs;
        }

        private static bool IsCommandLinePresent(string[] args, PropertyInfo property)
        {
            bool attributeFound = false;

            var attribute = property.GetCustomAttribute<ArgShortcut>();

            if(attribute != null)
                attributeFound = args.Any(a => a.Contains((attribute as ArgShortcut).Shortcut));

            var result = args.Any(a => a.Contains(property.Name)) || attributeFound;
            return result;
        }

        private static object GetPropertyDefaultValue(PropertyInfo property)
        {
            var defaultAttribute = property.GetCustomAttribute<DefaultValueAttribute>();

            return defaultAttribute != null
                       ? ((PowerArgs.DefaultValueAttribute)defaultAttribute).Value
                       : property.PropertyType.GetDefaultValue();
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