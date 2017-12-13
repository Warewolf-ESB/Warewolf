/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.ViewModels;
using Dev2.Util;
using Moq;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.Interfaces.Enums;


namespace Dev2.Core.Tests
{
    public static class Dev2MockFactory
    {
        static Mock<ShellViewModel> _mockMainViewModel;
        static Mock<IContextualResourceModel> _mockResourceModel;

        static Dev2MockFactory()
        {
            AppUsageStats.LocalHost = "https://localhost:3143";
        }

        static public Mock<ShellViewModel> MainViewModel
        {
            get
            {
                if (_mockMainViewModel == null)
                {
                    var eventPublisher = new Mock<IEventAggregator>();
                    var environmentRepository = new Mock<IServerRepository>();
                    var environmentModel = new Mock<IServer>();
                    environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
                    environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
                    environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
                    var versionChecker = new Mock<IVersionChecker>();
                    var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
                    _mockMainViewModel = new Mock<ShellViewModel>(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object,
                                                                 versionChecker.Object, false, null, null, null, null);
                }
                return _mockMainViewModel;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModel
        {
            get
            {
                if (_mockResourceModel != null)
                {
                    return _mockResourceModel;
                }
                _mockResourceModel = SetupResourceModelMock();
                return _mockResourceModel;
            }
        }

        static public Mock<IServer> SetupEnvironmentModel()
        {
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock().Object);
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(AppUsageStats.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.AppServerUri).Returns(new Uri(AppUsageStats.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());
            var authService = new Mock<IAuthorizationService>();
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(authService.Object);
            return mockEnvironmentModel;
        }

        static public Mock<IServer> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, resourceRepositoryFakeBacker).Object);
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(AppUsageStats.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.Verify(It.IsAny<Action<ConnectResult>>(), false)).Callback(() => { });
            var authService = new Mock<IAuthorizationService>();
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(authService.Object);
            mockEnvironmentModel.Setup(e => e.IsLocalHostCheck()).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsLocalHost).Returns(true);
      
            return mockEnvironmentModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock()
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel>());

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false, false)).Returns(returnResource.Object);
            if (resourceRepositoryFakeBacker != null)
            {
                mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(resourceRepositoryFakeBacker.Add);
            }
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel> { returnResource.Object });

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock()
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlServiceDefinition));
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(ResourceType resourceType, string resourceName, bool returnSelf = true)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlServiceDefinition));
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Category\\Testing");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");

            if (returnSelf)
            {
                mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);
            }
            else
            {
                mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel().Object);
            }

            return mockResourceModel;
        }

        public static Mock<IDataListViewModel> SetupDataListViewModel()
        {
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            return mockDataListViewModel;
        }

        

        public static Mock<IPopupController> CreateIPopup(MessageBoxResult returningResult)
        {
            var result = new Mock<IPopupController>();
            result.Setup(moq => moq.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(returningResult).Verifiable();

            return result;
        }
    }
}
