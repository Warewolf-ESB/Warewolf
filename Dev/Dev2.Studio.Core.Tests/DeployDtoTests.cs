using System.Collections.Generic;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Core.Tests
{
    [TestClass]
    public class DeployDtoTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeployDto_GivenInstance_ShouldHaveEmptyValues()
        {
            //---------------Set up test pack-------------------
            var deployDto = new DeployDto();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deployDto);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(default(bool), deployDto.DeployTests);
            Assert.AreEqual(default(IList<ResourceModel>), deployDto.ResourceModels);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeployDto_GivenInstance_ShouldInheritFromIdeployDto()
        {
            //---------------Set up test pack-------------------
            var deployDto = new DeployDto();
            //---------------Assert Precondition----------------
            Assert.AreEqual(default(bool), deployDto.DeployTests);
            Assert.AreEqual(default(IList<ResourceModel>), deployDto.ResourceModels);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
                Assert.IsInstanceOfType(deployDto, typeof(IDeployDto));    
        }

      
    }
}
