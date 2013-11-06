using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers2.DataMerge
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

        void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tmpCbx = sender as ComboBox;
            if (tmpCbx != null)
            {
                var model = (ModelItem)tmpCbx.DataContext;

                if (tmpCbx.SelectedItem.ToString() == "Index" || tmpCbx.SelectedItem.ToString() == "Chars")
                {
                    ModelItemUtils.SetProperty("EnableAt", true, model);
                }
                else
                {
                    ModelItemUtils.SetProperty("At", string.Empty, model);
                    ModelItemUtils.SetProperty("EnableAt", false, model);
                }
            }
        }
    }
}
