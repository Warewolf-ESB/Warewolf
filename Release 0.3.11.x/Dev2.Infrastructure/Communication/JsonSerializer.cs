using System;
using System.IO;
using Newtonsoft.Json;

namespace Dev2.Communication
{
    /// <summary>
    /// A JSON implementation of an <see cref="ISerializer"/>
    /// </summary>
    public class JsonSerializer : ISerializer
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
    }
}