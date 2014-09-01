using System;
using Caliburn.Micro;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Base
{
    public class BaseConductor<T> : Conductor<T>.Collection.OneActive, IDisposable
        where T : IScreen
    {
        protected readonly IEventAggregator EventPublisher;
        private bool _disposed;

        protected BaseConductor(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            EventPublisher = eventPublisher;
            EventPublisher.Subscribe(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if(disposing)
                {
                    // If we have any managed, IDisposable resources, Dispose of them here.
                    EventPublisher.Unsubscribe(this);
                }

            }
            // Mark us as disposed, to prevent multiple calls to dispose from having an effect, 
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
