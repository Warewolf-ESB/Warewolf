using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers2.Core.Source
{
    public class WcfSourceRegion : ISourceToolRegion<IWcfServerSource>
    {
        private bool _isEnabled;
        private IWcfServerSource _selectedSource;
        private ICollection<IWcfServerSource> _sources;
        private readonly ModelItem _modelItem;
        readonly Dictionary<Guid, IList<IToolRegion>> _previousRegions = new Dictionary<Guid, IList<IToolRegion>>();
        private Guid _sourceId;
        private Action _sourceChangedAction;
        private double _labelWidth;
        private string _newSourceHelpText;
        private string _editSourceHelpText;
        private string _sourcesHelpText;
        private string _newSourceToolText;
        private string _editSourceToolText;
        private string _sourcesToolText;

        public WcfSourceRegion(IWcfServiceModel model, ModelItem modelItem)
        {
            LabelWidth = 70;
            ToolRegionName = "WcfSourceRegion";
            SetInitialValues();
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.Core.WcfServiceSourcesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.Core.WcfServiceEditSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.Core.WcfServiceNewSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Core.ManageWcfServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Core.ManageWcfServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Core.ManageWcfServiceNewSourceTooltip;

            if (SourceId != Guid.Empty)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
            }
        }

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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

        private void SetInitialValues()
        {
            IsEnabled = true;
        }

        public WcfSourceRegion()
        {
            SetInitialValues();
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
                if (_modelItem != null)
                {
                    _modelItem.SetProperty("SourceId", value);
                }
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
                return _sourceChangedAction ?? (() => { });
            }
            set
            {
                _sourceChangedAction = value;
            }
        }

        public event SomethingChanged SomethingChanged;

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            return new WcfSourceRegion()
            {
                IsEnabled = IsEnabled,
                SelectedSource = SelectedSource
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as WcfSourceRegion;
            if (region != null)
            {
                SelectedSource = region.SelectedSource;
                IsEnabled = region.IsEnabled;
            }
        }

        #endregion

        #region Implementation of ISourceToolRegion<IWcfSource>

        public IWcfServerSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                if (!Equals(value, _selectedSource) && _selectedSource != null)
                {
                    if (!String.IsNullOrEmpty(_selectedSource.Name))
                        StorePreviousValues(_selectedSource.Id);
                }

                if (IsAPreviousValue(value) && _selectedSource != null)
                {
                    RestorePreviousValues(value);
                    SetSelectedSource(value);
                }
                else
                {
                    SetSelectedSource(value);
                    SourceChangedAction();
                    OnSomethingChanged(this);
                }
                var delegateCommand = EditSourceCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                if (delegateCommand != null)
                {
                    delegateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void SetSelectedSource(IWcfServerSource value)
        {
            if (value != null)
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.Id;
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged("SelectedSource");
        }

        private void StorePreviousValues(Guid id)
        {
            _previousRegions.Remove(id);
            _previousRegions[id] = new List<IToolRegion>(Dependants.Select(a => a.CloneRegion()));
        }

        private void RestorePreviousValues(IWcfServerSource value)
        {
            var toRestore = _previousRegions[value.Id];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IWcfServerSource value)
        {
            return _previousRegions.Keys.Any(a => a == value.Id);
        }

        public ICollection<IWcfServerSource> Sources
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

        public IList<string> Errors
        {
            get
            {
                return SelectedSource == null ? new List<string> { "Invalid Source Selected" } : new List<string>();
            }
        }

        public IWcfServerSource SavedSource
        {
            get
            {
                return _modelItem.GetProperty<IWcfServerSource>("SavedSource");
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
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}
