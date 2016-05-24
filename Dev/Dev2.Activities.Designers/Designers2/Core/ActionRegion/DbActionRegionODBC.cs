﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;

// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DbActionRegionOdbc : IODBCActionToolRegion<IDbAction>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IDbSource> _source;
        private bool _isEnabled;

        private Action _sourceChangedAction;
        private Action _commandTextChangedAction;
        private IDbAction _selectedAction;
        private readonly IDbServiceModel _model;
        private ICollection<IDbAction> _actions;
        private bool _isActionEnabled;
        private bool _isGenerateOutputsEnabled;
        private bool _isRefreshing;
        private double _labelWidth;
        private IList<string> _errors;
        private string _commandText;

        public DbActionRegionOdbc()
        {
            ToolRegionName = "DbActionRegionODBC";
        }

        public DbActionRegionOdbc(IDbServiceModel model, ModelItem modelItem, ISourceToolRegion<IDbSource> source)
        {
            Errors = new List<string>();

            LabelWidth = 46;
            ToolRegionName = "DbActionRegionODBC";
            _modelItem = modelItem;
            _model = model;
            _source = source;
            _source.SomethingChanged += SourceOnSomethingChanged;
            Dependants = new List<IToolRegion>();

            IsEnabled = true;
            _modelItem = modelItem;
            CommandText = _modelItem.GetProperty<string>("CommandText") ?? "";
            IsActionEnabled = _source != null && _source.SelectedSource != null;
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (_source != null && _source.SelectedSource != null)
            {
                CommandText = String.Empty;
                IsActionEnabled = true;
                IsEnabled = true;
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsVisible");
        }

        public IDbAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                _selectedAction = value;
                OnPropertyChanged();
            }
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
        public string CommandText
        {
            get
            {
                return _commandText ?? "";
            }
            set
            {
                IsGenerateOutputsEnabled = !string.IsNullOrWhiteSpace(value);
                _commandText = value;
                _modelItem.SetProperty("CommandText", value);                
                OnSomethingChanged(this);
                OnPropertyChanged();
            }
        }
        public bool IsGenerateOutputsEnabled
        {
            get
            {
                return _isGenerateOutputsEnabled;
            }
            set
            {
                _isGenerateOutputsEnabled = value;
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
        public Action CommandTextChangedAction
        {
            get
            {
                return _commandTextChangedAction ?? (() => { });
            }
            set
            {
                _commandTextChangedAction = value;
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
            return null;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }


        public int GetId()
        {
            return SelectedAction.Name.GetHashCode();
        }

        #endregion

        #region Implementation of IActionToolRegion<IDbAction>

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

        public IDbServiceModel Model
        {
            get
            {
                return _model;
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
