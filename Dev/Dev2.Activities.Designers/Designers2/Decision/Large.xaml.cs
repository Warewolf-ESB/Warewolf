/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;
#endif

namespace Dev2.Activities.Designers2.Decision
{
    public partial class Large:IView
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = LargeDataGrid;
        }

        protected override IInputElement GetInitialFocusElement() => DataGrid;

#if !NETFRAMEWORK
        public string Path => throw new System.NotImplementedException();

        public Task RenderAsync(ViewContext context)
        {
            throw new System.NotImplementedException();
        }
#endif
    }
}
