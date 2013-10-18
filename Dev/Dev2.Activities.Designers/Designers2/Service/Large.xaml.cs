
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core.Controls;
using Dev2.Data.Interfaces;

namespace Dev2.Activities.Designers2.Service
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = InputsDataGrid;
            Loaded += (sender, args) => InitializeHeight();
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }

        void InitializeHeight()
        {
            //var viewModel = (ServiceDesignerViewModel)DataContext;

            //var inputsHeight = GetHeightInfo(viewModel.DataMappingViewModel.Inputs, InputsDataGrid);
            //var outputsHeight = GetHeightInfo(viewModel.DataMappingViewModel.Outputs, OutputsDataGrid);

            //if(inputsHeight.Count + outputsHeight.Count > 12)
            //{
            //    var diff = ContentGrid.ActualHeight - InputsDataGrid.ActualHeight - OutputsDataGrid.ActualHeight;
            //    ContentGrid.Height = diff + (outputsHeight.Row * 5) + (inputsHeight.Row * 5) + outputsHeight.Header + inputsHeight.Header;
            //}

            const double Offset = 20;
            const double MaxHeightValue = 500;

            var actualHeight = ActualHeight;
            if(actualHeight > MaxHeightValue)
            {
                actualHeight = MaxHeightValue;
            }

            MinHeight = actualHeight + Offset;
            MaxHeight = actualHeight + Offset;
        }

        HeightInfo GetHeightInfo(IReadOnlyCollection<IInputOutputViewModel> mappings, Dev2DataGrid dataGrid)
        {
            var info = new HeightInfo { Count = mappings.Count };
            if(mappings.Count > 0)
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
            public int Count { get; set; }
        }
    }
}
