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









namespace Dev2.Activities.Designers2.Core.NamespaceRegion
{
    public class DotNetNamespaceRegion : INamespaceToolRegion<INamespaceItem>
    {
        readonly ModelItem _modelItem;
        readonly ISourceToolRegion<IPluginSource> _source;
        bool _isEnabled;

        Action _sourceChangedNamespace;
        INamespaceItem _selectedNamespace;
        readonly IPluginServiceModel _model;
        ICollection<INamespaceItem> _namespaces;
        bool _isRefreshing;
        double _labelWidth;
        bool _isNamespaceEnabled;
        IList<string> _errors;

        public DotNetNamespaceRegion()
        {
            ToolRegionName = "DotNetNamespaceRegion";
        }

        public DotNetNamespaceRegion(IPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IPluginSource> source)
        {
            try
            {
                Errors = new List<string>();
                LabelWidth = 74;
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
                        Namespaces = _modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity) ? _model.GetNameSpacesWithJsonRetunrs(_source.SelectedSource) : _model.GetNameSpaces(_source.SelectedSource);
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

        public bool IsNewPluginNamespace { get; set; }
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


        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();


                UpdateBasedOnSource();
                SelectedNamespace = null;

                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                _errors.Add(e.Message);
                Errors = _errors;
            }
            finally
            {
                OnSomethingChanged(this);
            }
        }

        void UpdateBasedOnSource()
        {
            if (_source?.SelectedSource != null)
            {
                if (_modelItem.ItemType == typeof(DsfEnhancedDotNetDllActivity))
                {
                    Namespaces = _model.GetNameSpacesWithJsonRetunrs(_source.SelectedSource);
                    IsNamespaceEnabled = true;
                    IsEnabled = true;
                }
                else
                {
                    Namespaces = _model.GetNameSpaces(_source.SelectedSource);
                    IsNamespaceEnabled = true;
                    IsEnabled = true;
                }
            }
        }

        public bool CanRefresh()
        {
            IsNamespaceEnabled = _source.SelectedSource != null;
            return _source.SelectedSource != null;
        }

        public INamespaceItem SelectedNamespace
        {
            get
            {
                return _selectedNamespace;
            }
            set
            {
                SetSelectedNamespace(value);
                SourceChangedNamespace?.Invoke();
                OnSomethingChanged(this);

                var delegateCommand = RefreshNamespaceCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
                delegateCommand?.RaiseCanExecuteChanged();

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
            if (toRestore is DotNetNamespaceRegion region)
            {
                SelectedNamespace = region.SelectedNamespace;
                IsEnabled = region.IsEnabled;
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of INamespaceToolRegion<INamespaceItem>

        void SetSelectedNamespace(INamespaceItem value)
        {
            if (value != null)
            {
                _selectedNamespace = value;
                SavedNamespace = value;
                Namespace = value;
            }
            OnPropertyChanged("SelectedNamespace");
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }
}