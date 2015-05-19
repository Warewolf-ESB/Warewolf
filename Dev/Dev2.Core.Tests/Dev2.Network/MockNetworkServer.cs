
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
using System.Network;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    public class MockNetworkServer : TCPServer<MockNetworkContext>
    {
        public MockNetworkServer()
            : base("", null)
        {
        }

        protected override void OnExecuteCommand(MockNetworkContext context, ByteBuffer payload, Packet writer)
        {
            throw new NotImplementedException();
        }

        protected override string OnExecuteCommand(MockNetworkContext context, string payload, Guid dataListID)
        {
            throw new NotImplementedException();
        }
    }
}
