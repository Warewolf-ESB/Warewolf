using System.Windows;

namespace Dev2.Activities.Designers2.GatherSystemInformation
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
        }

        protected override IInputElement GetInitialFocusElement() => DataGrid.GetFocusElement(0);
    }
}