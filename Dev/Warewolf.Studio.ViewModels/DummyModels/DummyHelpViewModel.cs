using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Studio.Models.Help;
using Warewolf.Studio.ViewModels.Help;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyHelpViewModel : BindableBase,IHelpWindowViewModel
    {
        IHelpDescriptorViewModel _currentHelpText;
        readonly IEventAggregator _aggregator;
        public DummyHelpViewModel(IHelpDescriptorViewModel currentHelpText, IEventAggregator aggregator)
        {
            _currentHelpText = currentHelpText;
            _aggregator = aggregator;
            _aggregator = aggregator;
            _aggregator.GetEvent<HelpChangedEvent>().Subscribe(FireOnHelpReceived);
        }


        private void FireOnHelpReceived(IHelpDescriptor obj)
        {
            CurrentHelpText= new HelpDescriptorViewModel(obj);
        }
        #region Implementation of IHelpWindowViewModel

        /// <summary>
        /// Wpf component binds here
        /// </summary>
        public IHelpDescriptorViewModel CurrentHelpText
        {
            get
            {
                return _currentHelpText;
            }
            private set { _currentHelpText = value; }
        }

        #endregion
    }
}
