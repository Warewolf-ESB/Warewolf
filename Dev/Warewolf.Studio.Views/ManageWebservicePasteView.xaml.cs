using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebservicePasteView.xaml
    /// </summary>
    public partial class ManageWebservicePasteView:IPasteView
    {
        Grid _blackoutGrid;
        Window _window;

        public ManageWebservicePasteView()
        {
            InitializeComponent();
        }



        public string ShowView(string text)
        {
            IsModal = true;
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid { Background = new SolidColorBrush(Colors.Black), Opacity = 0.75 };
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.Manual, MinWidth = 640, MinHeight = 480, ResizeMode = ResizeMode.CanResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            var vm = new PasteVm(text, RequestClose);
            _window.DataContext = vm;
            _window.ShowDialog();
            return vm.Text;
        }

        void RemoveBlackOutEffect()
        {
            Application.Current.MainWindow.Effect = null;
            var content = Application.Current.MainWindow.Content as Grid;
            if (content != null)
            {
                content.Children.Remove(_blackoutGrid);
            }
        }

        public void RequestClose()
        {
            RemoveBlackOutEffect();
            _window.Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            RequestClose();
        }


    }

    public class PasteVm : BindableBase
    {
         string _text;
        readonly Action _action;
        DelegateCommand _saveCommand;
        DelegateCommand _cancelCommand;

        public PasteVm(string text, Action action)
        {
            _text = text;
            _action = action;
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        void Cancel()
        {
            Text = "";
            _action();
        }

        void Save()
        {
            _action();
        }

        public DelegateCommand CancelCommand
        {
            get
            {
                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
            }
        }

        public DelegateCommand SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                _saveCommand = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value; 
                OnPropertyChanged(()=>Text);
            }
        }

        
    }
}
