
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Providers.Events;

namespace Dev2.Services.Events
{
    public static class EventPublishers
    {
        #region Studio

        static volatile IEventPublisher _studioPublisher;
        static readonly object StudioLock = new object();

        public static IEventPublisher Studio
        {
            get
            {
                if(_studioPublisher == null)
                {
                    lock(StudioLock)
                    {
                        if(_studioPublisher == null)
                        {
                            _studioPublisher = new EventPublisher();
                        }
                    }
                }

                return _studioPublisher;
            }
        }

        #endregion

        // TODO: Remove IEventAggregator completely!!

        #region Aggregator

        static volatile IEventAggregator _aggregator;
        static readonly object AggregatorLock = new object();

        public static IEventAggregator Aggregator
        {
            get
            {
                if(_aggregator == null)
                {
                    lock(AggregatorLock)
                    {
                        if(_aggregator == null)
                        {
                            _aggregator = new EventAggregator();
                        }
                    }
                }

                return _aggregator;
            }
            set
            {
                _aggregator = value;
            }
        }

        #endregion
    }
}
