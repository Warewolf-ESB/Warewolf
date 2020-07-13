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
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.RedisCache;
using Dev2.Activities.RedisCache;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.RedisCache
{
    [TestClass]
    public class RedisCacheDesignerViewModelTests
    {
        static ModelItem CreateModelItem(string key,int ttl)
        {
            var innerActivity = new DsfMultiAssignActivity() { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[objectId1]]", "ObjectName1", 1), new ActivityDTO("[[objectId2]]", "ObjectName2", 2) } };

            var redisCacheActivity = new RedisCacheActivity
            {
                Key = key,
                TTL = ttl,
                ActivityFunc = new ActivityFunc<string, bool>
                {
                    Handler = innerActivity
                }
            };
            return ModelItemUtils.CreateModelItem(redisCacheActivity);
        }
         DsfMultiAssignActivity CommonAssign(Guid? uniqueId = null)
        {
            return uniqueId.HasValue ? new DsfMultiAssignActivity { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
        }
        static ModelItem CreateModelItem()
        {
          
            return ModelItemUtils.CreateModelItem(new RedisCacheActivity());
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisCacheDesignerViewModel_Constructor_ModelItemIsValid_Null_EnvironmentModel()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            _ = new RedisCacheDesignerViewModel(CreateModelItem(), null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RedisCacheDesignerViewModel_Constructor_ModelItemIsValid_Null_ShellViewModel()
        {
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            //------------Execute Test---------------------------
            _ = new RedisCacheDesignerViewModel(CreateModelItem(), mockServer.Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        public void RedisCacheDesignerViewModel_Constructor_ModelItemIsValid_Constructor()
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
            var RedisCacheDesignerViewModel = new RedisCacheDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(RedisCacheDesignerViewModel.HasLargeView);
            Assert.IsTrue(RedisCacheDesignerViewModel.ShowLarge);
            Assert.AreEqual(string.Empty, RedisCacheDesignerViewModel.ActivityFuncDisplayName);
            Assert.IsNull(RedisCacheDesignerViewModel.ActivityFuncIcon);
            Assert.AreEqual(1, RedisCacheDesignerViewModel.RedisSources.Count);
            Assert.AreEqual(expectedId, RedisCacheDesignerViewModel.RedisSources[0].ResourceID);
            Assert.AreEqual("ResourceName", RedisCacheDesignerViewModel.RedisSources[0].ResourceName);
            Assert.AreEqual("HostName", RedisCacheDesignerViewModel.RedisSources[0].HostName);
            Assert.AreEqual("6379", RedisCacheDesignerViewModel.RedisSources[0].Port);
            Assert.AreEqual(Runtime.ServiceModel.Data.AuthenticationType.Anonymous, RedisCacheDesignerViewModel.RedisSources[0].AuthenticationType);
            Assert.AreEqual(null, RedisCacheDesignerViewModel.Key);
            Assert.IsNull(RedisCacheDesignerViewModel.SelectedRedisSource);
            Assert.IsFalse(RedisCacheDesignerViewModel.IsRedisSourceSelected);
            Assert.AreEqual(5, RedisCacheDesignerViewModel.TTL);
            Assert.IsFalse(RedisCacheDesignerViewModel.EditRedisSourceCommand.CanExecute(null));
            Assert.IsFalse(RedisCacheDesignerViewModel.IsKeyFocused);
            Assert.IsFalse(RedisCacheDesignerViewModel.IsRedisSourceFocused);
            RedisCacheDesignerViewModel.SelectedRedisSource = redisSource;

            Assert.IsNotNull(RedisCacheDesignerViewModel.SelectedRedisSource);
            Assert.IsTrue(RedisCacheDesignerViewModel.IsRedisSourceSelected);
            Assert.AreEqual(redisSource.ResourceID, RedisCacheDesignerViewModel.SelectedRedisSource.ResourceID);
            Assert.IsTrue(RedisCacheDesignerViewModel.EditRedisSourceCommand.CanExecute(null));

            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        public void RedisCacheDesignerViewModel_Constructor_ModelItemIsValid_UpdateHelpDescriptor()
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
            var RedisCacheDesignerViewModel = new RedisCacheDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            RedisCacheDesignerViewModel.UpdateHelpDescriptor(expectedHelpText);
            Assert.AreEqual(5, RedisCacheDesignerViewModel.TTL);
            mockHelpViewModel.Verify(helpViewModel => helpViewModel.UpdateHelpText(expectedHelpText), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        public void RedisCacheDesignerViewModel_Constructor_ModelItemIsValid_EditRedisServerSource()
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
            var RedisCacheDesignerViewModel = new RedisCacheDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object)
            {
                SelectedRedisSource = redisSource
            };
            RedisCacheDesignerViewModel.EditRedisSourceCommand.Execute(null);

            mockShellViewModel.Verify(shellViewModel => shellViewModel.OpenResource(expectedId, environmentId, mockServer.Object), Times.Once);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        public void RedisCacheDesignerViewModel_Constructor_ModelItemIsValid_NewRedisServerSource()
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
            var RedisCacheDesignerViewModel = new RedisCacheDesignerViewModel(CreateModelItem(), mockServer.Object, mockShellViewModel.Object);
            RedisCacheDesignerViewModel.NewRedisSourceCommand.Execute(null);

            mockShellViewModel.Verify(shellViewModel => shellViewModel.NewRedisSource(""), Times.Once);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        public void RedisCacheDesignerViewModel_EditKey_EditTTL_Edit()
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
            var key = "redisKey";
            var ttl = 30;
            var redisSources = new List<RedisSource> { redisSource };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource)).Returns(redisSources);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(p => p.TryFindResource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockApplicationAdapter.Object);
            //------------Execute Test---------------------------
            var RedisCacheDesignerViewModel = new RedisCacheDesignerViewModel(CreateModelItem(key,ttl), mockServer.Object, mockShellViewModel.Object);
            RedisCacheDesignerViewModel.IsKeyFocused = false;
            RedisCacheDesignerViewModel.IsRedisSourceFocused = false;
            //------------Assert Results-------------------------
            Assert.IsTrue(RedisCacheDesignerViewModel.HasLargeView);
            Assert.IsTrue(RedisCacheDesignerViewModel.ShowLarge);
            Assert.AreEqual("Assign", RedisCacheDesignerViewModel.ActivityFuncDisplayName);
            Assert.IsNull(RedisCacheDesignerViewModel.ActivityFuncIcon);
            Assert.AreEqual(1, RedisCacheDesignerViewModel.RedisSources.Count);
            Assert.AreEqual(expectedId, RedisCacheDesignerViewModel.RedisSources[0].ResourceID);
            Assert.AreEqual("ResourceName", RedisCacheDesignerViewModel.RedisSources[0].ResourceName);
            Assert.AreEqual("HostName", RedisCacheDesignerViewModel.RedisSources[0].HostName);
            Assert.AreEqual("6379", RedisCacheDesignerViewModel.RedisSources[0].Port);
            Assert.AreEqual(Runtime.ServiceModel.Data.AuthenticationType.Anonymous, RedisCacheDesignerViewModel.RedisSources[0].AuthenticationType);
            Assert.AreEqual("redisKey", RedisCacheDesignerViewModel.Key);
            Assert.IsNull(RedisCacheDesignerViewModel.SelectedRedisSource);
            Assert.IsFalse(RedisCacheDesignerViewModel.IsRedisSourceSelected);
            Assert.AreEqual(30, RedisCacheDesignerViewModel.TTL);
            Assert.IsFalse(RedisCacheDesignerViewModel.IsKeyFocused);
            Assert.IsFalse(RedisCacheDesignerViewModel.EditRedisSourceCommand.CanExecute(null));

            RedisCacheDesignerViewModel.SelectedRedisSource = redisSource;
            RedisCacheDesignerViewModel.Key = "RedisKeyTest";
            RedisCacheDesignerViewModel.TTL = 20;

            Assert.IsNotNull(RedisCacheDesignerViewModel.SelectedRedisSource);
            Assert.IsTrue(RedisCacheDesignerViewModel.IsRedisSourceSelected);
            Assert.AreEqual(redisSource.ResourceID, RedisCacheDesignerViewModel.SelectedRedisSource.ResourceID);
            Assert.IsTrue(RedisCacheDesignerViewModel.EditRedisSourceCommand.CanExecute(null));
            Assert.AreEqual(20, RedisCacheDesignerViewModel.TTL);
            Assert.AreEqual("RedisKeyTest", RedisCacheDesignerViewModel.Key);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(RedisCacheDesignerViewModel))]
        public void RedisCacheDesignerViewModel_ActivityFuncIcon()
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

            var key = "redisKey";
            var ttl = 30;
            var redisSources = new List<RedisSource> { redisSource };

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindSourcesByType<RedisSource>(It.IsAny<IServer>(), enSourceType.RedisSource)).Returns(redisSources);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(p => p.TryFindResource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockApplicationAdapter.Object);

            //------------Execute Test---------------------------
            var RedisCacheDesignerViewModel = new RedisCacheDesignerViewModel(CreateModelItem(key, ttl), mockServer.Object, mockShellViewModel.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(RedisCacheDesignerViewModel.HasLargeView);
            Assert.AreEqual("Assign", RedisCacheDesignerViewModel.ActivityFuncDisplayName);
            Assert.IsNull(RedisCacheDesignerViewModel.ActivityFuncIcon);
        }
    }
}
