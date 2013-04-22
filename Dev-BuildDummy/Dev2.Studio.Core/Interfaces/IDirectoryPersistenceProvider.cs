using System.Collections.Generic;

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
