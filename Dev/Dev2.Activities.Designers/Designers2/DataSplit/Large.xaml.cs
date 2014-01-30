using System.Windows;

namespace Dev2.Activities.Designers2.DataSplit
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = LargeDataGrid;
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
