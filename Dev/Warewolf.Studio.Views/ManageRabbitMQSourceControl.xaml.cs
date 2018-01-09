﻿using System;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageRabbitMQSourceControl.xaml
    /// </summary>
    
    public partial class ManageRabbitMQSourceControl : IView, ICheckControlEnabledView
    {
        public ManageRabbitMQSourceControl()
        {
            InitializeComponent();
        }

        public string GetHeaderText()
        {
            var be = HeaderTextBlock.GetBindingExpression(TextBlock.TextProperty);
            be?.UpdateTarget();
            return HeaderTextBlock.Text;
        }

        public string GetInputValue(string controlName)
        {
            switch (controlName)
            {
                case "Host":
                    return HostTextBox.Text;
                case "Port":
                    return PortTextBox.Text;
                case "User Name":
                    return UserNameTextBox.Text;
                case "Password":
                    return PasswordTextBox.Password;
                case "Virtual Host":
                    return VirtualHostTextBox.Text;
                default:
                    break;
            }
            return String.Empty;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Test Connection":
                    return TestPublishCommand.Command.CanExecute(null);
                case "Save":
                    var viewModel = DataContext as ManageRabbitMQSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                default:
                    break;
            }
            return false;
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

        public void TestPublish()
        {
            TestPublishCommand.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManageRabbitMQSourceViewModel;
            viewModel?.OkCommand.Execute(null);
        }
    }
}