using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dev2.Studio.Interfaces
{
    public interface IConnectControlViewModel
    {
        ObservableCollection<IServer> Servers { get; }
        IServer SelectedConnection { get; set; }
        ICommand EditConnectionCommand { get; }
        ICommand NewConnectionCommand { get; }
        bool IsConnected { get; }
        bool IsConnecting { get;  }
        bool IsLoading { get; set; }
        string NewConnectionToolTip { get; }
        string EditConnectionToolTip { get; }
        string ConnectionsToolTip { get; }
        EventHandler<IServer> ServerConnected { get; set; }
        EventHandler<IServer> ServerDisconnected { get; set; }
        EventHandler<IServer> ServerHasDisconnected { get; set; }
         EventHandler<IServer> ServerReConnected { get; set; }
        bool ShouldUpdateActiveEnvironment { get; set; }
        bool CanEditServer { get; set; }
        bool CanCreateServer { get; set; }

        Task<bool> Connect(IServer connection);
        event SelectedServerChanged SelectedEnvironmentChanged;
        void LoadServers();
    }
}