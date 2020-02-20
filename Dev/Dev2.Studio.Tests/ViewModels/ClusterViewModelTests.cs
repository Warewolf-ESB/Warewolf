/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Settings.Clusters;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Options;

namespace Dev2.Studio.Tests.ViewModels
{
    [TestClass]
    public class ClusterViewModelTests
    {
        [TestMethod]
        public void ClusterViewModel_ServerOptions()
        {
            //-------------------------Arrange-----------------------
            var expectedId = Guid.NewGuid();
            const string expectedName = "ServerName";
            var mockServerSource = new Mock<IServerSource>();
            mockServerSource.Setup(o => o.ID).Returns(expectedId);
            mockServerSource.Setup(o => o.Name).Returns(expectedName);
            var expected = new List<IServerSource>
            {
                mockServerSource.Object,
            };
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o =>
                o.FindSourcesByType<IServerSource>(It.IsAny<IServer>(), It.IsAny<enSourceType>())).Returns(expected);
            
            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(mockServer.Object);
            
            CustomContainer.Register(mockShellViewModel.Object);
            
            //-------------------------Act---------------------------
            var serverOptions = new ServerOptions();
            var result = OptionConvertor.Convert(serverOptions);

            //-------------------------Assert------------------------
            Assert.IsNotNull(result);
            var optionSourceCombobox = result[0] as OptionSourceCombobox;
            Assert.IsNotNull(optionSourceCombobox);
            Assert.AreEqual("Leader", optionSourceCombobox.Name);
            Assert.AreEqual(expectedName, optionSourceCombobox.Options[0].Name);
            Assert.AreEqual(expectedId, optionSourceCombobox.Options[0].Value);
            
            mockResourceRepository.Verify(o => 
                o.FindSourcesByType<IServerSource>(It.IsAny<IServer>(), It.IsAny<enSourceType>()), Times.Exactly(1));
        }
        
    }
}