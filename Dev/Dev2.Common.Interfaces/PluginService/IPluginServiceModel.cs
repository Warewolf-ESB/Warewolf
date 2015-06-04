using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.PluginService
{
    public interface IPluginServiceModel
    {
        ICollection<IPluginSource> RetrieveSources();
        ICollection<IPluginAction> GetActions(IPluginSource source);
        void CreateNewSource();
        void EditSource(IPluginSource selectedSource);
        string TestService(IPluginService inputValues);
        IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IPluginAction action);
        void SaveService(IPluginService model);
    }
}