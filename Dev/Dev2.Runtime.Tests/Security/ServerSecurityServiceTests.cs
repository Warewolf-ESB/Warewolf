
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
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Security
{
    [TestClass]
    [Ignore] // These fail intermittently when using mstest runner???
    public class ServerSecurityServiceTests
    {
        static void WaitForEvents()
        {
            Thread.Sleep(2000); // wait for event to fire
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerSecurityService_Constructor")]
        public void ServerSecurityService_Constructor_FileSystemWatcher_ChangedEventWiredUp()
        {
            //------------Setup for test--------------------------
            var fileName = string.Format("secure_{0}.config", Guid.NewGuid());
            File.WriteAllText(fileName, @"xxx");

            var serverSecurityService = new TestServerSecurityService(fileName);

            //------------Execute Test---------------------------
            File.WriteAllText(fileName, @"ssss");
            WaitForEvents();

            //------------Assert Results-------------------------

            // FileSystemWatcher may raise this event twice if we don't disable RaisingEvents!!
            Assert.IsTrue(serverSecurityService.OnFileChangedHitCount > 0);

            serverSecurityService.Dispose();
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerSecurityService_Constructor")]
        public void ServerSecurityService_Constructor_FileSystemWatcher_CreatedEventWiredUp()
        {
            //------------Setup for test--------------------------
            var fileName = string.Format("secure_{0}.config", Guid.NewGuid());

            var serverSecurityService = new TestServerSecurityService(fileName);

            //------------Execute Test---------------------------
            File.WriteAllText(fileName, @"xxx");
            WaitForEvents();

            //------------Assert Results-------------------------
            Assert.IsTrue(serverSecurityService.OnFileChangedHitCount > 0);

            serverSecurityService.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerSecurityService_Constructor")]
        public void ServerSecurityService_Constructor_FileSystemWatcher_DeletedEventWiredUp()
        {
            //------------Setup for test--------------------------
            var fileName = string.Format("secure_{0}.config", Guid.NewGuid());
            File.WriteAllText(fileName, @"xxx");

            var serverSecurityService = new TestServerSecurityService(fileName);

            //------------Execute Test---------------------------
            File.Delete(fileName);
            WaitForEvents();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, serverSecurityService.OnFileChangedHitCount);

            serverSecurityService.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerSecurityService_Constructor")]
        public void ServerSecurityService_Constructor_FileSystemWatcher_RenamedEventWiredUp()
        {
            //------------Setup for test--------------------------
            var fileName1 = string.Format("secure_{0}.config", Guid.NewGuid());
            var fileName2 = string.Format("secure_{0}.config", Guid.NewGuid());
            File.WriteAllText(fileName1, @"xxx");

            var serverSecurityService = new TestServerSecurityService(fileName1);

            //------------Execute Test---------------------------
            File.Move(fileName1, fileName2);
            WaitForEvents();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, serverSecurityService.OnFileRenamedHitCount);

            serverSecurityService.Dispose();
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerSecurityService_OnFileChanged")]
        public void ServerSecurityService_OnFileChanged_RaisingEvents_DisabledAndEnabled()
        {
            //------------Setup for test--------------------------
            var fileName = string.Format("secure_{0}.config", Guid.NewGuid());
            File.WriteAllText(fileName, @"xxx");

            var serverSecurityService = new TestServerSecurityService(fileName);

            //------------Execute Test---------------------------
            File.WriteAllText(fileName, @"ssss");
            WaitForEvents();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, serverSecurityService.OnFileChangedEnableRaisingEventsEnabled.Count);
            Assert.IsFalse(serverSecurityService.OnFileChangedEnableRaisingEventsEnabled[0]);
            Assert.IsTrue(serverSecurityService.OnFileChangedEnableRaisingEventsEnabled[1]);

            serverSecurityService.Dispose();
        }
    }
}
