using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
// ReSharper disable UnusedMember.Local

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

        public ExchangeSourceRegion(IExchangeServiceModel model, ModelItem modelItem, string type)
        {
            LabelWidth = 70;
            ToolRegionName = "ExchangeSourceRegion";
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new DelegateCommand(o=>model.CreateNewSource());
            EditSourceCommand = new DelegateCommand(o => model.EditSource(SelectedSource),o=> CanEditSource());
            var sources = model.RetrieveSources().OrderBy(source => source.ResourceName);
            Sources = sources.Where(source => source != null && source.ResourceType == type).ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");

            if (SavedSource != null)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.ResourceID == SavedSource.ResourceID);
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
                _modelItem?.SetProperty("SourceId", value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public IList<string> Errors { get; set; }

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

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

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
                delegateCommand?.RaiseCanExecuteChanged();
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
            // ReSharper disable once ExplicitCallerInfoArgument
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
