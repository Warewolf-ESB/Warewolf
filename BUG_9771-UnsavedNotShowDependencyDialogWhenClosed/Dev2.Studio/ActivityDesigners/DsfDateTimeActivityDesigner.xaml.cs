using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfDateTimeActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void DsfDateTimeActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfDateTimeActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
