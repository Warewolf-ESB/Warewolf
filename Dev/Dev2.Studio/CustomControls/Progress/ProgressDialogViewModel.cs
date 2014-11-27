
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;

namespace Dev2.CustomControls.Progress
{
    public class ProgressDialogViewModel : INotifyPropertyChanged, IProgressDialog
    {
        readonly Action _showDialog;
        readonly Action _closeDialog;
        string _label;
        string _subLabel;
        double _progressValue;
        bool _isCancelButtonEnabled;

        #region CTOR

        public ProgressDialogViewModel(Action cancelAction,
                                       Action showDialog,
                                       Action closeDialog)
        {
            VerifyArgument.IsNotNull("cancelAction", cancelAction);
            VerifyArgument.IsNotNull("showDialog", showDialog);
            VerifyArgument.IsNotNull("closeDialog", closeDialog);

            _showDialog = showDialog;
            _closeDialog = closeDialog;
            CancelCommand = new ActionCommand(cancelAction);
        }

        #endregion

        #region Properties / Events
        public void StartCancel()
        {
            SubLabel = "Please wait while the process is being cancelled...";
            IsCancelButtonEnabled = false;
        }
        
        public void Close()
        {
            _closeDialog();
        }

        public void StatusChanged(string fileName, int progressPercent, long totalBytes)
        {
            Label = string.Format("{0} downloaded {1}% of {2:0} KB", fileName, progressPercent, totalBytes / 1024);
            ProgressValue = progressPercent;
        }
        
        public ICommand CancelCommand { get; set; }

        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                if(_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SubLabel
        {
            get
            {
                return _subLabel;
            }
            set
            {
                if(_subLabel != value)
                {
                    _subLabel = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if(_progressValue != value)
                // ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    _progressValue = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsCancelButtonEnabled
        {
            get
            {
                return _isCancelButtonEnabled;
            }
            set
            {
                if(_isCancelButtonEnabled != value)
                {
                    _isCancelButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Show

        public void Show()
        {
            _showDialog();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
