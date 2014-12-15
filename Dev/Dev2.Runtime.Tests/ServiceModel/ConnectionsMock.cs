
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
using System.Collections.Generic;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class ConnectionsMock : Connections
    {
        public ConnectionsMock(Func<string> connectToServerResult)
            : this(() => new List<string> { "RSAKLFSVRGENDEV", "RSAKLFSVRTFSBLD" }, connectToServerResult)
        {
        }

        public ConnectionsMock(Func<List<string>> fetchComputersFn, Func<string> connectToServerResult)
            : base(fetchComputersFn)
        {
            ConnectToServerResult = connectToServerResult;
        }

        public Func<string> ConnectToServerResult { get; private set; }
        protected override string ConnectToServer(Connection connection)
        {
            return ConnectToServerResult == null ? null : ConnectToServerResult();
        }
    }
}
