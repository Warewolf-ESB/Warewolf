using Dev2.CustomControls.Panels;
using System.Windows;

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
