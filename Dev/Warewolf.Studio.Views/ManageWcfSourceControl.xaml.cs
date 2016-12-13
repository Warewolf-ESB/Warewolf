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
    public partial class ManageWcfSourceControl : IView, ICheckControlEnabledView
    {
        public ManageWcfSourceControl()
        {
            InitializeComponent();
        }

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
                case "WCF Endpoint Url":
                    return EndpointUrlTxtBox.Text;
            }
            return String.Empty;
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Test Connection":
                    return TestSendCommand.Command.CanExecute(null);
                case "Save":
                    var viewModel = DataContext as ManageWcfSourceViewModel;
                    return viewModel != null && viewModel.SaveCommand.CanExecute(null);
            }
            return false;
        }

        #endregion

        public void EnterEndpointUrl(string endpointUrl)
        {
            EndpointUrlTxtBox.Text = endpointUrl;
        }

        public void TestSend()
        {
            TestSendCommand.Command.Execute(null);
        }
    }
}
