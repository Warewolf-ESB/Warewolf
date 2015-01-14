using System;
using Dev2;
using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.Models.Help
{
    public class HelpModel:IHelpWindowModel,IDisposable
    {
        #region Implementation of IHelpWindowModel

        readonly IEventAggregator _aggregator;

        public HelpModel(IEventAggregator aggregator)
        {
            VerifyArgument.IsNotNull("aggregator",aggregator);
            _aggregator = aggregator;
            _token=_aggregator.GetEvent<HelpChangedEvent>().Subscribe(FireOnHelpReceived);
        }

        private void FireOnHelpReceived(IHelpDescriptor obj)
        {
            if(OnHelpTextReceived != null)
            {
                OnHelpTextReceived(this, obj);
            }
        }

        public event HelpTextReceived OnHelpTextReceived;
        private readonly SubscriptionToken _token;     

        #endregion

        public void Dispose()
        {
            if(_token!=null)
            _aggregator.GetEvent<HelpChangedEvent>().Unsubscribe(_token);
        }
    }
}