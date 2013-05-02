using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Composition;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public class BaseConductor<T> : Conductor<T>.Collection.OneActive, IDisposable
        where T : IScreen
    {
        private bool _disposed;

        [Import]
        public IEventAggregator EventAggregator { get; set; }
        
        protected BaseConductor()
        {
            ImportService.TrySatisfyImports(this);

            if (EventAggregator != null)
                EventAggregator.Subscribe(this);
        }

        protected virtual void Dispose(bool disposing)
        {        
            if (!this._disposed)
            {
                if (disposing)
                {
                    // If we have any managed, IDisposable resources, Dispose of them here.
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
