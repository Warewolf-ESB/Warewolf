using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.DataSplit
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
            return InitialFocusElement;
        }

        void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmpCbx = sender as ComboBox;
            if(tmpCbx != null)
            {
                dynamic tmpDto = tmpCbx.DataContext;
                if (tmpCbx.SelectedItem.ToString() == "Index" || tmpCbx.SelectedItem.ToString() == "Chars")
                {
                    tmpDto.EnableAt = true;
                }
                else
                {
                    tmpDto.At = string.Empty;
                    tmpDto.EnableAt = false;
                }
            }
        }
    }
}
