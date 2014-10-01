
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Communication;
using Newtonsoft.Json;

namespace Dev2.Communication
{
    /// <summary>
    /// A JSON implementation of an <see cref="ISerializer"/>
    /// </summary>
    public class Dev2JsonSerializer : ISerializer
    {
// ReSharper disable RedundantNameQualifier
        const Formatting Formatting = Newtonsoft.Json.Formatting.Indented;
// ReSharper restore RedundantNameQualifier
        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };
        readonly JsonSerializerSettings _deSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
        public string Serialize<T>(T message)
        {
            VerifyArgument.IsNotNull("message", message);
            return JsonConvert.SerializeObject(message, Formatting, _serializerSettings);
        }

        public T Deserialize<T>([NotNull] string message)
        {
            VerifyArgument.IsNotNull("message", message);
            return JsonConvert.DeserializeObject<T>(message, _deSerializerSettings);
        }

        public object Deserialize(string message, Type type)
        {
            VerifyArgument.IsNotNull("message", message);
            VerifyArgument.IsNotNull("type", type);
            return JsonConvert.DeserializeObject(message, type, _deSerializerSettings);
        }

        public StringBuilder SerializeToBuilder(object obj)
        {
            StringBuilder result = new StringBuilder();

            using(StringWriter sw = new StringWriter(result))
            {
                var jsonSerializer = new JsonSerializer { TypeNameHandling = _serializerSettings.TypeNameHandling, TypeNameAssemblyFormat = _serializerSettings.TypeNameAssemblyFormat };
                using(var jsonTextWriter = new JsonTextWriter(sw))
                {
                    jsonSerializer.Serialize(jsonTextWriter, obj);
                    jsonTextWriter.Flush();
                    jsonTextWriter.Close();
                }
            }

            return result;
        }

        // Please use this for all your deserialize needs ;)
        public T Deserialize<T>(StringBuilder message)
        {
            if(message != null && message.Length > 0)
            {
                JsonSerializer serializer = new JsonSerializer { TypeNameHandling = _deSerializerSettings.TypeNameHandling };
                using(MemoryStream ms = new MemoryStream(message.Length))
                {
                    // now load the stream ;)

                    var length = message.Length;
                    var startIdx = 0;
                    var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);


                    for(int i = 0; i < rounds; i++)
                    {
                        var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                        if(len > (message.Length - startIdx))
                        {
                            len = (message.Length - startIdx);
                        }

                        var bytes = Encoding.UTF8.GetBytes(message.Substring(startIdx, len));
                        ms.Write(bytes, 0, bytes.Length);
                        startIdx += len;
                    }

                    // rewind
                    ms.Flush();
                    ms.Position = 0;

                    try
                    {
                        // finally do the conversion ;)
                        using(StreamReader sr = new StreamReader(ms))
                        {
                            using(JsonReader jr = new JsonTextReader(sr))
                            {
                                var result = serializer.Deserialize<T>(jr);
                                return result;
                            }
                        }
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        // Do nothing default(T) returned below ;)
                    }
                }

            }

            return default(T);
        }

    }
}
