﻿#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Warewolf.Studio.ViewModels;
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;
#else
using Microsoft.Practices.Prism.Mvvm;
#endif

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageEmailSourceControl.xaml
    /// </summary>
    public partial class ManageExchangeSourceControl : IView, ICheckControlEnabledView
    {
#if !NETFRAMEWORK
		public string Path => throw new System.NotImplementedException();
#endif

		public ManageExchangeSourceControl()
        {
            InitializeComponent();
        }

        public void TestSend()
        {
            TestSendCommand.Command.Execute(null);
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
                default:
                    break;
            }
            return false;
        }

#if !NETFRAMEWORK
		public Task RenderAsync(ViewContext context)
		{
			throw new System.NotImplementedException();
		}
#endif

		#endregion
	}
}
