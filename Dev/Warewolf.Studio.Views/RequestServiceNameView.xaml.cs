using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for RequestServiceNameView.xaml
    /// </summary>
    public partial class RequestServiceNameView:IRequestServiceNameView
    {
        private Grid _blackoutGrid;
        Window _window;

        public RequestServiceNameView()
        {
            InitializeComponent();

            var textBlock = new TextBlock();
            textBlock.FontSize = 16.0;
            textBlock.Margin = new Thickness(10, 0, 0, 0);
            textBlock.Text = "Please enter a service name";
            Header = textBlock; 

        }

        public void ShowView()
        {
            IsModal = true;
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid();
            _blackoutGrid.Background = new SolidColorBrush(Colors.Black);
            _blackoutGrid.Opacity = 0.75;
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;

            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            _window.ShowDialog();
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

        public bool HasServer(string serverName)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            var environmentViewModel = viewTestClass.OpenEnvironmentNode(serverName);
            return environmentViewModel != null;
        }

        public void CreateNewFolder(string newFolderName, string rootPath)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.PerformFolderAdd(newFolderName,rootPath);
        }

        public IExplorerView GetExplorerView()
        {
            return ExplorerView;
        }

        public void OpenFolder(string folderName)
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            viewTestClass.OpenFolderNode(folderName);
        }

        public void EnterName(string serviceName)
        {
            ServiceNameTextBox.Text = serviceName;            
        }

        public bool IsSaveButtonEnabled()
        {
            return OkButton.Command.CanExecute(null);
        }

        public string GetValidationMessage()
        {
            BindingExpression be = ErrorMessageTextBlock.GetBindingExpression(TextBlock.TextProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return ErrorMessageTextBlock.Text;
        }

        public List<IExplorerItemViewModel> GetFoldersVisible()
        {
            ExplorerViewTestClass viewTestClass = new ExplorerViewTestClass(ExplorerView);
            return viewTestClass.GetFoldersVisible();
        }
    }
}
