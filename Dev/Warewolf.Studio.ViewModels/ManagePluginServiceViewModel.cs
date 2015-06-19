using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.PluginService;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManagePluginServiceViewModel : SourceBaseImpl<IPluginService>, IPluginServiceViewModel
    {
        readonly IPluginServiceModel _model;
        readonly IRequestServiceNameViewModel _saveDialog;
        string _mappingsHeader;
        ICollection<IServiceInput> _inputs;
        IList<IServiceOutputMapping> _outputMapping;
        string _recordsetName;
        bool _isInputsEmptyRows;
        bool _isOutputMappingEmptyRows;
        ICollection<IPluginSource> _sources;
        IPluginSource _selectedSource;
        IPluginAction _selectedAction;
        IPluginAction _refreshSelectedAction;
        ICollection<IPluginAction> _avalaibleActions;
        string _pluginSourceHeader;
        string _pluginSourceActionHeader;
        bool _canEditSource;
        bool _canEditNamespace;
        string _newButtonLabel;
        string _testHeader;
        string _inputsLabel;
        string _outputsLabel;
        bool _isRefreshing;
        bool _inputsRequired;
        string _testResults;
        bool _isTestResultsEmptyRows;
        bool _isTesting;
        bool _canSelectMethod;
        bool _canEditMappings;
        bool _canTest;
        string _path;
        string _name;
        Guid _id;
        bool _testSuccessful;
        bool _testResultsAvailable;
        string _errorText;
        bool _showResults;
        RecordsetList _responseService;
        bool _canSave;
        ICollection<INamespaceItem> _nameSpaces;
        INamespaceItem _selectedNamespace;
        ICollection<NameValue> _inputValues;

        #region Implementation of IServiceMappings

        public ManagePluginServiceViewModel(IPluginServiceModel model, IRequestServiceNameViewModel saveDialog)
            : base(ResourceType.PluginService)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "model", model }, { "saveDialog", saveDialog } });
            _model = model;
            _saveDialog = saveDialog;
            Inputs = new ObservableCollection<IServiceInput>();
            OutputMapping = new ObservableCollection<IServiceOutputMapping>();
            AvalaibleActions = new ObservableCollection<IPluginAction>();
            NameSpaces = new ObservableCollection<INamespaceItem>();
            Sources = _model.RetrieveSources();
            Header = Resources.Languages.Core.PluginServiceNewHeaderLabel;
            RefreshCommand = new DelegateCommand(Refresh);
            ErrorText = "";
            TestPluginCommand = new DelegateCommand(() => Test(_model));
            SaveCommand = new DelegateCommand(() => Save(ToModel()), () => CanSave);
            CreateNewSourceCommand = new DelegateCommand(() => _model.CreateNewSource());
            EditSourceCommand = new DelegateCommand(() => _model.EditSource(SelectedSource));
        }

        void Save(IPluginService toModel)
        {
            if (Item == null)
            {
                var saveOutPut = _saveDialog.ShowSaveDialog();
                if (saveOutPut == MessageBoxResult.OK || saveOutPut == MessageBoxResult.Yes)
                {
                    Name = _saveDialog.ResourceName.Name;
                    Path = _saveDialog.ResourceName.Path;
                    Id = Guid.NewGuid();
                    _model.SaveService(ToModel());
                    Item = ToModel();
                    Header = Path + Name;

                }
            }
            else
            {
                _model.SaveService(ToModel());
            }
            ErrorText = "";
        }

        public string MappingsHeader
        {
            get
            {
                return _mappingsHeader;
            }
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
                IsInputsEmptyRows = true;
                if (_inputs != null && _inputs.Count >= 1)
                {
                    IsInputsEmptyRows = false;
                }
                OnPropertyChanged(()=>Inputs);
            }
        }
        public IList<IServiceOutputMapping> OutputMapping
        {
            get
            {
                return _outputMapping;
            }
            set
            {
                _outputMapping = value;
                IsOutputMappingEmptyRows = true;
                if (_outputMapping != null)
                {
                    if (_outputMapping.Count >= 1)
                    {
                        IsOutputMappingEmptyRows = false;
                    }
                }
                OnPropertyChanged(() => OutputMapping);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }
        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                _recordsetName = value;
                OnPropertyChanged(() => RecordsetName);
            }
        }
        public bool IsInputsEmptyRows
        {
            get
            {
                return _isInputsEmptyRows;
            }
            set
            {
                _isInputsEmptyRows = value;
                OnPropertyChanged(() => IsInputsEmptyRows);
            }
        }
        public bool IsOutputMappingEmptyRows
        {
            get
            {
                return _isOutputMappingEmptyRows;
            }
            set
            {
                _isOutputMappingEmptyRows = value;
                OnPropertyChanged(() => IsOutputMappingEmptyRows);
            }
        }

        #endregion

        #region Implementation of IPluginServiceViewModel

        public ICollection<IPluginSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged(() => Sources);
            }
        }
        public IPluginSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                if (!Equals(_selectedSource, value))
                {
                    _selectedSource = value;
                    CanTest = false;
                    CanEditMappings = false;
                    CanSelectMethod = false;
                    CanEditNamespace = true;
                    NameSpaces = new ObservableCollection<INamespaceItem>(_model.GetNameSpaces(value));
                    OnPropertyChanged(() => SelectedSource);
                    ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
                }
            }
        }

        void Test(IPluginServiceModel model)
        {
            try
            {
                IsTesting = true;
                var serializer = new Dev2JsonSerializer();
                ResponseService = serializer.Deserialize<RecordsetList>( model.TestService(ToModel()));
                UpdateMappingsFromResponse();
                ErrorText = "";
                CanEditMappings = true;
                CanSave = true;
                IsTesting = false;
               
            }
            catch (Exception err)
            {
                ErrorText = err.Message;
                OutputMapping = new ObservableCollection<IServiceOutputMapping>();
                IsTesting = false;
                CanEditMappings = false;
            }


        }
        void UpdateMappingsFromResponse()
        {
            if (ResponseService != null)
            {
                Response = ResponseService;
                // ReSharper disable MaximumChainedReferences
                //var outputMapping = ResponseService.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                //{
                //    RecordsetName = recordset.Name;
                //    var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias) { RecordSetName = recordset.Name };
                //    return serviceOutputMapping;
                //}).Cast<IServiceOutputMapping>().ToList();
                // ReSharper restore MaximumChainedReferences

                OutputMapping = new IServiceOutputMapping[]{new ServiceOutputMapping("Name","Name"){RecordSetName = "rec"} };

            }
        }
        public RecordsetList ResponseService
        {
            get
            {
                return _responseService;
            }
            set
            {
                _responseService = value;
                OnPropertyChanged(() => ResponseService);
            }
        }

        public IPluginAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                if ((!Equals(value, _selectedAction) && _selectedAction != null) || _selectedAction == null)
                {
                    IsTestResultsEmptyRows = false;
                    TestResultsAvailable = false;
                    CanEditMappings = false;
                    TestSuccessful = false;
                    OutputMapping = null;
                }
                _selectedAction = value;
                CanTest = _selectedAction != null;
                Inputs = _selectedAction != null ? _selectedAction.Inputs : new Collection<IServiceInput>();
                if(_selectedAction != null)
                {
                    InputValues = new ObservableCollection<NameValue>(_selectedAction.Variables.Select(a=>a as NameValue));
                }

                OnPropertyChanged(() => SelectedAction);
            }
        }
        public ICollection<NameValue> InputValues
        {
            get
            {
                return _inputValues;
            }
            set
            {
                _inputValues = value;
                OnPropertyChanged(()=>InputValues);
            }
        }
        public ICollection<IPluginAction> AvalaibleActions
        {
            get
            {
                return _avalaibleActions;
            }
            set
            {
                _avalaibleActions = value;
                OnPropertyChanged(() => AvalaibleActions);
            }
        }
        public string PluginSourceHeader
        {
            get
            {
                return _pluginSourceHeader;
            }
        }
        public string PluginSourceActionHeader
        {
            get
            {
                return _pluginSourceActionHeader;
            }
        }
        public ICommand EditSourceCommand
        {
            get;
            set;
        }
        public bool CanEditSource
        {
            get
            {
                return _canEditSource;
            }
            set
            {
                _canEditSource = value;
                OnPropertyChanged(()=>CanEditSource);
            }
        }
        public bool CanEditNamespace
        {
            get
            {
                return _canEditSource;
            }
            set
            {
                _canEditSource = value;
                OnPropertyChanged(() => CanEditNamespace);
            }
        }
        public string NewButtonLabel
        {
            get
            {
                return _newButtonLabel;
            }
        }
        public string TestHeader
        {
            get
            {
                return _testHeader;
            }
        }
        public string InputsLabel
        {
            get
            {
                return _inputsLabel;
            }
        }
        public string OutputsLabel
        {
            get
            {
                return _outputsLabel;
            }
        }
        public ICommand RefreshCommand { get; set; }
        private void Refresh()
        {
            IsRefreshing = true;
            PerformRefresh();
            IsRefreshing = false;
        }
        private void PerformRefresh()
        {
            try
            {
                _refreshSelectedAction = SelectedAction;
                AvalaibleActions = new ObservableCollection<IPluginAction>(_model.GetActions(SelectedSource, SelectedNamespace));
                SelectedAction = AvalaibleActions.FirstOrDefault(action => _refreshSelectedAction != null && action.FullName == _refreshSelectedAction.FullName);
                ErrorText = "";
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
                AvalaibleActions = null;
            }
        }
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(()=>IsRefreshing);
            }
        }
        public bool InputsRequired
        {
            get
            {
                return _inputsRequired;
            }
            set
            {
                _inputsRequired = value;
                OnPropertyChanged(() => InputsRequired);
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
        public ICommand CreateNewSourceCommand { get; set; }
        public ICommand TestPluginCommand { get; set; }
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
        public ICommand SaveCommand { get; set; }
        public bool CanSelectMethod
        {
            get
            {
                return _canSelectMethod;
            }
            set
            {
                _canSelectMethod = value;
                OnPropertyChanged(() => CanSelectMethod);
            }
        }
        public bool CanEditMappings
        {
            get
            {
                return _canEditMappings;
            }
            set
            {
                _canEditMappings = value;
                OnPropertyChanged(() => CanEditMappings);
            }
        }
        public bool CanTest
        {
            get
            {
                return _canTest;
            }
            set
            {
                _canTest = value;
                OnPropertyChanged(() => CanTest);
            }
        }
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
            }
        }
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged(() => Id);
            }
        }
        public bool TestSuccessful
        {
            get
            {
                return _testSuccessful;
            }
            set
            {
                _testSuccessful = value;
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
                OnPropertyChanged(() => TestSuccessful);
            }
        }
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
        public string ErrorText
        {
            get
            {
                return _errorText;
            }
            set
            {
                _errorText = value;
                OnPropertyChanged(() => ErrorText);
            }
        }
        public bool ShowResults
        {
            get
            {
                return _showResults;
            }
            set
            {
                _showResults = value;
                OnPropertyChanged(() => ShowResults);
            }
        }

        #endregion

        #region Overrides of SourceBaseImpl<IPluginService>

        public override IPluginService ToModel()
        {
            if (Item != null)
            {
                return new PluginServiceDefinition()
                {
                    Name = Item.Name,
                    Action = SelectedAction,
                    Inputs = Inputs == null ? new List<IServiceInput>() : Inputs.ToList(),
                    OutputMappings = OutputMapping,
                    Source = SelectedSource,
                    Path = Item.Path,
                    Id = Id
                };
            }
            return new PluginServiceDefinition
            {
                Action = SelectedAction,
                Inputs = Inputs == null ? new List<IServiceInput>() : Inputs.ToList(),
                OutputMappings = OutputMapping,
                Source = SelectedSource,
                Name = Name,
                Path = Path,
                Id = Id
            };

        }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        public RecordsetList Response { get; set; }
        public bool CanSave
        {
            get
            {
                return _canSave;
            }
            set
            {
                _canSave = value;
            }
        }
        public ICollection<INamespaceItem> NameSpaces
        {
            get
            {
                return _nameSpaces;
            }
            set
            {
                _nameSpaces = value;
                OnPropertyChanged(()=>NameSpaces);
            }
        }
        public INamespaceItem SelectedNamespace
        {
            get
            {
                return _selectedNamespace;
            }
            set
            {
                _selectedNamespace = value;
                if (SelectedNamespace != null)
                {
                    CanTest = false;
                    CanEditMappings = false;
                    CanSelectMethod = true;
                    AvalaibleActions = new ObservableCollection<IPluginAction>(_model.GetActions(SelectedSource, value));
                }
                OnPropertyChanged(() => SelectedNamespace);
            }
        }
    }
}
