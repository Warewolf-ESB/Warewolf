
using System.Runtime.Serialization;

namespace Dev2.PathOperations.Interfaces
{
    /// <summary>
    /// Defines the requirements for a repository item
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IRepositoryItem<TKey> : ISerializable
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        TKey Key { get; set; }
    }
}
