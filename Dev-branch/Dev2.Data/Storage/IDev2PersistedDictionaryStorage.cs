using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Storage
{
    public interface IDev2PersistedDictionaryStorage
    {
        /// <summary>
        /// Gets the index file path.
        /// </summary>
        /// <value>
        /// The index file path.
        /// </value>
        string IndexFilePath { get; }

        /// <summary>
        /// Gets the data file path.
        /// </summary>
        /// <value>
        /// The data file path.
        /// </value>
        string DataFilePath { get; }


        /// <summary>
        /// Used to sync data via a network ;)
        /// </summary>
        /// <param name="sendTo"></param>
        void SyncTo(NetworkContext sendTo);
    }
}
