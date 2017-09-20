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
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;





namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class ComActionRegion : IActionToolRegion<IPluginAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IComPluginSource> _source;
        private readonly INamespaceToolRegion<INamespaceItem> _namespace;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedAction;
        private IPluginAction _selectedAction;
        private readonly IComPluginServiceModel _model;
        private ICollection<IPluginAction> _actions;
        private bool _isActionEnabled;
        private bool _isRefreshing;
        private double _labelWidth;
        private IList<string> _errors;
        private bool _isEnabled;

        public ComActionRegion()
        {
            ToolRegionName = "ComActionRegion";
        }

        public ComActionRegion(IComPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IComPluginSource> source, INamespaceToolRegion<INamespaceItem> namespaceItem)
        {
            try
            {
                Errors = new List<string>();

                LabelWidth = 70;
                ToolRegionName = "ComActionRegion";
                _modelItem = modelItem;
                _model = model;
                _source = source;
                _namespace = namespaceItem;
                _namespace.SomethingChanged += SourceOnSomethingChanged;
                Dependants = new List<IToolRegion>();
                IsRefreshing = false;
                if (_source.SelectedSource != null)
                {
                    Actions = model.GetActions(_source.SelectedSource, _namespace.SelectedNamespace);
                }
                if (Method != null && Actions != null)
                {
                    IsActionEnabled = true;
                    SelectedAction = Actions.FirstOrDefault(action => action.Method == Method.Method);
                }
                RefreshActionsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
                {
                    IsRefreshing = true;
                    if (_source.SelectedSource != null)
                    {
                        Actions = model.GetActions(_source.SelectedSource, _namespace.SelectedNamespace);
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

        IPluginAction Method
        {
            get
            {
                return _modelItem.GetProperty<IPluginAction>("Method");
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

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                IsRefreshing = true;
                
                UpdateBasedOnNamespace();
                IsRefreshing = false;
                
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

        private void UpdateBasedOnNamespace()
        {
            if (_source?.SelectedSource != null)
            {
                Actions = _model.GetActions(_source.SelectedSource, _namespace.SelectedNamespace);
                SelectedAction = null;
                IsActionEnabled = true;
                IsEnabled = true;
            }
        }

        public bool CanRefresh()
        {
            var isActionEnabled = _source.SelectedSource != null;
            IsActionEnabled = isActionEnabled;
            return isActionEnabled;
        }

        public IPluginAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                if (!Equals(value, _selectedAction) && _selectedAction != null)
                {
                    if (!string.IsNullOrEmpty(_selectedAction.Method))
                        StorePreviousValues(_selectedAction.GetIdentifier());
                }
                if (Dependants != null)
                {
                    var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                    var region = outputs as OutputsRegion;
                    if (region != null)
                    {
                        region.Outputs = new ObservableCollection<IServiceOutputMapping>();
                        region.RecordsetName = string.Empty;
                        region.ObjectResult = string.Empty;
                        region.IsObject = false;
                        region.ObjectName = string.Empty;
                    }
                }
                RestoreIfPrevious(value);
                OnPropertyChanged();
            }
        }

        private void RestoreIfPrevious(IPluginAction value)
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

        public ICollection<IPluginAction> Actions
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
            return new ComActionRegion()
            {
                IsEnabled = IsEnabled,
                SelectedAction = SelectedAction == null ? null : new PluginAction
                {
                    Inputs = SelectedAction?.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                    FullName = SelectedAction.FullName,
                    Method = SelectedAction.Method
                }
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as ComActionRegion;
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

        #region Implementation of IActionToolRegion<IPluginAction>

        private void SetSelectedAction(IPluginAction value)
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

        private void RestorePreviousValues(IPluginAction value)
        {
            var toRestore = _previousRegions[value.GetIdentifier()];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IPluginAction value)
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
            }
        }

        public IPluginAction SavedAction
        {
            get
            {
                return _modelItem.GetProperty<IPluginAction>("SavedAction");
            }
            set
            {
                _modelItem.SetProperty("SavedAction", value);
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }
}