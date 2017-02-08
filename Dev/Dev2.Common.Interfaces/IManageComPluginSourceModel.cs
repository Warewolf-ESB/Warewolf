using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IManageComPluginSourceModel
    {
        string ServerName { get; }
        IList<IFileListing> GetComDllListings(IFileListing listing);
        void Save(IComPluginSource toDbSource);

        IComPluginSource FetchSource(Guid pluginSourceId);
    }
}