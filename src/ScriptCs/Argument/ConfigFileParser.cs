using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs.Argument
{
    public class ConfigFileParser : IConfigFileParser
    {
        private readonly IConsole _console;

        public ConfigFileParser(IConsole console)
        {
            _console = console;
        }

        public ScriptCsArgs Parse(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var fromJson = ParseJson(content);

                if (fromJson != null)
                {
                    var arguments = new ScriptCsArgs();
                    var configFileValues = new Dictionary<string, string>(fromJson, StringComparer.InvariantCultureIgnoreCase);

                    foreach (var property in typeof(ScriptCsArgs).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        string key = "";
                        if (configFileValues.ContainsKey(property.Name))
                        {
                            key = property.Name;
                        }
                        else
                        {
                            var attributes = property.GetCustomAttributes(false)
                                                     .ToDictionary(a => a.GetType().Name, a => a);

                            if (attributes.ContainsKey(typeof(ArgIgnoreAttribute).Name)) continue;

                            if (attributes.ContainsKey(typeof(ArgShortcut).Name))
                            {
                                var attribute = (attributes[typeof(ArgShortcut).Name] as ArgShortcut);

                                if (attribute != null)
                                {
                                    if (string.IsNullOrEmpty(key) && configFileValues.ContainsKey(attribute.Shortcut))
                                    {
                                        key = attribute.Shortcut;
                                    }
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
            }

            return null;
        }

        private Dictionary<string, string> ParseJson(string content)
        {
            Dictionary<string, string> dict = null;

            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }
            catch 
            {
                _console.WriteLine("Error parsing configuration file.");
            }

            return dict;
        }
    }
}