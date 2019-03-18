#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;



namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class WcfActionRegion : IActionToolRegion<IWcfAction>
    {
        readonly ModelItem _modelItem;
        readonly ISourceToolRegion<IWcfServerSource> _source;
        bool _isEnabled;
        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        Action _sourceChangedAction;
        IWcfAction _selectedAction;
        readonly IWcfServiceModel _model;
        ICollection<IWcfAction> _actions;
        bool _isActionEnabled;
        bool _isRefreshing;
        double _labelWidth;
        IList<string> _errors;

        public WcfActionRegion() => ToolRegionName = "WcfActionRegion";

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

        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                IsRefreshing = true;
                if (_source?.SelectedSource != null)
                {
                    Actions = _model.GetActions(_source.SelectedSource);
                    SelectedAction = null;
                    IsActionEnabled = true;
                    IsEnabled = true;
                }
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

        void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        IWcfAction Method
        {
            get => _modelItem.GetProperty<IWcfAction>("Method");
            set => _modelItem.SetProperty("Method", value);
        }

        public bool CanRefresh()
        {
            IsActionEnabled = _source.SelectedSource != null;
            return _source?.SelectedSource != null;
        }

        public IWcfAction SelectedAction
        {
            get => _selectedAction;
            set
            {
                if (!Equals(value, _selectedAction) && _selectedAction != null && !String.IsNullOrEmpty(_selectedAction.Method))
                {
                    StorePreviousValues(_selectedAction.GetHashCodeBySource());
                }

                var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                if (outputs is OutputsRegion region)
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

        void RestoreIfPrevious(IWcfAction value)
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

        public ICollection<IWcfAction> Actions
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
            get => _sourceChangedAction ?? (() => { });
            set => _sourceChangedAction = value;
        }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth
        {
            get => _labelWidth;
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
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion() => new WcfActionMemento
        {
            IsEnabled = IsEnabled,
            SelectedAction = SelectedAction == null ? null : new WcfAction()
            {
                Inputs = SelectedAction?.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                FullName = SelectedAction.FullName,
                Method = SelectedAction.Method
            }
        };

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is WcfActionMemento region)
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

        void SetSelectedAction(IWcfAction value)
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

        void RestorePreviousValues(IWcfAction value)
        {
            var toRestore = _previousRegions[value.GetHashCodeBySource()];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        bool IsAPreviousValue(IWcfAction value) => value != null && _previousRegions.Keys.Any(a => a == value.GetHashCodeBySource());

        public IList<string> Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                OnPropertyChanged();
            }
        }

        public IWcfAction SavedAction
        {
            get => _modelItem.GetProperty<IWcfAction>("SavedAction");
            set => _modelItem.SetProperty("SavedAction", value);
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
