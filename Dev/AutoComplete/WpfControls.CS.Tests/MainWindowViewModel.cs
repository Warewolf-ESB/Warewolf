

using System.Collections.ObjectModel;
using Dev2;

namespace WpfControls.CS.Test
{
    using System;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Windows;

    public class MainWindowViewModel : INotifyPropertyChanged
    {

        #region "Fields"

        private ICommand _cancelCommand;
        private string _fileName;
        private ICommand _openCommand;

        private string _selectedPath;
        int _level;
        Dev2TrieSugggestionProvider _dev2Provider;
        Dev2TrieSugggestionProvider _dev2RecsetProvider;
        Dev2TrieSugggestionProvider _dev2ScalarProvider;
        string _selectedPathrecset;
        string _selectedPathScalar;

        #endregion

        #region "Constructor"
        public MainWindowViewModel()
        {
            SelectedPath = "";
            SelectedPathRecset = "";
            SelectedPathScalar = "";
            
            Level = 5;
            BuildTrie(Level);
        }

        void BuildTrie(int level)
        {
            Dev2Provider = new Dev2TrieSugggestionProvider(IntellisenseStringProvider.FilterOption.All, level)
            {
                VariableList = new ObservableCollection<string>
                {
                    "[[a]]",
                    "[[b]]",
                    "[[Rec().a]]",
                    "[[Rec().b]]",
                    "[[Rec().d]]",
                    "[[Bob().e]]",
                    "[[The().d]]",
                    "[[Builder().e]]",
                    "[[Can().t]]",
                    "[[We().t]]",
                    "[[Build().d]]",
                    "[[Build().it]]",
                }
                
            };
            Dev2Provider.Level = level;

            Dev2RecsetProvider = new Dev2TrieSugggestionProvider(IntellisenseStringProvider.FilterOption.Recordsets,level)
            {
          
                VariableList = new ObservableCollection<string>
                {
                    "[[a]]",
                    "[[b]]",
                    "[[Rec().a]]",
                    "[[Rec().b]]",
                    //"[[Rec()]]",
                    //"[[Bob()]]",
                    //"[[The()]]",
                    //"[[Builder()]]",
                    //"[[Can()]]",
                    //"[[We()]]",
                    //"[[Build()]]",
                    //"[[Build().it]]",
                }
            };
            Dev2ScalarProvider = new Dev2TrieSugggestionProvider(IntellisenseStringProvider.FilterOption.Scalars,level)
            {
              
                VariableList = new ObservableCollection<string>
                {
                    "[[a]]",
                    "[[b]]",
                    "[[Rec().a]]",
                    "[[Rec().b]]",
                    //"[[Rec()]]",
                    //"[[Bob()]]",
                    //"[[The()]]",
                    //"[[Builder()]]",
                    //"[[Can()]]",
                    //"[[We()]]",
                    //"[[Build()]]",
                    //"[[Build().it]]",
                }
            };
        }

        #endregion

        #region "Events"

        public Dev2TrieSugggestionProvider Dev2Provider
        {
            get
            {
                return _dev2Provider;
            }
            set
            {
                _dev2Provider = value;
                RaisePropertyChanged("Dev2Provider");
            }
        }
        public Dev2TrieSugggestionProvider Dev2RecsetProvider
        {
            get
            {
                return _dev2RecsetProvider;
            }
            set
            {
                _dev2RecsetProvider = value;
                RaisePropertyChanged("Dev2RecsetProvider");
            }
        }
        public Dev2TrieSugggestionProvider Dev2ScalarProvider
        {
            get
            {
                return _dev2ScalarProvider;
            }
            set
            {
                _dev2ScalarProvider = value;
                RaisePropertyChanged("Dev2ScalarProvider");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region "Properties"

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(ExecuteCancelCommand, null);
                }
                return _cancelCommand;
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new DelegateCommand(ExecuteOpenCommand, null);
                }
                return _openCommand;
            }
        }

        public string SelectedPath
        {
            get { return _selectedPath; }
            set { _selectedPath = value; RaisePropertyChanged("SelectedPath"); }
        }

        public string SelectedPathRecset
        {
            get { return _selectedPathrecset; }
            set { _selectedPathrecset = value; RaisePropertyChanged("SelectedPath"); }
        }

        public string SelectedPathScalar
        {
            get { return _selectedPathScalar; }
            set { _selectedPathScalar = value; RaisePropertyChanged("SelectedPath"); }
        }
        #endregion

        #region "Methods"

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ExecuteCancelCommand(object param)
        {
            Application.Current.Shutdown();
        }

        private void ExecuteOpenCommand(object param)
        {
            try
            {
                Process.Start(SelectedPath);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {

            }
        }

        #endregion

        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                BuildTrie(Level);
                RaisePropertyChanged("Level"); 
            }
        }
    }
}
