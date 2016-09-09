using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.Common.ExtMethods
{
    /// <summary>
    /// T Must be serializable
    /// </summary>
    public static class ObjectExtensions
    {
        public static T DeepCopy<T>(this T other) where T : new() 
        {
            try
            {
               
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, other);
                    ms.Position = 0;
                    return (T)formatter.Deserialize(ms);
                }
            }
                // ReSharper disable once UnusedVariable
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default(T);
            }

        }
    }
}
