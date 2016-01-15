using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IConnectControlViewModel
    {
        IServer Server { get; set; }
        ObservableCollection<IServer> Servers { get; set; }
        IServer SelectedConnection { get; set; }
        ICommand EditConnectionCommand { get; set; }
        ICommand ToggleConnectionStateCommand { get; set; }

        bool AllowConnection { get; set; }
        bool IsConnected { get;set; }
        bool IsConnecting { get; set; }
        bool IsLoading { get; set; }
        string ToggleConnectionToolTip { get; }
        string EditConnectionToolTip { get; }
        string ConnectionsToolTip { get; }
        EventHandler<IServer> ServerConnected { get; set; }
        EventHandler<IServer> ServerDisconnected { get; set; }
        EventHandler<IServer> ServerHasDisconnected { get; set; }
         EventHandler<IServer> ServerReConnected { get; set; }
        Task<bool> Connect(IServer connection);

        void Disconnect(IServer connection);

        void Refresh();

        void Edit();

        event SelectedServerChanged SelectedEnvironmentChanged;

        void ConnectOrDisconnect();

        void LoadNewServers();
    }
}