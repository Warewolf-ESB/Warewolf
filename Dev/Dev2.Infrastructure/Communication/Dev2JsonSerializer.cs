using System;
using System.IO;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Newtonsoft.Json;

namespace Dev2.Communication
{
    /// <summary>
    /// A JSON implementation of an <see cref="ISerializer"/>
    /// </summary>
    public class Dev2JsonSerializer : ISerializer
    {
        public string Serialize<T>(T message)
        {
            VerifyArgument.IsNotNull("message", message);
            return JsonConvert.SerializeObject(message);
        }

        public T Deserialize<T>([NotNull] string message)
        {
            VerifyArgument.IsNotNull("message", message);
            return JsonConvert.DeserializeObject<T>(message);
        }

        public object Deserialize(string message, Type type)
        {
            VerifyArgument.IsNotNull("message", message);
            VerifyArgument.IsNotNull("type", type);
            return JsonConvert.DeserializeObject(message, type);
        }

        public StringBuilder SerializeToBuilder(object obj)
        {
            StringBuilder result = new StringBuilder();

            using(StringWriter sw = new StringWriter(result))
            {
                var jsonSerializer = new JsonSerializer();

                var jsonTextWriter = new JsonTextWriter(sw);
                jsonSerializer.Serialize(jsonTextWriter, obj);
                jsonTextWriter.Flush();
                jsonTextWriter.Close();
            }

            return result;
        }

        // Please use this for all your deserialize needs ;)
        public T Deserialize<T>(StringBuilder message)
        {
            if(message != null && message.Length > 0)
            {
                JsonSerializer serializer = new JsonSerializer();
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
                            JsonReader jr = new JsonTextReader(sr);
                            var result = serializer.Deserialize<T>(jr);
                            return result;
                        }
                    }
                    catch
                    {
                        // Do nothing default(T) returned below ;)
                    }
                }

            }

            return default(T);
        }

    }
}