using System;
using Caliburn.Micro;
using Dev2.Composition;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public class BaseConductor<T> : Conductor<T>.Collection.OneActive, IDisposable
        where T : IScreen
    {
        protected readonly IEventAggregator _eventPublisher;
        private bool _disposed;

        protected BaseConductor(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);

            ImportService.TrySatisfyImports(this);
        }

        protected virtual void Dispose(bool disposing)
        {        
            if (!this._disposed)
            {
                if (disposing)
                {
                    // If we have any managed, IDisposable resources, Dispose of them here.
                    _eventPublisher.Unsubscribe(this);
                }

            }
            // Mark us as disposed, to prevent multiple calls to dispose from having an effect, 
            this._disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
