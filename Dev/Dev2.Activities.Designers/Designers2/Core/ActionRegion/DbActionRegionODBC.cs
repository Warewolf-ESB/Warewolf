using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;

// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DbActionRegionOdbc : IActionToolRegion<IDbAction>
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
            IsRefreshing = false;

            if (!string.IsNullOrEmpty(CommandText))
            {
                IsActionEnabled = true;
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
            _commandText = _modelItem.GetProperty<string>("CommandText");
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (_source != null && _source.SelectedSource != null)
            {
                SelectedAction = null;
                CommandText = String.Empty;
                IsActionEnabled = true;
                IsEnabled = true;
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsVisible");
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
            return SelectedAction != null;
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
            var delegateCommand = RefreshActionsCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
            if (delegateCommand != null)
            {
                delegateCommand.RaiseCanExecuteChanged();
            }

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

        private void SetSelectedAction(IDbAction value)
        {
            if (value != null)
            {
                _selectedAction = value;
                SavedAction = value;
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
            var toRestore = _previousRegions[value.Name];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(IDbAction value)
        {
            return _previousRegions.Keys.Any(a => a == value.Name);
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
