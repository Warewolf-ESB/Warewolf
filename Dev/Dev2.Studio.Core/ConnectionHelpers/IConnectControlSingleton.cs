
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        event EventHandler<ConnectionStatusChangedEventArg> ConnectedStatusChanged;
        event EventHandler<ConnectedServerChangedEvent> ConnectedServerChanged;
        ObservableCollection<IConnectControlEnvironment> Servers { get; set; }
        void Remove(Guid environmentId);
    }
}
