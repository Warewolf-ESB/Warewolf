#pragma warning disable
﻿using System;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Warewolf.Studio.ViewModels;
#if !NETFRAMEWORK
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
#else
using Microsoft.Practices.Prism.Mvvm;
#endif

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageEmailSourceControl.xaml
    /// </summary>
    public partial class ManageWcfSourceControl : IView, ICheckControlEnabledView
    {
#if !NETFRAMEWORK
		public string Path => throw new NotImplementedException();
#endif

		public ManageWcfSourceControl()
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
                case "WCF Endpoint Url":
                    return EndpointUrlTxtBox.Text;
                default:
                    break;
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
                default:
                    break;
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

#if !NETFRAMEWORK
		public Task RenderAsync(ViewContext context)
		{
			throw new NotImplementedException();
		}
#endif
	}
}
