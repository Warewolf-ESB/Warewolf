using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

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

    public interface IWebServiceModel
    {

        ICollection<IWebServiceSource> RetrieveSources();
       
        void CreateNewSource();
        void EditSource(IWebServiceSource selectedSource);
        string TestService(IWebServiceSource inputValues);
        IEnumerable<IDbOutputMapping> GetDbOutputMappings(IDbAction action);
        void SaveService(IWebService toModel);
    }
    public interface IWebserviceInputs
    {
    }

    public interface IWebserviceOutputs
    {
    }
}