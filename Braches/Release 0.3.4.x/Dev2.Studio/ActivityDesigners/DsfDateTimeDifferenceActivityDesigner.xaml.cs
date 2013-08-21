using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Converters.DateAndTime;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfDateTimeActivityDesigner.xaml 
    public partial class DsfDateTimeDifferenceActivityDesigner
    {
        public IList<string> ItemList { get; set; }

        public DsfDateTimeDifferenceActivityDesigner()
        {
            InitializeComponent();
            ItemList = DateTimeComparer.OutputFormatTypes;
            cbxOutputTypes.ItemsSource = ItemList;            
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfDateTimeDifferenceActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void DsfDateTimeDifferenceActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfDateTimeDifferenceActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
