using Dev2.Studio.Core.Interfaces;
using Moq;

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
            _resourceModels.Add(mockRes);
        }

        public int LoadResourcesHitCount { get; private set; }

        protected override void LoadResources()
        {
            LoadResourcesHitCount++;
            base.LoadResources();
        }
    }
}