/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

#pragma warning disable
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




namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DotNetActionRegion : IActionToolRegion<IPluginAction>
    {
        readonly ModelItem _modelItem;
        bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        Action _sourceChangedAction;
        IPluginAction _selectedAction;
        ICollection<IPluginAction> _actions;
        bool _isActionEnabled;
        bool _isRefreshing;
        double _labelWidth;
        IList<string> _errors;

        public DotNetActionRegion()
        {
            ToolRegionName = "DotNetActionRegion";
            Errors = new List<string>();
            IsEnabled = true;
        }

        IPluginAction Method
        {
            set => _modelItem.SetProperty("Method", value);
            get => _modelItem.GetProperty("Method") as IPluginAction;
        }

        public double LabelWidth
        {
            get => _labelWidth;
            set
            {
                _labelWidth = value;
                OnPropertyChanged();
            }
        }

        public IPluginAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                if (!Equals(value, _selectedAction) && _selectedAction != null && !string.IsNullOrEmpty(_selectedAction.Method))
                {
                    StorePreviousValues(_selectedAction.GetIdentifier());
                }

                if (Dependants != null)
                {
                    var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                    if (outputs is OutputsRegion region)
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

        void RestoreIfPrevious(IPluginAction value)
        {
            if (IsAPreviousValue(value) && _selectedAction != null)
            {
                RestorePreviousValues(value);
                SetSelectedAction(value);
            }
            else
            {
                SetSelectedAction(value);
                SourceChangedAction?.Invoke();
                OnSomethingChanged(this);
            }
            var delegateCommand = RefreshActionsCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            _selectedAction = value;
        }

        public ICollection<IPluginAction> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                OnPropertyChanged();
            }
        }
        public ICommand RefreshActionsCommand { get; set; }
        public bool IsActionEnabled
        {
            get => _isActionEnabled;
            set
            {
                _isActionEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsRefreshing
        {
            get => _isRefreshing;
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
            set => _sourceChangedAction = value;
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

        public IToolRegion CloneRegion() => new DotNetActionRegion
        {
            IsEnabled = IsEnabled,
            SelectedAction = SelectedAction == null ? null : new PluginAction
            {
                Inputs = SelectedAction?.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                FullName = SelectedAction.FullName,
                Method = SelectedAction.Method
            }
        };

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is DotNetActionRegion region)
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

        void SetSelectedAction(IPluginAction value)
        {
            _selectedAction = value;
            SavedAction = value;
            if (value != null)
            {
                Method = value;
            }

            OnPropertyChanged("SelectedAction");
        }


        void StorePreviousValues(string actionName)
        {
            _previousRegions.Remove(actionName);
            _previousRegions[actionName] = new List<IToolRegion>(Dependants.Select(a => a.CloneRegion()));
        }

        void RestorePreviousValues(IPluginAction value)
        {
            var toRestore = _previousRegions[value.GetIdentifier()];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        bool IsAPreviousValue(IPluginAction value) => value != null && _previousRegions.Keys.Any(a => a == value.GetIdentifier());

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
            get => _modelItem.GetProperty<IPluginAction>("SavedAction");
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
}
