using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public abstract class BaseDisposable : IDisposable
    {
        #region Instance Fields
        private bool _isDisposed;
        private bool _isDisposing;
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets a value indicating whether or not this object is in the process of disposing.
        /// </summary>
        protected bool IsDisposing { get { return _isDisposing; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets a value indicating whether or not the Dispose method has been called.
        /// </summary>
        public bool IsDisposed { get { return _isDisposed; } }
        #endregion

        #region Disposal Handling
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _isDisposing = true;
                OnDisposing();
                OnDispose();
            }
            finally
            {
                _isDisposing = false;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Raised when the Dispose method is invoked. Occurs before OnDispose is raised. If this method is raised, Finalization is suppressed and
        /// the Finalize method will not be raised.
        /// </summary>
        protected virtual void OnDisposing() { }
        /// <summary>
        /// Raised when the Dispose method is invoked. Occurs after OnDisposing is raised. If this method is raised, Finalization is suppressed and
        /// the Finalize method will not be raised. Both managed and unmanaged resources should be released by OnDispose
        /// </summary>
        protected virtual void OnDispose() { }
        #endregion
    }
}
