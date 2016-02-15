
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Activities.Designers2.Core
{
    public class ManagePluginServiceInputViewModel : BindableBase, IManagePluginServiceInputViewModel
    {
        private ICollection<IServiceInput> _inputs;
        private string _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private ManagePluginServiceInputView _manageServiceInputView;
        private Action _testAction;
        private bool _okSelected;

        public ManagePluginServiceInputViewModel()
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
        public string TestResults
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

        public ImageSource TestIconImageSource
        {
            get
            {
                return Application.Current.TryFindResource("Explorer-DLL-White") as DrawingImage;
            }
            set
            {
                
            }
        }
        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public Action CloseAction { get; set; }
        public IPluginService Model { get; set; }
        public string TestHeader { get; set; }
        public Action OkAction { get; set; }
        public List<IServiceOutputMapping> OutputMappings { get; set; }
        public IOutputDescription Description { get; set; }

        public void ShowView()
        {
            _manageServiceInputView = new ManagePluginServiceInputView { DataContext = this };
            _manageServiceInputView.ShowView();
        }

        public void CloseView()
        {
            if (_manageServiceInputView != null)
            {
                _manageServiceInputView.RequestClose();
            }
        }
    }
}