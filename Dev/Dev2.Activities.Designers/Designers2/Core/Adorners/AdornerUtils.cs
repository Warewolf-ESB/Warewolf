using System.Activities.Presentation;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.Activities.Designers2.Core.Adorners
{
    public static class AdornerUtils
    {
        public static Adorner[] Empty = new Adorner[0];

        public static Adorner[] GetConnectors(this ActivityDesigner designer)
        {
            var element = VisualTreeHelper.GetParent(designer) as FrameworkElement;
            var parentUI = designer.Parent as UIElement;
            var layer = designer.GetAdornerLayer();

            if(parentUI == null || layer == null)
            {
                return Empty;
            }

            if(parentUI.Equals(element))
            {
            }

            var adorners = layer.GetAdorners(parentUI);
            return adorners ?? Empty;
        }

        public static void SetConnectorVisibility(this ActivityDesigner designer, Visibility visibility)
        {
            var adorners = GetConnectors(designer);
            foreach(var adorner in adorners)
            {
                //if(adorner.GetType().Name.Contains("FlowchartConnectionPointsAdorner"))
                //{
                //}
                adorner.Visibility = visibility;
            }
        }
    }
}
