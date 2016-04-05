using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Exchange;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Activities.Designers2.Core
{
    public class ManageExchangeServiceInputViewModel : IManageExchangeInputViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; }
        private string _testResults;
        public IToolRegion CloneRegion()
        {
            return this;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            
        }

        public Action TestAction { get; set; }
        public ICommand TestCommand { get; }
        public bool TestResultsAvailable { get; set; }
        public bool IsTestResultsEmptyRows { get; set; }
        public bool IsTesting { get; set; }
        public ImageSource TestIconImageSource { get; set; }
        public ICommand CloseCommand { get; }
        public ICommand OkCommand { get; }
        public Action OkAction { get; set; }
        public Action CloseAction { get; set; }
        public IExchangeService Model { get; set; }
        public string TestHeader { get; set; }
        private bool _okSelected;
        private readonly IExchangeServiceViewModel _viewmodel;
        private readonly IExchangeServiceModel _serverModel;
        readonly IGenerateOutputArea _generateOutputArea;
        readonly IGenerateInputArea _generateInputArea;

        public ManageExchangeServiceInputViewModel(IExchangeServiceViewModel model, IExchangeServiceModel serviceModel)
        {
            IsTesting = false;
            CloseCommand = new DelegateCommand(ExecuteClose);
            OkCommand = new DelegateCommand(ExecuteOk);
            TestCommand = new DelegateCommand(ExecuteTest);
            _generateOutputArea = new GenerateOutputsRegion();
            _generateInputArea = new GenerateInputsRegion();
            Errors = new List<string>();
            _viewmodel = model;
            _serverModel = serviceModel;
        }

        public void ExecuteClose()
        {
            
        }

        public void ExecuteOk()
        {
            try
            {
               
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                _viewmodel.ErrorMessage(e, true);
            }

            OkSelected = true;
        }

        public void ExecuteTest()
        {
            OutputArea.IsEnabled = true;
            TestResults = null;
            IsTesting = true;

            try
            {
                TestResults = _serverModel.TestService(Model);
                if (TestResults != null)
                {
                    //TestResultsAvailable = TestResults.Rows.Count != 0;
                    //IsTestResultsEmptyRows = TestResults.Rows.Count < 1;
                    //_generateOutputArea.IsEnabled = true;
                    //OutputCountExpandAllowed = TestResults.Rows.Count > 3;

                    if (!OutputCountExpandAllowed)
                    {
                        InputCountExpandAllowed = true;
                    }

                    IsTesting = false;
                }
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                _generateOutputArea.IsEnabled = false;
                _generateOutputArea.Outputs = new List<IServiceOutputMapping>();
                _viewmodel.ErrorMessage(e, true);
            }
        }

        void ResetOutputsView()
        {
            IsEnabled = false;
            _viewmodel.GenerateOutputsVisible = false;
            InputArea.IsEnabled = false;
            OutputArea.IsEnabled = false;
            TestResults = string.Empty;
            TestResultsAvailable = false;

            _viewmodel.SetDisplayName("");
            _viewmodel.ErrorMessage(new Exception(), false);
        }

        [ExcludeFromCodeCoverage]
        public void ShowView()
        {
           
        }

        [ExcludeFromCodeCoverage]
        public void CloseView()
        {
           
        }

        public ICollection<IServiceInput> Inputs { get; set; }
        public bool OutputCountExpandAllowed { get; set; }
        public bool InputCountExpandAllowed { get; set; }
        public bool IsGenerateInputsEmptyRows { get; set; }

        #region Implementation of IManageDatabaseInputViewModel

        public string TestResults
        {
            get
            {
                return _testResults;
            }
            set
            {
                _testResults = value;
                OnPropertyChanged();
            }
        }
        public bool OkSelected
        {
            get
            {
                return _okSelected;
            }
            set
            {
                _okSelected = value;
                OnPropertyChanged();
            }
        }
        public IGenerateOutputArea OutputArea
        {
            get
            {
                return _generateOutputArea;
            }
            [ExcludeFromCodeCoverage]
            set
            {

            }
        }
        public IOutputDescription Description { get; set; }
        public IGenerateInputArea InputArea
        {
            get
            {
                return _generateInputArea;
            }
            [ExcludeFromCodeCoverage]
            set
            {

            }
        }

        public void SetInitialVisibility()
        {
            IsEnabled = true;
            InputArea.IsEnabled = true;
            OutputArea.IsEnabled = false;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
