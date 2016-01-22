
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using Infragistics.Controls.Grids;

namespace Dev2.Activities.Designers2.Net_DLL
{
    // Interaction logic for Large.xaml
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = LargeDataGrid;
            SetInitialFocus();
            SetInitialHeight();
        }

        void SetInitialHeight()
        {
            Height = 320;
            MinHeight = 320;
            MainGrid.RowDefinitions[5].Height = GridLength.Auto;
        }
        void SetNewHeight()
        {
            MinHeight = 400;
            Height = 400;
            MainGrid.RowDefinitions[5].Height = new GridLength(10, GridUnitType.Star);
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return SourcesComboBox;
        }

        #endregion

        void TestInputButton_OnClick(object sender, RoutedEventArgs e)
        {
            RecordSetTextBox.Focus();
        }

        void OutputsMappingDataGrid_OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetInitialHeight();
            var grid = sender as XamGrid;
            if (grid != null)
            {
                var context = grid.DataContext;
                var items = context as DotNetDllViewModel;
                if (items != null)
                {
                    if (items.TestComplete)
                    {
                        SetNewHeight();
                    }
                }
            }
        }
    }
}
