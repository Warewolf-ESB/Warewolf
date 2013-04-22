using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Studio;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace Dev2.Integration.Tests.MEF
{
    public class TestAggregateCatalog : AggregateCatalog
    {
        public TestAggregateCatalog()
        {
            //this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(IEventAggregator))));
            //this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(Bootstrapper))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(IEnvironmentModel))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(INetworkMessage))));
            this.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(INetworkExecutionChannel))));
        }
    }
}
