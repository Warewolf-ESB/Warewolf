using Caliburn.Micro;
using Dev2.Composition;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public abstract class SimpleBaseViewModel : BaseValidatable, IDisposable
    {
        #region Class Members

        private bool _closeRequested = false;
        private ViewModelDialogResults _viewModelResults = ViewModelDialogResults.Cancel;

        #endregion Class Members

        #region Constructor

        protected SimpleBaseViewModel()
        {
            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            if (EventAggregator != null)
                EventAggregator.Subscribe(this);
        }

        #endregion // Constructor

        #region Debugging Aids

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        //[DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region Protected Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            NotifyOfPropertyChange(propertyName);
        }

        #endregion Protected Methods

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        #endregion // IDisposable Members

        #region Methods

        /// <summary>
        /// Requests tha the view bound to this view model closes
        /// </summary>
        public void RequestClose()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        /// <summary>
        /// Requests tha the view bound to this view model closes
        /// </summary>
        public void RequestClose(ViewModelDialogResults dialogResult)
        {
            DialogResult = dialogResult;
            CloseRequested = true;
        }

        #endregion Methods

        #region Properties

        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Indicates if a close has been requested
        /// </summary>
        public bool CloseRequested
        { 
            get
            {
                return _closeRequested;
            }
            private set
            {
                _closeRequested = value;
                OnPropertyChanged("CloseRequested");
            }
        }

        public ViewModelDialogResults DialogResult
        {
            get
            {
                return _viewModelResults;
            }
            set
            {
                _viewModelResults = value;
                OnPropertyChanged("DialogResult");
            }
        }

        #endregion Properties
    }
}
