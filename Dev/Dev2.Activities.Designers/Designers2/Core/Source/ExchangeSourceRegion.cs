﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers2.Core.Source
{
    public class ExchangeSourceRegion : ISourceToolRegion<IExchangeSource>
    {
        private Guid _sourceId;
        private readonly ModelItem _modelItem;
        private Action _sourceChangedAction;
        private IExchangeSource _selectedSource;
        private ICollection<IExchangeSource> _sources;
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

        public ICollection<IExchangeSource> Sources
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

        public event SomethingChanged SomethingChanged;
        public double LabelWidth { get; set; }
        public string NewSourceHelpText { get; set; }
        public string EditSourceHelpText { get; set; }
        public string SourcesHelpText { get; set; }
        public string NewSourceTooltip { get; set; }
        public string EditSourceTooltip { get; set; }
        public string SourcesTooltip { get; set; }

        public ExchangeSourceRegion()
        {
            
        }

        public ExchangeSourceRegion(IExchangeServiceModel model, ModelItem modelItem, enSourceType type)
        {
            LabelWidth = 70;
            ToolRegionName = "ExchangeSourceRegion";
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.Where(source => source != null && source.Type == type).ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.Core.ExchangeServiceSourceTypesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.Core.ExchangeServiceEditSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.Core.ExchangeServiceNewSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Core.ManageExchangeServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Core.ManageExchangeServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Core.ManageExchangeServiceNewSourceTooltip;

            if (SourceId != Guid.Empty)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.ResourceID == SourceId);
            }
        }

        public bool CanEditSource()
        {
            return SelectedSource != null;
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

        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; }
        public IToolRegion CloneRegion()
        {
            return new ExchangeSourceRegion
            {
                SelectedSource = SelectedSource
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as ExchangeSourceRegion;
            if (region != null)
            {
                SelectedSource = region.SelectedSource;
            }
        }
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

        public IExchangeSource SelectedSource
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
                if (delegateCommand != null)
                {
                    delegateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void SetSelectedSource(IExchangeSource value)
        {
            if (value != null)
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.ResourceID;
            }
            OnPropertyChanged("SelectedSource");
        }

        public IExchangeSource SavedSource
        {
            get
            {
                return _modelItem.GetProperty<IExchangeSource>("SavedSource");
            }
            set
            {
                _modelItem.SetProperty("SavedSource", value);
            }
        }
    }
}
