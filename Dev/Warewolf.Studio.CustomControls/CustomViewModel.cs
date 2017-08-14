using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.CustomControls
{
    public class EditableSourceControl : Control
    {
        
    }


    public class CustomViewModel : INotifyPropertyChanged
    {
        readonly IServer _server;

        public CustomViewModel(IServer server)
        {
            _server = server;

            Sources = new List<string>(new[] { "Item One", "Item Two", "Item Three" });
            SelectedSource = Sources.FirstOrDefault();

            Inputs = new ObservableCollection<IInputOutputViewModel>();
            Outputs = new ObservableCollection<IInputOutputViewModel>();

            NewSourceCommand = new RelayCommand(o => NewSource());
            EditSourceCommand = new RelayCommand(o => EditSource(), o => IsSourceSelected);
            RefreshActionsCommand = new RelayCommand(o => RefreshActions(), o => IsActionSelected);
            TestInputCommand = new RelayCommand(o => TestInput());

            SourceVisible = Visibility.Visible;
            NamespaceVisible = Visibility.Collapsed;
            ActionVisible = Visibility.Visible;
            AdditionalInfoVisible = Visibility.Collapsed;
            InputsOutputsVisible = Visibility.Visible;
            OnErrorVisible = Visibility.Visible;
        }

        public bool IsActionSelected => !string.IsNullOrWhiteSpace(SelectedSource);
        public bool IsSourceSelected => !string.IsNullOrWhiteSpace(SelectedSource);

        public event PropertyChangedEventHandler PropertyChanged;

        #region SOURCES

        private IEnumerable<string> _sources;
        public IEnumerable<string> Sources
        {
            get { return _sources; }
            set
            {
                _sources = value;
                // ReSharper disable once UseNullPropagation
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Sources"));
                }
            }
        }

        private string _selectedSource;
        public string SelectedSource
        {
            get { return _selectedSource; }
            set
            {
                _selectedSource = value;
                // ReSharper disable once UseNullPropagation
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedSource"));
                }
            }
        }

        public RelayCommand NewSourceCommand { get; private set; }
        public RelayCommand EditSourceCommand { get; private set; }

        public Visibility SourceVisible { get; set; }

        #endregion

        #region NAMESPACES

        private IEnumerable<string> _namespaces;
        public IEnumerable<string> Namespaces
        {
            get { return _namespaces; }
            set
            {
                _namespaces = value;
                // ReSharper disable once UseNullPropagation
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Namespaces"));
                }
            }
        }

        private string _selectedNamespace;
        public string SelectedNamespace
        {
            get { return _selectedNamespace; }
            set
            {
                _selectedNamespace = value;
                // ReSharper disable once UseNullPropagation
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedNamespace"));
                }
            }
        }

        public Visibility NamespaceVisible { get; set; }

        #endregion

        #region ACTIONS

        private IEnumerable<string> _actions;
        public IEnumerable<string> Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                // ReSharper disable once UseNullPropagation
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Actions"));
                }
            }
        }

        private string _selectedAction;
        public string SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                _selectedAction = value;
                // ReSharper disable once UseNullPropagation
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedAction"));
                }
            }
        }

        public RelayCommand RefreshActionsCommand { get; private set; }

        public Visibility ActionVisible { get; set; }

        #endregion

        #region ADDITIONAL DETAILS

        public Visibility AdditionalInfoVisible { get; set; }

        #endregion

        #region INPUTS/OUTPUTS

        public ObservableCollection<IInputOutputViewModel> Outputs { get; private set; }

        public ObservableCollection<IInputOutputViewModel> Inputs { get; private set; }

        public RelayCommand TestInputCommand { get; private set; }

        public Visibility InputsOutputsVisible { get; set; }

        #endregion

        #region ERROR

        public Visibility OnErrorVisible { get; set; }

        #endregion

        #region METHODS

        static void NewSource()
        {
        }

        static void EditSource()
        {

        }

        static void RefreshActions()
        {

        }

        static void TestInput()
        {
        }

        #endregion
    }
}
