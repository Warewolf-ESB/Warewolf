using Dev2.Studio.ViewModels.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Studio.Diagnostics
{
    public class DebugStyleSelector : StyleSelector
    {

        public Style DebugStringStyle { get; set; }

        public Style DebugStateStyle { get; set; }

        public override Style SelectStyle(object item,
        DependencyObject container)
        {
            if (item.GetType() == typeof(DebugStringTreeViewItemViewModel))
            {
                return DebugStringStyle;
            }

            if (item.GetType() == typeof(DebugStateTreeViewItemViewModel))
            {
                return DebugStateStyle;
            }
            return null;
        }

    }
}
