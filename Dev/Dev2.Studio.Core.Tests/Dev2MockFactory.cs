
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
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Utils;
using Dev2.CustomControls.Connections;
using Dev2.Network;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.ViewModels;
using Dev2.Util;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    public static class Dev2MockFactory
    {
        private static Mock<MainViewModel> _mockMainViewModel;
        private static Mock<IContextualResourceModel> _mockResourceModel;
        
        static Dev2MockFactory()
        {
            AppSettings.LocalHost = "https://localhost:3143";
        }

        /*
         * Travis.Frisinger : 04-04-2012
         * 
         * The stuff for ILayoutObjectViewModel is ugly, but given that I need to return from a callback???
         * It was the easiest method I would find to correctly result the LayoutGrid without taking another day to get it right
         * http://stackoverflow.com/questions/2494930/settings-variable-values-in-a-moq-callback-call
         * sites a method of returning inline, but it does not take paramers into the callback method and I could not get the ordering right
         * hence this not-so-nice workaround
         * 
         */
        // required for IsSelected Property of LayoutGridObjectViewModel

        static public Mock<MainViewModel> MainViewModel
        {
            get
            {
                if(_mockMainViewModel == null)
                {
                    var eventPublisher = new Mock<IEventAggregator>();
                    var environmentRepository = new Mock<IEnvironmentRepository>();
                    var environmentModel = new Mock<IEnvironmentModel>();
                    environmentModel.Setup(c => c.CanStudioExecute).Returns(false);
                    environmentRepository.Setup(c => c.ReadSession()).Returns(new[] { Guid.NewGuid() });
                    environmentRepository.Setup(c => c.All()).Returns(new[] { environmentModel.Object });
                    environmentRepository.Setup(c => c.Source).Returns(environmentModel.Object);
                    var versionChecker = new Mock<IVersionChecker>();
                    var asyncWorker = AsyncWorkerTests.CreateSynchronousAsyncWorker();
                    _mockMainViewModel = new Mock<MainViewModel>(eventPublisher.Object, asyncWorker.Object, environmentRepository.Object,
                                                                 versionChecker.Object, false, null, null, null, null, new Mock<IStudioResourceRepository>().Object,
                                                                 new Mock<IConnectControlSingleton>().Object, new Mock<IConnectControlViewModel>().Object);
                }
                return _mockMainViewModel;
            }
        }

        static public Mock<IContextualResourceModel> ResourceModel
        {
            get
            {
                if(_mockResourceModel != null)
                {
                    return _mockResourceModel;
                }
                _mockResourceModel = SetupResourceModelMock();
                return _mockResourceModel;
            }
        }

        static public Mock<IEnvironmentConnection> SetupIEnvironmentConnection()
        {
            Mock<IEnvironmentConnection> mockIEnvironmentConnection = new Mock<IEnvironmentConnection>();
            return mockIEnvironmentConnection;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel()
        {
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock().Object);
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(AppSettings.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.AppServerUri).Returns(new Uri(AppSettings.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());
            var authService = new Mock<IAuthorizationService>();
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(authService.Object);
            return mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, resourceRepositoryFakeBacker).Object);
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(AppSettings.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.Verify(It.IsAny<Action<ConnectResult>>(), false)).Callback(() => { });
            var authService = new Mock<IAuthorizationService>();
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(authService.Object);
            mockEnvironmentModel.Setup(e => e.IsLocalHostCheck()).Returns(true);
            mockEnvironmentModel.Setup(e => e.IsLocalHost).Returns(true);

            return mockEnvironmentModel;
        }

        static public Mock<IEnvironmentModel> SetupEnvironmentModel(Mock<IContextualResourceModel> returnResource, List<IResourceModel> returnResources, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connect()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.LoadResources()).Verifiable();
            mockEnvironmentModel.Setup(environmentModel => environmentModel.ResourceRepository).Returns(SetupFrameworkRepositoryResourceModelMock(returnResource, returnResources, resourceRepositoryFakeBacker).Object);
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.WebServerUri).Returns(new Uri(AppSettings.LocalHost));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.ServerEvents).Returns(new EventPublisher());
            var authService = new Mock<IAuthorizationService>();
            mockEnvironmentModel.Setup(a => a.AuthorizationService).Returns(authService.Object);
            return mockEnvironmentModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock()
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel>());
            //mockResourceModel.Setup(resource => resource.Save().Verifiable();
            //mockResourceModel.Setup(resource => resource.ResourceType).Returns(enResourceType.Service);

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(returnResource.Object);
            if(resourceRepositoryFakeBacker != null)
            {
                mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(resourceRepositoryFakeBacker.Add);
            }
            mockResourceModel.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<Studio.Core.AppResources.Enums.ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true)).Returns(new List<IResourceModel> { returnResource.Object });
            mockResourceModel.Setup(r => r.All()).Returns(new List<IResourceModel> { returnResource.Object });

            return mockResourceModel;
        }

        static public Mock<IResourceRepository> SetupFrameworkRepositoryResourceModelMock(Mock<IContextualResourceModel> returnResource, List<IResourceModel> returnResources, List<IResourceModel> resourceRepositoryFakeBacker)
        {
            var mockResourceModel = new Mock<IResourceRepository>();
            mockResourceModel.Setup(r => r.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(returnResource.Object);
            mockResourceModel.Setup(r => r.Save(It.IsAny<IResourceModel>())).Callback<IResourceModel>(resourceRepositoryFakeBacker.Add);
            mockResourceModel.Setup(r => r.ReloadResource(It.IsAny<Guid>(), It.IsAny<Studio.Core.AppResources.Enums.ResourceType>(), It.IsAny<IEqualityComparer<IResourceModel>>(), true)).Returns(new List<IResourceModel> { returnResource.Object });
            mockResourceModel.Setup(r => r.All()).Returns(returnResources);

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
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(Studio.Core.AppResources.Enums.ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);

            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType resourceType, Guid resourceID = new Guid())
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlServiceDefinition));
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);
            mockResourceModel.Setup(resModel => resModel.ID).Returns(resourceID);


            return mockResourceModel;
        }

        static public Mock<IContextualResourceModel> SetupResourceModelMock(Studio.Core.AppResources.Enums.ResourceType resourceType, string resourceName, bool returnSelf = true)
        {
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlServiceDefinition));
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns(resourceName);
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Category\\Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(resourceType);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");

            if(returnSelf)
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

        // ReSharper disable ParameterHidesMember

        public static Mock<IPopupController> CreateIPopup(MessageBoxResult returningResult)
        {
            Mock<IPopupController> result = new Mock<IPopupController>();
            result.Setup(moq => moq.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>())).Returns(returningResult).Verifiable();

            return result;
        }
    }
}
