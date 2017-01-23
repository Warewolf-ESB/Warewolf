using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace Dev2.Common.ExtMethods
{
   public class ObjectExtensions
    {
        // ReSharper disable RedundantNameQualifier
        const Formatting Formatting = Newtonsoft.Json.Formatting.Indented;
        // ReSharper restore RedundantNameQualifier
       static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
        };
       static readonly JsonSerializerSettings DeSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
        };

       private static T Deserialize<T>(string message)
        {
            return JsonConvert.DeserializeObject<T>(message, DeSerializerSettings);
        }

       private static string Serialize<T>(T message)
        {
            return JsonConvert.SerializeObject(message, Formatting, SerializerSettings);
        }
        public static T DeepCopy<T>(T other) where T : new() 
        {
            try
            {
                var serialize = Serialize(other);
                var deserialize = Deserialize<T>(serialize);
                return deserialize;
            }
                // ReSharper disable once UnusedVariable
            catch (Exception)
            {
                
                return default(T);
            }

        }
    }

    
}
