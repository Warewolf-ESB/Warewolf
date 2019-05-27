#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        void FireOnHelpReceived(IHelpDescriptor obj)
        {
            OnHelpTextReceived?.Invoke(this, obj);
        }

        public event HelpTextReceived OnHelpTextReceived;
        readonly SubscriptionToken _token;

        #endregion

        public void Dispose()
        {
            if(_token!=null)
            {
                _aggregator.GetEvent<HelpChangedEvent>().Unsubscribe(_token);
            }
        }
    }
}