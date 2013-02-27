using System.Collections.Generic;
using Dev2.Converters.DateAndTime;
using System.Windows.Controls;
using Dev2.Studio.Core.Activities.Utils;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml
    public partial class DsfDateTimeActivityDesigner
    {
        public IList<string> ItemList { get; set; }

        public DsfDateTimeActivityDesigner()
        {
            InitializeComponent();
            ItemList = DateTimeFormatter.TimeModifierTypes;
            cbxTimeModifers.ItemsSource = ItemList;
            if (cbxTimeModifers.SelectedValue == null)
                    txtTimeMofifierAmount.IsEnabled = false;
        }

        private void cbxTimeModifers_DropDownClosed(object sender, System.EventArgs e)
        {
            ComboBox timeModifierComboBox = sender as ComboBox;


            if ((string)timeModifierComboBox.SelectedItem == string.Empty)
            {
                ModelItemUtils.SetProperty<string>("TimeModifierAmountDisplay", string.Empty, ModelItem);
                txtTimeMofifierAmount.IsEnabled = false;
            }
            else
            {
                txtTimeMofifierAmount.IsEnabled = true;
            }
        }
    }
}
