using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Studio.Core.Activities.Utils;
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DotNetActionRegion : IActionToolRegion<IPluginAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IPluginSource> _source;
        private readonly INamespaceToolRegion<INamespaceItem> _namespace;
        private bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedAction;
        private IPluginAction _selectedAction;
        private IPluginServiceModel _model;
        private ICollection<IPluginAction> _actions;
        private bool _isActionEnabled;
        private bool _isRefreshing;
        private double _labelWidth;

        public DotNetActionRegion()
        {
            ToolRegionName = "DotNetActionRegion";
        }

        public DotNetActionRegion(IPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IPluginSource> source, INamespaceToolRegion<INamespaceItem> namespaceItem)
        {
            LabelWidth = 70;
            ToolRegionName = "DotNetActionRegion";
            _modelItem = modelItem;
            _model = model;
            _source = source;
            _namespace = namespaceItem;
            _namespace.SomethingChanged += SourceOnSomethingChanged;
            Dependants = new List<IToolRegion>();
            IsRefreshing = false;
            UpdateBasedOnNamespace();
            if (Method != null)
            {
                SelectedAction = Actions.FirstOrDefault(action => action.FullName == Method.FullName);
            }
            RefreshActionsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
            {
                IsRefreshing = true;
                if(_source.SelectedSource != null)
                {
                    Actions = model.GetActions(_source.SelectedSource, _namespace.SelectedNamespace);
                }
                IsRefreshing = false;
            }, CanRefresh);

            IsEnabled = true;
            _modelItem = modelItem;
        }

        IPluginAction Method
        {
            get
            {
                return _modelItem.GetProperty<IPluginAction>("Method");
            }
            set
            {
                _modelItem.SetProperty("Method",value);
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
            // ReSharper disable once ExplicitCallerInfoArgument
            UpdateBasedOnNamespace();
            SelectedAction = null;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsEnabled");
        }

        private void UpdateBasedOnNamespace()
        {
            if(_source != null && _source.SelectedSource != null && _namespace != null && _namespace.SelectedNamespace != null)
            {
                Actions = _model.GetActions(_source.SelectedSource, _namespace.SelectedNamespace);
                IsActionEnabled = true;
                IsEnabled = true;
            }
        }

        public bool CanRefresh()
        {
            return SelectedAction != null;
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
                    if (!String.IsNullOrEmpty(_selectedAction.FullName))
                        StorePreviousValues(_selectedAction.FullName);
                }

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
                OnPropertyChanged();
            }
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
            return new DotNetActionRegion
            {
                IsEnabled = IsEnabled,
                SelectedAction = SelectedAction
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DotNetActionRegion;
            if (region != null)
            {
                SelectedAction = region.SelectedAction;
                IsEnabled = region.IsEnabled;
            }
        }

        public int GetId()
        {
            return SelectedAction.FullName.GetHashCode();
        }

        #endregion

        #region Implementation of IActionToolRegion<IPluginAction>

        private void SetSelectedAction(IPluginAction value)
        {
            if (value != null)
            {
                _selectedAction = value;
                SavedAction = value;
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
            var toRestore = _previousRegions[value.FullName];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IPluginAction value)
        {
            return value != null && _previousRegions.Keys.Any(a => value.FullName != null && (a == value.FullName));
        }

        public IList<string> Errors
        {
            get
            {
                return SelectedAction == null ? new List<string> { "Invalid Action Selected" } : new List<string>();
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
