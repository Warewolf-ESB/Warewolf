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
    public partial class ManageEmailSourceControl : IView, ICheckControlEnabledView
    {
        public ManageEmailSourceControl()
        {
            InitializeComponent();
        }

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
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public string GetInputValue(string controlName)
        {
            switch (controlName)
            {
                case "Host":
                    return HostTextBox.Text;
                case "User Name":
                    return UserNameTextBox.Text;
                case "Password":
                    return PasswordTextBox.Password;
                case "Enable SSL":
                    if (EnableSslYes.IsChecked == true)
                    {
                        return "True";
                    }
                    return "False";
                case "Port":
                    return PortTextBox.Text;
                case "Timeout":
                    return TimeoutTextBox.Text;
                case "From":
                    return FromTextBox.Text;
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
            viewModel?.OkCommand.Execute(null);
        }

        public void EnterHostName(string hostname)
        {
            HostTextBox.Text = hostname;
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

        public void EnterEmailFrom(string emailFrom)
        {
            FromTextBox.Text = emailFrom;
        }
    }
}
