using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;

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
    }
}
