using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for RequestServiceNameView.xaml
    /// </summary>
    public partial class RequestServiceNameView: IRequestServiceNameView
    {
        readonly Grid _blackoutGrid = new Grid();

        public RequestServiceNameView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);

            ContentRendered += (sender, args) =>
            {
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Render,
                    new Action(delegate
                    {
                        ServiceNameTextBox.Focus();
                    }));
            };
            Closing += WindowClosing;
            ShowDialog();
        }

        void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        public void RequestClose()
        {
            Close();
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
            be?.UpdateTarget();
            return ErrorMessageTextBlock.Text;
        }


        public void Cancel()
        {
            CancelButton.Command.Execute(null);
        }

        public void Save()
        {
            OkButton.Command.Execute(null);
        }

        private void RequestServiceNameView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ExplorerView_OnKeyUp(object sender, KeyEventArgs e)
        {
            var environmentViewModel = ExplorerView.ExplorerTree.Items.CurrentItem as EnvironmentViewModel;
            var explorerItemViewModelRename = environmentViewModel?.Children.Flatten(model => model.Children)
                .FirstOrDefault(model => model.IsRenaming);

            if (e.Key == Key.Escape)
            {
                if (explorerItemViewModelRename != null)
                {
                    var textBox = e.OriginalSource as TextBox;
                    explorerItemViewModelRename.ResourceName = textBox?.Text;

                    e.Handled = true;
                    return;
                }
                var requestServiceNameViewModel = DataContext as RequestServiceNameViewModel;
                requestServiceNameViewModel?.CancelCommand.Execute(this);
            }
            else if (e.Key == Key.Delete && !ExplorerView.SearchTextBox.IsFocused)
            {
                var explorerItemViewModelSelected = environmentViewModel?.Children.Flatten(model => model.Children)
                .FirstOrDefault(model => model.IsSelected);
                if (explorerItemViewModelSelected != null && !explorerItemViewModelSelected.IsRenaming && explorerItemViewModelRename == null)
                {
                    explorerItemViewModelSelected.DeleteCommand.Execute(null);
                }
            }
        }

        private void RequestServiceNameView_OnKeyUp(object sender, KeyEventArgs e)
        {
            var environmentViewModel = ExplorerView.ExplorerTree.Items.CurrentItem as EnvironmentViewModel;
            var explorerItemViewModelRename = environmentViewModel?.Children.Flatten(model => model.Children)
                .FirstOrDefault(model => model.IsRenaming);

            if (e.Key == Key.Escape)
            {
                if (explorerItemViewModelRename != null)
                {
                    e.Handled = true;
                    return;
                }
                var requestServiceNameViewModel = DataContext as RequestServiceNameViewModel;
                requestServiceNameViewModel?.CancelCommand.Execute(this);
            }

            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                mainViewModel?.ResetMainView();
            }
        }
    }
}
