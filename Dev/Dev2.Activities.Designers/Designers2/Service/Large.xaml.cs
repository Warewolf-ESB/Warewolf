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

            if(viewModel.DataMappingViewModel != null)
            {
                var inputsCount = viewModel.DataMappingViewModel.Inputs.Count;
                var outputsCount = viewModel.DataMappingViewModel.Outputs.Count;

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
                var height = heightInfo.Header + (minItemCount * heightInfo.Row) + DataGridBorderThickness;
                rowDef.Height = new GridLength(CalcPercentage(itemCount, totalCount), GridUnitType.Star);
                rowDef.MinHeight = height;
                return rowDef;
            }
            return new RowDefinition();
        }

        static double CalcPercentage(int itemCount, double totalCount)
        {
            const double Weight = 100; // Otherwize vertical scrollbar disappears behind errors control
            var p = (int)((itemCount / totalCount) * 100);
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
    }
}
