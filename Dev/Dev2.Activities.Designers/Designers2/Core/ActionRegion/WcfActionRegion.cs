using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;
// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class WcfActionRegion : IActionToolRegion<IWcfAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IWcfServerSource> _source;
        private bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedAction;
        private IWcfAction _selectedAction;
        // ReSharper disable once NotAccessedField.Local
        private IWcfServiceModel _model;
        private ICollection<IWcfAction> _actions;
        private bool _isActionEnabled;
        private bool _isRefreshing;
        private double _labelWidth;

        public WcfActionRegion()
        {
            ToolRegionName = "WcfActionRegion";
        }

        public WcfActionRegion(IWcfServiceModel model, ModelItem modelItem, ISourceToolRegion<IWcfServerSource> source)
        {
            LabelWidth = 70;
            ToolRegionName = "WcfActionRegion";
            _modelItem = modelItem;
            _model = model;
            _source = source;
            Dependants = new List<IToolRegion>();
            _source.SomethingChanged += SourceOnSomethingChanged;
            IsRefreshing = false;
            if (_source.SelectedSource != null)
            {
                Actions = model.GetActions(_source.SelectedSource);
            }
            if (Method != null && Actions != null)
            {
                IsActionEnabled = true;
                SelectedAction = Actions.FirstOrDefault(action => action.FullName == Method.FullName);
            }
            RefreshActionsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
            {
                IsRefreshing = true;
                if (_source.SelectedSource != null)
                {
                    Actions = model.GetActions(_source.SelectedSource);
                }
                IsRefreshing = false;
            }, CanRefresh);

            IsEnabled = true;
            _modelItem = modelItem;
        }

        IWcfAction Method
        {
            get
            {
                return _modelItem.GetProperty<IWcfAction>("Method");
            }
            set
            {
                _modelItem.SetProperty("Method", value);
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

        public bool CanRefresh()
        {
            return SelectedAction != null;
        }

        public IWcfAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                if (!Equals(value, _selectedAction) && _selectedAction != null)
                {
                    if (!String.IsNullOrEmpty(_selectedAction.Method))
                        StorePreviousValues(_selectedAction.GetHashCodeBySource());
                }
                if (Dependants != null)
                {
                    var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                    var region = outputs as OutputsRegion;
                    if (region != null)
                    {
                        region.Outputs = new ObservableCollection<IServiceOutputMapping>();
                        region.RecordsetName = String.Empty;

                    }
                }
                RestoreIfPrevious(value);
                OnPropertyChanged();
            }
        }

        private void RestoreIfPrevious(IWcfAction value)
        {
            if (IsAPreviousValue(value) && _selectedAction != null)
            {
                RestorePreviousValues(value);
                SetSelectedAction(value);
            }
            else
            {
                SetSelectedAction(value);
                SourceChangedAction();
                OnSomethingChanged(this);
            }
            var delegateCommand = RefreshActionsCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
            if (delegateCommand != null)
            {
                delegateCommand.RaiseCanExecuteChanged();
            }

            _selectedAction = value;
        }

        public ICollection<IWcfAction> Actions
        {
            get
            {
                return _actions;
            }
            set
            {
                _actions = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshActionsCommand { get; set; }
        public bool IsActionEnabled
        {
            get
            {
                return _isActionEnabled;
            }
            set
            {
                _isActionEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

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
            return new WcfActionRegion()
            {
                IsEnabled = IsEnabled,
                SelectedAction = SelectedAction == null ? null : new WcfAction()
                {
                    Inputs = SelectedAction == null ? null : SelectedAction.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                    FullName = SelectedAction.FullName,
                    Method = SelectedAction.Method
                }
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as WcfActionRegion;
            if (region != null)
            {
                SelectedAction = region.SelectedAction;
                RestoreIfPrevious(region.SelectedAction);
                IsEnabled = region.IsEnabled;
                OnPropertyChanged("SelectedAction");
            }
        }
        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            UpdateBasedOnNamespace();
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsEnabled");
        }
        private void UpdateBasedOnNamespace()
        {
            if (_source != null && _source.SelectedSource != null)
            {
                Actions = _model.GetActions(_source.SelectedSource);
                SelectedAction = null;
                IsActionEnabled = true;
                IsEnabled = true;
            }
        }

        public int GetId()
        {
            return SelectedAction.FullName.GetHashCode();
        }

        #endregion

        #region Implementation of IActionToolRegion<IWcfAction>

        private void SetSelectedAction(IWcfAction value)
        {
            _selectedAction = value;
            SavedAction = value;
            if (value != null)
            {
                Method = value;
            }

            OnPropertyChanged("SelectedAction");
        }


        private void StorePreviousValues(string actionName)
        {
            _previousRegions.Remove(actionName);
            _previousRegions[actionName] = new List<IToolRegion>(Dependants.Select(a => a.CloneRegion()));
        }

        private void RestorePreviousValues(IWcfAction value)
        {
            var toRestore = _previousRegions[value.GetHashCodeBySource()];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IWcfAction value)
        {
            return value != null && _previousRegions.Keys.Any(a => a == value.GetHashCodeBySource());
        }

        public IList<string> Errors
        {
            get
            {
                return SelectedAction == null ? new List<string> { "Invalid Action Selected" } : new List<string>();
            }
        }

        public IWcfAction SavedAction
        {
            get
            {
                return _modelItem.GetProperty<IWcfAction>("SavedAction");
            }
            set
            {
                _modelItem.SetProperty("SavedAction", value);
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
