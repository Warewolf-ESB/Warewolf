using System.Activities.Presentation.Model;
using Dev2.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;

namespace Dev2.Core.Tests.ViewModelTests.ViewModelMocks
{
    public class MockDsfActivityViewModel : DsfActivityViewModel
    {
        public MockDsfActivityViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IDesignValidationService validationService,IDataMappingViewModel mappingViewModel)
            : base(modelItem,  rootModel, validationService, mappingViewModel)
        {
            OnDesignValidationReceived += (sender, args) => OnDesignValidationReceivedHitCount++;
        }

        public int OnDesignValidationReceivedHitCount { get; private set; }
    }
}
