using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.DynamicServices;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.ViewModels.ActivityViewModels
{
    public class DsfSendEmailActivityViewModel : DsfSendEmailActivityViewModelBase
    {
        public DsfSendEmailActivityViewModel(DsfSendEmailActivity activity)
            : this(activity, EventPublishers.Aggregator)
        {
        }

        public DsfSendEmailActivityViewModel(DsfSendEmailActivity activity, IEventAggregator eventPublisher)
            : base(activity, eventPublisher)
        {
        }

        #region Overrides of DsfSendEmailActivityViewModel

        public override List<UnlimitedObject> GetSources(IEnvironmentModel environmentModel)
        {
            return ResourceRepository.FindSourcesByType(environmentModel, enSourceType.EmailSource);
        }

        #endregion
    }
}