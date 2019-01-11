/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using WarewolfCOMIPC.Client;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class ServerlifeCycleManagerTests
    {

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_OpenCOMStream_Fails()
        {
            //------------------------Arrange------------------------
            var mockWriter = new Mock<IWriter>();
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };
            //------------------------Act----------------------------
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Throws(new System.Exception("The system cannot find the file specified")).Verifiable(); 
            using (var serverLifeCylcleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object))
            {
                serverLifeCylcleManager.Run(items);
            }
            //------------------------Assert-------------------------
            mockSerLifeCycleWorker.Verify();
        }
        
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_IsServerOnline_True()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockDirectoryHelper = new Mock<IDirectoryHelper>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;
            
            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName() { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });
            //------------------------Act----------------------------
            using (var serverLifeCylcleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object, mockIpcClient.Object, mockAssemblyLoader.Object, mockDirectory.Object, mockResourceCatalogFactory.Object, mockDirectoryHelper.Object, mockWebServerConfiguration.Object, mockWriter.Object ,mockPauseHelper.Object, mockStartWebServer.Object))
            {
                serverLifeCylcleManager.Run(items);
            }
            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Press <ENTER> to terminate service and/or web server if started"), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockSerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_IsServerOnline_False()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockDirectoryHelper = new Mock<IDirectoryHelper>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };
            
            EnvironmentVariables.IsServerOnline = false;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName() { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });
            //------------------------Act----------------------------
            using (var serverLifeCylcleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object, mockIpcClient.Object, mockAssemblyLoader.Object, mockDirectory.Object, mockResourceCatalogFactory.Object, mockDirectoryHelper.Object, mockWebServerConfiguration.Object, mockWriter.Object, mockPauseHelper.Object, mockStartWebServer.Object))
            {
                serverLifeCylcleManager.Run(items);
            }
            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Press <ENTER> to terminate service and/or web server if started"), Times.Once);
            mockWriter.Verify(o => o.Write("Failed to start Server"), Times.Once);
            mockSerLifeCycleWorker.Verify();
        } 
    }
}
