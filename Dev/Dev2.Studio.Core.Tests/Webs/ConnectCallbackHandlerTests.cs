
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
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Util;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Core.Tests.Webs
{

    // SN : 23-01-2013 : Missing Tests 
    // 
    // Saving with Invalid connection
    // Dev2 Set with invalid environment connection

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConnectCallbackHandlerTests
    {
        const string ConnectionId = "1478649D-CF54-4D0D-8E26-CA9B81454B66";
        const string ConnectionName = "TestConnection";
        const string ConnectionAddress = "http://RSAKLFBRANDONPA:77/dsf";
        const int ConnectionWebServerPort = 1234;

        // Keys are case-sensitive!
        static readonly string ConnectionJson =
            "{\"ResourceID\":\"" + ConnectionId +
            "\",\"ResourceName\":\"" + ConnectionName +
            "\",\"ResourcePath\":\"TEST\",\"ResourceType\":\"Server\",\"Address\":\"" + ConnectionAddress +
            "\",\"AuthenticationType\":\"Windows\",\"UserName\":\"\",\"Password\":\"\",\"WebServerPort\":" + ConnectionWebServerPort + "}";


        #region Class/TestInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            EventPublishers.Aggregator = null;
            AppSettings.LocalHost = "https://localhost:3143";
        }

        #endregion

        #region Save

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithNullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save(null, null);
            handler.Save(null, null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesSaveEnvironment()
        // ReSharper restore InconsistentNaming - Unit Tests
        {

            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockResourceRepo = new Mock<IResourceRepository>();
            var env = new EnvironmentModel(Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object, new Mock<IStudioResourceRepository>().Object);

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(env);
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(new Mock<IEnvironmentModel>().Object);

            var handler = new ConnectCallbackHandler(currentRepository.Object);
            handler.Save(ConnectionJson, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.IsAny<IEnvironmentModel>()));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidConnection_Expected_InvokesSaveEnvironmentWithCategory()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            // BUG: 8786 - TWR - 2013.02.20
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockResourceRepo = new Mock<IResourceRepository>();
            var enviro = new EnvironmentModel(Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object, new Mock<IStudioResourceRepository>().Object);

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(enviro);

            var env = new Mock<IEnvironmentModel>();
            env.SetupProperty(e => e.Category); // start tracking sets/gets to this property

            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(env.Object);

            var handler = new ConnectCallbackHandler(new Mock<IEventAggregator>().Object, currentRepository.Object);
            handler.Save(ConnectionJson, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.IsAny<IEnvironmentModel>()));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }


        #endregion

        #region Save

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithInvalidJson_Expected_ThrowsJsonReaderException()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var handler = new ConnectCallbackHandler();
            handler.Save("xxxxx", null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Tests
        public void Save_WithValidJson_Expected_InvokesSave()
        // ReSharper restore InconsistentNaming - Unit Tests
        {
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockResourceRepo = new Mock<IResourceRepository>();
            var enviro = new EnvironmentModel(Guid.NewGuid(), mockConnection.Object, mockResourceRepo.Object, new Mock<IStudioResourceRepository>().Object);

            var currentRepository = new Mock<IEnvironmentRepository>();
            currentRepository.Setup(e => e.Save(It.IsAny<IEnvironmentModel>())).Verifiable();
            currentRepository.Setup(e => e.Fetch(It.IsAny<IEnvironmentModel>())).Returns(new Mock<IEnvironmentModel>().Object);
            currentRepository.Setup(c => c.ActiveEnvironment).Returns(enviro);

            var handler = new ConnectCallbackHandler(new Mock<IEventAggregator>().Object, currentRepository.Object);
            handler.Save(ConnectionJson, null);

            // ReSharper disable PossibleUnintendedReferenceComparison - expected to be the same instance
            currentRepository.Verify(r => r.Save(It.IsAny<IEnvironmentModel>()));
            // ReSharper restore PossibleUnintendedReferenceComparison
        }

        #endregion

    }
}
