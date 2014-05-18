using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ObjectSerializer : IObjectSerializer
    {
        private static readonly Regex RefOrIdRegex = new Regex(string.Format(@"(.+\""(\$id|\$ref)\"".+{0})", Environment.NewLine),
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            MaxDepth = 4
        };

        public string Serialize(object value)
        {
            return RefOrIdRegex.Replace(JsonConvert.SerializeObject(value, Formatting.Indented, Settings), string.Empty);
        }
    }
}
