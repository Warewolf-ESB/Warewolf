/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using Dev2.Common;
using Prism.Mvvm;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Warewolf.Studio.Views
{
    public partial class ManageChatbotSourceControl : UserControl, IView
    {
        public ManageChatbotSourceControl()
        {
            InitializeComponent();
        }

#if !NETFRAMEWORK
		public string Path => throw new System.NotImplementedException();

		public Task RenderAsync(ViewContext context)
		{
			throw new System.NotImplementedException();
		}
#endif
	}
}
