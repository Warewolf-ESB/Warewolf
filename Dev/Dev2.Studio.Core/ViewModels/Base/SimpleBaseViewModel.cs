using Caliburn.Micro;
using Dev2.Composition;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public abstract class SimpleBaseViewModel : Screen, IDisposable
    {
        #region Class Members

        private bool _closeRequested = false;
        private ViewModelDialogResults _viewModelResults = ViewModelDialogResults.Cancel;

        #endregion Class Members

        private ValidationController _validationController;

        public ValidationController ValidationController
        {
            get { return _validationController ?? (_validationController = new ValidationController()); }
            set
            {
                if (_validationController == value)
                    return;

                _validationController = value;
                NotifyOfPropertyChange(() => ValidationController);
            }
        }

        #region Constructor

        protected SimpleBaseViewModel()
        {
            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            if (EventAggregator != null)
                EventAggregator.Subscribe(this);
        }

        #endregion // Constructor

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
        public virtual void Dispose()
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
        public virtual void RequestClose()
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
                NotifyOfPropertyChange(() => CloseRequested);
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
                NotifyOfPropertyChange(() => DialogResult);
            }
        }

        #endregion Properties
    }
}
