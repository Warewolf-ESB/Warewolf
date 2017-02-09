using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IManagePluginSourceModel
    {
        string ServerName { get; }
        IList<IFileListing> GetDllListings(IFileListing listing);
        void Save(IPluginSource toDbSource);
        IPluginSource FetchSource(Guid resourceID);
    }
}
