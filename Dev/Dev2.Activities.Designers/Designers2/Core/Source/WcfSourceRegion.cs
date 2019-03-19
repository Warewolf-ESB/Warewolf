#pragma warning disable
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


namespace Dev2.Activities.Designers2.Core.Source
{
    public class WcfSourceRegion : ISourceToolRegion<IWcfServerSource>
    {
        IWcfServerSource _selectedSource;
        ICollection<IWcfServerSource> _sources;
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

        public WcfSourceRegion(IWcfServiceModel model, ModelItem modelItem)
        {
            LabelWidth = 46;
            ToolRegionName = "WcfSourceRegion";
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.HelpText.WcfServiceSourcesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.WcfServiceEditSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.WcfServiceNewSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageWcfServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageWcfServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageWcfServiceNewSourceTooltip;

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

        public WcfSourceRegion()
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

        public bool CanEditSource() => SelectedSource != null;

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

        public IToolRegion CloneRegion() => new WcfSourceRegion()
        {
            SelectedSource = SelectedSource
        };

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is WcfSourceRegion region)
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

        #region Implementation of ISourceToolRegion<IWcfSource>

        public IWcfServerSource SelectedSource
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
                var delegateCommand = EditSourceCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                delegateCommand?.RaiseCanExecuteChanged();
            }
        }

        void SetSelectedSource(IWcfServerSource value)
        {
            if (value != null)
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.Id;
            }
            OnPropertyChanged("SelectedSource");
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

        public IList<string> Errors => SelectedSource == null ? new List<string> { "Invalid Source Selected" } : new List<string>();

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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }
}
