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
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

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
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        readonly JsonSerializerSettings _deSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
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
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
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
                    PreserveReferencesHandling = _serializerSettings.PreserveReferencesHandling
                };
                using (var reader = new JsonTextReader(streamReader))
                {
                    var result = jsonSerializer.Deserialize<T>(reader);
                    return result;
                }
            }
        }
    }
}
