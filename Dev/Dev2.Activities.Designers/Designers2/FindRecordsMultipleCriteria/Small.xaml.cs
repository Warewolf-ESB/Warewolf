using System.Windows;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
