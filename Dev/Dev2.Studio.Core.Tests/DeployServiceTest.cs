using System;
using System.Collections.Generic;
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
    public class DeployServiceTest {

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
            var envMock = new Mock<IEnvironmentModel>();
            envMock.Setup(e => e.Resources.DeployResource(It.IsAny<IResourceModel>())).Verifiable();
            envMock.Setup(e => e.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            envMock.Setup(e => e.IsConnected).Returns(true);

            var dtoMock = new Mock<IDeployDTO>();
            dtoMock.Setup(d => d.ResourceModels).Returns(CreateModels(envMock.Object));

            var ds = new DeployService();
            ds.Deploy(dtoMock.Object, envMock.Object);

            envMock.Verify(e => e.Resources.DeployResource(It.IsAny<IResourceModel>()), Times.Exactly(_numModels));
        } 

        #endregion

        #region CreateModels

        IList<IResourceModel> CreateModels(IEnvironmentModel environment)
        {
            if (_numModels == -1)
            {
                _numModels = 0;
                return null;
            }

            var result = new List<IResourceModel>();
            for (var i = 0; i < _numModels; i++)
            {
                result.Add(new ResourceModel(environment) { ResourceName = string.Format("Test{0}", i) });
            }
            return result;
        } 

        #endregion

    }
}
