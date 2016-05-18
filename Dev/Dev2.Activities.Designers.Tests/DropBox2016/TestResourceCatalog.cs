using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
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
    }
}