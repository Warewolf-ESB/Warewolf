/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Diagnostics.Test.Debug
{
    [TestClass]
    public class RemoteDebugMessageRepoTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RemoteDebugMessageRepo))]
        public void RemoteDebugMessageRepo_AddDebugItem_Invalid_Guid()
        {
            Guid.TryParse("test", out Guid remoteInvokeId);
            var mockDebugState = new Mock<IDebugState>();

            var instance = RemoteDebugMessageRepo.Instance;

            instance.AddDebugItem(remoteInvokeId.ToString(), mockDebugState.Object);
            var list = instance.FetchDebugItems(remoteInvokeId);

            Assert.IsNull(list);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RemoteDebugMessageRepo))]
        public void RemoteDebugMessageRepo_AddDebugItem()
        {
            var remoteInvokeId = Guid.NewGuid();
            var mockDebugState = new Mock<IDebugState>();

            var instance = RemoteDebugMessageRepo.Instance;

            instance.AddDebugItem(remoteInvokeId.ToString(), mockDebugState.Object);
            var list = instance.FetchDebugItems(remoteInvokeId);

            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RemoteDebugMessageRepo))]
        public void RemoteDebugMessageRepo_AddDebugItem_AddToList()
        {
            var remoteInvokeId = Guid.NewGuid();
            var mockDebugState = new Mock<IDebugState>();

            var instance = RemoteDebugMessageRepo.Instance;

            instance.AddDebugItem(remoteInvokeId.ToString(), mockDebugState.Object);
            instance.AddDebugItem(remoteInvokeId.ToString(), mockDebugState.Object);
            var list = instance.FetchDebugItems(remoteInvokeId);

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RemoteDebugMessageRepo))]
        public void RemoteDebugMessageRepo_AddDebugItem_Contains_Id()
        {
            var remoteInvokeId = Guid.NewGuid();

            var debugState = new DebugState { ID = remoteInvokeId };

            var instance = RemoteDebugMessageRepo.Instance;

            instance.AddDebugItem(remoteInvokeId.ToString(), debugState);
            instance.AddDebugItem(remoteInvokeId.ToString(), debugState);
            var list = instance.FetchDebugItems(remoteInvokeId);

            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RemoteDebugMessageRepo))]
        public void RemoteDebugMessageRepo_FetchDebugItems()
        {
            var remoteInvokeId = Guid.NewGuid();

            var instance = RemoteDebugMessageRepo.Instance;
            var list = instance.FetchDebugItems(remoteInvokeId);

            Assert.IsNull(list);
        }
    }
}
