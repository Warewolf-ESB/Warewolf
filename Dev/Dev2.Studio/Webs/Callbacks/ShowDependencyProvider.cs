using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utils;

// ReSharper disable once CheckNamespace
namespace Dev2.Webs.Callbacks
{
    public class ShowDependencyProvider : IShowDependencyProvider
    {
        readonly ResourceChangeHandler _resourceChangeHandler;

        public ShowDependencyProvider()
            : this(EventPublishers.Aggregator)
        {
        }

        public ShowDependencyProvider(IEventAggregator eventPublisher)
        {
            _resourceChangeHandler = new ResourceChangeHandler(eventPublisher);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
        }

        #region Implementation of IShowDependencyProvider

        public void ShowDependencyViewer(IContextualResourceModel resource, IList<string> numberOfDependants)
        {
            _resourceChangeHandler.ShowResourceChanged(resource, numberOfDependants);
        }

        #endregion
    }
}