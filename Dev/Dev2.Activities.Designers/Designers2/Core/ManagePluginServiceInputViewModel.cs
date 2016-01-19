using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Activities.Designers2.Core
{
    public class ManagePluginServiceInputViewModel : BindableBase,IManagePluginServiceInputViewModel
    {
        private ICollection<IServiceInput> _inputs;
        private string _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private ManageServiceInputView _manageServiceInputView;
        private Action _testAction;
        private List<IServiceOutputMapping> _outputMappings;

        public ManagePluginServiceInputViewModel()
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
        public string TestResults
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
        public IPluginService Model { get; set; }
        public Action OkAction { get; set; }
        public List<IServiceOutputMapping> OutputMappings
        {
            get
            {
                return _outputMappings;
            }
            set
            {
                _outputMappings = value;
            }
        }

        public void ShowView()
        {
            _manageServiceInputView = new ManageServiceInputView { DataContext = this };
            _manageServiceInputView.ShowView();
        }
    }
}