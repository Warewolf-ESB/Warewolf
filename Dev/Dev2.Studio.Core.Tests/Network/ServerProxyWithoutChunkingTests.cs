/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Markup;
using Dev2.SignalR.Wrappers;
using Warewolf.UnitTestAttributes;

namespace Dev2.Core.Tests.Network
{
    [TestClass]
    public class ServerProxyWithoutChunkingTests
    {
        [TestMethod]
        [Timeout(16000)]
        public void ServerProxyWithoutChunking_GivenServerAvailable_ExpectConnected()
        {
            var depend = new Depends(Depends.ContainerType.Warewolf);
            var host = $"http://{depend.Container.IP}:{depend.Container.Port}";

            var proxy = new ServerProxyWithoutChunking(new Uri(host));
            Assert.AreEqual(ConnState.Disconnected, proxy.StateController.Current);
            Assert.AreEqual(ConnState.Disconnected, proxy.State);
            proxy.Connect(Guid.NewGuid());
            while (proxy.HubConnection.State != SignalR.Wrappers.ConnectionStateWrapped.Connected) { System.Threading.Thread.Sleep(1000); }
            Assert.AreEqual(ConnState.Connected, proxy.StateController.Current);
            Assert.AreEqual(ConnState.Connected, proxy.State);
        }


        [TestMethod]
        [Timeout(16000)]
        public void ServerProxyWithoutChunking_GivenServerAvailable_AndConnect_WhenDisconnectRequest_ExpectDisconnect()
        {
            var proxy = new ServerProxyWithoutChunking(new Uri("http://localhost:3142"));
            Assert.AreEqual(ConnState.Disconnected, proxy.StateController.Current);
            Assert.AreEqual(ConnState.Disconnected, proxy.State);
            proxy.Connect(Guid.NewGuid());
            while (proxy.HubConnection.State != SignalR.Wrappers.ConnectionStateWrapped.Connected) { System.Threading.Thread.Sleep(1000); }
            Assert.AreEqual(ConnState.Connected, proxy.StateController.Current);
            Assert.AreEqual(ConnState.Connected, proxy.State);

            proxy.Disconnect();
            Assert.AreEqual(ConnState.Disconnected, proxy.StateController.Current);
            Assert.AreEqual(ConnState.Disconnected, proxy.State);
        }
    }
}
