
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dev2.Studio.ActivityDesigners
{
    // Interaction logic for DsfScriptingJavaScrip.xaml
    public partial class DsfScriptingJavaScriptDesigner
    {
        public DsfScriptingJavaScriptDesigner()
        {
            InitializeComponent();
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfScriptingJavaScriptDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void DsfScriptingJavaScriptDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
        }

        void DsfScriptingJavaScriptDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
        }
    }
}
