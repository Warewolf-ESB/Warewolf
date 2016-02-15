using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.WebServices;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Core
{
    public class ManageWebServiceInputViewModel : BindableBase, IManageWebServiceInputViewModel
    {
        private ICollection<IServiceInput> _inputs;
        private string _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private Action _testAction;
        private bool _okSelected;
        private IWebService _model;
        bool _pasteResponseVisible;
        bool _pasteResponseAvailable;

        public ManageWebServiceInputViewModel()
        {
            PasteResponseAvailable = true;
            IsTesting = false;
            CloseCommand = new DelegateCommand(ExecuteClose);
            OkCommand = new DelegateCommand(ExecuteOk);
            PasteResponseCommand = new DelegateCommand(ExecutePaste);
            TestCommand = new DelegateCommand(ExecuteTest);
        }

        [ExcludeFromCodeCoverage]
        void ExecuteTest()
        {
            TestAction();
            PasteResponseVisible = false;
        }

        [ExcludeFromCodeCoverage]
        void ExecutePaste()
        {
            PasteResponseVisible = true;
        }

        [ExcludeFromCodeCoverage]
        void ExecuteOk()
        {
            OkAction();
            OkSelected = true;
        }

        [ExcludeFromCodeCoverage]
        private void ExecuteClose()
        {
            CloseAction();
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

        public bool PasteResponseVisible
        {
            get
            {
                return _pasteResponseVisible;
            }
            set
            {
                _pasteResponseVisible = value;
                OnPropertyChanged(() => PasteResponseVisible);
            }
        }

        public bool PasteResponseAvailable
        {
            get
            {
                return _pasteResponseAvailable;
            }
            set
            {
                _pasteResponseAvailable = value;
                OnPropertyChanged(() => PasteResponseAvailable);
            }
        }

        public ImageSource TestIconImageSource
        {
            get
            {
                return Application.Current.TryFindResource("Explorer-WebService-White") as DrawingImage;
            }
            set
            {

            }
        }
        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public Action CloseAction { get; set; }
        public IWebService Model
        {
            get
            {
                var model = new WebServiceDefinition()
                {
                    Headers = new List<NameValue>(_model.Headers.Select(a => new NameValue(ReplaceString(a.Name), ReplaceString(a.Name)))),
                    QueryString = ReplaceString(_model.QueryString),
                    Id = _model.Id,
                    Path = _model.Path,
                    PostData = _model.PostData,
                    Inputs = _model.Inputs,
                    OutputMappings = _model.OutputMappings,
                    Method = _model.Method,
                    Name = _model.Name,
                    Response = _model.Response,
                    Source = _model.Source,
                    SourceUrl = _model.SourceUrl

                };
                return model;
            }
            set
            {
                _model = value;
            }
        }
        public string TestHeader { get; set; }

        private string ReplaceString(string name)
        {
            if (Inputs == null)
                return name;
            return Inputs.Aggregate(name, (current, serviceInput) => current.Replace(serviceInput.Name, serviceInput.Value));

        }

        public Action OkAction { get; set; }
        public ICommand PasteResponseCommand { get; private set; }
        public List<IServiceOutputMapping> OutputMappings { get; set; }
        public IOutputDescription Description { get; set; }
        [ExcludeFromCodeCoverage]
        public virtual void ShowView()
        {
        }
        [ExcludeFromCodeCoverage]
        public void CloseView()
        {
        }
    }
}