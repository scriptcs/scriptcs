using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public class Printers
    {
         private readonly IObjectSerializer _serializer;
         private readonly Dictionary<Type, Func<object, string>> _dictionary = new Dictionary<Type, Func<object, string>>();
         public Printers(IObjectSerializer serializer)
         {
         	  _serializer = serializer;
         }

        public void AddCustomPrinter<T>(Func<T, string> printer)
        {
            _dictionary[typeof(T)] = x => printer((T) x);
        }

        private string GetStringFor(Type t,  object obj)
        {
           Func<object, string> printer;
           if(_dictionary.TryGetValue(t, out printer)) {
                return printer(obj);
           } else {
               return _serializer.Serialize(obj);
           }
        }

        public string GetStringFor(object obj)
        {
           return GetStringFor(obj.GetType(), obj);
        }

        public string GetStringFor<T>(T obj)
        {
           return GetStringFor(typeof(T), obj);
        }
    }
}