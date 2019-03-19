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
using Caliburn.Micro;


namespace Dev2.Studio.Core.ViewModels.Base

{
    public abstract class SimpleBaseViewModel : Screen, IDisposable
    {
        #region Class Members

        bool _closeRequested;
        ViewModelDialogResults _viewModelResults = ViewModelDialogResults.Cancel;

        public bool IsDisposed {private set; get;}

        #endregion Class Members

        ValidationController _validationController;

        public ValidationController ValidationController
        {
            get { return _validationController ?? (_validationController = new ValidationController()); }
            set
            {
                if(_validationController == value)
                {
                    return;
                }

                _validationController = value;
                NotifyOfPropertyChange(() => ValidationController);
            }
        }

        #region Protected Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            NotifyOfPropertyChange(propertyName);
        }

        #endregion Protected Methods

        #region IDisposable Members
        
        protected virtual void OnDispose()
        {
            _validationController?.Dispose();
        }

        ~SimpleBaseViewModel()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);

        }
        
        void Dispose(bool disposing)
        {
            if(!IsDisposed)
            {
                if(disposing)
                {
                    OnDispose();
                }
                IsDisposed = true;
            }
        }

        #endregion

        #region Methods
        
        public virtual void RequestClose()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }
        
        public void RequestClose(ViewModelDialogResults dialogResult)
        {
            DialogResult = dialogResult;
            CloseRequested = true;
        }

        #endregion Methods

        #region Properties
        
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
