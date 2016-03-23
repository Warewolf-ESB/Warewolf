using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public ExchangeSourceRegion()
        {
            
        }

        public ExchangeSourceRegion(IExchangeServiceModel model, ModelItem modelItem, enSourceType type)
        {
            LabelWidth = 46;
            ToolRegionName = "ExchangeSourceRegion";
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.Where(source => source != null && source.Type == type).ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceTypesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceEditSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceNewSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Core.ManageDbServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Core.ManageDbServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Core.ManageDbServiceNewSourceTooltip;

            if (SourceId != Guid.Empty)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
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

        public IExchangeSource SelectedSource { get; set; }
        public ICollection<IExchangeSource> Sources { get; set; }
        public ICommand EditSourceCommand { get; }
        public ICommand NewSourceCommand { get; }
        public Action SourceChangedAction { get; set; }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth { get; set; }
        public string NewSourceHelpText { get; set; }
        public string EditSourceHelpText { get; set; }
        public string SourcesHelpText { get; set; }
        public string NewSourceTooltip { get; set; }
        public string EditSourceTooltip { get; set; }
        public string SourcesTooltip { get; set; }
    }
}
