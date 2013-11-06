using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.GatherSystemInformation
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
        }

        public ICommand SearchTypeUpdatedCommand { get; private set; }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }
    }
}
