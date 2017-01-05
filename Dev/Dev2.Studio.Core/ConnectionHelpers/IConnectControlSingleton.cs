/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.ConnectionHelpers
{
    public interface IConnectControlSingleton
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void EditConnection(int selectedIndex, Action<int> openWizard);
        void ToggleConnection(int selectedIndex);
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void ToggleConnection(Guid environmentId);
        void Refresh(Guid environmentId);
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void SetConnectionState(Guid environmentId, ConnectionEnumerations.ConnectedState connectedState);
        event EventHandler<ConnectionStatusChangedEventArg> ConnectedStatusChanged;
        event EventHandler<ConnectedServerChangedEvent> ConnectedServerChanged;
        ObservableCollection<IConnectControlEnvironment> Servers { get; set; }
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Remove(Guid environmentId);

        void ReloadServer();

        event EventHandler<ConnectedServerChangedEvent> AfterReload;
    }
}
