
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.PathOperations.Interfaces
{
    /// <summary>
    /// Defines the requirements for a repository.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public interface IRepository<in TKey, TItem>
        where TItem : IRepositoryItem<TKey>
    {
        /// <summary>
        /// Gets the number of items in the repository.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the item with the specified key.
        /// </summary>
        /// <param name="key">The key to be queried.</param>
        /// <param name="force"><code>true</code> if the item should be re-read even it is found; <code>false</code> otherwise.</param>
        /// <returns>The item with specified key; or <code>null</code> if not found.</returns>
        TItem Get(TKey key, bool force = false);

        /// <summary>
        /// Saves the specified item to the repository.
        /// </summary>
        /// <param name="item">The item to be saved.</param>
        void Save(TItem item);

        /// <summary>
        /// Deletes the specified item from the repository.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        void Delete(TItem item);
    }
}
