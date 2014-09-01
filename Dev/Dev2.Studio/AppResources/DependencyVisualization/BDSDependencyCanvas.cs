using System.Windows;
using Dev2.CustomControls.Panels;

// ReSharper disable once CheckNamespace
namespace CircularDependencyTool
{
    public class BDSDependencyCanvas : DragCanvas
    {
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            UpdateLayout();
        }
    }
}
