using Dev2.Providers.Events;

namespace Dev2.Services.Events
{
    public static class EventPublishers
    {
        static volatile IEventPublisher _studioPublisher;
        static readonly object StudioLock = new object();

        #region Studio

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

    }
}
