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
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Redis;
using Dev2.Activities.Redis;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.Redis
{
    [TestClass]
    public class RedisDesignerViewModelTests
    {
        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new RedisActivity());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisDesignerViewModel_Constructor_ModelItemIsValid_Null_EnvironmentModel()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            _ = new RedisDesignerViewModel(CreateModelItem(), null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisDesignerViewModel_Constructor_ModelItemIsValid_Null_ShellViewModel()
        {
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            //------------Execute Test---------------------------
            _ = new RedisDesignerViewModel(CreateModelItem(), mockServer.Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisDesignerViewModel))]
        public void RedisDesignerViewModel_Constructor_ModelItemIsValid_Constructor()
        {
            //------------Setup for test--------------------------
            var expectedId = Guid.NewGuid();
            var redisSource = new RedisSource
            {
                ResourceID = expectedId,
                ResourceName = "ResourceName",
                HostName = "HostName",
                Port = "6379",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };

            var redisSources = new List<RedisSource> { redisSource };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource)).Returns(redisSources);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();

            //------------Execute Test---------------------------
            var redisDesignerViewModel = new RedisDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(redisDesignerViewModel.HasLargeView);
            Assert.AreEqual(string.Empty, redisDesignerViewModel.ActivityFuncDisplayName);
            Assert.IsNull(redisDesignerViewModel.ActivityFuncIcon);
            Assert.AreEqual(1, redisDesignerViewModel.RedisServers.Count);
            Assert.AreEqual(expectedId, redisDesignerViewModel.RedisServers[0].ResourceID);
            Assert.AreEqual("ResourceName", redisDesignerViewModel.RedisServers[0].ResourceName);
            Assert.AreEqual("HostName", redisDesignerViewModel.RedisServers[0].HostName);
            Assert.AreEqual("6379", redisDesignerViewModel.RedisServers[0].Port);
            Assert.AreEqual(Runtime.ServiceModel.Data.AuthenticationType.Anonymous, redisDesignerViewModel.RedisServers[0].AuthenticationType);

            Assert.IsNull(redisDesignerViewModel.SelectedRedisServer);
            Assert.IsFalse(redisDesignerViewModel.IsRedisServerSelected);
            Assert.IsFalse(redisDesignerViewModel.EditRedisServerCommand.CanExecute(null));

            redisDesignerViewModel.SelectedRedisServer = redisSource;

            Assert.IsNotNull(redisDesignerViewModel.SelectedRedisServer);
            Assert.IsTrue(redisDesignerViewModel.IsRedisServerSelected);
            Assert.AreEqual(redisSource.ResourceID, redisDesignerViewModel.SelectedRedisServer.ResourceID);
            Assert.IsTrue(redisDesignerViewModel.EditRedisServerCommand.CanExecute(null));

            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisDesignerViewModel))]
        public void RedisDesignerViewModel_Constructor_ModelItemIsValid_UpdateHelpDescriptor()
        {
            var expectedHelpText = "redis help text";

            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(helpViewModel => helpViewModel.UpdateHelpText(expectedHelpText));
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockShellViewModel.Object);

            var expectedId = Guid.NewGuid();
            var redisSource = new RedisSource
            {
                ResourceID = expectedId,
                ResourceName = "ResourceName",
                HostName = "HostName",
                Port = "6379",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };

            var redisSources = new List<RedisSource> { redisSource };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource)).Returns(redisSources);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            //------------Execute Test---------------------------
            var redisDesignerViewModel = new RedisDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            redisDesignerViewModel.UpdateHelpDescriptor(expectedHelpText);

            mockHelpViewModel.Verify(helpViewModel => helpViewModel.UpdateHelpText(expectedHelpText), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisDesignerViewModel))]
        public void RedisDesignerViewModel_Constructor_ModelItemIsValid_EditRedisServerSource()
        {
            var expectedId = Guid.NewGuid();
            var redisSource = new RedisSource
            {
                ResourceID = expectedId,
                ResourceName = "ResourceName",
                HostName = "HostName",
                Port = "6379",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };

            var redisSources = new List<RedisSource> { redisSource };

            var environmentId = Guid.NewGuid();

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource)).Returns(redisSources);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.EnvironmentID).Returns(environmentId);
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.ActiveServer).Returns(mockServer.Object);
            mockShellViewModel.Setup(shellViewModel => shellViewModel.OpenResource(redisSource.ResourceID, environmentId, mockServer.Object));
            CustomContainer.Register(mockShellViewModel.Object);

            //------------Execute Test---------------------------
            var redisDesignerViewModel = new RedisDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object)
            {
                SelectedRedisServer = redisSource
            };
            redisDesignerViewModel.EditRedisServerCommand.Execute(null);

            mockShellViewModel.Verify(shellViewModel => shellViewModel.OpenResource(expectedId, environmentId, mockServer.Object), Times.Once);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisDesignerViewModel))]
        public void RedisDesignerViewModel_Constructor_ModelItemIsValid_NewRedisServerSource()
        {
            var expectedId = Guid.NewGuid();
            var redisSource = new RedisSource
            {
                ResourceID = expectedId,
                ResourceName = "ResourceName",
                HostName = "HostName",
                Port = "6379",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.Anonymous
            };

            var redisSources = new List<RedisSource> { redisSource };

            var environmentId = Guid.NewGuid();

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource)).Returns(redisSources);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.EnvironmentID).Returns(environmentId);
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);


            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.ActiveServer).Returns(mockServer.Object);
            mockShellViewModel.Setup(shellViewModel => shellViewModel.NewRedisSource(""));
            CustomContainer.Register(mockShellViewModel.Object);

            //------------Execute Test---------------------------
            var redisDesignerViewModel = new RedisDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            redisDesignerViewModel.NewRedisServerCommand.Execute(null);

            mockShellViewModel.Verify(shellViewModel => shellViewModel.NewRedisSource(""), Times.Once);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Exactly(2));
        }
    }
}
