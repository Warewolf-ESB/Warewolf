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
    public class ComNamespaceRegion : INamespaceToolRegion<INamespaceItem>
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IComPluginSource> _source;
        private bool _isEnabled;

        private Action _sourceChangedNamespace;
        private INamespaceItem _selectedNamespace;
        private readonly IComPluginServiceModel _model;
        private ICollection<INamespaceItem> _namespaces;
        private bool _isRefreshing;
        private double _labelWidth;
        private bool _isNamespaceEnabled;
        private IList<string> _errors;

        public ComNamespaceRegion()
        {
            ToolRegionName = "ComNamespaceRegion";
        }

        public ComNamespaceRegion(IComPluginServiceModel model, ModelItem modelItem, ISourceToolRegion<IComPluginSource> source)
        {
            try
            {
                Errors = new List<string>();

                LabelWidth = 70;
                ToolRegionName = "ComNamespaceRegion";
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
            catch (Exception e)
            {
                Errors.Add(e.Message);

            }
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
            try
            {
                Errors.Clear();

                
                UpdateBasedOnSource();
                
                
                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                _errors.Add(e.Message);
                Errors = _errors;


            }
            finally
            {
               // OnSomethingChanged(this);
            }
        }

        private void UpdateBasedOnSource()
        {
            if (_source?.SelectedSource != null)
            {
                SelectedNamespace = null;
                var ns = _model.GetNameSpaces(_source.SelectedSource);
                Namespaces = ns;


                IsNamespaceEnabled = true;
                IsEnabled = true;
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
                SourceChangedNamespace();
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
            return new ComNamespaceRegion
            {
                IsEnabled = IsEnabled,
                SelectedNamespace = SelectedNamespace
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is ComNamespaceRegion region)
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

        private void SetSelectedNamespace(INamespaceItem value)
        {
            //if (value != null)
            //{
                _selectedNamespace = value;
                SavedNamespace = value;
                Namespace = value;
            //}
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnSomethingChanged(IToolRegion args)
        {
            SomethingChanged?.Invoke(this, args);
        }
    }
}