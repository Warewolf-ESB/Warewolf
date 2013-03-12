using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;


namespace Dev2.Studio.Core.Factories     
{
    public static class ResourceRepositoryFactory
    {
        public static IResourceRepository CreateResourceRepository(IEnvironmentModel environment)
        {
            var resourceRepository = new ResourceRepository(environment);
            
            return resourceRepository;
        }
    }
}
