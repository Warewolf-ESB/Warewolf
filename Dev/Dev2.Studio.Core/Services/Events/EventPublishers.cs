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
