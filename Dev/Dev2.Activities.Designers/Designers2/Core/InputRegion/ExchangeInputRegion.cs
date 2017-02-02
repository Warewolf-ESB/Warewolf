using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Annotations;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class ExchangeInputRegion 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; set; }
        private bool _isInputsEmptyRows;
        private readonly ModelItem _modelItem;
        private IList<IServiceInput> _inputs;

        public ExchangeInputRegion(ModelItem modelItem)
        {
            ToolRegionName = "ExchangeInputRegion";
            _modelItem = modelItem;
         
            var inputsFromModel = _modelItem.GetProperty<List<IServiceInput>>("Inputs");
            Inputs = new List<IServiceInput>(inputsFromModel ?? new List<IServiceInput>());
        }

        public bool IsInputsEmptyRows
        {
            get
            {
                return _isInputsEmptyRows;
            }
            set
            {
                _isInputsEmptyRows = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public IList<IServiceInput> Inputs
        {
            get
            {
                return _modelItem.GetProperty<List<IServiceInput>>("Inputs") ?? new List<IServiceInput>();
            }
            set
            {
                _inputs = value;
                OnPropertyChanged();
                _modelItem.SetProperty("Inputs", value);
                OnPropertyChanged();
            }
        }
    }
}
