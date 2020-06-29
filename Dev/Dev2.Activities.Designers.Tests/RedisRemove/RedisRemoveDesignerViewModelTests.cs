/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.RedisRemove;
using Dev2.Activities.RedisRemove;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.RedisRemove
{
    [TestClass]
    public class RedisRemoveDesignerViewModelTests
    {
        private static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new RedisRemoveActivity());
        }

        private static ModelItem CreateModelItemWithKey()
        {
            var redisRemove = new RedisRemoveActivity {Key = "newKey"};
            return ModelItemUtils.CreateModelItem(redisRemove);
        }

        private static ModelItem CreateModelItemWithResult()
        {
            var redisRemove = new RedisRemoveActivity {Key = "newKey", Result = "someResult"};
            return ModelItemUtils.CreateModelItem(redisRemove);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisRemoveDesignerViewModel_Constructor_ModelItemIsValid_Null_EnvironmentModel()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            _ = new RedisRemoveDesignerViewModel(CreateModelItem(), null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisRemoveDesignerViewModel_Constructor_ModelItemIsValid_Null_ShellViewModel()
        {
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            //------------Execute Test---------------------------
            _ = new RedisRemoveDesignerViewModel(CreateModelItem(), mockServer.Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        public void RedisRemoveDesignerViewModel_Constructor_ModelItemIsValid_Constructor()
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
            var redisRemoveDesignerViewModel = new RedisRemoveDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(redisRemoveDesignerViewModel.HasLargeView);
            Assert.IsTrue(redisRemoveDesignerViewModel.ShowLarge);
            Assert.AreEqual(1, redisRemoveDesignerViewModel.RedisSources.Count);
            Assert.AreEqual(expectedId, redisRemoveDesignerViewModel.RedisSources[0].ResourceID);
            Assert.AreEqual("ResourceName", redisRemoveDesignerViewModel.RedisSources[0].ResourceName);
            Assert.AreEqual("HostName", redisRemoveDesignerViewModel.RedisSources[0].HostName);
            Assert.AreEqual("6379", redisRemoveDesignerViewModel.RedisSources[0].Port);
            Assert.AreEqual(Runtime.ServiceModel.Data.AuthenticationType.Anonymous, redisRemoveDesignerViewModel.RedisSources[0].AuthenticationType);

            Assert.IsNull(redisRemoveDesignerViewModel.SelectedRedisSource);
            Assert.IsFalse(redisRemoveDesignerViewModel.IsRedisSourceSelected);
            Assert.IsFalse(redisRemoveDesignerViewModel.EditRedisSourceCommand.CanExecute(null));

            redisRemoveDesignerViewModel.SelectedRedisSource = redisSource;

            Assert.IsNotNull(redisRemoveDesignerViewModel.SelectedRedisSource);
            Assert.IsTrue(redisRemoveDesignerViewModel.IsRedisSourceSelected);
            Assert.AreEqual(redisSource.ResourceID, redisRemoveDesignerViewModel.SelectedRedisSource.ResourceID);
            Assert.IsTrue(redisRemoveDesignerViewModel.EditRedisSourceCommand.CanExecute(null));

            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        public void RedisRemoveDesignerViewModel_Constructor_ModelItemIsValid_UpdateHelpDescriptor()
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
            var redisRemoveDesignerViewModel = new RedisRemoveDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            Assert.AreEqual("Use the Redis Remove tool to remove existing key with data from cache.", redisRemoveDesignerViewModel.HelpText);
            redisRemoveDesignerViewModel.UpdateHelpDescriptor(expectedHelpText);
            
            mockHelpViewModel.Verify(helpViewModel => helpViewModel.UpdateHelpText(expectedHelpText), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        public void RedisRemoveDesignerViewModel_Constructor_ModelItemIsValid_EditRedisServerSource()
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
            var redisRemoveDesignerViewModel = new RedisRemoveDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object)
            {
                SelectedRedisSource = redisSource
            };
            redisRemoveDesignerViewModel.EditRedisSourceCommand.Execute(null);

            mockShellViewModel.Verify(shellViewModel => shellViewModel.OpenResource(expectedId, environmentId, mockServer.Object), Times.Once);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        public void RedisRemoveDesignerViewModel_Constructor_ModelItemIsValid_NewRedisServerSource()
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
            var redisRemoveDesignerViewModel = new RedisRemoveDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            redisRemoveDesignerViewModel.NewRedisSourceCommand.Execute(null);

            mockShellViewModel.Verify(shellViewModel => shellViewModel.NewRedisSource(""), Times.Once);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        public void RedisRemoveDesignerViewModel_Constructor_KeyNotNull()
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
            var redisRemoveDesignerViewModel = new RedisRemoveDesignerViewModel(CreateModelItemWithKey(), mockServer.Object, mockShellViewModel.Object);

            Assert.AreEqual("newKey", redisRemoveDesignerViewModel.Key);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisRemoveDesignerViewModel))]
        public void RedisRemoveDesignerViewModel_Constructor_ResultNotNull()
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
            var redisRemoveDesignerViewModel = new RedisRemoveDesignerViewModel(CreateModelItemWithResult(), mockServer.Object, mockShellViewModel.Object);

            Assert.AreEqual("someResult", redisRemoveDesignerViewModel.Result);
        }
    }
}
