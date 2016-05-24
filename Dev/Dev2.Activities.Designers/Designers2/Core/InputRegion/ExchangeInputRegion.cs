using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Annotations;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;
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

        public IToolRegion CloneRegion()
        {
            var inputs2 = new List<IServiceInput>(Inputs.Select(a => new ServiceInput(a.Name, a.Value)
            {
                EmptyIsNull = a.EmptyIsNull,
                TypeName = a.TypeName
            }));
            return new ExchangeInputRegionClone()
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as ExchangeInputRegionClone;
            if (region != null)
            {
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    var inp = region.Inputs.ToList();

                    Inputs = inp;
                }
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("Inputs");
                IsInputsEmptyRows = Inputs == null || Inputs.Count == 0;
            }
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
                if (value.Count < 1)
                {

                }
                else
                {

                }
                _inputs = value;
                OnPropertyChanged();
                _modelItem.SetProperty("Inputs", value);
                OnPropertyChanged();
            }
        }
    }
}
