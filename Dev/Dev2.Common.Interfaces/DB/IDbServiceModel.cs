using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDbServiceModel {
        ICollection<IDbSource> RetrieveSources();
        ICollection<IDbAction> GetActions(IDbSource source); 
        void CreateNewSource();
        void EditSource(IDbSource selectedSource);
        DataTable TestService(IDatabaseService inputValues);
        IEnumerable<IDbOutputMapping> GetDbOutputMappings(IDbAction action);
        void SaveService(IDatabaseService toModel);
    }
}