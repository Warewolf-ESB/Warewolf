using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DbActionRegion : IActionToolRegion<IDbAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IDbSource> _source;
        private bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedAction;
        private IDbAction _selectedAction;
        private readonly IDbServiceModel _model;
        private ICollection<IDbAction> _actions;
        private bool _isActionEnabled;
        private bool _isRefreshing;
        private double _labelWidth;
        private IList<string> _errors;
        private IAsyncWorker _worker;

        public DbActionRegion()
        {
            ToolRegionName = "DbActionRegion";
        }

        public DbActionRegion(IDbServiceModel model, ModelItem modelItem, ISourceToolRegion<IDbSource> source,IAsyncWorker worker)
        {
            try
            {
                Errors = new List<string>();
                
                LabelWidth = 46;
                ToolRegionName = "DbActionRegion";
                _modelItem = modelItem;
                _model = model;
                _source = source;
                _worker = worker;
                _source.SomethingChanged += SourceOnSomethingChanged;
                Dependants = new List<IToolRegion>();
                if (_source.SelectedSource != null)
                {
                    LoadActions(model);
                }
                
                RefreshActionsCommand = new DelegateCommand(o =>
                {
                    if (_source.SelectedSource != null)
                    {
                        _source.SelectedSource.ReloadActions = true;
                        LoadActions(model);
                    }
                }, o=>CanRefresh());

                _modelItem = modelItem;
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
        }

        private void LoadActions(IDbServiceModel model)
        {
            IsRefreshing = true;
            SelectedAction = null;
            IsActionEnabled = false;
            IsEnabled = false;
            _worker.Start(() => model.GetActions(_source.SelectedSource), delegate(ICollection<IDbAction> actions)
            {
                Actions = actions;
                IsRefreshing = false;
                IsActionEnabled = true;
                IsEnabled = true;
                if (!string.IsNullOrEmpty(ProcedureName))
                {
                    SelectedAction = Actions.FirstOrDefault(action => action.Name == ProcedureName);
                }
            });
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                // ReSharper disable once ExplicitCallerInfoArgument
                if (_source?.SelectedSource != null)
                {
                    LoadActions(_model);                   
                }
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

        string ProcedureName
        {
            get { return _modelItem.GetProperty<string>("ProcedureName"); }
            set
            {
                _modelItem.SetProperty("ProcedureName", value);
            }
        }

        public bool CanRefresh()
        {
            var isActionEnabled = _source.SelectedSource != null && !IsRefreshing;
            IsActionEnabled = isActionEnabled;
            return isActionEnabled;
        }

        public IDbAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                if (!Equals(value, _selectedAction) && _selectedAction != null)
                {
                    if (!String.IsNullOrEmpty(_selectedAction.Name))
                        StorePreviousValues(_selectedAction.GetIdentifier());
                }
                var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                var region = outputs as OutputsRegion;
                if (region != null)
                {
                    region.Outputs = new ObservableCollection<IServiceOutputMapping>();
                    region.RecordsetName = String.Empty;

                }
                RestoreIfPrevious(value);
                OnPropertyChanged();
            }
        }

        private void RestoreIfPrevious(IDbAction value)
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
            var delegateCommand = RefreshActionsCommand as DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            _selectedAction = value;
        }

        public ICollection<IDbAction> Actions
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
                var delegateCommand = RefreshActionsCommand as DelegateCommand;
                delegateCommand?.RaiseCanExecuteChanged();
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
            var action = new DbActionMemento
            {
                IsEnabled = IsEnabled,
                SelectedAction = SelectedAction == null ? null : new DbAction
                {
                    Inputs = SelectedAction?.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                    Name = SelectedAction.Name,
                    SourceId = SelectedAction.SourceId
                }
            };
            return action;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DbActionMemento;
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

        #region Implementation of IActionToolRegion<IDbAction>

        private void SetSelectedAction(IDbAction value)
        {
            _selectedAction = value;
            SavedAction = value;
            if (value != null)
            {
                ProcedureName = value.Name;
            }

            OnPropertyChanged("SelectedAction");
        }
        private void StorePreviousValues(string name)
        {
            _previousRegions.Remove(name);
            _previousRegions[name] = new List<IToolRegion>(Dependants.Select(a => a.CloneRegion()));
        }

        private void RestorePreviousValues(IDbAction value)
        {
            var toRestore = _previousRegions[value.GetIdentifier()];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IDbAction value)
        {
            return value != null && _previousRegions.Keys.Any(a => a == value.GetIdentifier());
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
                ErrorsHandler?.Invoke(this, new List<string>(value));
            }
        }

        public IDbAction SavedAction
        {
            get
            {
                return _modelItem.GetProperty<IDbAction>("SavedAction");
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

    public class DbActionMemento : IActionToolRegion<IDbAction>
    {
        private IDbAction _selectedAction;

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
            get
            {
                return null;
            }
            set
            {
            }
        }

        #endregion

        #region Implementation of IActionToolRegion<IDbAction>

        public IDbAction SelectedAction
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
        public ICollection<IDbAction> Actions { get; set; }
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
