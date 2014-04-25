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

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public ICommand MergeTypeUpdatedCommand { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }
    }
}
