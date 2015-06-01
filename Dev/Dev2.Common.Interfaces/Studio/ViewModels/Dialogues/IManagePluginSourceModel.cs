using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IManagePluginSourceModel
    {
        string ServerName { get; }
        List<DllListing> GetDllListings();
        void Save(IPluginSource toDbSource);
    }
}