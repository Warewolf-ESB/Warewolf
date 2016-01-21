using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
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

        public ManageServiceInputViewModel()
        {
            IsTesting = false;
            CloseCommand = new DelegateCommand(() =>
            {
                if(_manageServiceInputView != null)
                {
                    _manageServiceInputView.RequestClose();
                }
            });
            OkCommand = new DelegateCommand(() =>
            {
                if (_manageServiceInputView != null)
                {
                    OkAction();
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
                OnPropertyChanged(()=>Inputs);
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
                OnPropertyChanged(()=>TestResults);
            }
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
                OnPropertyChanged(()=>TestResultsAvailable);
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
                OnPropertyChanged(()=>IsTestResultsEmptyRows);
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

        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public IDatabaseService Model { get; set; }
        public Action OkAction { get; set; }

        public void ShowView()
        {
            _manageServiceInputView = new ManageServiceInputView { DataContext = this };
            _manageServiceInputView.ShowView();
        }
    }
}