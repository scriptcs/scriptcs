using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Common.Logging;
using PowerArgs;
using ServiceStack.Text;

namespace ScriptCs
{
    public interface IArgumentParser
    {
        ScriptCsArgs Parse(string[] args);
    }

    public interface IConfigFileParser
    {
        ScriptCsArgs Parse(string content);
    }

    public interface IArgumentHandler
    {
        ArgumentHandler.ArgumentHandlerResult Parse(string[] args);
    }

    public class JsonConfigFileParser : IConfigFileParser
    {
        public ScriptCsArgs Parse(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var arguments = new ScriptCsArgs();

                var fromJson = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(content);
                var configFileValues = new Dictionary<string, string>(fromJson, StringComparer.InvariantCultureIgnoreCase);

                foreach (var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    string key = "";

                    var attributes = property.GetCustomAttributes(false)
                                             .ToDictionary(a => a.GetType().Name, a => a);

                    if (attributes.ContainsKey(typeof(ArgIgnoreAttribute).Name))
                        continue;

                    if (attributes.ContainsKey(typeof(ArgShortcut).Name))
                    {
                        var attribute = (attributes[typeof(ArgShortcut).Name] as ArgShortcut);

                        if (attribute != null)
                        {
                            if (configFileValues.ContainsKey(property.Name))
                            {
                                key = property.Name;
                            }

                            if (string.IsNullOrEmpty(key) && configFileValues.ContainsKey(attribute.Shortcut))
                            {
                                key = attribute.Shortcut;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(key))
                    {
                        string value = configFileValues[key];
                        var converter = TypeDescriptor.GetConverter(property.PropertyType);
                        var result = converter.ConvertFrom(value);

                        property.SetValue(arguments, result);
                    }
                }

                return arguments;
            }

            return null;
        }
    }

    public class ArgumentParser : IArgumentParser
    {
        public ScriptCsArgs Parse(string[] args)
        {
            var commandArgs = new ScriptCsArgs() { Repl = true };

            if (args.Length > 0)
            {
                const string unexpectedArgumentMessage = "Unexpected Argument: ";

                try
                {
                    commandArgs = Args.Parse<ScriptCsArgs>(args);
                }
                catch (ArgException ex)
                {
                    if (ex.Message.StartsWith(unexpectedArgumentMessage))
                    {
                        var token = ex.Message.Substring(unexpectedArgumentMessage.Length);
                        Console.WriteLine("Parameter \"{0}\" is not supported!", token);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return commandArgs;
        }
    }

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

        public ArgumentHandlerResult Parse(string[] args)
        {
            string[] scriptArgs;
            SplitScriptArgs(ref args, out scriptArgs);

            var configArgs = _argumentParser.Parse(args);
            var commandArgs = _configFileParser.Parse(GetFileContent(configArgs.Config));

            var finalArguments = ReconcileArguments(configArgs, commandArgs);

            return new ArgumentHandlerResult(finalArguments, scriptArgs);
        }

        private string GetFileContent(string fileName)
        {
            string filePath = _fileSystem.PathCombine(_fileSystem.CurrentDirectory, fileName);
            if (_fileSystem.FileExists(filePath))
            {
                return _fileSystem.ReadFile(filePath);
            }

            return null;
        }

        public static void SplitScriptArgs(ref string[] args, out string[] scriptArgs)
        {
            Guard.AgainstNullArgument("args", args);

            // Split the arguments list on "--".
            // The arguments before the "--" (or all arguments if there is no "--") are
            // for ScriptCs.exe, and the arguments after that are for the csx script.
            int separatorLocation = Array.IndexOf(args, "--");
            int scriptArgsCount = separatorLocation == -1 ? 0 : args.Length - separatorLocation - 1;
            scriptArgs = new string[scriptArgsCount];
            Array.Copy(args, separatorLocation + 1, scriptArgs, 0, scriptArgsCount);
            if (separatorLocation != -1) args = args.Take(separatorLocation).ToArray();
        }

        private static ScriptCsArgs ReconcileArguments(ScriptCsArgs configArgs, ScriptCsArgs commandArgs)
        {
            if (configArgs == null)
                return commandArgs;

            if (commandArgs == null)
                return configArgs;

            foreach (var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var a = property.GetValue(configArgs);
                var b = property.GetValue(commandArgs);
                var d = GetPropertyDefaultValue(property);

                // if config arg is different from command line arg and command line arg value is not default - change
                if (!object.Equals(a, b) && !object.Equals(b, d))
                {
                    property.SetValue(configArgs, b);
                }
            }

            return configArgs;
        }

        private static object GetPropertyDefaultValue(PropertyInfo property)
        {
            var defaultAttribute = property.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(PowerArgs.DefaultValueAttribute));

            return defaultAttribute != null
                       ? ((PowerArgs.DefaultValueAttribute)defaultAttribute).Value
                       : property.PropertyType.GetDefaultValue();
        }

        public class ArgumentHandlerResult
        {
            public ArgumentHandlerResult(ScriptCsArgs commandArguments, string[] scriptArguments)
            {
                CommandArguments = commandArguments;
                ScriptArguments = scriptArguments;
            }

            public ScriptCsArgs CommandArguments { get; private set; }
            public string[] ScriptArguments { get; private set; }
        }
    }
}