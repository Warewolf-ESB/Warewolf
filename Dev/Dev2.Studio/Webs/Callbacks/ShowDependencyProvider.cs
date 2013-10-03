using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utils;

namespace Dev2.Studio.Webs.Callbacks
{
    public class ShowDependencyProvider:IShowDependencyProvider
    {
        readonly ShowResourceChangedUtil _showResourceChangedUtil;

        public ShowDependencyProvider()
            : this(EventPublishers.Aggregator)
        {
        }

        public ShowDependencyProvider(IEventAggregator eventPublisher)
        {
            _showResourceChangedUtil = new ShowResourceChangedUtil(eventPublisher);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
        }

        #region Implementation of IShowDependencyProvider

        public void ShowDependencyViewer(IContextualResourceModel resource, IList<string> numberOfDependants)
        {
            _showResourceChangedUtil.ShowResourceChanged(resource, numberOfDependants);
        }

        #endregion
    }
}