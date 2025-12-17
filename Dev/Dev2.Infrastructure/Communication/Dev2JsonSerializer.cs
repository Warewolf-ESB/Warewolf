#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Communication;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dev2.Communication
{
    public class Dev2JsonSerializer : IBuilderSerializer
    {
        const Formatting Formatting = Newtonsoft.Json.Formatting.Indented;

        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Converters = { new PortableTypeJsonConverter() }
        };
        readonly JsonSerializerSettings _deSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            SerializationBinder = new DotNetCompatibleSerializationBinder(),
            Converters = { new PortableTypeJsonConverter() }
        };
        public string Serialize<T>(T obj) => this.Serialize<T>(obj, Formatting);
        public string Serialize<T>(T obj, Formatting formatting) => JsonConvert.SerializeObject(obj, formatting, _serializerSettings);

        public T Deserialize<T>(string obj)
        {
            VerifyArgument.IsNotNull("message", obj);
            return JsonConvert.DeserializeObject<T>(obj, _deSerializerSettings);
        }

        public object Deserialize(string obj, Type type)
        {
            VerifyArgument.IsNotNull("message", obj);
            VerifyArgument.IsNotNull("type", type);
            return JsonConvert.DeserializeObject(obj, type, _deSerializerSettings);
        }

        public StringBuilder SerializeToBuilder(object obj)
        {
            var result = new StringBuilder();

            using (StringWriter sw = new StringWriter(result))
            {
                var jsonSerializer = new JsonSerializer
                {
                    TypeNameHandling = _serializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                jsonSerializer.Converters.Add(new PortableTypeJsonConverter());

                using (var jsonTextWriter = new JsonTextWriter(sw))
                {
                    jsonSerializer.Serialize(jsonTextWriter, obj);
                    jsonTextWriter.Flush();
                    jsonTextWriter.Close();
                }
            }
            return result;
        }

        public T Deserialize<T>(StringBuilder message) where T : class
        {
            if (message != null && message.Length > 0)
            {
                var serializer = new JsonSerializer
                {
                    TypeNameHandling = _deSerializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling,
                    SerializationBinder = _deSerializerSettings.SerializationBinder
                };
                serializer.Converters.Add(new PortableTypeJsonConverter());

                using (MemoryStream ms = new MemoryStream(message.Length))
                {
                    // now load the stream ;)

                    var length = message.Length;
                    var startIdx = 0;
                    var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);

                    for (int i = 0; i < rounds; i++)
                    {
                        var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                        if (len > message.Length - startIdx)
                        {
                            len = message.Length - startIdx;
                        }

                        var bytes = Encoding.UTF8.GetBytes(message.Substring(startIdx, len));
                        ms.Write(bytes, 0, bytes.Length);
                        startIdx += len;
                    }
                    ms.Flush();
                    ms.Position = 0;

                    try
                    {
                        using (StreamReader sr = new StreamReader(ms))
                        {
                            using (JsonReader jr = new JsonTextReader(sr))
                            {
                                var result = serializer.Deserialize(jr, typeof(T));
                                return result as T;
                            }
                        }
                    }
                    catch
                    {
                        // Do nothing default(T) returned below
                    }
                }
            }
            return default(T);
        }

        public void Serialize(StreamWriter streamWriter, object obj)
        {
            using (streamWriter)
            {
                var jsonSerializer = new JsonSerializer
                {
                    TypeNameHandling = _serializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                jsonSerializer.Converters.Add(new PortableTypeJsonConverter());

                using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    jsonSerializer.Serialize(jsonTextWriter, obj);
                    jsonTextWriter.Flush();
                    jsonTextWriter.Close();
                }
            }
        }

        public T Deserialize<T>(StreamReader streamReader)
        {
            using (streamReader)
            {
                var jsonSerializer = new JsonSerializer
                {
                    TypeNameHandling = _serializerSettings.TypeNameHandling,
                    TypeNameAssemblyFormatHandling = _serializerSettings.TypeNameAssemblyFormatHandling,
                    ReferenceLoopHandling = _serializerSettings.ReferenceLoopHandling,
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling,
                    SerializationBinder = _deSerializerSettings.SerializationBinder
                };
                using (var reader = new JsonTextReader(streamReader))
                {
                    var result = jsonSerializer.Deserialize<T>(reader);
                    return result;
                }
            }
        }
    }

    internal sealed class DotNetCompatibleSerializationBinder : Newtonsoft.Json.Serialization.DefaultSerializationBinder
    {
        private const string CoreLibAssembly = "System.Private.CoreLib";
        private const string AltCoreLibAssembly = "System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
		private const string MscorlibAssembly = "mscorlib";

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName == CoreLibAssembly)
            {
                assemblyName = MscorlibAssembly;
                typeName = typeName.Replace(CoreLibAssembly, MscorlibAssembly);
            }
            else if (assemblyName == AltCoreLibAssembly)
            {
                assemblyName = MscorlibAssembly;
                typeName = typeName.Replace(AltCoreLibAssembly, MscorlibAssembly);
            }
            return base.BindToType(assemblyName, typeName);
        }
    }

    internal sealed class PortableTypeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Type);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var type = (Type)value;

            // For simple types we just emit FullName (e.g., System.Int32).
            // Arrays or generics can be added if required later.
            writer.WriteValue(type.FullName);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var raw = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // If assembly-qualified, try binder first
            var commaIndex = raw.IndexOf(',');
            if (commaIndex > 0 && serializer.SerializationBinder is DefaultSerializationBinder binder)
            {
                // Split "Namespace.Type, Assembly, Version=..., Culture=..., PublicKeyToken=..."
                var typePart = raw.Substring(0, commaIndex).Trim();
                var asmPart = raw.Substring(commaIndex + 1).Trim();
                try
                {
                    var t = binder.BindToType(asmPart, typePart);
                    if (t != null) return t;
                }
                catch { /* fall through */ }
            }

            var resolved = TryGetSystemType(raw) ?? MapDbSpecificTypeName(raw);
            return resolved ?? typeof(object);
        }

        static Type TryGetSystemType(string fullName)
        {
            // Try without assembly (works for many in current AppDomain)
            var t = Type.GetType(fullName);
            if (t != null) return t;

            // .NET Framework core
            t = Type.GetType($"{fullName}, mscorlib");
            if (t != null) return t;

            // .NET Core / .NET 6 core
            t = Type.GetType($"{fullName}, System.Private.CoreLib");
            if (t != null) return t;

            // Common primitive aliases
            switch (fullName)
            {
                case "string": return typeof(string);
                case "bool":
                case "boolean": return typeof(bool);
                case "byte": return typeof(byte);
                case "short":
                case "Int16": return typeof(short);
                case "int":
                case "Int32": return typeof(int);
                case "long":
                case "Int64": return typeof(long);
                case "float":
                case "Single": return typeof(float);
                case "double":
                case "Double": return typeof(double);
                case "decimal": return typeof(decimal);
                case "Guid":
                case "System.Guid": return typeof(Guid);
                case "DateTime": return typeof(DateTime);
                case "TimeSpan": return typeof(TimeSpan);
            }

            return null;
        }

        static Type MapDbSpecificTypeName(string name)
        {
            // Normalize DB type names (various providers)
            var upper = name.Trim().ToUpperInvariant();

            switch (upper)
            {
                // Character / text
                case "CHAR":
                case "NCHAR":
                case "VARCHAR":
                case "NVARCHAR":
                case "VARCHAR2":
                case "NVARCHAR2":
                case "TEXT":
                case "NTEXT":
                case "CLOB":
                case "NCLOB":
                case "XML":
                case "JSON":
                case "UUID": // treat UUID as Guid elsewhere
                    if (upper == "UUID") return typeof(Guid);
                    return typeof(string);

                // Integer family
                case "INT":
                case "INTEGER":
                case "INT4":
                case "MEDIUMINT":
                case "SMALLINT":
                case "INT2":
                case "NUMBER(10)": // example Oracle numeric mapping
                    return typeof(int);
                case "BIGINT":
                case "INT8":
                    return typeof(long);
                case "TINYINT":
                    return typeof(byte);

                // Decimal / numeric
                case "DECIMAL":
                case "NUMERIC":
                case "NUMBER":
                case "MONEY":
                case "SMALLMONEY":
                    return typeof(decimal);

                // Floating point
                case "FLOAT":
                case "REAL":
                case "DOUBLE":
                case "DOUBLE PRECISION":
                    return typeof(double);

                // Date / time
                case "DATE":
                case "DATETIME":
                case "SMALLDATETIME":
                case "TIMESTAMP":
                case "TIMESTAMPTZ":
                case "DATETIME2":
                case "DATETIMEOFFSET":
                    return typeof(DateTime);
                case "TIME":
                case "TIMETZ":
                    return typeof(TimeSpan);

                // Boolean
                case "BIT":
                case "BOOLEAN":
                case "BOOL":
                    return typeof(bool);

                // GUID
                case "UNIQUEIDENTIFIER":
                    return typeof(Guid);

                // Binary / blob
                case "BLOB":
                case "BYTEA":
                case "VARBINARY":
                case "BINARY":
                case "IMAGE":
                case "ROWVERSION":
                    return typeof(byte[]);

                default:
                    return null;
            }
        }
    }
}
