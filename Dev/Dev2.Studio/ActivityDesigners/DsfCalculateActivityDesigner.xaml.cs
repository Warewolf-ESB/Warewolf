using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfCalculateActivityDesigner.xaml
    public partial class DsfCalculateActivityDesigner
    {
        public DsfCalculateActivityDesigner()
        {
            InitializeComponent();
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfCalculateActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {        
            e.Handled = true;        
        }

        void DsfCalculateActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfCalculateActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
