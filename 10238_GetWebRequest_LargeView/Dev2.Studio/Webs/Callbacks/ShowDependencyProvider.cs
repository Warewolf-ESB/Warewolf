using Caliburn.Micro;
using Dev2.Data.Enums;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Views.ResourceManagement;

namespace Dev2.Studio.Webs.Callbacks
{
    public class ShowDependencyProvider:IShowDependencyProvider
    {
        readonly IEventAggregator _eventPublisher;

        public ShowDependencyProvider()
            : this(EventPublishers.Aggregator)
        {
        }

        public ShowDependencyProvider(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
        }

        #region Implementation of IShowDependencyProvider

        public void ShowDependencyViewer(IContextualResourceModel resource, int numberOfDependants)
        {
            var dialog = new ResourceChangedDialog(resource, numberOfDependants, StringResources.MappingChangedWarningDialogTitle);
            dialog.ShowDialog();
            if (dialog.OpenDependencyGraph)
            {
                _eventPublisher.Publish(new ShowReverseDependencyVisualizer(resource));
            }
        }

        #endregion
    }
}