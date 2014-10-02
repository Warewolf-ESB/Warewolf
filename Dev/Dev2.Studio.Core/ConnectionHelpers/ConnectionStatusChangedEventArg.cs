
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

namespace Dev2.ConnectionHelpers
{
    public class ConnectionStatusChangedEventArg : EventArgs
    {
        public ConnectionEnumerations.ConnectedState ConnectedStatus { get; private set; }
        public Guid EnvironmentId { get; private set; }
        public bool DoCallback { get; private set; }

        public ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState connectedStatus, Guid environmentId, bool doCallback)
        {
            ConnectedStatus = connectedStatus;
            EnvironmentId = environmentId;
            DoCallback = doCallback;
        }
    }
}
    
