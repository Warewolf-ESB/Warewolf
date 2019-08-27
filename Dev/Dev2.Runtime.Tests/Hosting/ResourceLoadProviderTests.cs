/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Versioning;
using Dev2.DynamicServices;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ResourceLoadProviderTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ResourceLoadProvider))]
        public void ResourceLoadProvider_FindByType_GivenWorkflowResource_ExpectWorkflowResource()
        {
            var workspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
            var id = GlobalConstants.ServerWorkspaceID;
            var expected = new Workflow();
            workspaceResources.GetOrAdd(id, (newId) => new List<IResource> { expected });
            var serverVersionRepository = new Mock<IServerVersionRepository>().Object;
            var managementServices = new List<DynamicService>();
            var provider = new ResourceLoadProvider(workspaceResources, serverVersionRepository, managementServices);


            var resources = provider.FindByType(typeof(Workflow).FullName);

            Assert.IsTrue(resources.Any(o => o == expected));

            var resources2 = provider.FindByType<Workflow>();

            Assert.IsTrue(resources2.Any(o => o == expected));
        }
    }
}
