using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Moq;

namespace Dev2.Activities.Designers.Tests.DropBox2016
{
    public class TestResourceCatalog
    {
        public static Lazy<Mock<IEventAggregator>> EventAggr = new Lazy<Mock<IEventAggregator>>(() => new Mock<IEventAggregator>());
        public static Lazy<IResourceCatalog> ResourceCatalog = new Lazy<IResourceCatalog>(
            () =>
            {
                var mock = new Mock<IResourceCatalog>();
                mock.Setup(catalog => catalog.GetResourceList<Resource>(It.IsAny<Guid>())).Returns(new List<IResource>());
                return mock.Object;
            });

        public static Lazy<IEnvironmentModel> EnvLazy = new Lazy<IEnvironmentModel>(() =>
        {
            var env = new Mock<IEnvironmentModel>();
            var mockResourceRepo = new Mock<IResourceRepository>();
            var oauthSources = new List<DropBoxSource> { new DropBoxSource { ResourceName = "Dropbox Source" } };
            mockResourceRepo.Setup(repository => repository.GetResourceList<DropBoxSource>(env.Object)).Returns(oauthSources);
            env.Setup(model => model.ResourceRepository).Returns(mockResourceRepo.Object);
            return env.Object;
        });

        public static Lazy<IDropboxSourceManager> LazySourceManager = new Lazy<IDropboxSourceManager>(() =>
        {
            var mock = new Mock<DropboxSourceManager>(EnvLazy.Value);
            return mock.Object;
        });
    }
}