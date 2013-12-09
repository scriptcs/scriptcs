using Newtonsoft.Json;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ObjectSerializer : IObjectSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            MaxDepth = 4
        };

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented, Settings);
        }
    }
}