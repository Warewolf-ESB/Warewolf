
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
//using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Activities.Designers2.Core
{
    public class ManageServiceInputViewModel : BindableBase,IManageServiceInputViewModel
    {
        private ICollection<IServiceInput> _inputs;
        private DataTable _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private ManageServiceInputView _manageServiceInputView;
        private Action _testAction;
        private bool _okSelected;

        public ManageServiceInputViewModel()
        {
            IsTesting = false;
            CloseCommand = new DelegateCommand(() =>
            {
                if (_manageServiceInputView != null)
                {
                    _manageServiceInputView.RequestClose();
                }
            });
            OkCommand = new DelegateCommand(() =>
            {
                if (_manageServiceInputView != null)
                {
                    OkAction();
                    OkSelected = true;
                    _manageServiceInputView.RequestClose();
                }
            });
        }

        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
                OnPropertyChanged(() => Inputs);
            }
        }
        public DataTable TestResults
        {
            get
            {
                return _testResults;
            }
            set
            {
                _testResults = value;
                OnPropertyChanged(() => TestResults);
            }
        }

        public bool OkSelected
        {
            get { return _okSelected; }
            set
            {
                _okSelected = value;
                OnPropertyChanged(() => OkSelected);
            }
        }
        public IGenerateOutputArea OutputArea { get; set; }
        public IOutputDescription Description { get; set; }
        public IGenerateInputArea InputArea { get; set; }

        public void SetInitialVisibility()
        {
        }

        public Action TestAction
        {
            get
            {
                return _testAction;
            }
            set
            {
                _testAction = value;
                TestCommand = new DelegateCommand(value);
            }
        }
        public ICommand TestCommand { get; private set; }
        public bool TestResultsAvailable
        {
            get
            {
                return _testResultsAvailable;
            }
            set
            {
                _testResultsAvailable = value;
                if (_testResultsAvailable)
                {
                    _manageServiceInputView.OutputDataGridResize();
                }
                OnPropertyChanged(() => TestResultsAvailable);
            }
        }
        public bool IsTestResultsEmptyRows
        {
            get
            {
                return _isTestResultsEmptyRows;
            }
            set
            {
                _isTestResultsEmptyRows = value;
                OnPropertyChanged(() => IsTestResultsEmptyRows);
            }
        }

        public bool IsTesting
        {
            get
            {
                return _isTesting;
            }
            set
            {
                _isTesting = value;
                OnPropertyChanged(() => IsTesting);
            }
        }

        public ImageSource TestIconImageSource { get; set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public Action CloseAction { get; set; }
        public IDatabaseService Model { get; set; }
        public Action OkAction { get; set; }

        public string TestHeader { get; set; }

        public void ShowView()
        {
            _manageServiceInputView = new ManageServiceInputView { DataContext = this };
            _manageServiceInputView.ShowView();
        }

        public void CloseView()
        {
            if (_manageServiceInputView != null)
            {
                _manageServiceInputView.RequestClose();
            }
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public double MinHeight { get; set; }
        public double CurrentHeight { get; set; }
        public bool IsVisible { get; set; }
        public double MaxHeight { get; set; }
        //public event HeightChanged HeightChanged;
        public IList<IToolRegion> Dependants { get; set; }
        //public IList<string> Errors { get; private set; }

        public IToolRegion CloneRegion()
        {
            return null;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        #endregion
    }
}