
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
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Core.Tests.Environments;
using Dev2.Diagnostics.Debug;
using Dev2.Factory;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.AppResources.Comparers
{
    /// <summary>
    /// Summary description for WorkSurfaceKeyEqualityComparerTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkSurfaceKeyEqualityComparerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            AppSettings.LocalHost = "http://localhost:3142";
        }
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CreateKeysExpectedKeysCreated()
        {
            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var enviroId = Guid.NewGuid();

            var resource1 = Dev2MockFactory.SetupResourceModelMock();
            resource1.Setup(c => c.ID).Returns(resId);
            resource1.Setup(c => c.ServerID).Returns(serverId);
            resource1.Setup(c => c.Environment.ID).Returns(enviroId);

            var key1 = WorkSurfaceKeyFactory.CreateKey(resource1.Object);

            var enviroId2 = Guid.NewGuid();

            var resource2 = Dev2MockFactory.SetupResourceModelMock();
            resource2.Setup(c => c.ID).Returns(resId);
            resource2.Setup(c => c.ServerID).Returns(serverId);
            resource2.Setup(c => c.Environment.ID).Returns(enviroId2);

            var key2 = WorkSurfaceKeyFactory.CreateKey(resource2.Object);
            if(WorkSurfaceKeyEqualityComparer.Current.Equals(key1, key2))
            {
                Assert.Fail("The keys should not be the same as they are from two different environments.");
            }
        }

        [TestMethod]
        public void CreateKeysWithDebugStateExpectedKeysCreatedWhenNoWorkspaceID()
        {
            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();


            var debugState = new Mock<IDebugState>();
            debugState.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState.Setup(c => c.ServerID).Returns(serverId);
            debugState.Setup(c => c.WorkspaceID).Returns(Guid.Empty);

            var key1 = WorkSurfaceKeyFactory.CreateKey(debugState.Object);


            var debugState2 = new Mock<IDebugState>();
            debugState2.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState2.Setup(c => c.ServerID).Returns(serverId);
            debugState2.Setup(c => c.WorkspaceID).Returns(Guid.Empty);

            var key2 = WorkSurfaceKeyFactory.CreateKey(debugState2.Object);
            Assert.IsTrue(WorkSurfaceKeyEqualityComparer.Current.Equals(key1, key2), "keys should be equal");

        }

        [TestMethod]
        public void CreateKeysWithDebugStateExpectedKeysCreatedWhenHasWorkspaceID()
        {
            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var enviroId = Guid.NewGuid();
            var enviroId2 = Guid.NewGuid();

            var source = new Mock<IEnvironmentModel>();
            var sourceConnection = new Mock<IEnvironmentConnection>();
            sourceConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid);
            source.Setup(model => model.Connection).Returns(sourceConnection.Object);
            var e1 = new Mock<IEnvironmentModel>();
            e1.Setup(model => model.ID).Returns(Guid.NewGuid);
            var connection1 = new Mock<IEnvironmentConnection>();
            connection1.Setup(connection => connection.WorkspaceID).Returns(enviroId);
            e1.Setup(model => model.Connection).Returns(connection1.Object);
            var e2 = new Mock<IEnvironmentModel>();
            e2.Setup(model => model.ID).Returns(Guid.NewGuid);
            var connection2 = new Mock<IEnvironmentConnection>();
            connection2.Setup(connection => connection.WorkspaceID).Returns(enviroId2);
            e2.Setup(model => model.Connection).Returns(connection2.Object);
            var repo = new TestLoadEnvironmentRespository(source.Object, e1.Object, e2.Object);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(repo);
            // ReSharper restore ObjectCreationAsStatement
            var debugState = new Mock<IDebugState>();
            debugState.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState.Setup(c => c.ServerID).Returns(serverId);
            debugState.Setup(c => c.WorkspaceID).Returns(enviroId);

            var key1 = WorkSurfaceKeyFactory.CreateKey(debugState.Object);



            var debugState2 = new Mock<IDebugState>();
            debugState2.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState2.Setup(c => c.ServerID).Returns(serverId);
            debugState2.Setup(c => c.WorkspaceID).Returns(enviroId2);

            var key2 = WorkSurfaceKeyFactory.CreateKey(debugState2.Object);
            Assert.IsFalse(WorkSurfaceKeyEqualityComparer.Current.Equals(key1, key2), "keys should not be equal");
        }
    }
}
