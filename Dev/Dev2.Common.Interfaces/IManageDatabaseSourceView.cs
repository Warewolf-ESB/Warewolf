using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IManageDatabaseSourceView:IView
    {
        void EnterServerName(string serverName);
    }
}