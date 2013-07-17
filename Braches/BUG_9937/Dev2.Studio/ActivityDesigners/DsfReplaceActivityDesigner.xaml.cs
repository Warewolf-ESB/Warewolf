

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfFindRecordsActivityDesigner.xaml
    public partial class DsfReplaceActivityDesigner
    {
        public DsfReplaceActivityDesigner()
        {
            InitializeComponent();
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfReplaceActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void DsfReplaceActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfReplaceActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
