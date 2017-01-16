using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Resource.Errors;

// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.Source
{
    public class DatabaseSourceRegion : ISourceToolRegion<IDbSource>
    {
        private IDbSource _selectedSource;
        private ICollection<IDbSource> _sources;
        private readonly ModelItem _modelItem;

        private Guid _sourceId;
        private Action _sourceChangedAction;
        private double _labelWidth;
        private string _sourcesHelpText;
        private string _editSourceHelpText;
        private string _newSourceHelpText;
        private string _newSourceToolText;
        private string _editSourceToolText;
        private string _sourcesToolText;

        public DatabaseSourceRegion(IDbServiceModel model, ModelItem modelItem,enSourceType type)
        {
            LabelWidth = 46;
            ToolRegionName = "DatabaseSourceRegion";
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.CreateNewSource(type));
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource, type), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.Where(source => source != null && source.Type == type).ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.HelpText.DatabaseServiceSourceTypesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.DatabaseServiceEditSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.DatabaseServiceNewSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageDbServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageDbServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageDbServiceNewSourceTooltip;

            if (SourceId != Guid.Empty)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
            }
        }
        public string NewSourceHelpText
        {
            get
            {
                return _newSourceHelpText;
            }
            set
            {
                _newSourceHelpText = value;
                OnPropertyChanged();
            }
        }

        public string EditSourceHelpText
        {
            get
            {
                return _editSourceHelpText;
            }
            set
            {
                _editSourceHelpText = value;
                OnPropertyChanged();
            }
        }
        public string SourcesHelpText
        {
            get
            {
                return _sourcesHelpText;
            }
            set
            {
                _sourcesHelpText = value;
                OnPropertyChanged();
            }
        }

        public string NewSourceTooltip
        {
            get
            {
                return _newSourceToolText;
            }
            set
            {
                _newSourceToolText = value;
                OnPropertyChanged();
            }
        }

        public string EditSourceTooltip
        {
            get
            {
                return _editSourceToolText;
            }
            set
            {
                _editSourceToolText = value;
                OnPropertyChanged();
            }
        }
        public string SourcesTooltip
        {
            get
            {
                return _sourcesToolText;
            }
            set
            {
                _sourcesToolText = value;
                OnPropertyChanged();
            }
        }

        public DatabaseSourceRegion()
        {
            
        }

        Guid SourceId
        {
            get
            {
                return _sourceId;
            }
            set
            {
                _sourceId = value;
                _modelItem?.SetProperty("SourceId", value);
            }
        }

        public bool CanEditSource()
        {
            return SelectedSource != null;
        }

        public ICommand EditSourceCommand { get; set; }

        public ICommand NewSourceCommand { get; set; }
        public Action SourceChangedAction
        {
            get
            {
                return _sourceChangedAction??(()=>{});
            }
            set
            {
                _sourceChangedAction = value;
            }
        }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth
        {
            get
            {
                return _labelWidth;
            }
            set
            {
                _labelWidth = value;
                OnPropertyChanged();
            }
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            return new DatabaseSourceRegion
            {
                SelectedSource = SelectedSource
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DatabaseSourceRegion;
            if (region != null)
            {
                SelectedSource = region.SelectedSource;
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of ISourceToolRegion<IDbSource>

        public IDbSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                SetSelectedSource(value);
                SourceChangedAction();
                OnSomethingChanged(this);
                var delegateCommand = EditSourceCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                delegateCommand?.RaiseCanExecuteChanged();
            }
        }

        private void SetSelectedSource(IDbSource value)
        {
            if (value != null)
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.Id;
            }
            OnPropertyChanged("SelectedSource");
        }

        public ICollection<IDbSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged();
            }
        }

        public IList<string> Errors => SelectedSource == null ? new List<string> { ErrorResource.InvalidSource } : new List<string>();

        public IDbSource SavedSource
        {
            get
            {
                return _modelItem.GetProperty<IDbSource>("SavedSource");
            }
            set
            {
                _modelItem.SetProperty("SavedSource", value);
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }
}
