using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IConnectControlViewModel
    {
        IServer Server { get; set; }
        IList<IServer> Servers { get; set; }
        IServer SelectedConnection { get; set; }
        ICommand EditConnectionCommand { get; set; }
        ICommand ToggleConnectionStateCommand { get; set; }

        bool IsConnected { get;set; }
        bool IsConnecting { get; set; }
        string ToggleConnectionToolTip { get; }
        string EditConnectionToolTip { get; }
        string ConnectionsToolTip { get; }
        

        void Connect(IServer connection);

        void Disconnect(IServer connection);

        void Refresh();

        void Edit();
    }
}