using Caliburn.Micro;
using Dev2.Network.Execution;
using Dev2.Network.Messages;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Bootstrapper = Dev2.Studio.Bootstrapper;

namespace Dev2.Integration.Tests.MEF
{
    public class TestAggregateCatalog : AggregateCatalog
    {
        public TestAggregateCatalog()
        {
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(IEventAggregator))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(Bootstrapper))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(IEnvironmentModel))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(INetworkMessage))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(INetworkExecutionChannel))));
        }
    }
}
