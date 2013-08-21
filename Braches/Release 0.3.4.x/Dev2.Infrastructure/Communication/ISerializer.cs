using System;

namespace Dev2.Communication
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
    }
}