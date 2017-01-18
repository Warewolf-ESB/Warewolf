﻿using System;
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
    public class DotNetMethodRegion : IMethodToolRegion<IPluginAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IPluginSource> _source;
        private readonly INamespaceToolRegion<INamespaceItem> _namespace;
        private bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedAction;
        private IPluginAction _selectedMethod;
        private IPluginServiceModel _model;
        private ICollection<IPluginAction> _methodsToRun;
        private bool _isActionEnabled;
        private bool _isRefreshing;
        private double _labelWidth;
        private IList<string> _errors;

        public DotNetMethodRegion()
        {
            ToolRegionName = "DotNetActionRegion";
        }

        public DotNetMethodRegion(IPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IPluginSource> source, INamespaceToolRegion<INamespaceItem> namespaceItem)
        {
            try
            {
                Errors = new List<string>();

                LabelWidth = 70;
                ToolRegionName = "DotNetActionRegion";
                _modelItem = modelItem;
                _model = model;
                _source = source;
                _namespace = namespaceItem;
                _namespace.SomethingChanged += SourceOnSomethingChanged;
                Dependants = new List<IToolRegion>();
                IsRefreshing = false;
                if (_source.SelectedSource != null)
                {
                    MethodsToRun = model.GetActionsWithReturns(_source.SelectedSource, _namespace.SelectedNamespace);
                }
                if (Method != null && MethodsToRun != null)
                {
                    IsActionEnabled = _source.SelectedSource != null && _namespace.SelectedNamespace != null;
                    SelectedMethod = MethodsToRun.FirstOrDefault(action => action.Method == Method.Method);
                }
                RefreshMethodsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
                {
                    IsRefreshing = true;
                    if (_source.SelectedSource != null)
                    {
                        MethodsToRun = model.GetActionsWithReturns(_source.SelectedSource, _namespace.SelectedNamespace);
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
                // ReSharper disable once ExplicitCallerInfoArgument
                UpdateBasedOnNamespace();
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

        private void UpdateBasedOnNamespace()
        {
            if (_source?.SelectedSource != null)
            {
                MethodsToRun = _model.GetActionsWithReturns(_source.SelectedSource, _namespace.SelectedNamespace);
                SelectedMethod = null;
                IsActionEnabled = true;
                IsEnabled = true;
            }
        }

        public bool CanRefresh()
        {
            var isActionEnabled = _source.SelectedSource != null && _namespace.SelectedNamespace != null;
            IsActionEnabled = isActionEnabled;
            return isActionEnabled;
        }

        public IPluginAction SelectedMethod
        {
            get
            {
                return _selectedMethod;
            }
            set
            {
                if (!Equals(value, _selectedMethod) && _selectedMethod != null)
                {
                    if (!string.IsNullOrEmpty(_selectedMethod.Method))
                        StorePreviousValues(_selectedMethod.GetIdentifier());
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
            if (IsAPreviousValue(value) && _selectedMethod != null)
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
            var delegateCommand = RefreshMethodsCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            _selectedMethod = value;
        }

        public ICollection<IPluginAction> MethodsToRun
        {
            get
            {
                return _methodsToRun;
            }
            set
            {
                _methodsToRun = value;
                OnPropertyChanged();
            }
        }
        public ICommand RefreshMethodsCommand { get; set; }
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
            return new DotNetMethodRegion
            {
                IsEnabled = IsEnabled,
                SelectedMethod = SelectedMethod == null ? null : new PluginAction
                {
                    Inputs = SelectedMethod?.Inputs.Select(a => new ServiceInput(a.Name, a.Value) as IServiceInput).ToList(),
                    FullName = SelectedMethod.FullName,
                    Method = SelectedMethod.Method
                }
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DotNetMethodRegion;
            if (region != null)
            {
                SelectedMethod = region.SelectedMethod;
                RestoreIfPrevious(region.SelectedMethod);
                IsEnabled = region.IsEnabled;
                OnPropertyChanged("SelectedMethod");
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
            _selectedMethod = value;
            SavedAction = value;
            if (value != null)
            {
                Method = value;
            }

            OnPropertyChanged("SelectedMethod");
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