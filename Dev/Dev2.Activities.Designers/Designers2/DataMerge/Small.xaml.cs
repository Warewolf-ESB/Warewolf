using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.DataMerge
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
        }

        public ICommand MergeTypeUpdatedCommand { get; private set; }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }
    }
}
