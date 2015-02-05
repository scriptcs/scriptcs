using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ObjectSerializer : IObjectSerializer
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        };

        public string Serialize(object value)
        {
            var writer = new JTokenWriter();
            var serializer = JsonSerializer.Create(settings);
            serializer.Serialize(writer, value);

            var container = writer.Token as JContainer;
            if (container != null)
            {
                var idProperties = container.Descendants().OfType<JProperty>().Where(d => d.Name == "$id").ToList();
                if (idProperties.Any())
                {
                    var refProperties = container.Descendants().OfType<JProperty>().Where(d => d.Name == "$ref").ToList();
                    if (refProperties.Any())
                    {
                        foreach (var idProperty in idProperties
                            .Where(idProperty => refProperties
                                .All(refProperty => refProperty.Value.ToString() != idProperty.Value.ToString())))
                        {
                            idProperty.Remove();
                        }
                    }
                    else
                    {
                        foreach (var idProperty in idProperties)
                        {
                            idProperty.Remove();
                        }
                    }
                }
            }

            return writer.Token.ToString();
        }
    }
}
