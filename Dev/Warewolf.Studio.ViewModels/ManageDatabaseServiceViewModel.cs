using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    [assembly:ComVisible(false)]
    public class ManageDatabaseServiceViewModel : SourceBaseImpl<IDatabaseService>, IManageDbServiceViewModel
    {
        readonly IDbServiceModel _model;
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

        public ManageDatabaseServiceViewModel(IDbServiceModel model):base(ResourceType.DbService)
        {
            _model = model;
            CanEditSource = true;
            CreateNewSourceCommand = new DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new DelegateCommand(()=>model.EditSource(SelectedSource));
            EditSourceCommand = new DelegateCommand(()=>{});
            Sources = model.RetrieveSources();
            Item = new DatabaseService();
            Header = "New DB Service";
            Inputs = new Collection<IDbInput> { new DbInput("bob", "the"), new DbInput("dora", "eplorer"), new DbInput("Zummy", "Gummy") };
            CreateNewSourceCommand = new DelegateCommand(()=>{});
            TestProcedureCommand = new DelegateCommand(() =>
            {
                TestResults = model.TestService(GetInputValues());
                if (TestResults != null)
                {
                    CanEditMappings = true;
                    OutputMapping = new ObservableCollection<IDbOutputMapping>( _model.GetDbOutputMappings(SelectedAction));
                }

                
            });
            Inputs = new ObservableCollection<IDbInput>();
             SaveCommand = new DelegateCommand(()=>{});
             Header = "New DB Service";
        }

        IList<IDbInput> GetInputValues()
        {
            return Inputs.ToList();
        }

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
                _selectedSource = value;
                CanSelectProcedure = value != null;
                AvalaibleActions = _model.GetActions();
                OnPropertyChanged(() => Sources);
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
                _selectedAction = value;

                CanTest = _selectedAction != null;
                Inputs = _selectedAction != null ? _selectedAction.Inputs : new Collection<IDbInput>();
                OnPropertyChanged(() => Sources);
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
                    Source = SelectedSource
                };
            }
            return new DatabaseService
            {
                Action = SelectedAction,
                Inputs = Inputs == null ? new List<IDbInput>() : Inputs.ToList(),
                OutputMappings = OutputMapping,
                Source = SelectedSource
            };
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}