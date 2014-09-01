using System;
using System.Text;

namespace Dev2.Common.Interfaces.Communication
{
    /// <summary>
    /// Describes a serializer
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        string Serialize<T>(T obj);

        /// <summary>
        /// Deserializes the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        T Deserialize<T>(string obj);

        /// <summary>
        /// Deserializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        object Deserialize(string obj, Type type);

        /// <summary>
        /// Deserializes the specified message via a stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        T Deserialize<T>(StringBuilder message);
    }
}