using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;

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
