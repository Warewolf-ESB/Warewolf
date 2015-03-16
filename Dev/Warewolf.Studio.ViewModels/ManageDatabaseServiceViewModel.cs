using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;

namespace Warewolf.Studio.ViewModels
{
    [ComVisible(false)]
    public class ManageDatabaseServiceViewModel : SourceBaseImpl<IDatabaseService>, IManageDbServiceViewModel
    {
        readonly IDbServiceModel _model;
        readonly IRequestServiceNameViewModel _saveDialog;
        ICollection<IDbSource> _sources;
        IDbSource _selectedSource;
        IDbAction _selectedAction;
        ICollection<IDbInput> _inputs;
        DataTable _testResults;
        IList<IDbOutputMapping> _outputMapping;
        bool _canSelectProcedure;
        bool _canEditMappings;
        bool _canTest;
        ICollection<IDbAction> _avalaibleActions;
        bool _testSuccessful;
        bool _testResultsAvailable;
        string _errorText;
        string _recordsetName;

        public ManageDatabaseServiceViewModel(IDbServiceModel model,IRequestServiceNameViewModel saveDialog):base(ResourceType.DbService)
        {
            _model = model;
            _saveDialog = saveDialog;
            CanEditSource = true;
            CreateNewSourceCommand = new DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new DelegateCommand(()=>model.EditSource(SelectedSource));
            Sources = model.RetrieveSources();
           
            Header = "New DB Service";
            TestProcedureCommand = new DelegateCommand(() =>
            {
                try
                {
                    TestResults = model.TestService(ToModel());
                    if (TestResults != null)
                    {
                        CanEditMappings = true;
                        OutputMapping =
                            new ObservableCollection<IDbOutputMapping>(GetDbOutputMappingsFromTable(TestResults));
                        TestSuccessful = true;
                        TestResultsAvailable = true;

                    }
                    ErrorText = "";
                }
                catch (Exception e)
                {
                    ErrorText = e.Message;
                    TestSuccessful = false;
                }


            },CanTestProcedure);
            Inputs = new ObservableCollection<IDbInput>();
            SaveCommand = new DelegateCommand(Save,CanSave);
        }

        public ManageDatabaseServiceViewModel(IDbServiceModel model,IRequestServiceNameViewModel saveDialog,IDatabaseService service):base(ResourceType.DbService)
        {
            _model = model;
            _saveDialog = saveDialog;
            FromService(service);
            CanEditSource = true;
            CreateNewSourceCommand = new DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new DelegateCommand(()=>model.EditSource(SelectedSource));
            Sources = model.RetrieveSources();
           
            TestProcedureCommand = new DelegateCommand(() =>
            {
                try
                {
                    TestResults = model.TestService(ToModel());
                    if (TestResults != null)
                    {
                        CanEditMappings = true;
                        OutputMapping =
                            new ObservableCollection<IDbOutputMapping>(GetDbOutputMappingsFromTable(TestResults));
                        TestSuccessful = true;
                        TestResultsAvailable = true;

                    }
                    ErrorText = "";
                }
                catch (Exception e)
                {
                    ErrorText = e.Message;
                    TestSuccessful = false;
                }


            },CanTestProcedure);
            SaveCommand = new DelegateCommand(Save,CanSave);
        }

        void FromService(IDatabaseService service)
        {
            Item = service;
            Id = service.Id;
            Name=service.Name;
            Path = service.Path;
            SelectedSource = service.Source;
            SelectedAction = service.Action;
            Inputs = service.Inputs;
            OutputMapping = service.OutputMappings;
            Header = "Edit:" + Name;
        }

        bool CanTestProcedure()
        {
            return SelectedAction != null;
        }

        bool CanSave()
        {
            return TestSuccessful;
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

                OnPropertyChanged(()=>TestSuccessful);
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
                OnPropertyChanged(()=>TestResultsAvailable);
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
                OnPropertyChanged(()=>ErrorText);
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
                if (OutputMapping != null && OutputMapping.Count > 0)
                {
                    foreach(var dbOutputMapping in OutputMapping)
                    {
                        dbOutputMapping.RecordSetName = _recordsetName;
                    }
                }
                OnPropertyChanged(() => RecordsetName);
            }
        }

        List<IDbOutputMapping> GetDbOutputMappingsFromTable(DataTable testResults)
        {
            List<IDbOutputMapping> mappings = new List<IDbOutputMapping>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < testResults.Columns.Count; i++)
            {
                var column = testResults.Columns[i];
                if (i == 0)
                {
                    RecordsetName = SelectedAction.Name;
                }
                else
                {
                    var dbOutputMapping = new DbOutputMapping(column.ToString(), column.ToString()) { RecordSetName = RecordsetName };
                    mappings.Add(dbOutputMapping);
                }
            }

            return mappings;
        }

        private void Save()
        {
            try
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
                    Header = "Edit:" + Path + Name;

                }
            }
            else
            {
                _model.SaveService(ToModel());
            }
                ErrorText = "";
            }
            catch (Exception err)
            {

                ErrorText = err.Message;
            }
        }

        public Guid Id { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        #region Implementation of IManageDbServiceViewModel

        public ICollection<IDbSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
      
                OnPropertyChanged(()=>Sources);
                
            }
        }
        public IDbSource SelectedSource
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
                    CanSelectProcedure = value != null;
                    try
                    {
                        AvalaibleActions = _model.GetActions(SelectedSource);
                        ErrorText = "";
                    }
                    catch (Exception e)
                    {
                        ErrorText = e.Message;
                        AvalaibleActions = null;
                    }
                    SelectedAction = null;
                    OnPropertyChanged(() => SelectedSource); 
                    ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
                }

            }
        }

        public IDbAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                if((!Equals(value, _selectedAction) && _selectedAction!= null) || _selectedAction == null)
                {
                    TestResultsAvailable = false;
                    CanEditMappings = false;
                    TestSuccessful = false;
                    OutputMapping = null;
                }
                
                _selectedAction = value;
                CanTest = _selectedAction != null;
                Inputs = _selectedAction != null ? _selectedAction.Inputs : new Collection<IDbInput>();
                ViewModelUtils.RaiseCanExecuteChanged(TestProcedureCommand);
                OnPropertyChanged(() => SelectedAction);
            }
        }
        public ICollection<IDbAction> AvalaibleActions
        {
            get
            {
                return _avalaibleActions;
            }
            set
            {
                _avalaibleActions = value;
                OnPropertyChanged(()=>AvalaibleActions);
            }
        }

        public string DataSourceHeader
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceDBSourceHeader;

            }
        }
        public string DataSourceActionHeader
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceActionHeader;
            }
        }
        public ICommand EditSourceCommand { get; private set; }
        public bool CanEditSource { get; private set; }
        public string NewButtonLabel
        {
            get
            {
                return Resources.Languages.Core.New;
            }
        }

        public string MappingsHeader
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceMappingsHeader;
            }
        }
        public string TestHeader
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceTestHeader;
            }
        }
        public string InputsLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceInputsHeader;
            }
        }

        public string OutputsLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceOutputsLabel;
            }
        }

        public ICollection<IDbInput> Inputs
        {
            get
            {
                return _inputs;
            }
            private set
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
        public ICommand CreateNewSourceCommand { get; set; }
        public ICommand TestProcedureCommand { get;  set; }
        public IList<IDbOutputMapping> OutputMapping
        {
            get
            {
                return _outputMapping;
            }
            set
            {
                _outputMapping = value;
                OnPropertyChanged(()=>OutputMapping);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }
        public ICommand SaveCommand { get;  set; }
        public bool CanSelectProcedure
        {
            get
            {
                return _canSelectProcedure;
            }
            set
            {
                _canSelectProcedure = value;
                OnPropertyChanged(() => CanSelectProcedure);
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
                OnPropertyChanged(()=>CanTest);
            }
        }

        #endregion

        #region Implementation of ISourceBase<IDatabaseService>



        public override IDatabaseService ToModel()
        {
            if(Item!= null)
            {
                return new DatabaseService
                {
                    Name= Item.Name,
                    Action = SelectedAction,
                    Inputs = Inputs==null? new List<IDbInput>(): Inputs.ToList(),
                    OutputMappings = OutputMapping,
                    Source = SelectedSource,
                    Path = Item.Path,
                    Id = Id
                };
            }
            return new DatabaseService
            {
                Action = SelectedAction,
                Inputs = Inputs == null ? new List<IDbInput>() : Inputs.ToList(),
                OutputMappings = OutputMapping,
                Source = SelectedSource,
                Name=Name,
                Path = Path,
                Id = Id
            };
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}