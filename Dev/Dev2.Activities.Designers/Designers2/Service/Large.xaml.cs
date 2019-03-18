#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Activities.Designers2.Core.Controls;

namespace Dev2.Activities.Designers2.Service
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            Loaded += (sender, args) => InitializeHeight();
            SetInitialFocus();
        }

        protected override IInputElement GetInitialFocusElement()
        {
            if(InputsDataGrid.Items.Count > 0)
            {
                DataGrid = InputsDataGrid;
                return DataGrid.GetFocusElement(0);
            }
            if(OutputsDataGrid.Items.Count > 0)
            {
                DataGrid = OutputsDataGrid;
                return DataGrid.GetFocusElement(0);
            }
            return OnErrorControl.ErrorVariable;
        }

        void InitializeHeight()
        {
            var viewModel = (ServiceDesignerViewModel)DataContext;

            if(viewModel.MappingManager.DataMappingViewModel != null)
            {
                var inputsCount = viewModel.MappingManager.DataMappingViewModel.Inputs.Count;
                var outputsCount = viewModel.MappingManager.DataMappingViewModel.Outputs.Count;

                if(inputsCount == 0 && outputsCount == 0)
                {
                    viewModel.ThumbVisibility = Visibility.Collapsed;
                    return;
                }
                viewModel.ThumbVisibility = Visibility.Visible;

                var totalCount = (double)(inputsCount + outputsCount);

                var inputsRowDef = InitializeHeight(InputsDataGrid, 0, inputsCount, totalCount);
                var outputsRowDef = InitializeHeight(OutputsDataGrid, 1, outputsCount, totalCount);

                MinHeight = inputsRowDef.MinHeight + outputsRowDef.MinHeight + OnErrorControl.ActualHeight + 12;
            }
        }

        RowDefinition InitializeHeight(Dev2DataGrid dataGrid, int contentGridRow, int itemCount, double totalCount)
        {
            const double DataGridBorderThickness = 2;
            if(itemCount > 0)
            {
                var rowDef = ContentGrid.RowDefinitions[contentGridRow];

                var heightInfo = GetHeightInfo(dataGrid);
                var minItemCount = Math.Min(itemCount, 5);
                var height = heightInfo.Header + minItemCount * heightInfo.Row + DataGridBorderThickness;
                rowDef.Height = new GridLength(CalcPercentage(itemCount, totalCount), GridUnitType.Star);
                rowDef.MinHeight = height;
                return rowDef;
            }
            return new RowDefinition();
        }

        static double CalcPercentage(int itemCount, double totalCount)
        {
            const double Weight = 100; // Otherwize vertical scrollbar disappears behind errors control
            var p = (int)(itemCount / totalCount * 100);
            return p + Weight;
        }

        HeightInfo GetHeightInfo(Dev2DataGrid dataGrid)
        {
            var info = new HeightInfo();
            if(dataGrid.Items.Count > 0)
            {
                var dataGridRow = dataGrid.GetRow(0);
                if(dataGridRow != null)
                {
                    info.Row = dataGridRow.ActualHeight;
                }

                var dataGridColumnHeadersPresenter = InputsDataGrid.GetColumnHeadersPresenter();
                if(dataGridColumnHeadersPresenter != null)
                {
                    info.Header = dataGridColumnHeadersPresenter.ActualHeight;
                }
            }
            return info;
        }

        struct HeightInfo
        {
            public double Header { get; set; }
            public double Row { get; set; }
        }


        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Tag = e.Row.GetIndex();
        }
    }
}
