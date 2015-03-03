using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;

namespace Warewolf.Studio.ViewModels
{
    public interface ISourceBase<T> 
    {
        T Item { get; set; }
        bool HasChanged { get; }
        T ToModel();

    }

    public abstract class SourceBaseImpl<T> : BindableBase, ISourceBase<T>, IDockViewModel
    {
        public SourceBaseImpl(ResourceType? image)
        {
            Image = image;
        }

        #region Implementation of ISourceBase<T>

        public T Item { get; set; }
        public bool HasChanged { get { return Item.Equals(ToModel()); } }

        abstract  public T ToModel();

        #endregion

        #region Implementation of IDockAware

        public string Header { get; set; }
        public ResourceType? Image { get; private set; }

        #endregion

        public bool IsActive { get; set; }
        public event EventHandler IsActiveChanged;
        public abstract void UpdateHelpDescriptor(string helpText);
    }

    public interface IDatabaseService
    {
    }

    public class ManageDatabaseServiceViewModel : SourceBaseImpl<IDatabaseService>, IManageDbServiceViewModel
    {
        ICollection<IDbSource> _sources;
        IManageDatabaseSourceViewModel _selectedSource;
        IDbAction _selectedAction;
        ICollection<IDbInput> _inputs;

        public ManageDatabaseServiceViewModel( bool canEditSource, ICollection<string> actions):base(ResourceType.DbService)
        {
            Actions = actions;
            CanEditSource = canEditSource;
            EditSourceCommand = new DelegateCommand(()=>{});
            Sources = new ObservableCollection<IDbSource> { new DbSourceDefinition() { Name = "bob" }, new DbSourceDefinition() { Name = "dora" }, new DbSourceDefinition() { Name = "foo large" } };
            Actions = new ObservableCollection<string> {"sp_bob","sp_the_quick_brown_fox","sp_fetchPeople"};
            Header = "New DB Service";
            Inputs = new Collection<IDbInput> { new DbInput("bob", "the"), new DbInput("dora", "eplorer") };
            CreateNewSourceCommand = new DelegateCommand(()=>{});
            TestProcedureCommand = new DelegateCommand(()=>{});
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
        public ICommand CreateNewSourceCommand { get; set; }
        public ICommand TestProcedureCommand { get;  set; }

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