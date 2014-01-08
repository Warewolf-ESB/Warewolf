using Dev2.Studio.Core.Interfaces;
using Moq;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Repositories
{
    class TestResourceRepository : ResourceRepository
    {
        public TestResourceRepository()
            : this(new Mock<IEnvironmentModel>().Object)
        {
        }

        public TestResourceRepository(IEnvironmentModel environmentModel)
            : base(environmentModel)
        {
        }


        public void AddMockResource(IResourceModel mockRes)
        {
            ResourceModels.Add(mockRes);
        }

        public int LoadResourcesHitCount { get; private set; }

        protected override void LoadResources()
        {
            LoadResourcesHitCount++;
            base.LoadResources();
        }
    }
}