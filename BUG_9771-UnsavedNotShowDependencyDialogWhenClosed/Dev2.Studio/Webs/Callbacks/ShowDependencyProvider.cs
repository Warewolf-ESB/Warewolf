using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Views.ResourceManagement;

namespace Dev2.Studio.Webs.Callbacks
{
    public class ShowDependencyProvider:IShowDependencyProvider
    {
        #region Implementation of IShowDependencyProvider

        public void ShowDependencyViewer(IContextualResourceModel resource, int numberOfDependants,IEventAggregator eventAggregator)
        {
            ResourceChangedDialog dialog = new ResourceChangedDialog(resource, numberOfDependants);
            dialog.ShowDialog();
            if (dialog.OpenDependencyGraph)
            {
                eventAggregator.Publish(new ShowReverseDependencyVisualizer(resource));
            }
        }

        #endregion
    }
}