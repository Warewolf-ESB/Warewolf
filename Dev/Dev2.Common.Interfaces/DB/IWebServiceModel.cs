using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces.DB
{
    public interface IWebServiceModel
    {

        ICollection<IWebServiceSource> RetrieveSources();
        void CreateNewSource();
        void EditSource(IWebServiceSource selectedSource);
        string TestService(IWebService inputValues);
        void SaveService(IWebService toModel);
    }
}