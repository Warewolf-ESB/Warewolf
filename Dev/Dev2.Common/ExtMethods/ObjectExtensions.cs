using System;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Dev2.Common.Common;
using Newtonsoft.Json;

namespace Dev2.Common.ExtMethods
{
    public static class ObjectExtensions
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

        private static T Deserialize<T>(StringBuilder message)
        {
            return JsonConvert.DeserializeObject<T>(message.ToString(), DeSerializerSettings);
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

        public static string SerializeToJsonString<T>(this T objectToSerialize) where T : class, new()
        {
            var serialize = Serialize(objectToSerialize);
            return serialize;
        }
        public static StringBuilder SerializeToJsonStringBuilder<T>(this T objectToSerialize) where T : class, new()
        {
            var serialize = Serialize(objectToSerialize);
            return serialize.ToStringBuilder();
        }

        public static T DeserializeToObject<T>(this string objectToSerialize) 
        {
            var serialize = Deserialize<T>(objectToSerialize);
            return serialize;
        }

        public static object DeserializeToObject(this string objectToSerialize, Type type) 
        {
            var deserializeObject = JsonConvert.DeserializeObject(objectToSerialize, type, DeSerializerSettings);
            return deserializeObject;
        }

        public static T DeserializeToObject<T>(this StringBuilder objectToSerialize) 
        {
            var serialize = Deserialize<T>(objectToSerialize);
            return serialize;
        }
    }


}
