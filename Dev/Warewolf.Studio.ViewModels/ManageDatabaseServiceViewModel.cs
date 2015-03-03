using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ManageDatabaseServiceViewModel : SourceBaseImpl<IDatabaseService>, IManageDbServiceViewModel
    {
        readonly IDbServiceModel _model;
        ICollection<IDbSource> _sources;
        IManageDatabaseSourceViewModel _selectedSource;
        IDbAction _selectedAction;
        ICollection<IDbInput> _inputs;
        DataTable _testResults;
        IList<IDbOutputMapping> _outputMapping;
        bool _canSelectProcedure;
        bool _canEditMappings;
        bool _canTest;

        
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
            TestProcedureCommand = new DelegateCommand(()=>{});
            TestResults = new DataTable("Results");
            TestResults.Columns.Add(new DataColumn("Record Name"));
            TestResults.Columns.Add(new DataColumn("Windows Group"));
            TestResults.Columns.Add(new DataColumn("Response"));
            TestResults.Columns.Add(new DataColumn("Bob"));
            TestResults.Rows.Add(new object[] { "dbo_Save_person(1)", "asdasd", "dasdasd", "111" });
            TestResults.Rows.Add(new object[] { "dbo_Save_person(2)", "qweqwe", "dasfghfgdasd", "111" });
            TestResults.Rows.Add(new object[] { "dbo_Save_person(3)", "fghfhf", "fgh", "111" });
            OutputMapping = new List<IDbOutputMapping>() { new DbOutputMapping("bob", "The"), new DbOutputMapping("dora", "The"), new DbOutputMapping("Tree", "The") };
            SaveCommand = new DelegateCommand(()=>{});
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
        public IManageDatabaseSourceViewModel SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                _selectedSource = value;
                CanSelectProcedure = value != null;
                Actions = new ObservableCollection<string> { "sp_bob", "sp_the_quick_brown_fox", "sp_fetchPeople" };
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
                OnPropertyChanged(() => Sources);
            }
        }
        public ICollection<IDbAction> AvalaibleActions { get; set; }
        public IDbOutput Outputs { get; set; }
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
        public ICollection<string> Actions { get; private set; }
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
        public string InspectLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceInspectHeader;
            }
        }
        public string OutputsLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceOutputsLabel;
            }
        }
        public string MappingNamesHeader
        {
            get
            {
                return Resources.Languages.Core.DatabaseServiceMappingsNameHeader;
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
            return null;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}