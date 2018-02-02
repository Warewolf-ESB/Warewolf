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







namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DbActionRegionOdbc : IODBCActionToolRegion<IDbAction>
    {
        readonly ModelItem _modelItem;
        readonly ISourceToolRegion<IDbSource> _source;
        bool _isEnabled;

        Action _sourceChangedAction;
        IDbAction _selectedAction;
        readonly IDbServiceModel _model;
        ICollection<IDbAction> _actions;
        bool _isActionEnabled;
        bool _isGenerateOutputsEnabled;
        bool _isRefreshing;
        double _labelWidth;
        IList<string> _errors;
        string _commandText;

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
            IsActionEnabled = _source?.SelectedSource != null;
        }

        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                IsRefreshing = true;

                if (_source?.SelectedSource != null)
                {
                    CommandText = String.Empty;
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
                _modelItem.SetProperty("CommandText", value);
                _commandText = value;
                OnPropertyChanged();
                OnSomethingChanged(this);
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

        public IToolRegion CloneRegion() => null;

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

        public IDbServiceModel Model => _model;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            SomethingChanged?.Invoke(this, args);
        }
    }
}
