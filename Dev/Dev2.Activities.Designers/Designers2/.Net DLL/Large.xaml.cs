
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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
        void SetOutputInitialHeight()
        {
            if (Height > 220)
            {
                MinHeight = 290;
                Height = 290;
            }
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
            SetOutputInitialHeight();
            var grid = sender as XamGrid;
            if (grid != null)
            {
                var context = grid.DataContext;
                var items = context as DotNetDllViewModel;
                if (items != null)
                {
                    if (items.Outputs != null && items.OutputsVisible)
                    {
                        if (items.Outputs.Count == 0)
                        {
                            SetToolHeight(350);
                            SetOutputGridHeight(grid, 60);
                        }
                        else if (items.Outputs.Count > 0 && items.Outputs.Count < 5)
                        {
                            double gridHeight = 30 * (items.Outputs.Count + 1);
                            SetToolHeight(290 + gridHeight);
                            SetOutputGridHeight(grid, gridHeight);
                        }
                        else
                        {
                            const double GridHeight = 30 * 6;
                            SetToolHeight(290 + GridHeight);
                            SetOutputGridHeight(grid, GridHeight);
                            SetOutputNewHeight();
                        }
                    }
                }
            }
        }
        void SetOutputNewHeight()
        {
            MainGrid.RowDefinitions[5].Height = new GridLength(10, GridUnitType.Star);
        }
        void SetInputNewHeight()
        {
            MainGrid.RowDefinitions[3].Height = new GridLength(10, GridUnitType.Star);
        }

        void LargeDataGrid_OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetInitialHeight();
            var grid = sender as Dev2DataGrid;
            if (grid != null)
            {
                var context = grid.DataContext;
                var items = context as DotNetDllViewModel;
                if (items != null)
                {
                    if (items.Inputs != null && items.InputsVisible)
                    {
                        if (items.Inputs.Count == 0)
                        {
                            SetToolHeight(290);
                            SetInputGridHeight(grid, 60);
                        }
                        else if (items.Inputs.Count > 0 && items.Inputs.Count < 5)
                        {
                            double gridHeight = 30 * (items.Inputs.Count + 1);
                            SetToolHeight(230 + gridHeight);
                            SetInputGridHeight(grid, gridHeight);
                        }
                        else
                        {
                            const double GridHeight = 30 * 6;
                            SetToolHeight(230 + GridHeight);
                            SetInputGridHeight(grid, GridHeight);
                            SetInputNewHeight();
                        }
                    }
                }
            }
        }

        void SetToolHeight(double height)
        {
            Height = height;
            MinHeight = height;
        }

        static void SetInputGridHeight(Dev2DataGrid grid, double height)
        {
            grid.MinHeight = height;
        }
        static void SetOutputGridHeight(XamGrid grid, double height)
        {
            grid.MinHeight = height;
        }
    }
}
