using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable ConvertToAutoProperty

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
        private readonly IWcfServiceModel _model;
        private ICollection<IWcfAction> _actions;
        private bool _isActionEnabled;
        private bool _isRefreshing;
        private double _labelWidth;
        private IList<string> _errors;

        public WcfActionRegion()
        {
            ToolRegionName = "WcfActionRegion";
        }

        public WcfActionRegion(IWcfServiceModel model, ModelItem modelItem, ISourceToolRegion<IWcfServerSource> source)
        {
            try
            {
                Errors = new List<string>();

                LabelWidth = 46;
                ToolRegionName = "DbActionRegion";
                _modelItem = modelItem;
                _model = model;
                _source = source;
                _source.SomethingChanged += SourceOnSomethingChanged;
                Dependants = new List<IToolRegion>();
                IsRefreshing = false;
                if (_source.SelectedSource != null)
                {
                    Actions = model.GetActions(_source.SelectedSource);
                }
                if (!string.IsNullOrEmpty(Method.FullName))
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
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                IsRefreshing = true;
                // ReSharper disable once ExplicitCallerInfoArgument
                if (_source?.SelectedSource != null)
                {
                    Actions = _model.GetActions(_source.SelectedSource);
                    SelectedAction = null;
                    IsActionEnabled = true;
                    IsEnabled = true;
                }
                IsRefreshing = false;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                IsRefreshing = false;
                Errors.Add(e.Message);
            }
            finally
            {
                OnSomethingChanged(this);
                CallErrorsEventHandler();
            }
        }

        private void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        IWcfAction Method
        {
            get { return _modelItem.GetProperty<IWcfAction>("Method"); }
            set
            {
                _modelItem.SetProperty("Method", value);
            }
        }

        public bool CanRefresh()
        {
            IsActionEnabled = _source.SelectedSource != null;
            return _source?.SelectedSource != null;
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
                var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                var region = outputs as OutputsRegion;
                if (region != null)
                {
                    region.Outputs = new ObservableCollection<IServiceOutputMapping>();
                    region.RecordsetName = String.Empty;
                    region.ObjectResult = string.Empty;
                    region.IsObject = false;
                    region.ObjectName = string.Empty;
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
            delegateCommand?.RaiseCanExecuteChanged();

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
            return new WcfActionMemento
            {
                IsEnabled = IsEnabled,
                SelectedAction = SelectedAction == null ? null : new WcfAction()
                {
                    Inputs = SelectedAction?.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                    FullName = SelectedAction.FullName,
                    Method = SelectedAction.Method
                }
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as WcfActionMemento;
            if (region != null)
            {
                SelectedAction = region.SelectedAction;
                RestoreIfPrevious(region.SelectedAction);
                IsEnabled = region.IsEnabled;
                OnPropertyChanged("SelectedAction");
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
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
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged();
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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }

    public class WcfActionMemento : IActionToolRegion<IWcfAction>
    {
        private IWcfAction _selectedAction;

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; set; }

        public IToolRegion CloneRegion()
        {
            return null;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of IActionToolRegion<IDbAction>

        public IWcfAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                _selectedAction = value;
            }
        }
        public ICollection<IWcfAction> Actions { get; set; }
        public ICommand RefreshActionsCommand { get; set; }
        public bool IsActionEnabled { get; set; }
        public bool IsRefreshing { get; set; }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth { get; set; }

        #endregion

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
