using System;
using System.Collections.ObjectModel;

namespace Dev2.ConnectionHelpers
{
    public interface IConnectControlSingleton
    {
        void ToggleConnection(int selectedIndex);
        void ToggleConnection(Guid environmentId);
        void Refresh(Guid environmentId);
        void SetConnectionState(Guid environmentId, ConnectionEnumerations.ConnectedState connectedState);
        void EditConnection(int selectedIndex, Action<int> openWizard);
        ConnectEventHandlers.ConnectedStatusHandler ConnectedStatusChanged { get; set; }
        ConnectEventHandlers.ConnectedServerChangedHandler ConnectedServerChanged { get; set; }
        ObservableCollection<IConnectControlEnvironment> Servers { get; set; }
        void Remove(Guid environmentId);
    }
}