using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IConnectControlViewModel
    {
        ObservableCollection<IServer> Servers { get; }
        IServer SelectedConnection { get; set; }
        ICommand EditConnectionCommand { get; }
        ICommand ToggleConnectionStateCommand { get; }
        bool AllowConnection { get; }
        bool IsConnected { get; }
        bool IsConnecting { get;  }
        bool IsLoading { get; set; }
        string ToggleConnectionToolTip { get; }
        string EditConnectionToolTip { get; }
        string ConnectionsToolTip { get; }
        EventHandler<IServer> ServerConnected { get; set; }
        EventHandler<IServer> ServerDisconnected { get; set; }
        EventHandler<IServer> ServerHasDisconnected { get; set; }
         EventHandler<IServer> ServerReConnected { get; set; }
        bool ShouldUpdateActiveEnvironment { get; set; }

        Task<bool> Connect(IServer connection);
        event SelectedServerChanged SelectedEnvironmentChanged;
        //void LoadNewServers();
        void LoadServers();
    }
}