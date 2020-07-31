/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
        [TestCategory(nameof(DeployDto))]
        public void DeployDto_GivenInstance_ShouldHaveEmptyValues()
        {
            //---------------Set up test pack-------------------
            var deployDto = new DeployDto();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deployDto);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(default(bool), deployDto.DeployTests);
            Assert.AreEqual(default(bool), deployDto.DeployTriggers);
            Assert.AreEqual(default(IList<ResourceModel>), deployDto.ResourceModels);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DeployDto))]
        public void DeployDto_GivenInstance_ShouldInheritFromIDeployDto()
        {
            //---------------Set up test pack-------------------
            var deployDto = new DeployDto();
            //---------------Assert Precondition----------------
            Assert.AreEqual(default(bool), deployDto.DeployTests);
            Assert.AreEqual(default(bool), deployDto.DeployTriggers);
            Assert.AreEqual(default(IList<ResourceModel>), deployDto.ResourceModels);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsInstanceOfType(deployDto, typeof(IDeployDto));
        }
    }
}