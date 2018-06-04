﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Dev2.Common.ExtMethods
{
    public class KnownTypesBinder : ISerializationBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SerializationBinder"/> class.
        /// </summary>
        public KnownTypesBinder()
        {
            KnownTypes = new List<Type>();
        }

        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName) => KnownTypes.SingleOrDefault(t => t.FullName == typeName);

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }
    }
    public static class ObjectExtensions
    {
        
        const Formatting Formatting = Newtonsoft.Json.Formatting.Indented;
        
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        static readonly JsonSerializerSettings DeSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,

        };

        static T Deserialize<T>(string message) => JsonConvert.DeserializeObject<T>(message, DeSerializerSettings);

        static T Deserialize<T>(StringBuilder message) => JsonConvert.DeserializeObject<T>(message.ToString(), DeSerializerSettings);

        static string Serialize<T>(T message) => JsonConvert.SerializeObject(message, Formatting, SerializerSettings);

        public static T DeepCopy<T>(T other) where T : new()
        {
            try
            {
                var serialize = Serialize(other);
                var deserialize = Deserialize<T>(serialize);
                return deserialize;
            }
            
            catch (Exception)
            {

                return default(T);
            }

        }

        public static string SerializeToJsonString<T>(this T objectToSerialize, ISerializationBinder binder) where T : class, new()
        {
            SerializerSettings.SerializationBinder = binder;
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

        public static object DeserializeToObject(this string objectToSerialize, Type type, ISerializationBinder binder) 
        {
            DeSerializerSettings.SerializationBinder = binder;
            var deserializeObject = JsonConvert.DeserializeObject(objectToSerialize, type, DeSerializerSettings);
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
        public static T DeserializeToObject<T>(this StringBuilder objectToSerialize, ISerializationBinder binder) 
        {
            DeSerializerSettings.SerializationBinder = binder;
            var serialize = Deserialize<T>(objectToSerialize);
            return serialize;
        }
        
    }


}
