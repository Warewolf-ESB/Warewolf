using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources.Repositories;
using Unlimited.Framework;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using Dev2.Composition;


namespace Dev2.Studio.Core.Factories     
{
    public static class ResourceRepositoryFactory
    {
        public static IResourceRepository CreateResourceRepository(IEnvironmentModel environment)
        {
            ResourceRepository resourceRepository = new ResourceRepository(environment);
            ImportService.SatisfyImports(resourceRepository);
            
            return resourceRepository;
        }
    }
}
