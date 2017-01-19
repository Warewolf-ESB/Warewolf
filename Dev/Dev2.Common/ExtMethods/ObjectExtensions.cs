using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Dev2.Common.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Common.ExtMethods
{
    public class KnownTypesBinder : SerializationBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SerializationBinder"/> class.
        /// </summary>
        public KnownTypesBinder()
        {
            KnownTypes = new List<Type>();
        }

        public IList<Type> KnownTypes { get; set; }

        public override Type BindToType(string assemblyName, string typeName)
        {
            return KnownTypes.SingleOrDefault(t => t.FullName == typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }
    }
    public static class ObjectExtensions
    {
        // ReSharper disable RedundantNameQualifier
        const Formatting Formatting = Newtonsoft.Json.Formatting.Indented;
        // ReSharper restore RedundantNameQualifier
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        static readonly JsonSerializerSettings DeSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,

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

        public static string SerializeToJsonString<T>(this T objectToSerialize, SerializationBinder binder) where T : class, new()
        {
            SerializerSettings.Binder = binder;
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

        public static object DeserializeToObject(this string objectToSerialize, Type type, SerializationBinder binder) 
        {
            DeSerializerSettings.Binder = binder;
            var deserializeObject = JsonConvert.DeserializeObject(objectToSerialize, type, DeSerializerSettings);
            return deserializeObject;
        }

        public static object DeserializeToObject(this string objectToSerialize, SerializationBinder binder) 
        {
            DeSerializerSettings.Binder = binder;
            var deserializeObject = JsonConvert.DeserializeObject(objectToSerialize, DeSerializerSettings);
            return deserializeObject;
        }

        public static JContainer DeserializeToObject(this string objectToSerialize) 
        {
            var deserializeObject = JsonConvert.DeserializeObject(objectToSerialize, DeSerializerSettings) as JContainer;
            return deserializeObject;
        }

        

        public static T DeserializeToObject<T>(this StringBuilder objectToSerialize) 
        {
            var serialize = Deserialize<T>(objectToSerialize);
            return serialize;
        }
        
    }


}
