using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IManagePluginSourceModel
    {
        string ServerName { get; }
        IList<IDllListing> GetDllListings(IDllListing listing);
        void Save(IPluginSource toDbSource);
    }
}