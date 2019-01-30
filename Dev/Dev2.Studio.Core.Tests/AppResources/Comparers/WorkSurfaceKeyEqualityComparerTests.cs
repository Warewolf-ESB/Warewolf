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
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Factory;
using Dev2.Studio.AppResources.Comparers;

namespace Dev2.Core.Tests.AppResources.Comparers
{
    [TestClass]
    public class WorkSurfaceKeyEqualityComparerTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
        }

        [TestMethod]
        public void CreateKeysExpectedKeysCreated()
        {
            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var enviroId = Guid.NewGuid();

            var resource1 = Dev2MockFactory.SetupResourceModelMock();
            resource1.Setup(c => c.ID).Returns(resId);
            resource1.Setup(c => c.ServerID).Returns(serverId);
            resource1.Setup(c => c.Environment.EnvironmentID).Returns(enviroId);

            var key1 = WorkSurfaceKeyFactory.CreateKey(resource1.Object);

            var enviroId2 = Guid.NewGuid();

            var resource2 = Dev2MockFactory.SetupResourceModelMock();
            resource2.Setup(c => c.ID).Returns(resId);
            resource2.Setup(c => c.ServerID).Returns(serverId);
            resource2.Setup(c => c.Environment.EnvironmentID).Returns(enviroId2);

            var key2 = WorkSurfaceKeyFactory.CreateKey(resource2.Object);
            if(WorkSurfaceKeyEqualityComparer.Current.Equals(key1, key2))
            {
                Assert.Fail("The keys should not be the same as they are from two different environments.");
            }
        }

        [TestMethod]
        public void CreateShowDependencyKeysExpectedKeysCreated()
        {
            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var enviroId = Guid.NewGuid();

            var key1 = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DependencyVisualiser) as WorkSurfaceKey;
            Assert.IsNotNull(key1);
            key1.EnvironmentID = enviroId;
            key1.ResourceID = resId;
            key1.ServerID = serverId;

            var resource2 = Dev2MockFactory.SetupResourceModelMock();
            resource2.Setup(c => c.ID).Returns(resId);
            resource2.Setup(c => c.ServerID).Returns(serverId);
            resource2.Setup(c => c.Environment.EnvironmentID).Returns(enviroId);

            var key2 = WorkSurfaceKeyFactory.CreateKey(resource2.Object);
            if (WorkSurfaceKeyEqualityComparerWithContextKey.Current.Equals(key1, key2))
            {
                Assert.Fail("The WorkSurfaceContext should not be the same.");
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

            var serverRepo = new Mock<IServerRepository>();
            
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(serverRepo.Object);
            var source = new Mock<IServer>();
            var sourceConnection = new Mock<IEnvironmentConnection>();
            sourceConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid);
            source.Setup(model => model.Connection).Returns(sourceConnection.Object);
            var e1 = new Mock<IServer>();
            e1.Setup(model => model.EnvironmentID).Returns(Guid.NewGuid);
            //serverRepo.Setup(repository => repository.FindSingle(server => ))
            //    .Returns(source.Object);
            var connection1 = new Mock<IEnvironmentConnection>();
            connection1.Setup(connection => connection.WorkspaceID).Returns(enviroId);
            e1.Setup(model => model.Connection).Returns(connection1.Object);
            var e2 = new Mock<IServer>();
            e2.Setup(model => model.EnvironmentID).Returns(Guid.NewGuid);
            var connection2 = new Mock<IEnvironmentConnection>();
            connection2.Setup(connection => connection.WorkspaceID).Returns(enviroId2);
            e2.Setup(model => model.Connection).Returns(connection2.Object);
            var repo = new TestLoadServerRespository(source.Object, e1.Object, e2.Object);
            
            new ServerRepository(repo);
            
            var debugState = new Mock<IDebugState>();
            debugState.Setup(c => c.OriginatingResourceID).Returns(resId);
            debugState.Setup(c => c.ServerID).Returns(serverId);
            debugState.Setup(c => c.WorkspaceID).Returns(enviroId);
            serverRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer,bool>>>()))
                .Returns(e1.Object);
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
