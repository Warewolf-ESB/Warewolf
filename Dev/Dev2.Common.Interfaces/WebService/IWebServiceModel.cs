using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebServices;

namespace Dev2.Common.Interfaces.WebService
{
    public interface IWebServiceModel
    {

        ICollection<IWebServiceSource> RetrieveSources();
        void CreateNewSource();
        void EditSource(IWebServiceSource selectedSource);
        string TestService(IWebService inputValues);
        void SaveService(IWebService toModel);

        IStudioUpdateManager UpdateRepository { get; }
        IQueryManager QueryProxy { get; }
        ObservableCollection<IWebServiceSource> Sources { get; }

        string HandlePasteResponse(string current);
    }
}