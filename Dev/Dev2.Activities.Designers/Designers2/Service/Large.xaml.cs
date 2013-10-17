
using System.Windows;

namespace Dev2.Activities.Designers2.Service
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = InputsDataGrid;
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }
    }
}
