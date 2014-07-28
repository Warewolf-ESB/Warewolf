using System;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Moq;

namespace Dev2.Core.Tests.Environments
{
    public class TestEnvironmentModel : EnvironmentModel
    {
        Mock<IAuthorizationService> _authorizationServiceMock;

        public TestEnvironmentModel(IEventAggregator eventPublisher, Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, bool publishEventsOnDispatcherThread = true)
            : base(id, environmentConnection, resourceRepository, new Mock<IStudioResourceRepository>().Object)
        {
        }

        public Mock<IAuthorizationService> AuthorizationServiceMock
        {
            get
            {
                return _authorizationServiceMock;
            }
            set
            {
                _authorizationServiceMock = value;
            }
        }

        protected override IAuthorizationService CreateAuthorizationService(IEnvironmentConnection environmentConnection)
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            return AuthorizationServiceMock.Object;
        }
    }
}
