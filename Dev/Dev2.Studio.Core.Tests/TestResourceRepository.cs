using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Wizards.Interfaces;
using Moq;

namespace Dev2.Studio.Core.AppResources.Repositories
{
    class TestResourceRepository : ResourceRepository
    {
        public TestResourceRepository()
            : base(new Mock<IEnvironmentModel>().Object)
        {
        }

        public TestResourceRepository(IEnvironmentModel environmentModel)
            : base(environmentModel)
        {
        }

        public TestResourceRepository(IEnvironmentModel environmentModel, IWizardEngine wizardEngine)
            : base(environmentModel, wizardEngine)
        {
        }

        public void AddMockResource(IResourceModel mockRes)
        {
            _resourceModels.Add(mockRes);
        }
    }
}