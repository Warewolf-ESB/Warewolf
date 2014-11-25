
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



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
