﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.ViewModels.UiAutomation
{    
    public class AutomationIdCreaterViewModel :BaseViewModel
    {
        #region Fields

        public ICommand _OkCommand;
        public ICommand _CancelCommand;     

        #endregion Fields

        #region Properties

        public string AutomationID { get; set; }        

        #endregion Properties

        #region Ctor

        public AutomationIdCreaterViewModel()
        {
            
        }

        #endregion Ctor

        #region Methods

        public ICommand OkCommand
        {
            get
            {
                if (_OkCommand == null)
                {
                    _OkCommand = new RelayCommand(param => { SaveID(); });
                }
                return _OkCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(param => { Cancel(); });
                }
                return _CancelCommand;
            }
        }

        public void SaveID()
        {
            RequestClose(ViewModelDialogResults.Okay);
        }

        public void Cancel()
        {            
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion Methods
    }
}
