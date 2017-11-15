using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
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

        private void RequestServiceNameView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void ExplorerView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (IsRenaming)
                {
                    IsRenaming = false;
                    e.Handled = true;
                    return;
                }
                RequestServiceNameViewModel?.CancelCommand.Execute(this);
            }
            if (e.Key == Key.Delete && !ExplorerView.SearchTextBox.IsFocused)
            {
                var explorerItemViewModelSelected = RequestServiceNameViewModel?.ExplorerItemViewModelIsSelected();
                if (explorerItemViewModelSelected != null && !explorerItemViewModelSelected.IsRenaming)
                {
                    explorerItemViewModelSelected?.DeleteCommand.Execute(null);
                }
            }
        }

        RequestServiceNameViewModel RequestServiceNameViewModel => DataContext as RequestServiceNameViewModel;

        private bool IsRenaming { get; set; }

        private void ExplorerView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HandleRenameResource(e);
            }
        }

        private void RequestServiceNameView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HandleRenameResource(e);
                RequestServiceNameViewModel?.CancelCommand.Execute(this);
            }
        }

        private void RequestServiceNameView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleRenameResource(e);
            }
        }
        
        private void HandleRenameResource(KeyEventArgs e)
        {
            var explorerItemViewModelRename = RequestServiceNameViewModel?.ExplorerItemViewModelRename();
            if (explorerItemViewModelRename != null)
            {
                var textBox = e.OriginalSource as TextBox;
                explorerItemViewModelRename.ResourceName = textBox?.Text;
                IsRenaming = true;
                e.Handled = true;
            }
        }
    }
}
