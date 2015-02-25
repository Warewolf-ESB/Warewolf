using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Warewolf.Studio.ViewModels
{
    public interface ISourceBase<T> 
    {
        T Item { get; set; }
        bool HasChanged { get; }
        T ToModel();

    }

    public abstract class SourceBaseImpl<T> : ISourceBase<T>
    {
        #region Implementation of ISourceBase<T>

        public T Item { get; set; }
        public bool HasChanged { get { return Item.Equals(ToModel()); } }

        abstract  public T ToModel();

        #endregion
    }

    public interface IDatabaseService
    {
    }

    public class ManageDatabaseServiceViewModel : ISourceBase<IDatabaseService>, IManageDbServiceViewModel
    {
        ICollection<IManageDatabaseSourceViewModel> _sources;
        IManageDatabaseSourceViewModel _selectedSource;
        IDbAction _selectedAction;

        public ManageDatabaseServiceViewModel(ICommand editSourceCommand, bool canEditSource, ICollection<string> actions)
        {
            Actions = actions;
            CanEditSource = canEditSource;
            EditSourceCommand = editSourceCommand;
        }

        #region Implementation of IManageDbServiceViewModel

        public ICollection<IManageDatabaseSourceViewModel> Sources
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
                return Resources.Languages.Core.CancelTest;
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

        #endregion

        #region Implementation of ISourceBase<IDatabaseService>

        public IDatabaseService Item { get; set; }
        public bool HasChanged { get { return true; } }

        public IDatabaseService ToModel()
        {
            return null;
        }

        #endregion
    }


}