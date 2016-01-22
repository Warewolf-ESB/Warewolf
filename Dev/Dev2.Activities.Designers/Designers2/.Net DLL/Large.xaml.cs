
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
using Dev2.Activities.Designers2.Core.Controls;
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
            MainGrid.RowDefinitions[3].Height = GridLength.Auto;
            MainGrid.RowDefinitions[5].Height = GridLength.Auto;

            MinHeight = 220;
            Height = 220;
        }
        void SetNewHeight()
        {
            var height = 220 + MainGrid.RowDefinitions[3].ActualHeight + MainGrid.RowDefinitions[5].ActualHeight;

            MinHeight = height;
            Height = height;

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
        void SetInputNewHeight()
        {
            var height = 220 + MainGrid.RowDefinitions[3].ActualHeight;

            MinHeight = height;
            Height = height;

            MainGrid.RowDefinitions[3].Height = new GridLength(10, GridUnitType.Star);
        }

        void LargeDataGrid_OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as Dev2DataGrid;
            if (grid != null)
            {
                var context = grid.DataContext;
                var items = context as DotNetDllViewModel;
                if (items != null)
                {
                    if (items.Inputs != null)
                    {
                        if (items.Inputs.Count > 0)
                        {
                            var rowCount = grid.CountRows();
                            if (rowCount > 0)
                            {
                                if (rowCount > 5)
                                {
                                    grid.MaxHeight = grid.RowHeight * 5;
                                }
                                else
                                {
                                    grid.MaxHeight = rowCount * grid.RowHeight;
                                }
                            }
                            SetInputNewHeight();
                        }
                        else
                        {
                            MaxHeight = 230;
                            grid.MinHeight = 80;
                            grid.MaxHeight = 80;
                        }
                    }
                }
            }
        }

        void LargeDataGrid_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as Dev2DataGrid;
            if (grid != null)
            {
                var context = grid.DataContext;
                var items = context as DotNetDllViewModel;
                if (items != null)
                {
                    if (items.Inputs != null)
                    {

                    }
                }
            }
        }
    }
}
