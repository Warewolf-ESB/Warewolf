
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Providers.Events;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for DeployServiceTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DeployServiceTest
    {

        #region Test Variables

        int _numModels = 3;

        #endregion Test Variables

        #region Deploy Tests

        [TestMethod]
        public void DeployToEnvironmentWithZeroModels()
        {
            _numModels = 0;
            Run();
        }

        [TestMethod]
        public void DeployToEnvironmentWithMultipleModels()
        {
            _numModels = 3;
            Run();
        }

        [TestMethod]
        public void DeployToEnvironmentWithNullModels()
        {
            _numModels = -1;
            Run();
        }

        #endregion Deploy Tests

        #region Run

        void Run()
        {
            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);
            connection.Setup(e => e.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());

            var envMock = new Mock<IEnvironmentModel>();
            envMock.Setup(e => e.Connection).Returns(connection.Object);
            envMock.Setup(e => e.ResourceRepository.DeployResource(It.IsAny<IResourceModel>())).Verifiable();
            envMock.Setup(e => e.IsConnected).Returns(true);

            var dtoMock = new Mock<IDeployDto>();
            dtoMock.Setup(d => d.ResourceModels).Returns(CreateModels(envMock.Object));

            var ds = new DeployService();
            ds.Deploy(dtoMock.Object, envMock.Object);

            envMock.Verify(e => e.ResourceRepository.DeployResource(It.IsAny<IResourceModel>()), Times.Exactly(_numModels));
        }

        #endregion

        #region CreateModels

        IList<IResourceModel> CreateModels(IEnvironmentModel environment)
        {
            if(_numModels == -1)
            {
                _numModels = 0;
                return null;
            }

            var result = new List<IResourceModel>();

            for(var i = 0; i < _numModels; i++)
            {
                var moqRes = new Mock<ResourceModel>(environment);
                moqRes.Object.ResourceName = string.Format("Test{0}", i);
                result.Add(moqRes.Object);
            }
            return result;
        }

        #endregion

    }
}
