using System.Windows;
using System.Windows.Documents;

namespace Dev2.Activities.Designers
{
    public interface IActivityDesigner
    {
        UIElement GetContainingElement();
        AdornerLayer GetAdornerLayer();
    }
}
