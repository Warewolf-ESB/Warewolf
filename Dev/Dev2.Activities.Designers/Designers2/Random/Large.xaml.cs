using System.Windows;

namespace Dev2.Activities.Designers2.Random
{
    // Interaction logic for Large.xaml
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
        }

        protected override IInputElement GetInitialFocusElement()
        {
            return DataGrid.GetFocusElement(0);
        }
    }
}
