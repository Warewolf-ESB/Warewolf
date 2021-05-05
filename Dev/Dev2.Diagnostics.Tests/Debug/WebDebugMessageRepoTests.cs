/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Diagnostics.Test.Debug
{
    [TestClass]
    public class WebDebugMessageRepoTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebDebugMessageRepo))]
        public void WebDebugMessageRepo_Constructor_IsNew_ShouldInitialise_OnlyOnce()
        {
            //---------------Arrange-------------------
            //---------------Act------------------------
            //---------------Assert---------------------
            Parallel.Invoke(()=> { Assert.IsNotNull(WebDebugMessageRepo.Instance); },
                            () => { Assert.IsNotNull(WebDebugMessageRepo.Instance); },
                            () => { Assert.IsNotNull(WebDebugMessageRepo.Instance); }
                            );
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebDebugMessageRepo))]
        public void WebDebugMessageRepo_AddDebugItem_ValidArgs_ShouldAdd1Item()
        {
            //---------------Set up test pack-------------------
            var mockDebugState = new Mock<IDebugState>();

            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            var webDebugMessageRepo = WebDebugMessageRepo.Instance;
            //---------------Execute Test ----------------------
            webDebugMessageRepo.AddDebugItem(clientId, sessionId, mockDebugState.Object);
            //---------------Test Result -----------------------
            var fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId, sessionId);
            Assert.AreEqual(1, fetchDebugItems.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebDebugMessageRepo))]
        public void WebDebugMessageRepo_AddDebugItem_ValidArgsAndSessions_ShouldNotMixUpDebugStates()
        {
            //---------------Set up test pack-------------------
            var mockDebugState = new Mock<IDebugState>();
            var mockDebugState1 = new Mock<IDebugState>();

            var clientId = Guid.NewGuid();
            var clientId1 = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var sessionId1 = Guid.NewGuid();

            var webDebugMessageRepo = WebDebugMessageRepo.Instance;
            //---------------Execute Test ----------------------
            Parallel.Invoke(() => { webDebugMessageRepo.AddDebugItem(clientId, sessionId, mockDebugState.Object); },
                           () => { webDebugMessageRepo.AddDebugItem(clientId1, sessionId1, mockDebugState1.Object); }
                           );

            var fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId, sessionId);
            Assert.AreEqual(1, fetchDebugItems.Count);

            fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId1, sessionId1);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, fetchDebugItems.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebDebugMessageRepo))]
        public void WebDebugMessageRepo_AddDebugItem_KeyExists_ShouldNotMixUpDebugStates()
        {
            //---------------Set up test pack-------------------
            var mockDebugState = new Mock<IDebugState>();
            var mockDebugState1 = new Mock<IDebugState>();

            var clientId = Guid.NewGuid();
            var clientId1 = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var sessionId1 = Guid.NewGuid();

            var webDebugMessageRepo = WebDebugMessageRepo.Instance;
            //---------------Execute Test ----------------------
            Parallel.Invoke(() => { webDebugMessageRepo.AddDebugItem(clientId, sessionId, mockDebugState.Object); },
                            () => { webDebugMessageRepo.AddDebugItem(clientId1, sessionId1, mockDebugState1.Object); },
                            () => { webDebugMessageRepo.AddDebugItem(clientId1, sessionId1, mockDebugState1.Object); }
                            );

            var fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId, sessionId);
            var fetchDebugItems1 = webDebugMessageRepo.FetchDebugItems(clientId1, sessionId1);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, fetchDebugItems.Count);
            Assert.AreEqual(2, fetchDebugItems1.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebDebugMessageRepo))]
        public void WebDebugMessageRepo_AddDebugItem_StateTypeNone_ParentID_IsNull_ExpectTrue()
        {
            //---------------Set up test pack-------------------
            var mockDebugState = new Mock<IDebugState>();

            mockDebugState.Setup(o => o.StateType).Returns(StateType.None);
            mockDebugState.Setup(o => o.ParentID).Returns(default(Guid));

            var webDebugMessageRepo = WebDebugMessageRepo.Instance;
            //---------------Execute Test ----------------------
            webDebugMessageRepo.AddDebugItem(Guid.NewGuid(), Guid.NewGuid(), mockDebugState.Object);
            //---------------Test Result -----------------------
            mockDebugState.VerifySet(o => o.ParentID = null);
        }
    }
}
