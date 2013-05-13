using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Moq;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace Dev2.Integration.Tests.MEF
{
    public class CompositionInitializer
    {
        internal static ImportServiceContext InitializeMockedWindowNavigationBehavior()
        {
            ImportServiceContext importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog>
            {
                //new TestAggregateCatalog()
            });

            // set up window behavior
            Mock<IEventAggregator> eventAgg = new Mock<IEventAggregator>();
            Mock<IWindowManager> windowManMock = new Mock<IWindowManager>();
            ImportService.AddExportedValueToContainer<IEventAggregator>(eventAgg.Object);
            ImportService.AddExportedValueToContainer(windowManMock.Object);
            return importServiceContext;
        }

        internal static ImportServiceContext DefaultInitialize()
        {
            ImportServiceContext importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog> 
            { 
                new TestAggregateCatalog()
            });

            ImportService.AddExportedValueToContainer<IEventAggregator>(new EventAggregator());
            IFrameworkSecurityContext securityProvider = new MockSecurityProvider("IntegrationTestSecurity");
            ImportService.AddExportedValueToContainer<IFrameworkSecurityContext>(securityProvider);
            return importServiceContext;
        }

        internal static ImportServiceContext InitializeForResourceRepository()
        {

            ImportServiceContext importServiceContext = new ImportServiceContext();

            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog> {
                new TestAggregateCatalog()
            });

            IFrameworkSecurityContext securityProvider = new MockSecurityProvider("IntegrationTestSecurity");
            ImportService.AddExportedValueToContainer<IFrameworkSecurityContext>(securityProvider);
            return importServiceContext;
        }

        internal static ImportServiceContext DefaultInitialize11()
        {
            ImportServiceContext importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;

            ImportService.Initialize(new List<ComposablePartCatalog> 
            { 
                new TestAggregateCatalog()
            });

            ImportService.AddExportedValueToContainer<IEventAggregator>(new EventAggregator());

            return importServiceContext;
        }
    }
}
