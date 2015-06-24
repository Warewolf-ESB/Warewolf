
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net.Sockets;
using System.Network;
using Moq;

namespace Dev2.DynamicServices.Test
{
    public abstract class MockConnection : Connection
    {
        protected MockConnection()
            : base(new Mock<MockNetworkHost>().Object, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), NetworkDirection.Bidirectional)
        {

        }

        public int SendHitCount { get; set; }

        #region Overrides of Connection

        public override void Send(Packet p)
        {
            SendHitCount++;
        }


        #endregion
    }
}
