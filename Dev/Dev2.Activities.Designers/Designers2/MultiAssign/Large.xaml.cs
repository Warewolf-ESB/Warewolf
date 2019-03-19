#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Activities.Designers2.MultiAssign
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = LargeDataGrid;
        }

        protected override IInputElement GetInitialFocusElement() => DataGrid.GetFocusElement(0);

        void DataGrid_LoadingRow(Object sender, DataGridRowEventArgs e)
        {
            e.Row.Tag = e.Row.GetIndex();
        }
    }
}