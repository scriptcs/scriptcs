using ScriptCs.Contracts;

using ServiceStack.Text;

namespace ScriptCs
{
    public class ObjectSerializer : IObjectSerializer
    {
        public string Serialize(object value)
        {
            return value.ToCsv();
        }
    }
}