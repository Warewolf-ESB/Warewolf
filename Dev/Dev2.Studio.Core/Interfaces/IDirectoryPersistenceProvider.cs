
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    /// <summary>
    /// Defines the requirements for a persistence provider that 
    /// performs all operations relative to a common directory.
    /// </summary>
    public interface IDirectoryPersistenceProvider : IFilePersistenceProvider
    {
        /// <summary>
        /// Gets or sets the directory path.
        /// </summary>
        string DirectoryPath
        {
            get;
            set;
        }

        /// <summary>
        ///  Gets an enumeration of all content from the files in <see cref="DirectoryPath"/>.
        /// </summary>
        /// <returns>An enumeration of all content from the files in <see cref="DirectoryPath"/></returns>
        IEnumerable<string> Read();
    }
}
