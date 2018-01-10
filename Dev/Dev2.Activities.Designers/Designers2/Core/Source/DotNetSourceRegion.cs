﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism.Commands;


namespace Dev2.Activities.Designers2.Core.Source
{
    public class DotNetSourceRegion : ISourceToolRegion<IPluginSource>
    {
        bool _isEnabled;
        IPluginSource _selectedSource;
        ICollection<IPluginSource> _sources;
        readonly ModelItem _modelItem;
        Guid _sourceId;
        Action _sourceChangedAction;
        double _labelWidth;
        string _newSourceHelpText;
        string _editSourceHelpText;
        string _sourcesHelpText;
        string _newSourceToolText;
        string _editSourceToolText;
        string _sourcesToolText;

        public DotNetSourceRegion(IPluginServiceModel model, ModelItem modelItem)
        {
            LabelWidth = 74;
            ToolRegionName = "DotNetSourceRegion";
            SetInitialValues();
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.HelpText.PluginServiceSourcesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.PluginServiceEditSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.PluginServiceNewSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManagePluginServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManagePluginServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManagePluginServiceNewSourceTooltip;

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

        void SetInitialValues()
        {
            IsEnabled = true;
        }

        public DotNetSourceRegion()
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
            return new DotNetSourceRegion
            {
                IsEnabled = IsEnabled,
                SelectedSource = SelectedSource
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is DotNetSourceRegion region)
            {
                SelectedSource = region.SelectedSource;
                IsEnabled = region.IsEnabled;
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of ISourceToolRegion<IPluginSource>

        public IPluginSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {

                SetSelectedSource(value);
                SourceChangedAction?.Invoke();
                OnSomethingChanged(this);
                var delegateCommand = EditSourceCommand as DelegateCommand;
                delegateCommand?.RaiseCanExecuteChanged();
            }
        }

        void SetSelectedSource(IPluginSource value)
        {
            if (value != null)
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.Id;
            }
            OnPropertyChanged("SelectedSource");
        }

        public ICollection<IPluginSource> Sources
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

        public IList<string> Errors => SelectedSource == null ? new List<string> { "Invalid Source Selected" } : new List<string>();

        public IPluginSource SavedSource
        {
            get
            {
                return _modelItem.GetProperty<IPluginSource>("SavedSource");
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
