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

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.NamespaceRegion
{
    public class DotNetNamespaceRegion : INamespaceToolRegion<INamespaceItem>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IPluginSource> _source;
        private bool _isEnabled;

        readonly Dictionary<string, IList<IToolRegion>> _previousRegions = new Dictionary<string, IList<IToolRegion>>();
        private Action _sourceChangedNamespace;
        private INamespaceItem _selectedNamespace;
        private IPluginServiceModel _model;
        private ICollection<INamespaceItem> _namespaces;
        private bool _isRefreshing;
        private double _labelWidth;
        private bool _isNamespaceEnabled;

        public DotNetNamespaceRegion()
        {
            ToolRegionName = "DotNetNamespaceRegion";
        }

        public DotNetNamespaceRegion(IPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IPluginSource> source)
        {
            LabelWidth = 70;
            ToolRegionName = "DotNetNamespaceRegion";
            _modelItem = modelItem;
            _model = model;
            _source = source;
            _source.SomethingChanged += SourceOnSomethingChanged;
            Dependants = new List<IToolRegion>();
            IsRefreshing = false;
            UpdateBasedOnSource();
            if (Namespace != null)
            {
                SelectedNamespace = Namespaces.FirstOrDefault(item => item.FullName == Namespace.FullName);
            }
            RefreshNamespaceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
            {
                IsRefreshing = true;
                if (_source.SelectedSource != null)
                {
                    Namespaces = model.GetNameSpaces(_source.SelectedSource);
                }
                IsRefreshing = false;
            }, CanRefresh);

            IsEnabled = true;
            _modelItem = modelItem;
        }
        INamespaceItem Namespace
        {
            get
            {
                return _modelItem.GetProperty<INamespaceItem>("Namespace");
            }
            set
            {
                _modelItem.SetProperty("Namespace", value);
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
            UpdateBasedOnSource();
            SelectedNamespace = null;
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsEnabled");
        }

        private void UpdateBasedOnSource()
        {
            if (_source != null && _source.SelectedSource != null)
            {
                Namespaces = _model.GetNameSpaces(_source.SelectedSource);
                IsNamespaceEnabled = true;
                IsEnabled = true;
            }
        }

        public bool CanRefresh()
        {
            return SelectedNamespace != null;
        }

        public INamespaceItem SelectedNamespace
        {
            get
            {
                return _selectedNamespace;
            }
            set
            {
                if (!Equals(value, _selectedNamespace) && _selectedNamespace != null)
                {
                    if (!String.IsNullOrEmpty(_selectedNamespace.FullName))
                        StorePreviousValues(_selectedNamespace.FullName);
                }

                if (IsAPreviousValue(value) && _selectedNamespace != null)
                {
                    RestorePreviousValues(value);
                    SetSelectedNamespace(value);
                }
                else
                {
                    SetSelectedNamespace(value);
                    SourceChangedNamespace();
                    OnSomethingChanged(this);
                }
                var delegateCommand = RefreshNamespaceCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                if (delegateCommand != null)
                {
                    delegateCommand.RaiseCanExecuteChanged();
                }

                _selectedNamespace = value;
                OnPropertyChanged();
            }
        }
        public ICollection<INamespaceItem> Namespaces
        {
            get
            {
                return _namespaces;
            }
            set
            {
                _namespaces = value;
                OnPropertyChanged();
            }
        }
        public ICommand RefreshNamespaceCommand { get; set; }
        public bool IsNamespaceEnabled
        {
            get
            {
                return _isNamespaceEnabled;
            }
            set
            {
                _isNamespaceEnabled = value;
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

        public Action SourceChangedNamespace
        {
            get
            {
                return _sourceChangedNamespace ?? (() => { });
            }
            set
            {
                _sourceChangedNamespace = value;
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
            return new DotNetNamespaceRegion
            {
                IsEnabled = IsEnabled,
                SelectedNamespace = SelectedNamespace
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DotNetNamespaceRegion;
            if (region != null)
            {
                SelectedNamespace = region.SelectedNamespace;
                IsEnabled = region.IsEnabled;
            }
        }

        public int GetId()
        {
            return SelectedNamespace.FullName.GetHashCode();
        }

        #endregion

        #region Implementation of INamespaceToolRegion<INamespaceItem>

        private void SetSelectedNamespace(INamespaceItem value)
        {
            if (value != null)
            {
                _selectedNamespace = value;
                SavedNamespace = value;
                Namespace = value;
            }
            OnPropertyChanged("SelectedNamespace");
        }

        private void StorePreviousValues(string fullName)
        {
            _previousRegions.Remove(fullName);
            _previousRegions[fullName] = new List<IToolRegion>(Dependants.Select(a => a.CloneRegion()));
        }

        private void RestorePreviousValues(INamespaceItem value)
        {
            var toRestore = _previousRegions[value.FullName];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        private bool IsAPreviousValue(INamespaceItem value)
        {
            return value != null && _previousRegions.Keys.Any(a => a == value.FullName);
        }

        public IList<string> Errors
        {
            get
            {
                return SelectedNamespace == null ? new List<string> { "Invalid Namespace Selected" } : new List<string>();
            }
        }

        public INamespaceItem SavedNamespace
        {
            get
            {
                return _modelItem.GetProperty<INamespaceItem>("SavedNamespace");
            }
            set
            {
                _modelItem.SetProperty("SavedNamespace", value);
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