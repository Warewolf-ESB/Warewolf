using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Microsoft.Practices.Prism;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class DotNetMethodInputRegion : IDotNetMethodInputRegion
    {
        private readonly IMethodToolRegion<IPluginAction> _action;
        bool _isEnabled;
        private ICollection<IServiceInput> _inputs;
        private bool _isInputsEmptyRows;
        private readonly IActionInputDatatalistMapper _datatalistMapper;

        // ReSharper disable once UnusedMember.Global
        public DotNetMethodInputRegion()
        {
            ToolRegionName = "DotNetInputRegion";
        }

        public DotNetMethodInputRegion(IMethodToolRegion<IPluginAction> action)
            : this(new ActionInputDatatalistMapper())
        {
            ToolRegionName = "DotNetInputRegion";
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            Inputs = new ObservableCollection<IServiceInput>();
            var inputs = new ObservableCollection<IServiceInput>();
            if (action.SelectedMethod != null)
            {
                inputs.AddRange(action.SelectedMethod.Inputs);
                Inputs = inputs;
                IsInputsEmptyRows = Inputs.Count == 0;
                UpdateOnActionSelection();
            }
            IsEnabled = action.SelectedMethod != null;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public DotNetMethodInputRegion(IActionInputDatatalistMapper datatalistMapper)
        {
            _datatalistMapper = datatalistMapper;
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();

                // ReSharper disable once ExplicitCallerInfoArgument
                UpdateOnActionSelection();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(@"Inputs");
                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
            finally
            {
                CallErrorsEventHandler();
            }
        }

        private void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        private void UpdateOnActionSelection()
        {
            Inputs = new List<IServiceInput>();
            IsEnabled = false;
            if (_action?.SelectedMethod != null)
            {
                Inputs = _action.SelectedMethod.Inputs;
                _datatalistMapper.MapInputsToDatalist(Inputs);
                IsInputsEmptyRows = Inputs.Count < 1;
                IsEnabled = true;
            }
            OnPropertyChanged("Inputs");
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

            var inputs2 = new List<IServiceInput>(Inputs.Select(a => new ServiceInput(a.Name, a.Value)
            {
                EmptyIsNull = a.EmptyIsNull,
                TypeName = a.TypeName
            }));
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
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    var inp = region.Inputs.ToList();

                    Inputs = inp;
                }
                OnPropertyChanged("Inputs");
                IsInputsEmptyRows = Inputs == null || Inputs.Count == 0;
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of IDotNetInputRegion

        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                if (value != null)
                {
                    _inputs = value;
                    OnPropertyChanged();
                }
                else
                {
                    _inputs.Clear();
                    OnPropertyChanged();
                }
            }
        }

        #endregion
    }
}