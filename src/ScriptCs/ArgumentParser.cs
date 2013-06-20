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
    public class ArgumentParser
    {
        private readonly IFileSystem _fileSystem;

        public const string CofigurationFileName = "scriptcs.opts";
        //private readonly ILog _logger;

        public ArgumentParser(string[] args, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            string[] scriptArgs;
            ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);

            ScriptArguments = scriptArgs;
            CommandArguments = ParseArguments(args);
        }

        public ScriptCsArgs CommandArguments { get; private set; }

        public string[] ScriptArguments { get; private set; }

        private ScriptCsArgs ParseArguments(string[] args)
        {
            const string unexpectedArgumentMessage = "Unexpected Argument: ";

            ScriptCsArgs commandArgs = null;
            ScriptCsArgs configArgs = null;

            // todo: add proper error messages

            try
            {
                if(args.Length > 0)
                {
                    commandArgs = Args.Parse<ScriptCsArgs>(args);
                }

                string filePath = _fileSystem.PathCombine(_fileSystem.CurrentDirectory, CofigurationFileName);
                if(_fileSystem.FileExists(filePath))
                {
                    string content = _fileSystem.ReadFile(filePath);

                    configArgs = ParseConfigFile(content);
                }

                return ReconcileArguments(configArgs, commandArgs);
            }
            catch(ArgException ex)
            {
                if(ex.Message.StartsWith(unexpectedArgumentMessage))
                {
                    var token = ex.Message.Substring(unexpectedArgumentMessage.Length);
                    Console.WriteLine("Parameter \"{0}\" is not supported!", token);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return new ScriptCsArgs() { Repl = true };
        }

        private ScriptCsArgs ReconcileArguments(ScriptCsArgs configArgs, ScriptCsArgs commandArgs)
        {
            if(configArgs == null)
                return commandArgs;

            if(commandArgs == null)
                return configArgs;

            foreach(var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var a = property.GetValue(configArgs);
                var b = property.GetValue(commandArgs);
                var d = GetPropertyDefaultValue(property);

                // if config arg is different from command line arg and command line arg value is not default - change
                if(!object.Equals(a, b) && !object.Equals(b, d))
                {
                    property.SetValue(configArgs, b);
                }
            }

            return configArgs;
        }

        private object GetPropertyDefaultValue(PropertyInfo property)
        {
            var defaultAttribute = property.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(PowerArgs.DefaultValueAttribute));

            return defaultAttribute != null
                       ? ((PowerArgs.DefaultValueAttribute)defaultAttribute).Value
                       : property.PropertyType.GetDefaultValue();
        }

        private ScriptCsArgs ParseConfigFile(string content)
        {
            // todo: if something goes wrong - throw ArgException

            if(!string.IsNullOrWhiteSpace(content))
            {
                var arguments = new ScriptCsArgs();

                var fromJson = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(content);
                var configFileValues = new Dictionary<string, string>(fromJson, StringComparer.InvariantCultureIgnoreCase);

                foreach(var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    string key = "";

                    var attributes = property.GetCustomAttributes(false)
                                             .ToDictionary(a => a.GetType().Name, a => a);

                    if(attributes.ContainsKey(typeof(ArgIgnoreAttribute).Name))
                        continue;

                    if(attributes.ContainsKey(typeof(ArgShortcut).Name))
                    {
                        var attribute = (attributes[typeof(ArgShortcut).Name] as ArgShortcut);

                        if(attribute != null)
                        {
                            if(configFileValues.ContainsKey(property.Name))
                            {
                                key = property.Name;
                            }

                            if(string.IsNullOrEmpty(key) && configFileValues.ContainsKey(attribute.Shortcut))
                            {
                                key = attribute.Shortcut;
                            }
                        }
                    }

                    if(!string.IsNullOrEmpty(key))
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
}