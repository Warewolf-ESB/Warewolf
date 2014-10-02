
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Services.Security;
using Dev2RuntimeServiceModel = Dev2.Runtime.ServiceModel;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    public class TestDbServices : Dev2RuntimeServiceModel.Services
    {
        public SqlDatabaseBroker Broker { get; private set; }

        public TestDbServices(IResourceCatalog resourceCatalog, IAuthorizationService authorizationService)
            : base(resourceCatalog, authorizationService)
        {
        }

        public TestDbServices(IResourceCatalog resourceCatalog)
            : this(resourceCatalog, ServerAuthorizationService.Instance)
        {
        }

        public TestDbServices()
        {
        }

        public TestDbServices(SqlDatabaseBroker broker)
        {
            Broker = broker;
        }
        
        public TestDbServices(IResourceCatalog resourceCatalog,SqlDatabaseBroker broker):this(resourceCatalog)
        {
            Broker = broker;
        }

        protected override SqlDatabaseBroker CreateDatabaseBroker()
        {
            return Broker ?? base.CreateDatabaseBroker();
        }
    }
}
