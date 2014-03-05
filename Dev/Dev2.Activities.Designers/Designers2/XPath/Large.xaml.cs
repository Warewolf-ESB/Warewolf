
using System.Windows;

namespace Dev2.Activities.Designers2.XPath
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
            return DataGrid.GetFocusElement(0);
        }
    }
}
