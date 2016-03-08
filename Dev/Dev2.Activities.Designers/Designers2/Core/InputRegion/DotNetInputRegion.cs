using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
// ReSharper disable NotAccessedField.Local

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class DotNetInputRegion : IDotNetInputRegion
    {
         private readonly ModelItem _modelItem;
        private readonly IActionToolRegion<IPluginAction> _action;
        bool _isEnabled;
        private IList<IServiceInput> _inputs;
        private bool _isInputsEmptyRows;

        public DotNetInputRegion()
        {
            ToolRegionName = "DotNetInputRegion";
        }

        public DotNetInputRegion(ModelItem modelItem, IActionToolRegion<IPluginAction> action)
        {
            ToolRegionName = "DotNetInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            Inputs = new List<IServiceInput>();
            IsEnabled = action != null && action.SelectedAction != null;
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (_action != null && _action.SelectedAction != null)
            {
                Inputs.Clear();
                Inputs = _action.SelectedAction.Inputs;
                IsInputsEmptyRows = Inputs.Count < 1;
                IsEnabled = true;
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsEnabled");
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
            //var ser = new Dev2JsonSerializer();
            //return ser.Deserialize<IToolRegion>(ser.SerializeToBuilder(this));
            var inputs2 = new List<IServiceInput>();
            return new DotNetInputRegionClone
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DotNetInputRegionClone;
            if (region != null)
            {
                IsEnabled = region.IsEnabled;
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    Inputs = region.Inputs;
                }
            }
        }

        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
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

        #region Implementation of IDotNetInputRegion

        public IList<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
