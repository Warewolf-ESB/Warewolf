using System;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageEmailSourceControl.xaml
    /// </summary>
    public partial class ManageExchangeSourceControl : IView, ICheckControlEnabledView
    {
        public ManageExchangeSourceControl()
        {
            InitializeComponent();
        }

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Send":
                    return TestSendCommand.Command.CanExecute(null);
                case "Save":
                    var viewModel = DataContext as ManageEmailSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
            }
            return false;
        }

        #endregion

        public string GetHeaderText()
        {
            BindingExpression be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return HeaderTextBlock.Text;
        }

        public string GetInputValue(string controlName)
        {
            switch (controlName)
            {
                case "AutoDiscoverUrl":
                    return AutoDiscoverUrlTxtBox.Text;
                case "User Name":
                    return UserNameTextBox.Text;
                case "Password":
                    return PasswordTextBox.Password;
                case "Timeout":
                    return TimeoutTextBox.Text;
                case "To":
                    return ToTextBox.Text;
            }
            return String.Empty;
        }

        public void TestSend()
        {
            TestSendCommand.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManageEmailSourceViewModel;
            if (viewModel != null)
            {
                viewModel.OkCommand.Execute(null);
            }
        }

        public void EnterHostName(string hostname)
        {
            AutoDiscoverUrlTxtBox.Text = hostname;
        }

        public void EnterUserName(string username)
        {
            UserNameTextBox.Text = username;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }

        public void EnterEmailTo(string emailTo)
        {
            ToTextBox.Text = emailTo;
        }
    }
}
